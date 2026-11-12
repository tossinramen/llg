using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>The bosses of La Ciudadela del Sol: one floor every 5 levels, each testing one topic.</summary>
    public static class DungeonData
    {
        public enum Test { Words, FoodAnimals, SerEstar, Replies, Past, Future, Order, Translate, Everything }

        public class Floor
        {
            public int number, level, hp, damage;
            public string es, en, sprite, testEs;
            public Test test;
        }

        public static readonly Floor[] Floors =
        {
            F(1, 5, "El Guardián de Piedra", "Stone Guardian", "guardian", Test.Words, "Palabras básicas"),
            F(2, 10, "La Reina Araña", "Spider Queen", "spider", Test.FoodAnimals, "Comida y animales"),
            F(3, 15, "El Rey Trol", "Troll King", "troll", Test.SerEstar, "Ser o estar"),
            F(4, 20, "El Brujo de la Niebla", "Fog Sorcerer", "sorcerer", Test.Replies, "Preguntas y respuestas"),
            F(5, 25, "El Rey de Hielo", "Ice King", "iceking", Test.Past, "El pasado"),
            F(6, 30, "La Serpiente Emplumada", "Feathered Serpent", "serpent", Test.Future, "El futuro"),
            F(7, 35, "El Gigante de Lava", "Lava Giant", "lava", Test.Order, "Frases largas"),
            F(8, 40, "La Bruja del Espejo", "Mirror Witch", "witch", Test.Translate, "Traducción"),
            F(9, 45, "El Caballero Sin Nombre", "Nameless Knight", "knight", Test.Everything, "Todo"),
            F(10, 50, "El Rey Demonio", "Demon King", "demon", Test.Everything, "La prueba final"),
        };

        static Floor F(int n, int level, string es, string en, string sprite, Test test, string testEs) => new Floor
        {
            number = n, level = level, es = es, en = en, sprite = sprite, test = test, testEs = testEs,
            hp = 30 + n * 28, damage = 2 + n * 2,
        };

        // ---------------------------------------------------------------- question banks
        static readonly (string q, string a, string[] wrong)[] SerEstar =
        {
            ("Yo ___ estudiante.", "soy", new[] { "estoy", "es", "está" }),
            ("La sopa ___ caliente.", "está", new[] { "es", "son", "soy" }),
            ("Madrid ___ en España.", "está", new[] { "es", "son", "eres" }),
            ("Mi madre ___ profesora.", "es", new[] { "está", "están", "soy" }),
            ("Nosotros ___ cansados.", "estamos", new[] { "somos", "son", "está" }),
            ("Las flores ___ rojas.", "son", new[] { "están", "es", "somos" }),
            ("¿Dónde ___ el baño?", "está", new[] { "es", "son", "eres" }),
            ("Tú ___ muy simpático.", "eres", new[] { "estás", "es", "soy" }),
            ("Hoy ___ lunes.", "es", new[] { "está", "son", "estoy" }),
            ("El café ___ frío.", "está", new[] { "es", "son", "soy" }),
            ("Ellos ___ de México.", "son", new[] { "están", "es", "somos" }),
            ("Yo ___ feliz hoy.", "estoy", new[] { "soy", "es", "está" }),
        };
        static readonly (string q, string a)[] Replies =
        {
            ("¿Cómo te llamas?", "Me llamo Ana."), ("¿Cuántos años tienes?", "Tengo doce años."),
            ("¿De dónde eres?", "Soy de España."), ("¿Qué hora es?", "Son las tres."),
            ("¿Cómo estás?", "Estoy bien, gracias."), ("¿Qué quieres comer?", "Quiero una manzana."),
            ("¿Dónde vives?", "Vivo en Valdeluz."), ("¿Te gusta la música?", "Sí, me gusta mucho."),
            ("¿Tienes hermanos?", "Sí, tengo una hermana."), ("¿Qué tiempo hace?", "Hace sol."),
        };
        static readonly (string q, string a, string[] wrong)[] Past =
        {
            ("Ayer yo ___ pan.", "comí", new[] { "como", "comeré", "comes" }),
            ("Anoche ella ___ al cine.", "fue", new[] { "va", "irá", "vamos" }),
            ("El año pasado nosotros ___ en Madrid.", "vivimos", new[] { "vivo", "viviremos", "viven" }),
            ("Ayer tú ___ mucho.", "hablaste", new[] { "hablas", "hablarás", "habla" }),
            ("La semana pasada ellos ___ una casa.", "compraron", new[] { "compran", "comprarán", "compro" }),
            ("Ayer yo ___ agua.", "bebí", new[] { "bebo", "beberé", "bebe" }),
            ("El lunes pasado yo ___ la puerta.", "abrí", new[] { "abro", "abriré", "abre" }),
            ("Ayer el perro ___ en el jardín.", "durmió", new[] { "duerme", "dormirá", "duermo" }),
        };
        static readonly (string q, string a, string[] wrong)[] Future =
        {
            ("Mañana nosotros ___ al mercado.", "iremos", new[] { "fuimos", "vamos", "iban" }),
            ("El año que viene yo ___ español.", "hablaré", new[] { "hablé", "hablo", "hablaba" }),
            ("Mañana tú ___ pan.", "comerás", new[] { "comiste", "comes", "comías" }),
            ("La semana que viene ellos ___ una casa.", "comprarán", new[] { "compraron", "compran", "compro" }),
            ("Mañana ella ___ en la playa.", "estará", new[] { "estuvo", "está", "estaba" }),
            ("Pronto yo ___ un gato.", "tendré", new[] { "tuve", "tengo", "tenía" }),
            ("El sábado nosotros ___ en la fiesta.", "bailaremos", new[] { "bailamos", "bailan", "bailó" }),
            ("Mañana el sol ___.", "saldrá", new[] { "salió", "sale", "salía" }),
        };

        public class Question
        {
            public string prompt;
            public string[] options;
            public int answer;
        }

        static Question Make(string prompt, string right, IList<string> wrong)
        {
            var opts = new List<string> { right };
            foreach (var w in wrong) if (opts.Count < 4 && !opts.Contains(w)) opts.Add(w);
            for (int i = opts.Count - 1; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); (opts[i], opts[j]) = (opts[j], opts[i]); }
            return new Question { prompt = prompt, options = opts.ToArray(), answer = opts.IndexOf(right) };
        }

        static T Pick<T>(IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        /// <summary>A question for this floor. Battles never show translations or hints.</summary>
        public static Question Ask(Floor floor)
        {
            var test = floor.test;
            if (test == Test.Everything) test = (Test)UnityEngine.Random.Range(0, (int)Test.Everything);
            var pack = GameState.Pack;
            switch (test)
            {
                case Test.Words:
                case Test.FoodAnimals:
                {
                    var pool = pack.words.FindAll(w => w.tier <= 2 && (test == Test.Words || w.category == "food" || w.category == "animal"));
                    var right = Pick(pool);
                    var wrong = pool.FindAll(w => w != right).ConvertAll(w => w.word);
                    Shuffle(wrong);
                    return Make("\"" + right.english + "\"", right.word, wrong);
                }
                case Test.SerEstar: { var x = Pick(SerEstar); return Make(x.q, x.a, x.wrong); }
                case Test.Past: { var x = Pick(Past); return Make(x.q, x.a, x.wrong); }
                case Test.Future: { var x = Pick(Future); return Make(x.q, x.a, x.wrong); }
                case Test.Replies:
                {
                    var x = Pick(Replies);
                    var wrong = new List<string>();
                    foreach (var r in Replies) if (r.a != x.a) wrong.Add(r.a);
                    Shuffle(wrong);
                    return Make(x.q, x.a, wrong);
                }
                case Test.Order:
                {
                    var s = Pick(pack.sentences.FindAll(p => p.Tokens.Length >= 4));
                    var right = s.text;
                    var wrong = new List<string>();
                    for (int k = 0; k < 12 && wrong.Count < 3; k++)
                    {
                        var t = new List<string>(s.Tokens);
                        int a = UnityEngine.Random.Range(0, t.Count), b = UnityEngine.Random.Range(0, t.Count);
                        (t[a], t[b]) = (t[b], t[a]);
                        var joined = string.Join(" ", t);
                        if (joined != right && !wrong.Contains(joined)) wrong.Add(joined);
                    }
                    return Make("¿Cuál está bien?", right, wrong);
                }
                default:  // Translate: English sentence -> the Spanish one
                {
                    var s = Pick(pack.sentences);
                    var wrong = pack.sentences.FindAll(p => p != s).ConvertAll(p => p.text);
                    Shuffle(wrong);
                    return Make("\"" + s.english + "\"", s.text, wrong);
                }
            }
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); (list[i], list[j]) = (list[j], list[i]); }
        }
    }

    /// <summary>
    /// Inside La Ciudadela del Sol: the ten floors (terraces) you climb. Each floor has a boss, needs
    /// a character level, and opens once you beat the floor below it.
    /// </summary>
    public class DungeonWindow : GameWindow
    {
        Transform rows;
        BattleWindow battle;

        public void Build(Transform canvas, VillageController owner, BattleWindow battleWindow)
        {
            battle = battleWindow;
            CreateWindow(canvas, owner, new Vector2(0, -2), new Vector2(560, 344), "La Ciudadela del Sol", "The Citadel of the Sun");
            UIFactory.LLabel("Intro", W, new Vector2(0, 124), new Vector2(480, 18),
                "Sube los pisos y vence a los guardianes de las palabras.", "Climb the floors and defeat the guardians of words.", UIFactory.Faded);
            rows = UIFactory.Rect("Floors", W, Vector2.zero, Vector2.zero);
        }

        public void Toggle()
        {
            if (Open) { Close(); return; }
            OpenWindow();
            Rebuild();
        }

        void Rebuild()
        {
            foreach (Transform c in rows) Destroy(c.gameObject);
            int beaten = GameState.Data.dungeonFloor;
            int level = GameState.Data.level;
            for (int i = 0; i < DungeonData.Floors.Length; i++)
            {
                var f = DungeonData.Floors[i];
                // two columns of terraces, floor 1 at the bottom left like climbing up the mountain
                int col = i / 5, row = i % 5;
                var pos = new Vector2(-136 + col * 272, -122 + row * 50);
                bool done = f.number <= beaten;
                bool open = !done && f.number == beaten + 1 && level >= f.level;
                bool locked = !done && !open;
                Color back = done ? new Color32(206, 230, 190, 255) : open ? new Color32(252, 238, 206, 255) : new Color32(200, 190, 176, 255);
                var floor = f;
                var card = UIFactory.Btn("Floor", rows, pos, new Vector2(262, 46), "", back, () =>
                {
                    if (open) { Close(); battle.Begin(floor); }
                    else if (done) GameState.Say("¡Ya venciste a " + floor.es + "!");
                    else if (f.number > beaten + 1) GameState.Say("Primero vence el piso " + (beaten + 1) + ".  (Beat floor " + (beaten + 1) + " first.)");
                    else GameState.Say("Necesitas el nivel " + floor.level + ".  (You need level " + floor.level + ".)");
                });
                UIFactory.Img("Boss", card.transform, new Vector2(-108, 0), new Vector2(32, 32), locked ? new Color(0.35f, 0.35f, 0.35f) : Color.white,
                    UIFactory.LoadSprite("Art/Bosses/" + f.sprite));
                UIFactory.Label("Name", card.transform, new Vector2(-6, 8), new Vector2(156, 16), "Piso " + f.number + ": " + f.es, locked ? UIFactory.Faded : UIFactory.Ink, TextAnchor.MiddleLeft);
                UIFactory.LLabel("Test", card.transform, new Vector2(-6, -9), new Vector2(156, 14), f.testEs, f.en, UIFactory.Faded, TextAnchor.MiddleLeft, 12);
                string status = done ? "Vencido" : open ? "¡Lucha!" : "Nv " + f.level;
                UIFactory.Label("Status", card.transform, new Vector2(98, 0), new Vector2(50, 16), status,
                    done ? UIFactory.Green : open ? UIFactory.Red : UIFactory.Faded, TextAnchor.MiddleRight, 12);
                if (locked) UIFactory.Img("Lock", card.transform, new Vector2(-94, -10), new Vector2(12, 12), Color.white, UIFactory.LoadSprite("Art/UI/lock"));
            }
            translate.Set(translate.English);
        }
    }

    /// <summary>
    /// A boss battle, turn-based like Pokémon Showdown. Pick a move; to pull it off you must answer a
    /// Spanish question before the timer runs out. Then the boss strikes back.
    /// No translations and no hints in battle.
    /// Moves: Atacar (weapon damage), Hechizo (double damage, 5 mana), Defender (halve the next hit and
    /// heal a little), Comer (eat the best food in your bag to heal - no question).
    /// </summary>
    public class BattleWindow : MonoBehaviour
    {
        enum Phase { Choose, Question, Enemy, Over }

        GameObject window;
        Transform moves, answers;
        Text title, log, prompt, playerHp, bossHp;
        RectTransform playerBar, bossBar, timerBar, bossImage;
        CharacterPreview portrait;
        VillageController village;
        DungeonData.Floor floor;
        DungeonData.Question question;
        Phase phase;
        string pendingMove;
        int bossHpNow;
        bool defending;
        float timer, wait, shake;
        const float AnswerTime = 12f;

        public bool Open => window != null && window.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            var panel = UIFactory.Panel("Battle", canvas, new Vector2(0, -2), new Vector2(600, 344));
            window = panel.gameObject;
            var w = window.transform;
            UIFactory.Img("Arena", w, new Vector2(0, 52), new Vector2(560, 150), new Color32(96, 124, 84, 255));
            UIFactory.Img("Ground", w, new Vector2(0, 0), new Vector2(560, 46), new Color32(150, 140, 120, 255));
            title = UIFactory.Label("Title", w, new Vector2(0, 148), new Vector2(520, 24), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);

            // you (left) and the boss (right)
            var holder = UIFactory.Rect("You", w, new Vector2(-190, 42), new Vector2(72, 96));
            portrait = holder.gameObject.AddComponent<CharacterPreview>();
            portrait.Build(new Vector2(72, 96));
            playerBar = Bar(w, new Vector2(-190, -6), out playerHp, new Color32(80, 190, 90, 255));
            bossImage = UIFactory.Img("Boss", w, new Vector2(190, 52), new Vector2(96, 96), Color.white).rectTransform;
            bossBar = Bar(w, new Vector2(190, -6), out bossHp, new Color32(206, 52, 52, 255));

            log = UIFactory.Label("Log", w, new Vector2(0, 90), new Vector2(250, 60), "", Color.white);
            UIFactory.Outline(log);
            prompt = UIFactory.Label("Prompt", w, new Vector2(0, -42), new Vector2(540, 24), "", UIFactory.Ink);
            UIFactory.Box("TimerBack", w, new Vector2(0, -60), new Vector2(404, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", w, new Vector2(0, -60), new Vector2(400, 6), UIFactory.Gold).rectTransform;
            moves = UIFactory.Rect("Moves", w, Vector2.zero, Vector2.zero);
            answers = UIFactory.Rect("Answers", w, Vector2.zero, Vector2.zero);
            window.SetActive(false);
        }

        RectTransform Bar(Transform w, Vector2 pos, out Text text, Color color)
        {
            UIFactory.Box("BarBack", w, pos, new Vector2(150, 14), new Color32(40, 30, 34, 255));
            var fill = UIFactory.Img("Fill", w, pos, new Vector2(144, 8), color).rectTransform;
            fill.pivot = new Vector2(0, 0.5f);
            fill.anchoredPosition = pos - new Vector2(72, 0);
            text = UIFactory.Label("Hp", w, pos + new Vector2(0, -16), new Vector2(150, 14), "", Color.white, TextAnchor.MiddleCenter, 12);
            UIFactory.Outline(text);
            return fill;
        }

        public void Begin(DungeonData.Floor f)
        {
            floor = f;
            bossHpNow = f.hp;
            defending = false;
            title.text = "Piso " + f.number + ": " + f.es;
            bossImage.GetComponent<Image>().sprite = UIFactory.LoadSprite("Art/Bosses/" + f.sprite);
            portrait.Show(PlayerProfile.Current.appearance, CharacterLayers.FromEquipment());
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            Say(f.es + " te desafía.");
            ShowMoves();
        }

        void Say(string text) => log.text = text;

        void ShowMoves()
        {
            phase = Phase.Choose;
            Clear(answers);
            Clear(moves);
            prompt.text = "¿Qué haces?";
            timerBar.sizeDelta = new Vector2(400, 6);
            var stats = GameState.Data.stats;
            Move("Atacar", -207, UIFactory.Red, () => Ask("attack"));
            Move("Hechizo (5)", -69, UIFactory.Purple, () =>
            {
                if (stats.mana < 5) { Say("No tienes maná."); return; }
                Ask("spell");
            });
            Move("Defender", 69, UIFactory.Blue, () => Ask("defend"));
            Move("Comer", 207, UIFactory.Green, Eat);
            Move("Huir", 0, UIFactory.Faded, Flee, -122);
            Refresh();
        }

        void Move(string label, float x, Color color, Action onClick, float y = -88)
        {
            UIFactory.Btn("Move", moves, new Vector2(x, y), new Vector2(130, 30), label, color, () => { if (phase == Phase.Choose || label == "Huir") onClick(); });
        }

        void Ask(string move)
        {
            pendingMove = move;
            question = DungeonData.Ask(floor);
            phase = Phase.Question;
            timer = AnswerTime;
            Clear(moves);
            prompt.text = question.prompt;
            for (int i = 0; i < question.options.Length; i++)
            {
                int index = i;
                var pos = new Vector2(-136 + (i % 2) * 272, -88 - (i / 2) * 36);
                UIFactory.Btn("Answer", answers, pos, new Vector2(262, 30), question.options[i], UIFactory.Teal, () => Answer(index));
            }
        }

        void Answer(int index)
        {
            if (phase != Phase.Question) return;
            Clear(answers);
            bool right = index == question.answer;
            var stats = GameState.Data.stats;
            if (!right)
            {
                Say(index < 0 ? "¡Se acabó el tiempo!" : "¡Fallaste! Era: " + question.options[question.answer]);
            }
            else if (pendingMove == "defend")
            {
                defending = true;
                stats.Heal(3);
                Say("Te defiendes. (+3 HP)");
            }
            else
            {
                int weapon = GameState.EquippedWeapon()?.power ?? 1;
                int dmg = weapon * 2 + GameState.Data.level / 2 + UnityEngine.Random.Range(2, 6);
                if (pendingMove == "spell") { stats.TrySpendMana(5); dmg *= 2; }
                bossHpNow = Mathf.Max(0, bossHpNow - dmg);
                shake = 0.3f;
                Say((pendingMove == "spell" ? "¡Hechizo! " : "¡Golpe! ") + "-" + dmg);
                GameState.AddXp(SkillType.Vocabulary, 3);
            }
            EndPlayerTurn();
        }

        void Eat()
        {
            var bag = GameState.Data.bag;
            int best = -1, bestPower = 0;
            for (int i = 0; i < bag.Count; i++)
            {
                var item = ItemDatabase.Get(bag[i].id);
                if (item != null && item.kind == ItemKind.Food && item.power > bestPower) { best = i; bestPower = item.power; }
            }
            if (best < 0) { Say("No tienes comida."); return; }
            var food = ItemDatabase.Get(bag[best].id);
            GameState.RemoveAt(best);
            int heal = Mathf.Max(4, bestPower / 2);
            GameState.Data.stats.Heal(heal);
            Say("Comes " + food.spanish + ". (+" + heal + " HP)");
            Clear(moves);
            EndPlayerTurn();
        }

        void EndPlayerTurn()
        {
            Refresh();
            if (bossHpNow <= 0) { Win(); return; }
            phase = Phase.Enemy;
            wait = 1.2f;
        }

        void EnemyTurn()
        {
            int dmg = Mathf.Max(1, floor.damage - GameState.Defense() / 2);
            if (defending) dmg = Mathf.Max(1, dmg / 2);
            defending = false;
            GameState.Data.stats.Damage(dmg);
            Say(floor.es + " ataca. (-" + dmg + " HP)");
            Refresh();
            if (GameState.Data.stats.hp <= 0) { Lose(); return; }
            ShowMoves();
        }

        void Win()
        {
            phase = Phase.Over;
            Clear(moves);
            int reward = floor.number * 60;
            if (GameState.Data.dungeonFloor < floor.number) GameState.Data.dungeonFloor = floor.number;
            GameState.AddCurrency(reward);
            GameState.AddGold(floor.number * 40);
            GameState.AddXp(SkillType.Combat, floor.number * 40);
            prompt.text = "¡Venciste a " + floor.es + "!  +" + reward + " " + GameConfig.CurrencyName + "  +" + floor.number * 40 + " " + GameConfig.GoldName;
            Say("¡Victoria!");
            GameState.Save();
            UIFactory.Btn("Leave", moves, new Vector2(0, -96), new Vector2(180, 30), "Salir", UIFactory.Green, Close);
        }

        void Lose()
        {
            phase = Phase.Over;
            Clear(moves);
            var stats = GameState.Data.stats;
            int lost = stats.gold / 10;
            stats.AddGold(-lost);
            stats.Heal(stats.maxHp / 2 - stats.hp);
            prompt.text = "Has perdido... (-" + lost + " " + GameConfig.GoldName + ")";
            Say(floor.es + " gana esta vez.");
            GameState.Save();
            UIFactory.Btn("Leave", moves, new Vector2(0, -96), new Vector2(180, 30), "Salir", UIFactory.Red, Close);
        }

        void Flee()
        {
            if (phase == Phase.Over) { Close(); return; }
            Say("Huyes de la batalla.");
            Close();
        }

        public void Close()
        {
            if (!Open) return;
            window.SetActive(false);
            village.NoteWindowClosed();
        }

        void Refresh()
        {
            var s = GameState.Data.stats;
            playerBar.sizeDelta = new Vector2(144f * s.hp / Mathf.Max(1, s.maxHp), 8);
            playerHp.text = PlayerProfile.Current.playerName + "  " + s.hp + "/" + s.maxHp + "  Maná " + s.mana;
            bossBar.sizeDelta = new Vector2(144f * bossHpNow / floor.hp, 8);
            bossHp.text = floor.es + "  " + bossHpNow + "/" + floor.hp;
        }

        static void Clear(Transform t)
        {
            foreach (Transform c in t) Destroy(c.gameObject);
        }

        void Update()
        {
            if (!Open) return;
            if (shake > 0)
            {
                shake -= Time.deltaTime;
                bossImage.anchoredPosition = new Vector2(190, 52) + UnityEngine.Random.insideUnitCircle * 4 * shake / 0.3f;
                bossImage.GetComponent<Image>().color = shake > 0 ? new Color(1, 0.5f, 0.5f) : Color.white;
            }
            else bossImage.anchoredPosition = new Vector2(190, 52 + Mathf.Sin(Time.time * 2) * 3);   // the boss breathes

            if (phase == Phase.Question)
            {
                timer -= Time.deltaTime;
                timerBar.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / AnswerTime), 6);
                for (int i = 0; i < 4; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { Answer(i); return; }
                if (timer <= 0) Answer(-1);
            }
            else if (phase == Phase.Enemy)
            {
                wait -= Time.deltaTime;
                if (wait <= 0) EnemyTurn();
            }
        }
    }
}
