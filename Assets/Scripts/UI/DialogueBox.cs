using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>One thing somebody says: in the language being learned, with its English meaning.</summary>
    public class Line
    {
        public readonly string Speaker, Text, English;

        public Line(string speaker, string text, string english)
        {
            Speaker = speaker;
            Text = text;
            English = english;
        }
    }

    /// <summary>
    /// The talking box at the bottom of the screen. Lines are shown in Spanish; the globe button
    /// shows the English underneath. The use key, Space, Enter or a click moves to the next line.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        GameObject box;
        Text speaker, text, nextHint;
        GameObject englishListen, namePlate;
        LocalizedText english;
        TranslateToggle translate;
        List<Line> lines;
        int index, openedFrame;
        Action onDone;
        VillageController village;

        public bool Open => box != null && box.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            // layout: words on the left, a strip of buttons (translate, listen) down the right-hand side
            box = UIFactory.Panel("Dialogue", canvas, new Vector2(0, -100), new Vector2(600, 116)).gameObject;
            var b = box.transform;
            UIFactory.Img("Divider", b, new Vector2(240, 0), new Vector2(2, 80), new Color32(210, 186, 140, 255));
            UIFactory.TranslateButton(b, new Vector2(264, 26));
            translate = box.GetComponent<TranslateToggle>();
            UIFactory.ListenButton(b, new Vector2(264, 0), () => lines[index].Text, "es");
            englishListen = UIFactory.ListenButton(b, new Vector2(264, -26), () => lines[index].English, "en").gameObject;

            namePlate = UIFactory.Box("NamePlate", b, new Vector2(-196, 30), new Vector2(168, 20), new Color32(70, 52, 46, 255)).gameObject;
            speaker = UIFactory.Label("Speaker", namePlate.transform, new Vector2(0, 1), new Vector2(156, 16), "", Color.white);
            text = UIFactory.Label("Text", b, new Vector2(-28, 0), new Vector2(500, 34), "", UIFactory.Ink, TextAnchor.UpperLeft);
            UIFactory.Img("Rule", b, new Vector2(-28, -18), new Vector2(500, 1), new Color32(220, 200, 160, 255));
            english = UIFactory.LLabel("English", b, new Vector2(-58, -30), new Vector2(440, 18), "", "", UIFactory.Faded,
                TextAnchor.MiddleLeft, 16, false, true).GetComponent<LocalizedText>();
            nextHint = UIFactory.Label("Next", b, new Vector2(198, -30), new Vector2(54, 16), "", UIFactory.Teal, TextAnchor.MiddleRight);
            box.SetActive(false);
        }

        public void Show(List<Line> newLines, Action whenDone = null)
        {
            lines = newLines;
            onDone = whenDone;
            index = 0;
            openedFrame = Time.frameCount;
            box.SetActive(true);
            translate.Set(false);
            Refresh();
        }

        public void Show(string speakerName, string spanish, string englishText, Action whenDone = null)
        {
            Show(new List<Line> { new Line(speakerName, spanish, englishText) }, whenDone);
        }

        void Refresh()
        {
            nextHint.text = Keybinds.KeyName(GameAction.Use) + " ▶";
            speaker.text = lines[index].Speaker;
            // without a speaker name the line moves up into the name's place
            bool named = !string.IsNullOrEmpty(lines[index].Speaker);
            namePlate.SetActive(named);
            var rt = (RectTransform)text.transform;
            rt.anchoredPosition = new Vector2(-28, named ? 0 : 14);
            rt.sizeDelta = new Vector2(500, named ? 30 : 40);
            text.text = lines[index].Text;
            english.Set("", lines[index].English, true);
        }

        void Update()
        {
            if (!Open) return;
            englishListen.SetActive(translate.English);
            if (Time.frameCount == openedFrame) return;
            bool click = Input.GetMouseButtonDown(0) && !OverTranslateButton();
            if (!Keybinds.Down(GameAction.Use) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return) && !click) return;
            Advance();
        }

        /// <summary>Clicking the globe should translate, not skip to the next line.</summary>
        bool OverTranslateButton()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return false;
            var data = new UnityEngine.EventSystems.PointerEventData(es) { position = Input.mousePosition };
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            es.RaycastAll(data, hits);
            foreach (var h in hits)
                if (h.gameObject.GetComponentInParent<Button>() != null && h.gameObject.transform.IsChildOf(box.transform)
                    && h.gameObject.GetComponentInParent<Button>().name != "Dialogue") return true;
            return false;
        }

        /// <summary>Moves to the next line, or closes the box after the last one.</summary>
        public void Advance()
        {
            if (!Open) return;
            index++;
            if (index < lines.Count)
            {
                Refresh();
                return;
            }
            box.SetActive(false);
            village.NoteWindowClosed();
            var done = onDone;
            onDone = null;
            done?.Invoke();
        }
    }
}
