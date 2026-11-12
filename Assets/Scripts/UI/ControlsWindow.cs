using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>The page button next to the minimap: every control and what it does.</summary>
    public class ControlsWindow : GameWindow
    {
        Transform rows;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(420, 330), "Controles", "Controls");
            rows = UIFactory.Rect("Rows", W, Vector2.zero, Vector2.zero);
            Keybinds.Changed += Rebuild;
        }

        void OnDestroy() => Keybinds.Changed -= Rebuild;

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Rebuild();
        }

        void Rebuild()
        {
            if (rows == null) return;
            foreach (Transform child in rows) Destroy(child.gameObject);
            string move = Keybinds.KeyName(GameAction.Up) + Keybinds.KeyName(GameAction.Left) + Keybinds.KeyName(GameAction.Down) + Keybinds.KeyName(GameAction.Right);
            var lines = new (string key, string es, string en)[]
            {
                (move.Length <= 4 ? move : "Flechas", "Caminar", "Walk"),
                (Keybinds.KeyName(GameAction.Run), "Correr (mantener)", "Run (hold)"),
                (Keybinds.KeyName(GameAction.Crouch), "Agacharse (mantener)", "Crouch (hold)"),
                (Keybinds.KeyName(GameAction.Jump), "Saltar", "Jump"),
                (Keybinds.KeyName(GameAction.Use), "Usar / hablar / entrar", "Use / talk / enter"),
                (Keybinds.KeyName(GameAction.Backpack), "Mochila", "Bag"),
                (Keybinds.KeyName(GameAction.Profile), "Perfil y habilidades", "Profile and skills"),
                (Keybinds.KeyName(GameAction.Lessons), "Lecciones (tema y nivel)", "Lessons (topic and level)"),
                (Keybinds.KeyName(GameAction.Map), "Mapa grande (o clic en el minimapa)", "Big map (or click the minimap)"),
                ("Mantén clic izq.", "Leer el nombre de algo", "Hold left click: read what something is called"),
                ("Clic der.", "Atacar animales / cortar árboles", "Attack animals / chop trees"),
                ("Rueda", "Acercar y alejar la cámara", "Zoom the camera in and out"),
                ("Esc", "Menú (opciones, salir)", "Menu (options, exit)"),
            };
            for (int i = 0; i < lines.Length; i++)
            {
                float y = 106 - i * 19;
                var key = UIFactory.Box("Key", rows, new Vector2(-130, y), new Vector2(96, 20), UIFactory.Teal);
                var label = UIFactory.Label("KeyName", key.transform, new Vector2(0, 1), new Vector2(96, 20), lines[i].key, Color.white);
                UIFactory.Outline(label);
                UIFactory.LLabel("What", rows, new Vector2(46, y), new Vector2(240, 20), lines[i].es, lines[i].en, UIFactory.Ink, TextAnchor.MiddleLeft);
            }
            UIFactory.LLabel("Tip", rows, new Vector2(0, -144), new Vector2(380, 20),
                "Cambia las teclas en Esc > Opciones.", "Change keys in Esc > Options.", UIFactory.Faded);
            translate.Set(translate.English);
        }
    }
}
