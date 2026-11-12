using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Shared behaviour for the in-game windows: a parchment panel with a title, a translate (globe)
    /// button in the top-left, a close button in the top-right, and closing with Esc.
    /// </summary>
    public abstract class GameWindow : MonoBehaviour
    {
        protected GameObject window;
        protected VillageController village;
        protected TranslateToggle translate;
        int openedFrame;

        protected Transform W => window.transform;
        public bool Open => window != null && window.activeSelf;

        protected void CreateWindow(Transform canvas, VillageController owner, Vector2 pos, Vector2 size, string spanish, string english)
        {
            village = owner;
            window = UIFactory.Panel(GetType().Name, canvas, pos, size).gameObject;
            UIFactory.TranslateButton(W, new Vector2(-size.x / 2 + 30, size.y / 2 - 24));
            translate = window.GetComponent<TranslateToggle>();
            UIFactory.LLabel("Title", W, new Vector2(0, size.y / 2 - 24), new Vector2(size.x - 80, 28), spanish, english,
                UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            UIFactory.Btn("Close", W, new Vector2(size.x / 2 - 34, size.y / 2 - 24), new Vector2(24, 22), "x", UIFactory.Red, Close);
            window.SetActive(false);
        }

        /// <summary>Shows the window in Spanish.</summary>
        protected void OpenWindow()
        {
            openedFrame = Time.frameCount;
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            translate.Set(false);
        }

        public void Close()
        {
            if (!Open) return;
            window.SetActive(false);
            if (village != null) village.NoteWindowClosed();
            OnClosed();
        }

        protected virtual void OnClosed() { }

        /// <summary>The key that opens this window also closes it (B for the bag, U for the profile).</summary>
        protected virtual bool ToggleKeyPressed => false;

        protected virtual void Update()
        {
            if (!Open || Time.frameCount == openedFrame) return;
            if (Input.GetKeyDown(KeyCode.Escape) || ToggleKeyPressed) Close();
        }
    }

    /// <summary>Tells a window when the mouse moves over or off a slot (for tooltips).</summary>
    public class HoverHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action Entered, Exited;
        public void OnPointerEnter(PointerEventData eventData) => Entered?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => Exited?.Invoke();
    }

    /// <summary>An item slot: dark frame, icon, stack count. Used by the bag and the character sheet.</summary>
    public class ItemSlotView
    {
        public Button button;
        public Image icon;
        public Text count;
        public HoverHandle hover;

        public static ItemSlotView Create(Transform parent, Vector2 pos, float size, Action onClick)
        {
            var view = new ItemSlotView();
            var frame = UIFactory.Img("Slot", parent, pos, new Vector2(size, size), Color.white, UIFactory.LoadSprite("Art/UI/slot"));
            frame.raycastTarget = true;
            view.button = frame.gameObject.AddComponent<Button>();
            view.button.targetGraphic = frame;
            view.button.onClick.AddListener(() => onClick());
            view.icon = UIFactory.Img("Icon", frame.transform, new Vector2(0, 1), new Vector2(size - 14, size - 14), Color.white);
            view.count = UIFactory.Label("Count", frame.transform, new Vector2(size / 2 - 11, -size / 2 + 10), new Vector2(16, 12), "", Color.white, TextAnchor.LowerRight, 12);
            UIFactory.Outline(view.count);
            view.hover = frame.gameObject.AddComponent<HoverHandle>();
            return view;
        }

        public void Show(Sprite sprite, int amount, Color tint)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.color = tint;
            count.text = amount > 1 ? amount.ToString() : "";
        }
    }
}
