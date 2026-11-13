using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// El bingo de la taberna (Doña Lola): she calls numbers in Spanish words ("treinta y cinco"); click
    /// the number on your card if you have it. Four in a row (across, down or diagonal) wins.
    /// Clicking a number she hasn't called costs a life. Numbers go up to 30 at lesson levels 1-2,
    /// 60 at level 3 and 99 from level 4. Trains numbers. XP: Vocabulary + Listening.
    /// </summary>
    public class BingoGame : GameWindow
    {
        const int Size = 4, MaxCalls = 26;
        const float CallTime = 4.5f;
        static readonly string[] Units =
        {
            "cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince",
            "dieciséis", "diecisiete", "dieciocho", "diecinueve", "veinte", "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco",
            "veintiséis", "veintisiete", "veintiocho", "veintinueve",
        };
        static readonly string[] Tens = { "", "", "", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };

        /// <summary>A number from 0 to 99 in Spanish words.</summary>
        public static string Words(int n) => n < 30 ? Units[n] : Tens[n / 10] + (n % 10 > 0 ? " y " + Units[n % 10] : "");

        Text call, history, info;
        Transform cardRoot;
        Image timerBar;
        readonly int[] card = new int[Size * Size];
        readonly bool[] marked = new bool[Size * Size];
        readonly Image[] cells = new Image[Size * Size];
        readonly List<int> called = new List<int>();
        readonly List<int> deck = new List<int>();
        int lives, max;
        float timer;
        bool playing;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(540, 320), "El bingo de la taberna", "Tavern Bingo");
            info = UIFactory.Label("Info", W, new Vector2(0, 112), new Vector2(500, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            UIFactory.LLabel("Says", W, new Vector2(-130, 78), new Vector2(230, 16), "Doña Lola canta:", "Doña Lola calls:", UIFactory.Ink);
            call = UIFactory.Label("Call", W, new Vector2(-130, 44), new Vector2(240, 40), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            UIFactory.Box("TimerBack", W, new Vector2(-130, 12), new Vector2(204, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", W, new Vector2(-130, 12), new Vector2(200, 6), UIFactory.Gold);
            history = UIFactory.Label("History", W, new Vector2(-130, -40), new Vector2(240, 80), "", UIFactory.Faded, TextAnchor.UpperCenter, 12);
            cardRoot = UIFactory.Rect("Card", W, new Vector2(130, 6), Vector2.zero);
            UIFactory.LBtn("Leave", W, new Vector2(0, -136), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            OpenWindow();
            max = Lessons.Level <= 2 ? 30 : Lessons.Level == 3 ? 60 : 99;
            var numbers = new List<int>();
            for (int n = 1; n <= max; n++) numbers.Add(n);
            for (int i = numbers.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (numbers[i], numbers[j]) = (numbers[j], numbers[i]); }
            foreach (Transform c in cardRoot) Destroy(c.gameObject);
            for (int i = 0; i < card.Length; i++)
            {
                card[i] = numbers[i];
                marked[i] = false;
                int index = i;
                var b = UIFactory.Btn("Cell", cardRoot, new Vector2(-66 + (i % Size) * 44, 66 - (i / Size) * 44), new Vector2(40, 40), card[i].ToString(),
                    UIFactory.Blue, () => Click(index));
                cells[i] = b.GetComponent<Image>();
            }
            // the calling order: mostly numbers from your card, so a line is always possible
            deck.Clear();
            var mine = new List<int>(card);
            var others = numbers.GetRange(card.Length, numbers.Count - card.Length);
            while (deck.Count < MaxCalls && (mine.Count > 0 || others.Count > 0))
            {
                var from = mine.Count > 0 && (others.Count == 0 || Random.value < 0.6f) ? mine : others;
                int k = Random.Range(0, from.Count);
                deck.Add(from[k]);
                from.RemoveAt(k);
            }
            called.Clear();
            lives = 3;
            playing = true;
            NextCall();
        }

        void NextCall()
        {
            if (called.Count >= deck.Count) { Finish(false); return; }
            called.Add(deck[called.Count]);
            call.text = "¡" + Words(called[called.Count - 1]) + "!";
            timer = CallTime;
            var lines = new List<string>();
            for (int i = called.Count - 2; i >= 0 && lines.Count < 5; i--) lines.Add(Words(called[i]));
            history.text = lines.Count > 0 ? "Antes:\n" + string.Join("\n", lines) : "";
            info.text = "Número " + called.Count + "/" + deck.Count + "   ·   Vidas " + new string('♥', lives) + "   ·   Cuatro en raya para ganar";
        }

        void Click(int index)
        {
            if (!playing || marked[index]) return;
            if (!called.Contains(card[index]))
            {
                lives--;
                GameState.Say("¡Ese número no ha salido!");
                if (lives <= 0) Finish(false);
                else info.text = "Número " + called.Count + "/" + deck.Count + "   ·   Vidas " + new string('♥', lives);
                return;
            }
            marked[index] = true;
            cells[index].color = UIFactory.Gold;
            if (HasLine()) Finish(true);
        }

        bool HasLine()
        {
            for (int i = 0; i < Size; i++)
            {
                bool row = true, col = true;
                for (int k = 0; k < Size; k++) { row &= marked[i * Size + k]; col &= marked[k * Size + i]; }
                if (row || col) return true;
            }
            bool d1 = true, d2 = true;
            for (int k = 0; k < Size; k++) { d1 &= marked[k * Size + k]; d2 &= marked[k * Size + Size - 1 - k]; }
            return d1 || d2;
        }

        void Finish(bool won)
        {
            playing = false;
            call.text = won ? "¡BINGO!" : "Sin línea...";
            int marks = 0;
            foreach (bool m in marked) if (m) marks++;
            if (won) GameState.Reward(30 + (deck.Count - called.Count) * 2, 3);
            GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(marks * 2 + (won ? 10 : 0)));
            GameState.AddXp(SkillType.Listening, Lessons.Xp(marks));
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || !playing) return;
            timer -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(200 * Mathf.Clamp01(timer / CallTime), 6);
            if (timer <= 0) NextCall();
        }
    }

    /// <summary>
    /// Las parejas (Luna, the girl by the candy shop): a memory game. Sixteen cards face down: eight Spanish
    /// words and their eight English meanings. Turn two over; a word and its meaning stay up.
    /// Fewer turns = more gold. Uses the words of your lesson topic and level. XP: Vocabulary + Reading.
    /// </summary>
    public class PairsGame : GameWindow
    {
        Text info;
        Transform grid;
        readonly List<(string text, int pair, bool spanish)> cards = new List<(string, int, bool)>();
        readonly List<Button> buttons = new List<Button>();
        readonly List<bool> up = new List<bool>();
        int first = -1, second = -1, turns, found;
        float flipBack = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(560, 330), "Las parejas", "Pairs");
            info = UIFactory.Label("Info", W, new Vector2(0, 118), new Vector2(520, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            grid = UIFactory.Rect("Grid", W, new Vector2(0, -8), Vector2.zero);
            UIFactory.LBtn("Leave", W, new Vector2(0, -142), new Vector2(140, 22), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            var words = Lessons.Words(8).FindAll(w => w.word.Length <= 16 && w.english.Length <= 16);
            if (words.Count < 8) words = GameState.Pack.words.FindAll(w => w.tier <= Lessons.Level && w.word.Length <= 16 && w.english.Length <= 16);
            if (words.Count < 8) { GameState.Say("No hay bastantes palabras en esta lección."); return; }
            OpenWindow();
            cards.Clear();
            var picked = new List<VocabEntry>();
            for (int tries = 0; picked.Count < 8 && tries < 400; tries++)
            {
                var w = words[Random.Range(0, words.Count)];
                if (!picked.Contains(w) && !picked.Exists(p => p.english == w.english)) picked.Add(w);
            }
            if (picked.Count < 8) { Close(); GameState.Say("No hay bastantes palabras distintas."); return; }
            for (int i = 0; i < 8; i++) { cards.Add((picked[i].word, i, true)); cards.Add((picked[i].english, i, false)); }
            for (int i = cards.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (cards[i], cards[j]) = (cards[j], cards[i]); }
            foreach (Transform c in grid) Destroy(c.gameObject);
            buttons.Clear();
            up.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                buttons.Add(UIFactory.Btn("Card", grid, new Vector2(-198 + (i % 4) * 132, 84 - (i / 4) * 54), new Vector2(124, 46), "?", UIFactory.Purple, () => Turn(index)));
                up.Add(false);
            }
            first = second = -1;
            turns = found = 0;
            flipBack = -1;
            Show();
        }

        void Show()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                bool shown = up[i] || i == first || i == second;
                buttons[i].GetComponent<Image>().color = !shown ? UIFactory.Purple : up[i] ? UIFactory.Green : cards[i].spanish ? UIFactory.Gold : UIFactory.Teal;
                buttons[i].GetComponentInChildren<Text>().text = shown ? cards[i].text : "?";
            }
            info.text = "Turnos " + turns + "   ·   Parejas " + found + "/8   ·   amarillo = español, verde azulado = inglés";
        }

        void Turn(int index)
        {
            if (flipBack >= 0 || up[index] || index == first) return;
            if (first < 0) { first = index; Show(); return; }
            second = index;
            turns++;
            if (cards[first].pair == cards[second].pair && cards[first].spanish != cards[second].spanish)
            {
                up[first] = up[second] = true;
                found++;
                first = second = -1;
                if (found == 8) Finish();
            }
            else flipBack = 0.9f;
            Show();
        }

        void Finish()
        {
            GameState.Reward(Mathf.Max(8, 44 - turns * 2), 2);
            GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(16));
            GameState.AddXp(SkillType.Reading, Lessons.Xp(8));
            GameState.Say("¡Todas las parejas en " + turns + " turnos!");
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || flipBack < 0) return;
            flipBack -= Time.deltaTime;
            if (flipBack < 0) { first = second = -1; Show(); }
        }
    }

    /// <summary>
    /// La cosecha de palabras (the scarecrow in the wheat fields): words fall from the sky. Move the basket
    /// (mouse, or A / D) and catch only the words of the topic shown ("Atrapa: La comida"). A wrong word
    /// costs a life; it gets faster. 45 seconds. Uses the words of every topic. XP: Vocabulary + Farming.
    /// </summary>
    public class HarvestGame : GameWindow
    {
        const float Left = -230, Right = 230, Top = 96, Bottom = -104;
        Text info, topicText;
        RectTransform basket, field;
        readonly List<(RectTransform rect, bool good)> falling = new List<(RectTransform, bool)>();
        List<VocabEntry> good, bad;
        float timer, spawn, basketX;
        int score, lives;
        bool playing;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(560, 330), "La cosecha de palabras", "The Word Harvest");
            topicText = UIFactory.Label("Topic", W, new Vector2(0, 118), new Vector2(520, 18), "", UIFactory.Purple);
            field = UIFactory.Box("Field", W, new Vector2(0, -6), new Vector2(520, 224), new Color32(196, 222, 240, 255)).rectTransform;
            UIFactory.Img("Wheat", field, new Vector2(0, -100), new Vector2(516, 22), new Color32(206, 172, 90, 255));
            basket = UIFactory.Box("Basket", field, new Vector2(0, Bottom + 8), new Vector2(84, 20), new Color32(150, 100, 60, 255)).rectTransform;
            UIFactory.Label("BasketText", basket, Vector2.zero, new Vector2(80, 16), "la cesta", Color.white, TextAnchor.MiddleCenter, 12);
            info = UIFactory.Label("Info", W, new Vector2(-70, -140), new Vector2(380, 16), "", UIFactory.Ink, TextAnchor.MiddleLeft, 12);
            UIFactory.LBtn("Leave", W, new Vector2(200, -140), new Vector2(120, 22), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            var pack = GameState.Pack;
            int level = Mathf.Max(2, Lessons.Level);
            var topics = pack.categories.FindAll(c => c.id != "imported" && c.id != "phrase");
            CategoryEntry topic = null;
            for (int tries = 0; tries < 30; tries++)
            {
                topic = topics[Random.Range(0, topics.Count)];
                good = pack.words.FindAll(w => w.category == topic.id && w.tier <= level && w.word.Length <= 18);
                if (good.Count >= 6) break;
            }
            if (good == null || good.Count < 6) { GameState.Say("No hay bastantes palabras."); return; }
            bad = pack.words.FindAll(w => w.category != topic.id && w.category != "imported" && w.tier <= level && w.word.Length <= 18);
            OpenWindow();
            topicText.text = "Atrapa: " + topic.name + "  (" + topic.english + ")";
            foreach (var f in falling) Destroy(f.rect.gameObject);
            falling.Clear();
            timer = 45;
            spawn = 0.5f;
            score = 0;
            lives = 3;
            basketX = 0;
            playing = true;
        }

        void Drop()
        {
            bool isGood = Random.value < 0.55f;
            var list = isGood ? good : bad;
            var w = list[Random.Range(0, list.Count)];
            float width = w.word.Length * 7 + 16;
            var box = UIFactory.Box("Word", field, new Vector2(Random.Range(Left + 30, Right - 30), Top), new Vector2(width, 18), new Color32(252, 244, 220, 255));
            box.raycastTarget = false;
            UIFactory.Label("Text", box.transform, new Vector2(0, 1), new Vector2(width - 4, 16), w.word, UIFactory.Ink, TextAnchor.MiddleCenter, 12);
            falling.Add((box.rectTransform, isGood));
        }

        void Finish()
        {
            playing = false;
            topicText.text = lives > 0 ? "¡Buena cosecha! " + score + " palabras" : "Se acabó... " + score + " palabras";
            GameState.Reward(score * 3, 1 + score / 6);
            GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(score * 2));
            GameState.AddXp(SkillType.Farming, Lessons.Xp(score));
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || !playing) return;
            timer -= Time.deltaTime;
            if (timer <= 0 || lives <= 0) { Finish(); return; }

            // the basket follows the mouse, or A / D
            float move = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            if (move != 0) basketX += move * 300 * Time.deltaTime;
            else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(field, Input.mousePosition, Camera.main, out var local) && Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f)
                basketX = local.x;
            basketX = Mathf.Clamp(basketX, Left, Right);
            basket.anchoredPosition = new Vector2(basketX, Bottom + 8);

            float speed = 60 + (45 - timer) * 2.2f;
            spawn -= Time.deltaTime;
            if (spawn <= 0) { Drop(); spawn = Mathf.Max(0.45f, 1.1f - (45 - timer) * 0.012f); }
            for (int i = falling.Count - 1; i >= 0; i--)
            {
                var (rect, isGood) = falling[i];
                rect.anchoredPosition += new Vector2(0, -speed * Time.deltaTime);
                bool caught = rect.anchoredPosition.y < Bottom + 20 && Mathf.Abs(rect.anchoredPosition.x - basketX) < 42 + rect.sizeDelta.x / 2;
                if (caught)
                {
                    if (isGood) score++;
                    else { lives--; GameState.Say("¡Esa no!"); }
                }
                if (caught || rect.anchoredPosition.y < Bottom - 6)
                {
                    Destroy(rect.gameObject);
                    falling.RemoveAt(i);
                }
            }
            info.text = "Tiempo " + Mathf.CeilToInt(timer) + " s   ·   Palabras " + score + "   ·   Vidas " + new string('♥', Mathf.Max(0, lives)) + "   ·   ratón o A / D";
        }
    }

    /// <summary>
    /// People with a trade, out in the world. Talk to one and they say something; you answer by clicking
    /// the words of a fitting reply in order (the English of what to say is shown). Four lines each.
    /// Trains conversation. XP: Conversation; a small gift if you did well.
    /// </summary>
    public static class Specialists
    {
        public class Exchange { public string es, en, replyEs, replyEn; }

        public class Spec
        {
            public char marker;
            public string name, roleEs, roleEn, greetEs, greetEn, gift;
            public int giftCount, skin, hairStyle, hairColor, shirt, pants;
            public (string kind, byte r, byte g, byte b)[] worn;
            public Exchange[] lines;
        }

        static Exchange X(string es, string en, string replyEs, string replyEn) => new Exchange { es = es, en = en, replyEs = replyEs, replyEn = replyEn };

        public static readonly Spec[] All =
        {
            new Spec { marker = 'α', name = "Simón", roleEs = "el pescador del lago", roleEn = "Lake Fisherman", skin = 3, hairStyle = 0, hairColor = 4, shirt = 1, pants = 1,
                worn = new[] { ("hat", (byte)150, (byte)160, (byte)106), ("beard", (byte)120, (byte)120, (byte)126) },
                greetEs = "Buenas. Este lago tiene los mejores peces.", greetEn = "Hello. This lake has the best fish.", gift = "bait_worm", giftCount = 3,
                lines = new[]
                {
                    X("¿Te gusta pescar?", "Do you like fishing?", "Sí, me gusta mucho pescar", "Yes, I like fishing a lot"),
                    X("¿Qué pescaste hoy?", "What did you catch today?", "Hoy pesqué una trucha grande", "Today I caught a big trout"),
                    X("El lago está tranquilo por la mañana.", "The lake is calm in the morning.", "Entonces vendré mañana temprano", "Then I will come early tomorrow"),
                    X("¿Quieres un poco de cebo?", "Do you want a bit of bait?", "Sí, muchas gracias, Simón", "Yes, thank you very much, Simón"),
                } },
            new Spec { marker = 'β', name = "Rosa", roleEs = "la segadora", roleEn = "Reaper (wheat farmer)", skin = 2, hairStyle = 5, hairColor = 0, shirt = 3, pants = 3,
                worn = new[] { ("hat", (byte)200, (byte)168, (byte)108), ("skirt", (byte)150, (byte)90, (byte)60) },
                greetEs = "¡Buenos días! El trigo ya está dorado.", greetEn = "Good morning! The wheat is golden already.", gift = "bread", giftCount = 2,
                lines = new[]
                {
                    X("¿Vienes a ayudar con el trigo?", "Have you come to help with the wheat?", "Sí, quiero ayudar en el campo", "Yes, I want to help in the field"),
                    X("¿Sabes qué hacemos con el trigo?", "Do you know what we make with wheat?", "Con el trigo se hace el pan", "Bread is made with wheat"),
                    X("Hoy hace mucho calor, ¿verdad?", "It's very hot today, isn't it?", "Sí, necesito beber agua fresca", "Yes, I need to drink cool water"),
                    X("Gracias por tu ayuda. ¿Tienes hambre?", "Thanks for your help. Are you hungry?", "Un poco, ¿tienes pan y queso?", "A little, do you have bread and cheese?"),
                } },
            new Spec { marker = 'γ', name = "Benito", roleEs = "el apicultor", roleEn = "Beekeeper", skin = 1, hairStyle = 4, hairColor = 1, shirt = 4, pants = 3,
                worn = new[] { ("hat", (byte)240, (byte)236, (byte)220), ("apron", (byte)240, (byte)210, (byte)90) },
                greetEs = "¡Despacio! Mis abejas están trabajando.", greetEn = "Slowly! My bees are working.", gift = "candy", giftCount = 3,
                lines = new[]
                {
                    X("¿Te dan miedo las abejas?", "Are you afraid of bees?", "No, las abejas no me dan miedo", "No, bees don't scare me"),
                    X("¿Sabes qué hacen las abejas?", "Do you know what bees make?", "Las abejas hacen miel dulce", "Bees make sweet honey"),
                    X("¿Te gusta la miel?", "Do you like honey?", "Me encanta la miel con pan", "I love honey with bread"),
                    X("Las flores son importantes para ellas.", "Flowers are important for them.", "Por eso hay muchas flores aquí", "That's why there are many flowers here"),
                } },
            new Spec { marker = 'δ', name = "Pepe", roleEs = "el buscador de oro", roleEn = "Prospector", skin = 4, hairStyle = 4, hairColor = 5, shirt = 6, pants = 1,
                worn = new[] { ("hat", (byte)110, (byte)80, (byte)56), ("beard", (byte)230, (byte)230, (byte)236) },
                greetEs = "¡Estas colinas esconden tesoros!", greetEn = "These hills hide treasures!", gift = "ore_coal", giftCount = 2,
                lines = new[]
                {
                    X("¿Buscas oro en estas colinas?", "Are you looking for gold in these hills?", "Sí, busco oro y piedras preciosas", "Yes, I'm looking for gold and precious stones"),
                    X("¿Tienes un buen pico?", "Do you have a good pickaxe?", "Tengo un pico de hierro nuevo", "I have a new iron pickaxe"),
                    X("Ayer encontré una moneda romana.", "Yesterday I found a Roman coin.", "¡Qué suerte! Es muy antigua", "How lucky! It is very old"),
                    X("Las rocas duras necesitan paciencia.", "Hard rocks need patience.", "Trabajaré despacio y con cuidado", "I will work slowly and carefully"),
                } },
            new Spec { marker = 'ε', name = "Inés", roleEs = "la cazadora", roleEn = "Huntress", skin = 2, hairStyle = 1, hairColor = 3, shirt = 2, pants = 4,
                worn = new[] { ("cloak", (byte)60, (byte)100, (byte)64) },
                greetEs = "Chist... Hay lobos por aquí.", greetEn = "Shh... There are wolves around here.", gift = "apple", giftCount = 2,
                lines = new[]
                {
                    X("¿Oyes a los lobos?", "Can you hear the wolves?", "Sí, están cerca del río", "Yes, they are near the river"),
                    X("¿Vas solo por el bosque?", "Are you going through the forest alone?", "No, voy con mi perro", "No, I'm going with my dog"),
                    X("De noche el bosque es peligroso.", "At night the forest is dangerous.", "Volveré al pueblo antes de la noche", "I will go back to the village before night"),
                    X("¿Adónde vas?", "Where are you going?", "Voy a la torre de las runas", "I'm going to the tower of runes"),
                } },
            new Spec { marker = 'ζ', name = "Tomé", roleEs = "el peregrino", roleEn = "Pilgrim", skin = 0, hairStyle = 0, hairColor = 5, shirt = 7, pants = 2,
                worn = new[] { ("cloak", (byte)120, (byte)96, (byte)70), ("hat", (byte)90, (byte)70, (byte)50), ("beard", (byte)200, (byte)200, (byte)206) },
                greetEs = "Buenas tardes, joven. Vengo de muy lejos.", greetEn = "Good afternoon, young one. I come from very far away.", gift = "", giftCount = 0,
                lines = new[]
                {
                    X("¿De dónde eres?", "Where are you from?", "Soy de Valdeluz, ¿y usted?", "I'm from Valdeluz, and you?"),
                    X("Camino desde hace muchos días.", "I have been walking for many days.", "Debe de estar muy cansado", "You must be very tired"),
                    X("¿Sabe dónde puedo dormir?", "Do you know where I can sleep?", "Hay una taberna en el pueblo", "There is a tavern in the village"),
                    X("¿Está lejos el pueblo?", "Is the village far?", "No, está al otro lado del puente", "No, it is on the other side of the bridge"),
                } },
        };

        public static Spec Find(char marker) => System.Array.Find(All, s => s.marker == marker);
    }

    /// <summary>The reply-building window used with the specialists (see Specialists).</summary>
    public class ReplyWindow : GameWindow
    {
        LocalizedText npcLine;
        Text goal, info, built;
        Transform tiles;
        Specialists.Spec spec;
        string[] tokens;
        readonly List<string> pool = new List<string>();
        readonly Dictionary<char, float> giftReady = new Dictionary<char, float>();
        int line, next, mistakes;
        float pause = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(560, 300), "Conversación", "Conversation");
            info = UIFactory.Label("Info", W, new Vector2(0, 100), new Vector2(520, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            npcLine = UIFactory.LLabel("Npc", W, new Vector2(0, 70), new Vector2(520, 30), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            goal = UIFactory.Label("Goal", W, new Vector2(0, 34), new Vector2(520, 18), "", UIFactory.Teal);
            UIFactory.Box("Line", W, new Vector2(0, 2), new Vector2(520, 32), new Color32(236, 222, 184, 255));
            built = UIFactory.Label("Built", W, new Vector2(0, 2), new Vector2(510, 26), "", UIFactory.Purple);
            tiles = UIFactory.Rect("Tiles", W, new Vector2(0, -52), Vector2.zero);
            UIFactory.LBtn("Leave", W, new Vector2(0, -124), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin(Specialists.Spec who)
        {
            spec = who;
            line = -1;
            mistakes = 0;
            pause = -1;
            OpenWindow();
            NextLine();
        }

        void NextLine()
        {
            line++;
            if (line >= spec.lines.Length) { Finish(); return; }
            var x = spec.lines[line];
            npcLine.Set(spec.name + ": " + x.es, spec.name + ": " + x.en);
            goal.text = "Responde: \"" + x.replyEn + "\"";
            tokens = x.replyEs.Split(' ');
            next = 0;
            pool.Clear();
            pool.AddRange(tokens);
            var other = spec.lines[(line + 1 + Random.Range(0, spec.lines.Length - 1)) % spec.lines.Length].replyEs.Split(' ');
            for (int k = 0; k < 2 && k < other.Length; k++)
                if (!pool.Contains(other[other.Length - 1 - k])) pool.Add(other[other.Length - 1 - k]);      // two words that don't belong
            for (int i = pool.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            Redraw();
        }

        void Redraw()
        {
            foreach (Transform c in tiles) Destroy(c.gameObject);
            built.text = string.Join(" ", tokens, 0, next) + (next < tokens.Length ? " ..." : "");
            info.text = spec.name + ", " + spec.roleEs + "   ·   Frase " + (line + 1) + "/" + spec.lines.Length + "   ·   Fallos " + mistakes;
            float total = -6;
            foreach (var t in pool) total += t.Length * 8 + 24;
            float x = -total / 2, y = 0;
            if (total > 500) { x = -250; }
            foreach (var t in pool)
            {
                float w = t.Length * 8 + 18;
                if (x + w > 256) { x = -250; y -= 32; }
                string word = t;
                UIFactory.Btn("Word", tiles, new Vector2(x + w / 2, y), new Vector2(w, 28), word, UIFactory.Teal, () => Pick(word));
                x += w + 6;
            }
        }

        void Pick(string word)
        {
            if (pause >= 0 || next >= tokens.Length) return;
            if (word != tokens[next])
            {
                mistakes++;
                GameState.Say("Esa palabra no va ahora.");
                Redraw();
                return;
            }
            next++;
            pool.Remove(word);
            Redraw();
            if (next >= tokens.Length)
            {
                foreach (Transform c in tiles) Destroy(c.gameObject);
                built.text = string.Join(" ", tokens);
                pause = 1.0f;
            }
        }

        void Finish()
        {
            int n = spec.lines.Length;
            GameState.Reward(Mathf.Max(4, n * 5 - mistakes), 2);
            GameState.AddXp(SkillType.Conversation, Lessons.Xp(n * 8));
            bool gift = mistakes <= 2 && !string.IsNullOrEmpty(spec.gift) && (!giftReady.TryGetValue(spec.marker, out float ready) || Time.time >= ready);
            if (gift)
            {
                GameState.AddItem(spec.gift, spec.giftCount);
                giftReady[spec.marker] = Time.time + 180;
            }
            GameState.Save();
            Close();
            village.SayLine(spec.name, gift ? "¡Qué bien hablas! Toma, un regalo." : "¡Hasta pronto!", gift ? "You speak so well! Here, a gift." : "See you soon!");
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || pause < 0) return;
            pause -= Time.deltaTime;
            if (pause < 0) NextLine();
        }
    }
}
