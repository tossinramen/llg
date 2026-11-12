using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Options: change any key (click it, then press the new key) and set the music volume.
    /// Used from the Esc menu in the village and from the start screen.
    /// </summary>
    public class OptionsWindow : MonoBehaviour
    {
        static readonly KeyCode[] AllKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));

        GameObject window;
        readonly List<Text> keyLabels = new List<Text>();
        readonly List<Image> keyButtons = new List<Image>();
        Text hint, volumeText;
        LocalizedText hintText;
        Image speaker;
        Slider volume;
        int waitingFor = -1;          // which action is waiting for a new key, or -1
        int openedFrame;
        Action onClose;

        public bool Open => window != null && window.activeSelf;

        /// <summary>The frame the window was closed on, so the same Esc press doesn't also close the menu behind it.</summary>
        public int ClosedFrame { get; private set; } = -1;

        public void Build(Transform canvas)
        {
            window = UIFactory.Panel("Options", canvas, new Vector2(0, -6), new Vector2(580, 324)).gameObject;
            var w = window.transform;
            UIFactory.TranslateButton(w, new Vector2(-260, 134));
            UIFactory.LLabel("Title", w, new Vector2(0, 134), new Vector2(400, 28), "Opciones", "Options", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);

            // ---- controls (left column)
            UIFactory.LLabel("ControlsTitle", w, new Vector2(-130, 104), new Vector2(260, 20), "Controles", "Controls", UIFactory.Ink);
            for (int i = 0; i < Keybinds.All.Length; i++)
            {
                int index = i;
                float y = 82 - i * 21;
                UIFactory.LLabel("Action", w, new Vector2(-180, y), new Vector2(170, 20), Keybinds.Name(Keybinds.All[i]), Keybinds.EnglishName(Keybinds.All[i]), UIFactory.Ink, TextAnchor.MiddleLeft);
                var button = UIFactory.Btn("Key", w, new Vector2(-40, y), new Vector2(96, 20), "", UIFactory.Teal, () => StartRebind(index));
                keyButtons.Add(button.GetComponent<Image>());
                var label = UIFactory.Label("KeyName", button.transform, new Vector2(0, 1), new Vector2(96, 20), "", Color.white);
                UIFactory.Outline(label);
                keyLabels.Add(label);
            }
            UIFactory.LLabel("Fixed", w, new Vector2(0, -140), new Vector2(540, 20),
                "Clic izq.: leer   Clic der.: atacar   Esc: menú", "Left click: read   Right click: attack   Esc: menu", UIFactory.Faded);

            // ---- audio (right column)
            UIFactory.LLabel("AudioTitle", w, new Vector2(150, 104), new Vector2(240, 20), "Música", "Music", UIFactory.Ink);
            var speakerButton = UIFactory.Btn("Mute", w, new Vector2(54, 62), new Vector2(32, 26), "", UIFactory.Purple, ToggleMute);
            speaker = UIFactory.Img("Speaker", speakerButton.transform, Vector2.zero, new Vector2(16, 16), Color.white);
            volume = UIFactory.Slider("Volume", w, new Vector2(160, 62), new Vector2(156, 16), UIFactory.Gold);
            volume.onValueChanged.AddListener(v =>
            {
                AudioManager.Volume = v;
                if (v > 0 && AudioManager.Muted) AudioManager.Muted = false;
                RefreshAudio();
            });
            volumeText = UIFactory.Label("Percent", w, new Vector2(262, 62), new Vector2(50, 20), "", UIFactory.Ink);

            hint = UIFactory.LLabel("Hint", w, new Vector2(150, 0), new Vector2(250, 60), "", "", UIFactory.Ink);
            hint.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText = hint.GetComponent<LocalizedText>();

            UIFactory.LBtn("Reset", w, new Vector2(150, -76), new Vector2(200, 24), "Restablecer teclas", "Reset keys", UIFactory.Blue, () =>
            {
                waitingFor = -1;
                Keybinds.ResetAll();
                RefreshKeys();
            });
            UIFactory.LBtn("Back", w, new Vector2(150, -108), new Vector2(200, 24), "Volver", "Back", UIFactory.Red, Close);
            window.SetActive(false);
        }

        public void Show(Action whenClosed = null)
        {
            onClose = whenClosed;
            waitingFor = -1;
            openedFrame = Time.frameCount;
            window.GetComponent<TranslateToggle>().Set(false);
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            volume.SetValueWithoutNotify(AudioManager.Volume);
            RefreshKeys();
            RefreshAudio();
        }

        public void Close()
        {
            if (!Open) return;
            waitingFor = -1;
            window.SetActive(false);
            ClosedFrame = Time.frameCount;
            var done = onClose;
            onClose = null;
            done?.Invoke();
        }

        void StartRebind(int index)
        {
            waitingFor = index;
            RefreshKeys();
        }

        void RefreshKeys()
        {
            for (int i = 0; i < keyLabels.Count; i++)
            {
                bool waiting = i == waitingFor;
                keyLabels[i].text = waiting ? "..." : Keybinds.KeyName(Keybinds.All[i]);
                keyButtons[i].color = waiting ? UIFactory.Gold : UIFactory.Teal;
            }
            hintText.Set(
                waitingFor >= 0 ? "Pulsa una tecla para: " + Keybinds.Name(Keybinds.All[waitingFor]) + "\n(Esc cancela)" : "Haz clic en una tecla para cambiarla.",
                waitingFor >= 0 ? "Press a key for: " + Keybinds.EnglishName(Keybinds.All[waitingFor]) + "\n(Esc cancels)" : "Click a key to change it.");
        }

        void ToggleMute()
        {
            AudioManager.Muted = !AudioManager.Muted;
            RefreshAudio();
        }

        void RefreshAudio()
        {
            bool silent = AudioManager.Muted || AudioManager.Volume <= 0.001f;
            speaker.sprite = UIFactory.LoadSprite(silent ? "Art/UI/speaker_off" : "Art/UI/speaker_on");
            volumeText.text = AudioManager.Muted ? "Off" : Mathf.RoundToInt(AudioManager.Volume * 100) + "%";
        }

        void Update()
        {
            if (!Open || Time.frameCount == openedFrame) return;

            if (waitingFor >= 0)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    waitingFor = -1;
                    RefreshKeys();
                    return;
                }
                foreach (var key in AllKeys)
                {
                    if (key == KeyCode.None || key == KeyCode.Escape || key >= KeyCode.Mouse0) continue;   // mouse buttons and joystick keys are not allowed
                    if (!Input.GetKeyDown(key)) continue;
                    Keybinds.Set(Keybinds.All[waitingFor], key);
                    waitingFor = -1;
                    RefreshKeys();
                    return;
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }
    }
}
