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
        GameObject englishListen;
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
            box = UIFactory.Panel("Dialogue", canvas, new Vector2(0, -102), new Vector2(600, 112)).gameObject;
            var b = box.transform;
            UIFactory.TranslateButton(b, new Vector2(-270, 34));
            translate = box.GetComponent<TranslateToggle>();
            speaker = UIFactory.Label("Speaker", b, new Vector2(20, 34), new Vector2(520, 20), "", UIFactory.Purple, TextAnchor.MiddleLeft);
            text = UIFactory.Label("Text", b, new Vector2(-10, 6), new Vector2(540, 40), "", UIFactory.Ink, TextAnchor.UpperLeft);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            english = UIFactory.LLabel("English", b, new Vector2(0, -28), new Vector2(560, 22), "", "", UIFactory.Faded,
                TextAnchor.MiddleLeft, 16, false, true).GetComponent<LocalizedText>();
            nextHint = UIFactory.Label("Next", b, new Vector2(214, -42), new Vector2(80, 20), "", UIFactory.Teal, TextAnchor.MiddleRight);
            // speaker buttons: hear the Spanish line, and (once translated) the English one
            UIFactory.ListenButton(b, new Vector2(266, 10), () => lines[index].Text, "es");
            englishListen = UIFactory.ListenButton(b, new Vector2(266, -24), () => lines[index].English, "en").gameObject;
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
            nextHint.text = "[" + Keybinds.KeyName(GameAction.Use) + "]";
            speaker.text = lines[index].Speaker;
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
