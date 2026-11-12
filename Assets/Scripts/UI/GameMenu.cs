using UnityEngine;

namespace LLG
{
    /// <summary>The Esc menu in the village: resume, options, or save and go back to the title screen.
    /// The game is paused while it is open.</summary>
    public class GameMenu : MonoBehaviour
    {
        GameObject window;
        OptionsWindow options;
        VillageController village;
        int openedFrame;

        public bool Open => (window != null && window.activeSelf) || (options != null && options.Open);

        public void Build(Transform canvas, VillageController owner, OptionsWindow optionsWindow)
        {
            village = owner;
            options = optionsWindow;
            window = UIFactory.Panel("GameMenu", canvas, new Vector2(0, 0), new Vector2(300, 220)).gameObject;
            var w = window.transform;
            UIFactory.Label("Title", w, new Vector2(0, 76), new Vector2(260, 20), "MENÚ", UIFactory.Purple, TextAnchor.MiddleCenter, 16, true);
            UIFactory.Btn("Resume", w, new Vector2(0, 34), new Vector2(240, 28), "Continuar (Resume)", UIFactory.Green, Close);
            UIFactory.Btn("Options", w, new Vector2(0, -2), new Vector2(240, 28), "Opciones (Options)", UIFactory.Blue, ShowOptions);
            UIFactory.Btn("Exit", w, new Vector2(0, -38), new Vector2(240, 28), "Salir del juego (Exit Game)", UIFactory.Red, () => village.ExitToTitle());
            UIFactory.Label("Saved", w, new Vector2(0, -74), new Vector2(260, 20), "Tu progreso se guarda al salir.", UIFactory.Faded);
            window.SetActive(false);
        }

        public void Show()
        {
            openedFrame = Time.frameCount;
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            Time.timeScale = 0;          // pause hunger, timers and animations
        }

        public void Close()
        {
            window.SetActive(false);
            Time.timeScale = 1;
            village.NoteWindowClosed();
        }

        void ShowOptions()
        {
            window.SetActive(false);
            options.Show(() => window.SetActive(true));
        }

        void Update()
        {
            if (window == null || !window.activeSelf || Time.frameCount == openedFrame) return;
            if (options.Open || options.ClosedFrame == Time.frameCount) return;
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        void OnDestroy() => Time.timeScale = 1;
    }
}
