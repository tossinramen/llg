using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Press Enter to type, Enter again to say it: the words float above your head in a white speech
    /// bubble for a few seconds (like Roblox or WoW). Esc cancels. You can't walk while typing.
    /// </summary>
    public class ChatBox : MonoBehaviour
    {
        const float MaxWidth = 220;

        GameObject bar;
        InputField input;
        RectTransform bubble, canvasRect;
        Text bubbleText;
        CanvasGroup bubbleGroup;
        Transform speaker;
        VillageController village;
        Camera cam;
        float bubbleTimer;
        int openedFrame = -1;

        public bool Typing => bar != null && bar.activeSelf;

        public void Build(Transform canvas, VillageController owner, Transform player)
        {
            village = owner;
            speaker = player;
            canvasRect = (RectTransform)canvas;
            cam = Camera.main;

            // the speech bubble (behind every window, above the world)
            var b = UIFactory.Box("SpeechBubble", canvas, Vector2.zero, new Vector2(80, 26), Color.white);
            b.raycastTarget = false;
            var edge = b.gameObject.AddComponent<Outline>();
            edge.effectColor = new Color32(40, 30, 50, 255);
            edge.effectDistance = new Vector2(1, -1);
            bubble = b.rectTransform;
            var tail = UIFactory.Img("Tail", bubble, Vector2.zero, new Vector2(8, 8), Color.white);
            tail.rectTransform.anchorMin = tail.rectTransform.anchorMax = new Vector2(0.5f, 0);
            tail.rectTransform.anchoredPosition = new Vector2(0, -1);
            tail.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            tail.transform.SetAsFirstSibling();
            var tailEdge = tail.gameObject.AddComponent<Outline>();
            tailEdge.effectColor = new Color32(40, 30, 50, 255);
            tailEdge.effectDistance = new Vector2(1, -1);
            bubbleText = UIFactory.Label("Text", bubble, new Vector2(0, 1), new Vector2(60, 18), "", UIFactory.Ink);
            bubbleGroup = b.gameObject.AddComponent<CanvasGroup>();
            bubbleGroup.blocksRaycasts = false;
            bubble.gameObject.SetActive(false);

            // the typing bar along the bottom
            var panel = UIFactory.Panel("ChatBar", canvas, new Vector2(0, -112), new Vector2(440, 34));
            bar = panel.gameObject;
            UIFactory.LLabel("Say", bar.transform, new Vector2(-190, 1), new Vector2(40, 18), "Di:", "Say:", UIFactory.Purple);
            input = UIFactory.Input("Input", bar.transform, new Vector2(24, 0), new Vector2(380, 24), "Escribe algo y pulsa Enter...  (Esc: cancelar)");
            input.characterLimit = 80;
            input.lineType = InputField.LineType.SingleLine;
            bar.SetActive(false);
        }

        public void Open()
        {
            bar.SetActive(true);
            bar.transform.SetAsLastSibling();
            input.text = "";
            input.ActivateInputField();
            input.Select();
            openedFrame = Time.frameCount;
        }

        void CloseBar()
        {
            bar.SetActive(false);
            village.NoteWindowClosed();
        }

        /// <summary>Shows words in the bubble above your head.</summary>
        public void Say(string text)
        {
            text = text.Trim();
            if (text.Length == 0) return;
            float textWidth = Mathf.Min(MaxWidth, text.Length * 8 + 4);
            int lines = Mathf.CeilToInt(text.Length * 8 / MaxWidth);
            if (lines < 1) lines = 1;
            bubble.sizeDelta = new Vector2(textWidth + 18, lines * 17 + 10);
            bubbleText.rectTransform.sizeDelta = new Vector2(textWidth + 4, lines * 17);
            bubbleText.text = text;
            bubbleTimer = 3.5f + text.Length * 0.07f;
            bubble.gameObject.SetActive(true);
            Place();
        }

        void Place()
        {
            var screen = cam.WorldToScreenPoint(speaker.position + new Vector3(0, 2.25f, 0));
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out var local)) return;
            bubble.anchoredPosition = local + new Vector2(0, bubble.sizeDelta.y / 2);
        }

        void Update()
        {
            if (Typing && Time.frameCount != openedFrame)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) CloseBar();
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Say(input.text);
                    CloseBar();
                }
                else if (!input.isFocused) input.ActivateInputField();
            }
        }

        void LateUpdate()      // after the camera has followed you, so the bubble doesn't lag
        {
            if (!bubble.gameObject.activeSelf) return;
            bubbleTimer -= Time.unscaledDeltaTime;
            bubbleGroup.alpha = Mathf.Clamp01(bubbleTimer / 0.5f);
            if (bubbleTimer <= 0) bubble.gameObject.SetActive(false);
            else Place();
        }
    }
}
