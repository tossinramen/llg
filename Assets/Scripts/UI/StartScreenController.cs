using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// The title screen + character creator. It builds its whole UI from code when you press Play,
    /// so the scene looks empty in the editor until then - that is expected.
    /// Pressing Start saves the profile and opens the village.
    /// </summary>
    public class StartScreenController : MonoBehaviour
    {
        const float LabelX = -18, ValueX = 190;

        PlayerProfile profile;
        OptionsWindow options;
        CharacterPreview preview;
        Transform root;
        InputField nameInput;
        Text tagline, status;
        Button startButton;
        int languageIndex, taglineIndex;
        float taglineTimer, dragDistance;
        readonly List<Image> languageCards = new List<Image>();
        readonly List<Action> refreshers = new List<Action>();
        readonly List<RectTransform> clouds = new List<RectTransform>();
        readonly List<Image> sparkles = new List<Image>();
        readonly List<Image> genderButtons = new List<Image>();
        readonly List<string> taglines = new List<string>();

        void Awake()
        {
            profile = PlayerProfile.Load();
            languageIndex = Mathf.Max(0, Array.FindIndex(GameConfig.Languages, l => l.Code == profile.languageCode && l.Available));

            taglines.Add(GameConfig.EnglishTagline);
            foreach (var language in GameConfig.Languages) taglines.Add(language.Tagline);

            root = UIFactory.Canvas("StartScreenCanvas", 0);
            BuildBackground();
            BuildTitle();
            BuildCharacterPanel();
            BuildOptionsPanel();

            UIFactory.Btn("OptionsButton", root, new Vector2(272, -164), new Vector2(88, 22), "Opciones", UIFactory.Purple, () => options.Show());
            options = gameObject.AddComponent<OptionsWindow>();
            options.Build(root);
            RefreshAll();
        }

        // ------------------------------------------------------------ building

        void BuildBackground()
        {
            var bg = UIFactory.Img("Background", root, Vector2.zero, new Vector2(640, 360), Color.white,
                UIFactory.LoadSprite("Art/UI/title_background"));
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            var cloud = UIFactory.LoadSprite("Art/UI/cloud");
            var spots = new[] { new Vector3(-250, 96, 2f), new Vector3(-40, 172, 1.5f), new Vector3(190, 100, 2f) };
            foreach (var spot in spots)
            {
                var img = UIFactory.Img("Cloud", root, spot, new Vector2(48, 16) * spot.z, new Color(1, 1, 1, 0.95f), cloud);
                clouds.Add(img.rectTransform);
            }
        }

        void BuildTitle()
        {
            // twinkling runes and sparkles behind the title
            string[] shapes = { "rune0", "rune1", "rune2", "sparkle", "sparkle" };
            Color[] tints = { UIFactory.Gold, UIFactory.Pink, UIFactory.Teal, Color.white, UIFactory.Purple, UIFactory.Blue };
            var random = new System.Random(5);
            for (int i = 0; i < 18; i++)
            {
                var sprite = UIFactory.LoadSprite("Art/UI/" + shapes[i % shapes.Length]);
                if (sprite == null) continue;
                var pos = new Vector2(random.Next(-300, 300), random.Next(104, 174));
                var size = new Vector2(sprite.rect.width, sprite.rect.height) * 2;
                sparkles.Add(UIFactory.Img("Sparkle", root, pos, size, tints[i % tints.Length], sprite));
            }

            var title = UIFactory.Label("Title", root, new Vector2(0, 146), new Vector2(620, 56), GameConfig.Title,
                UIFactory.Gold, TextAnchor.MiddleCenter, 48, true);
            UIFactory.Outline(title, 2);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color32(150, 60, 140, 255);
            shadow.effectDistance = new Vector2(0, -3);

            tagline = UIFactory.Label("Tagline", root, new Vector2(0, 114), new Vector2(620, 20), taglines[0], Color.white);
            UIFactory.Outline(tagline);
        }

        void BuildCharacterPanel()
        {
            UIFactory.Panel("CharacterPanel", root, new Vector2(-205, -12), new Vector2(196, 226));
            UIFactory.Box("Stage", root, new Vector2(-205, 18), new Vector2(150, 150), new Color32(170, 226, 250, 255));

            var holder = UIFactory.Rect("CharacterPreview", root, new Vector2(-205, 18), new Vector2(108, 144));
            preview = holder.gameObject.AddComponent<CharacterPreview>();
            preview.Build(new Vector2(108, 144));

            // hold the left mouse button on the character and drag sideways to turn them
            var stage = UIFactory.Rect("DragArea", root, new Vector2(-205, 18), new Vector2(150, 150));
            var catcher = stage.gameObject.AddComponent<Image>();
            catcher.color = Color.clear;
            stage.gameObject.AddComponent<DragHandle>().Dragged += DragCharacter;
            UIFactory.Label("TurnHint", root, new Vector2(-205, -70), new Vector2(180, 20), "Drag to turn", UIFactory.Faded);

            UIFactory.Btn("Randomize", root, new Vector2(-205, -100), new Vector2(172, 24), "Randomize", UIFactory.Pink, Randomize);

            startButton = UIFactory.Btn("StartButton", root, new Vector2(-205, -152), new Vector2(196, 34), "Start Adventure", UIFactory.Green, StartGame);
        }

        void BuildOptionsPanel()
        {
            UIFactory.Panel("OptionsPanel", root, new Vector2(110, -22), new Vector2(404, 250));
            var look = profile.appearance;

            RowLabel("Name", 76);
            nameInput = UIFactory.Input("NameInput", root, new Vector2(ValueX, 76), new Vector2(180, 24), "Your name");
            nameInput.characterLimit = 12;
            nameInput.text = profile.playerName;

            RowLabel("Gender", 48);
            string[] icons = { "gender_male", "gender_female", "gender_nonbinary" };
            for (int i = 0; i < icons.Length; i++)
            {
                var gender = (Gender)i;
                var button = UIFactory.Btn("Gender" + gender, root, new Vector2(ValueX - 44 + i * 44, 48), new Vector2(34, 26), "",
                    UIFactory.Blue, () => { profile.gender = gender; RefreshAll(); });
                UIFactory.Img("Icon", button.transform, Vector2.zero, new Vector2(16, 16), Color.white, UIFactory.LoadSprite("Art/UI/" + icons[i]));
                genderButtons.Add(button.GetComponent<Image>());
            }

            ColorRow("Skin", 20, AppearancePalette.Skin, () => look.skin, v => look.skin = v);
            CycleRow("Hair style", -8, AppearancePalette.HairStyles.Length, () => look.hairStyle, v => look.hairStyle = v,
                (swatch, text) => { swatch.color = UIFactory.Purple; text.text = AppearancePalette.HairStyles[look.hairStyle]; });
            ColorRow("Hair color", -36, AppearancePalette.Hair, () => look.hairColor, v => look.hairColor = v);
            ColorRow("Shirt", -64, AppearancePalette.Shirt, () => look.shirt, v => look.shirt = v);
            ColorRow("Pants", -92, AppearancePalette.Pants, () => look.pants, v => look.pants = v);
            // one card per language; locked ones show a padlock and "coming soon"
            RowLabel("Language", -120);
            var lockSprite = UIFactory.LoadSprite("Art/UI/lock");
            for (int i = 0; i < GameConfig.Languages.Length; i++)
            {
                int index = i;
                var language = GameConfig.Languages[i];
                var card = UIFactory.Btn("Language" + language.Code, root, new Vector2(114 + i * 76, -120), new Vector2(72, 24),
                    language.NativeName, UIFactory.Teal, () => PickLanguage(index));
                if (!language.Available)
                    UIFactory.Img("Lock", card.transform, new Vector2(28, 10), new Vector2(16, 16), Color.white, lockSprite);
                languageCards.Add(card.GetComponent<Image>());
            }

            status = UIFactory.Label("Status", root, new Vector2(110, -162), new Vector2(404, 20), "", Color.white);
            UIFactory.Outline(status);
        }

        void RowLabel(string text, float y)
        {
            UIFactory.Label(text + "Label", root, new Vector2(LabelX, y), new Vector2(110, 22), text, UIFactory.Ink, TextAnchor.MiddleLeft);
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
            var swatch = UIFactory.Box(label + "Value", root, new Vector2(ValueX, y), new Vector2(120, 24), Color.white);
            var text = UIFactory.Label("Text", swatch.transform, new Vector2(0, 1), new Vector2(120, 24), "", Color.white);
            UIFactory.Outline(text);
            Action<int> step = delta => { set(((get() + delta) % count + count) % count); RefreshAll(); };
            UIFactory.Btn(label + "Prev", root, new Vector2(ValueX - 78, y), new Vector2(26, 24), "<", UIFactory.Teal, () => step(-1));
            UIFactory.Btn(label + "Next", root, new Vector2(ValueX + 78, y), new Vector2(26, 24), ">", UIFactory.Teal, () => step(1));
            refreshers.Add(() => show(swatch, text));
        }

        // ------------------------------------------------------------ behaviour

        void RefreshAll()
        {
            foreach (var refresh in refreshers) refresh();
            for (int i = 0; i < genderButtons.Count; i++)
                genderButtons[i].color = (int)profile.gender == i ? UIFactory.Gold : UIFactory.Blue;
            preview.Show(profile.appearance);

            for (int i = 0; i < languageCards.Count; i++)
                languageCards[i].color = i == languageIndex ? UIFactory.Gold
                    : GameConfig.Languages[i].Available ? UIFactory.Teal : UIFactory.Faded;
            status.text = "";
        }

        void PickLanguage(int index)
        {
            var language = GameConfig.Languages[index];
            if (!language.Available)
            {
                status.text = language.EnglishName + " is coming soon!";
                return;
            }
            languageIndex = index;
            RefreshAll();
        }

        void DragCharacter(float pixels)
        {
            float step = Mathf.Max(24, Screen.width * 0.03f);    // how far to drag for one quarter turn
            dragDistance += pixels;
            while (dragDistance > step) { preview.Turn(1); dragDistance -= step; }
            while (dragDistance < -step) { preview.Turn(-1); dragDistance += step; }
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

        public void StartGame()
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

            // swap the start screen for the village
            Destroy(root.gameObject);
            Destroy(gameObject);
            new GameObject("Village").AddComponent<VillageController>();
        }

        void Update()
        {
            if (tagline == null || taglines.Count == 0) return;    // scripts were reloaded mid-play; press Stop and Play again

            if (Input.GetKeyDown(KeyCode.Escape) && !options.Open && options.ClosedFrame != Time.frameCount) options.Show();

            foreach (var cloud in clouds)
            {
                var pos = cloud.anchoredPosition;
                pos.x += cloud.sizeDelta.x * 0.04f * Time.deltaTime;
                if (pos.x > 420) pos.x = -420;
                cloud.anchoredPosition = pos;
            }

            for (int i = 0; i < sparkles.Count; i++)
            {
                var color = sparkles[i].color;
                color.a = 0.25f + 0.75f * Mathf.Abs(Mathf.Sin(Time.time * (0.8f + i % 4 * 0.3f) + i));
                sparkles[i].color = color;
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
            var tint = tagline.color;
            tint.a = Mathf.Clamp01(Mathf.Min(taglineTimer, period - taglineTimer) / fade);
            tagline.color = tint;
        }
    }
}
