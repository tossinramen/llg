using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The title screen + character creator. It builds its whole UI from code when you press Play,
    /// so the scene looks empty in the editor until then - that is expected.
    /// </summary>
    public class StartScreenController : MonoBehaviour
    {
        static readonly Color PanelColor = new Color(0.10f, 0.12f, 0.26f, 0.85f);
        static readonly Color ButtonColor = new Color32(70, 96, 190, 255);
        static readonly Color AccentColor = new Color32(240, 170, 60, 255);
        static readonly Color TextColor = new Color32(245, 240, 225, 255);

        const float LabelX = 0, ValueX = 175;

        PlayerProfile profile;
        CharacterPreview preview;
        Transform root;
        InputField nameInput;
        Text tagline, status;
        Button startButton;
        int languageIndex, taglineIndex;
        float taglineTimer;
        readonly List<Action> refreshers = new List<Action>();
        readonly List<RectTransform> clouds = new List<RectTransform>();
        readonly List<Image> genderButtons = new List<Image>();
        readonly List<string> taglines = new List<string>();

        void Awake()
        {
            profile = PlayerProfile.Load();
            languageIndex = Mathf.Max(0, Array.FindIndex(GameConfig.Languages, l => l.Code == profile.languageCode));

            taglines.Add(GameConfig.EnglishTagline);
            foreach (var language in GameConfig.Languages) taglines.Add(language.Tagline);

            BuildCanvas();
            BuildBackground();
            BuildTitle();
            BuildCharacterPanel();
            BuildOptionsPanel();
            RefreshAll();
        }

        // ------------------------------------------------------------ building

        void BuildCanvas()
        {
            if (FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(640, 360);
            scaler.matchWidthOrHeight = 0.5f;
            root = go.transform;
        }

        void BuildBackground()
        {
            var bg = UIFactory.Img("Background", root, Vector2.zero, new Vector2(640, 360), Color.white,
                UIFactory.LoadSprite("Art/UI/title_background"));
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            var cloud = UIFactory.LoadSprite("Art/UI/cloud");
            var spots = new[] { new Vector3(-250, 150, 2f), new Vector3(-20, 105, 1.5f), new Vector3(210, 135, 2.5f) };
            foreach (var spot in spots)
            {
                var img = UIFactory.Img("Cloud", root, spot, new Vector2(48, 16) * spot.z, new Color(1, 1, 1, 0.9f), cloud);
                clouds.Add(img.rectTransform);
            }
        }

        void BuildTitle()
        {
            var title = UIFactory.Label("Title", root, new Vector2(0, 152), new Vector2(600, 40), GameConfig.Title, 32, TextColor);
            title.fontStyle = FontStyle.Bold;
            Shadow(title.gameObject, 2);
            tagline = UIFactory.Label("Tagline", root, new Vector2(0, 126), new Vector2(600, 20), taglines[0], 14, TextColor);
            Shadow(tagline.gameObject, 1);
        }

        void BuildCharacterPanel()
        {
            UIFactory.Img("CharacterPanel", root, new Vector2(-195, -15), new Vector2(180, 250), PanelColor);

            var holder = UIFactory.Rect("CharacterPreview", root, new Vector2(-195, 20), new Vector2(96, 144));
            preview = holder.gameObject.AddComponent<CharacterPreview>();
            preview.Build(new Vector2(96, 144));

            UIFactory.Btn("TurnLeft", root, new Vector2(-240, -80), new Vector2(60, 22), "< Turn", ButtonColor, () => preview.Turn(-1), 12);
            UIFactory.Btn("TurnRight", root, new Vector2(-150, -80), new Vector2(60, 22), "Turn >", ButtonColor, () => preview.Turn(1), 12);
            UIFactory.Btn("Randomize", root, new Vector2(-195, -112), new Vector2(150, 22), "Randomize", ButtonColor, Randomize, 12);
        }

        void BuildOptionsPanel()
        {
            UIFactory.Img("OptionsPanel", root, new Vector2(115, -15), new Vector2(390, 250), PanelColor);
            var look = profile.appearance;

            RowLabel("Name", 90);
            nameInput = UIFactory.Input("NameInput", root, new Vector2(ValueX, 90), new Vector2(170, 22), "Your name");
            nameInput.characterLimit = 12;
            nameInput.text = profile.playerName;

            RowLabel("Gender", 60);
            string[] icons = { "gender_male", "gender_female", "gender_nonbinary" };
            for (int i = 0; i < icons.Length; i++)
            {
                var gender = (Gender)i;
                var button = UIFactory.Btn("Gender" + gender, root, new Vector2(ValueX - 40 + i * 40, 60), new Vector2(26, 26), "",
                    ButtonColor, () => { profile.gender = gender; RefreshAll(); });
                UIFactory.Img("Icon", button.transform, Vector2.zero, new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/" + icons[i]));
                genderButtons.Add(button.GetComponent<Image>());
            }

            ColorRow("Skin", 30, AppearancePalette.Skin, () => look.skin, v => look.skin = v);
            CycleRow("Hair style", 0, AppearancePalette.HairStyles.Length, () => look.hairStyle, v => look.hairStyle = v,
                (swatch, text) => { swatch.color = ButtonColor; text.text = AppearancePalette.HairStyles[look.hairStyle]; });
            ColorRow("Hair color", -30, AppearancePalette.Hair, () => look.hairColor, v => look.hairColor = v);
            ColorRow("Shirt", -60, AppearancePalette.Shirt, () => look.shirt, v => look.shirt = v);
            ColorRow("Pants", -90, AppearancePalette.Pants, () => look.pants, v => look.pants = v);
            CycleRow("Language", -120, GameConfig.Languages.Length, () => languageIndex, v => languageIndex = v,
                (swatch, text) =>
                {
                    var language = GameConfig.Languages[languageIndex];
                    swatch.color = ButtonColor;
                    text.text = language.Available ? language.NativeName : language.NativeName + " (soon)";
                });

            startButton = UIFactory.Btn("StartButton", root, new Vector2(115, -160), new Vector2(160, 28), "Start Adventure", AccentColor, StartGame, 15);
            status = UIFactory.Label("Status", root, new Vector2(-195, -160), new Vector2(190, 28), "", 12, TextColor);
            Shadow(status.gameObject, 1);
        }

        void RowLabel(string text, float y)
        {
            UIFactory.Label(text + "Label", root, new Vector2(LabelX, y), new Vector2(110, 22), text, 14, TextColor, TextAnchor.MiddleLeft);
        }

        void ColorRow(string label, float y, Color32[] palette, Func<int> get, Action<int> set)
        {
            CycleRow(label, y, palette.Length, get, set, (swatch, text) =>
            {
                swatch.color = palette[get()];
                text.text = "";
            });
        }

        void CycleRow(string label, float y, int count, Func<int> get, Action<int> set, Action<Image, Text> show)
        {
            RowLabel(label, y);
            var swatch = UIFactory.Img(label + "Value", root, new Vector2(ValueX, y), new Vector2(110, 20), Color.white);
            var text = UIFactory.Label("Text", swatch.transform, Vector2.zero, new Vector2(110, 20), "", 13, Color.white);
            Action<int> step = delta => { set(((get() + delta) % count + count) % count); RefreshAll(); };
            UIFactory.Btn(label + "Prev", root, new Vector2(ValueX - 73, y), new Vector2(24, 22), "<", ButtonColor, () => step(-1));
            UIFactory.Btn(label + "Next", root, new Vector2(ValueX + 73, y), new Vector2(24, 22), ">", ButtonColor, () => step(1));
            refreshers.Add(() => show(swatch, text));
        }

        static void Shadow(GameObject go, float distance)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            shadow.effectDistance = new Vector2(distance, -distance);
        }

        // ------------------------------------------------------------ behaviour

        void RefreshAll()
        {
            foreach (var refresh in refreshers) refresh();
            for (int i = 0; i < genderButtons.Count; i++)
                genderButtons[i].color = (int)profile.gender == i ? AccentColor : ButtonColor;
            preview.Show(profile.appearance);

            bool available = GameConfig.Languages[languageIndex].Available;
            startButton.interactable = available;
            status.text = available ? "" : GameConfig.Languages[languageIndex].EnglishName + " is coming soon!";
        }

        void Randomize()
        {
            var look = profile.appearance;
            look.skin = UnityEngine.Random.Range(0, AppearancePalette.Skin.Length);
            look.hairStyle = UnityEngine.Random.Range(0, AppearancePalette.HairStyles.Length);
            look.hairColor = UnityEngine.Random.Range(0, AppearancePalette.Hair.Length);
            look.shirt = UnityEngine.Random.Range(0, AppearancePalette.Shirt.Length);
            look.pants = UnityEngine.Random.Range(0, AppearancePalette.Pants.Length);
            RefreshAll();
        }

        void StartGame()
        {
            string playerName = nameInput.text.Trim();
            if (playerName.Length == 0)
            {
                status.text = "Please enter a name first.";
                return;
            }
            profile.playerName = playerName;
            profile.languageCode = GameConfig.Languages[languageIndex].Code;
            profile.Save();

            var pack = LanguageDatabase.Load(profile.languageCode);
            status.text = "Saved! Welcome, " + playerName + ".\n" + pack.words.Count + " words loaded.";
            Debug.Log("Profile saved. TODO: load the first village scene here (Docs/05_TODO.md, phase 2).");
        }

        void Update()
        {
            // drifting clouds
            foreach (var cloud in clouds)
            {
                var pos = cloud.anchoredPosition;
                pos.x += cloud.sizeDelta.x * 0.04f * Time.deltaTime;
                if (pos.x > 420) pos.x = -420;
                cloud.anchoredPosition = pos;
            }

            // tagline fades between English and each supported language
            const float period = 3f, fade = 0.4f;
            taglineTimer += Time.deltaTime;
            if (taglineTimer >= period)
            {
                taglineTimer = 0;
                taglineIndex = (taglineIndex + 1) % taglines.Count;
                tagline.text = taglines[taglineIndex];
            }
            float alpha = Mathf.Clamp01(Mathf.Min(taglineTimer, period - taglineTimer) / fade);
            var color = tagline.color;
            color.a = alpha;
            tagline.color = color;
        }
    }
}
