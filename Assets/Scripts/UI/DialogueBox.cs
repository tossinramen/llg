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

    /// <summary>The talking box at the bottom of the screen. E / Space / click moves to the next line.</summary>
    public class DialogueBox : MonoBehaviour
    {
        GameObject box;
        Text speaker, text, english, nextHint;
        List<Line> lines;
        int index, openedFrame;
        Action onDone;
        VillageController village;

        public bool Open => box != null && box.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            var panel = UIFactory.Panel("Dialogue", canvas, new Vector2(0, -112), new Vector2(600, 112));
            box = panel.gameObject;
            speaker = UIFactory.Label("Speaker", box.transform, new Vector2(0, 36), new Vector2(560, 20), "", UIFactory.Purple, TextAnchor.MiddleLeft);
            text = UIFactory.Label("Text", box.transform, new Vector2(0, 8), new Vector2(560, 40), "", UIFactory.Ink, TextAnchor.UpperLeft);
            english = UIFactory.Label("English", box.transform, new Vector2(0, -28), new Vector2(560, 22), "", UIFactory.Faded, TextAnchor.MiddleLeft);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            nextHint = UIFactory.Label("Next", box.transform, new Vector2(256, -40), new Vector2(60, 20), "", UIFactory.Teal, TextAnchor.MiddleRight);
            box.SetActive(false);
        }

        public void Show(List<Line> newLines, Action whenDone = null)
        {
            lines = newLines;
            onDone = whenDone;
            index = 0;
            openedFrame = Time.frameCount;
            box.SetActive(true);
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
            english.text = lines[index].English;
        }

        void Update()
        {
            if (!Open || Time.frameCount == openedFrame) return;
            if (!Keybinds.Down(GameAction.Use) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return)
                && !Input.GetMouseButtonDown(0)) return;

            Advance();
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
