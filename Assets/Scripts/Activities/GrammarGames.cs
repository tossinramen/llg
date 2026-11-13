using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Spanish verb forms for the tense games: the regular endings for -ar / -er / -ir verbs, the spelling
    /// changes (busqué, pagué, empecé) and a table of the common irregular verbs.
    /// Persons: yo, tú, él/ella, nosotros, ellos. Tenses: present, past (pretérito), future.
    /// </summary>
    public static class Conjugator
    {
        public static readonly string[] Persons = { "yo", "tú", "él / ella", "nosotros", "ellos" };
        public static readonly string[] TenseEs = { "hoy (presente)", "ayer (pasado)", "mañana (futuro)" };
        public static readonly string[] TenseEn = { "today (present)", "yesterday (past)", "tomorrow (future)" };

        static readonly string[] ArNow = { "o", "as", "a", "amos", "an" }, ErNow = { "o", "es", "e", "emos", "en" }, IrNow = { "o", "es", "e", "imos", "en" };
        static readonly string[] ArPast = { "é", "aste", "ó", "amos", "aron" }, ErPast = { "í", "iste", "ió", "imos", "ieron" };
        static readonly string[] Future = { "é", "ás", "á", "emos", "án" };

        static readonly (string verb, string en)[] Regular =
        {
            ("hablar", "to speak"), ("comer", "to eat"), ("vivir", "to live"), ("comprar", "to buy"), ("cantar", "to sing"),
            ("bailar", "to dance"), ("caminar", "to walk"), ("trabajar", "to work"), ("estudiar", "to study"), ("cocinar", "to cook"),
            ("nadar", "to swim"), ("escuchar", "to listen"), ("mirar", "to look"), ("beber", "to drink"), ("correr", "to run"),
            ("aprender", "to learn"), ("vender", "to sell"), ("escribir", "to write"), ("abrir", "to open"), ("subir", "to go up"),
            ("llamar", "to call"), ("ayudar", "to help"), ("lavar", "to wash"), ("viajar", "to travel"), ("cortar", "to cut"),
            ("pescar", "to fish"), ("buscar", "to look for"), ("pagar", "to pay"), ("llegar", "to arrive"), ("recibir", "to receive"),
        };

        class Irregular { public string en; public string[] now, past; public string futureStem; }

        static Irregular I(string en, string now, string past, string futureStem = null) =>
            new Irregular { en = en, now = now?.Split(' '), past = past?.Split(' '), futureStem = futureStem };

        static readonly Dictionary<string, Irregular> Irregulars = new Dictionary<string, Irregular>
        {
            { "ser", I("to be (who you are)", "soy eres es somos son", "fui fuiste fue fuimos fueron") },
            { "estar", I("to be (how / where)", "estoy estás está estamos están", "estuve estuviste estuvo estuvimos estuvieron") },
            { "ir", I("to go", "voy vas va vamos van", "fui fuiste fue fuimos fueron") },
            { "tener", I("to have", "tengo tienes tiene tenemos tienen", "tuve tuviste tuvo tuvimos tuvieron", "tendr") },
            { "hacer", I("to do / to make", "hago haces hace hacemos hacen", "hice hiciste hizo hicimos hicieron", "har") },
            { "poder", I("to be able to", "puedo puedes puede podemos pueden", "pude pudiste pudo pudimos pudieron", "podr") },
            { "querer", I("to want", "quiero quieres quiere queremos quieren", "quise quisiste quiso quisimos quisieron", "querr") },
            { "decir", I("to say", "digo dices dice decimos dicen", "dije dijiste dijo dijimos dijeron", "dir") },
            { "venir", I("to come", "vengo vienes viene venimos vienen", "vine viniste vino vinimos vinieron", "vendr") },
            { "dar", I("to give", "doy das da damos dan", "di diste dio dimos dieron") },
            { "ver", I("to see", "veo ves ve vemos ven", "vi viste vio vimos vieron") },
            { "saber", I("to know", "sé sabes sabe sabemos saben", "supe supiste supo supimos supieron", "sabr") },
            { "poner", I("to put", "pongo pones pone ponemos ponen", "puse pusiste puso pusimos pusieron", "pondr") },
            { "salir", I("to go out", "salgo sales sale salimos salen", null, "saldr") },
            { "dormir", I("to sleep", "duermo duermes duerme dormimos duermen", "dormí dormiste durmió dormimos durmieron") },
            { "jugar", I("to play", "juego juegas juega jugamos juegan", "jugué jugaste jugó jugamos jugaron") },
            { "pensar", I("to think", "pienso piensas piensa pensamos piensan", null) },
            { "volver", I("to come back", "vuelvo vuelves vuelve volvemos vuelven", null) },
            { "pedir", I("to ask for", "pido pides pide pedimos piden", "pedí pediste pidió pedimos pidieron") },
            { "empezar", I("to begin", "empiezo empiezas empieza empezamos empiezan", "empecé empezaste empezó empezamos empezaron") },
            { "encontrar", I("to find", "encuentro encuentras encuentra encontramos encuentran", null) },
            { "leer", I("to read", null, "leí leíste leyó leímos leyeron") },
        };

        /// <summary>The form of a verb for a person (0-4) and tense (0 present, 1 past, 2 future).</summary>
        public static string Form(string verb, int person, int tense)
        {
            Irregulars.TryGetValue(verb, out var ir);
            string stem = verb.Substring(0, verb.Length - 2), end = verb.Substring(verb.Length - 2);
            if (tense == 0)
            {
                if (ir?.now != null) return ir.now[person];
                return stem + (end == "ar" ? ArNow : end == "er" ? ErNow : IrNow)[person];
            }
            if (tense == 1)
            {
                if (ir?.past != null) return ir.past[person];
                if (person == 0 && end == "ar")
                {
                    if (stem.EndsWith("c")) return stem.Substring(0, stem.Length - 1) + "qué";      // busqué
                    if (stem.EndsWith("g")) return stem + "ué";                                     // pagué
                    if (stem.EndsWith("z")) return stem.Substring(0, stem.Length - 1) + "cé";      // empecé
                }
                return stem + (end == "ar" ? ArPast : ErPast)[person];
            }
            return (ir?.futureStem ?? verb) + Future[person];
        }

        /// <summary>A verb to practise: regular ones at lesson levels 1-2, irregular ones mixed in from level 3.</summary>
        public static (string verb, string english) RandomVerb()
        {
            if (Lessons.Level >= 3 && Random.value < 0.5f || Lessons.Level >= 5)
            {
                var keys = new List<string>(Irregulars.Keys);
                string v = keys[Random.Range(0, keys.Count)];
                return (v, Irregulars[v].en);
            }
            return Regular[Random.Range(0, Regular.Length)];
        }

        /// <summary>Three wrong forms of the same verb (other persons and tenses), to choose between.</summary>
        public static List<string> Decoys(string verb, string right)
        {
            var all = new List<string>();
            for (int t = 0; t < 3; t++)
                for (int p = 0; p < 5; p++)
                {
                    string f = Form(verb, p, t);
                    if (f != right && !all.Contains(f)) all.Add(f);
                }
            var picked = new List<string>();
            while (picked.Count < 3 && all.Count > 0)
            {
                int i = Random.Range(0, all.Count);
                picked.Add(all[i]);
                all.RemoveAt(i);
            }
            return picked;
        }
    }

    /// <summary>
    /// La forja de verbos (Bruno's anvil in the Herrería): forge a blade in five strikes. For each strike,
    /// pick the right form of the verb for the person and the time (yesterday / today / tomorrow), then stop
    /// the hammer in the middle of the bar. Wrong forms cool the metal (three and it's scrap).
    /// Trains verb tenses. XP: Grammar + Crafting.
    /// </summary>
    public class ForgeGame : GameWindow
    {
        const int Rounds = 5;
        enum Phase { Pick, Strike, Pause, Done }
        Text info, cue, english, result;
        Transform options;
        RectTransform marker;
        GameObject strikeRow;
        Phase phase;
        string answer;
        int round, score, heat, rightPicks;
        float swing, pause;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(540, 300), "La forja de verbos", "The Verb Forge");
            info = UIFactory.Label("Info", W, new Vector2(0, 102), new Vector2(500, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            cue = UIFactory.Label("Cue", W, new Vector2(0, 72), new Vector2(500, 26), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            english = UIFactory.Label("English", W, new Vector2(0, 46), new Vector2(500, 18), "", UIFactory.Teal);
            options = UIFactory.Rect("Options", W, new Vector2(0, -4), Vector2.zero);
            strikeRow = UIFactory.Rect("Strike", W, new Vector2(0, -4), Vector2.zero).gameObject;
            UIFactory.Box("Bar", strikeRow.transform, new Vector2(0, 14), new Vector2(360, 18), new Color32(70, 60, 60, 255));
            UIFactory.Img("Good", strikeRow.transform, new Vector2(0, 14), new Vector2(120, 12), new Color32(230, 150, 60, 255));
            UIFactory.Img("Perfect", strikeRow.transform, new Vector2(0, 14), new Vector2(40, 12), new Color32(255, 230, 110, 255));
            marker = UIFactory.Img("Hammer", strikeRow.transform, new Vector2(0, 14), new Vector2(6, 26), Color.white).rectTransform;
            UIFactory.LBtn("Hit", strikeRow.transform, new Vector2(0, -22), new Vector2(200, 28), "¡Golpea!  (Espacio)", "Strike!  (Space)", UIFactory.Red, Strike);
            result = UIFactory.Label("Result", W, new Vector2(0, -76), new Vector2(500, 36), "", UIFactory.Ink);
            UIFactory.LBtn("Leave", W, new Vector2(0, -124), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            OpenWindow();
            round = score = rightPicks = 0;
            heat = 3;
            Next();
        }

        void Next()
        {
            if (round >= Rounds || heat <= 0) { Finish(); return; }
            round++;
            phase = Phase.Pick;
            strikeRow.SetActive(false);
            foreach (Transform c in options) Destroy(c.gameObject);
            var (verb, en) = Conjugator.RandomVerb();
            int person = Random.Range(0, 5), tense = Random.Range(0, 3);
            answer = Conjugator.Form(verb, person, tense);
            cue.text = Conjugator.TenseEs[tense].Split(' ')[0] + "  ·  " + Conjugator.Persons[person] + "  ·  " + verb;
            english.text = en + "  —  " + Conjugator.TenseEs[tense] + " / " + Conjugator.TenseEn[tense];
            info.text = "Golpe " + round + "/" + Rounds + "   ·   Calidad " + score + "   ·   Calor " + new string('●', heat) + new string('○', 3 - heat);
            var four = Conjugator.Decoys(verb, answer);
            four.Insert(Random.Range(0, four.Count + 1), answer);
            for (int i = 0; i < four.Count; i++)
            {
                string choice = four[i];
                UIFactory.Btn("Form", options, new Vector2(-124 + (i % 2) * 248, 14 - (i / 2) * 36), new Vector2(236, 30), choice, UIFactory.Teal, () => Pick(choice));
            }
        }

        void Pick(string choice)
        {
            if (phase != Phase.Pick) return;
            foreach (Transform c in options) Destroy(c.gameObject);
            if (choice == answer)
            {
                rightPicks++;
                phase = Phase.Strike;
                strikeRow.SetActive(true);
                swing = Random.value * 6;
                result.text = "¡" + answer + "! Ahora golpea en el centro.";
                return;
            }
            heat--;
            result.text = "No... era \"" + answer + "\". El metal se enfría.";
            phase = Phase.Pause;
            pause = 1.4f;
        }

        void Strike()
        {
            if (phase != Phase.Strike) return;
            float off = Mathf.Abs(marker.anchoredPosition.x);
            int pts = off < 20 ? 3 : off < 60 ? 2 : 1;
            score += pts;
            result.text = pts == 3 ? "¡Golpe perfecto!" : pts == 2 ? "Buen golpe." : "Golpe flojo...";
            phase = Phase.Pause;
            pause = 0.8f;
        }

        void Finish()
        {
            phase = Phase.Done;
            strikeRow.SetActive(false);
            foreach (Transform c in options) Destroy(c.gameObject);
            string made = heat <= 0 ? "chatarra (scrap)" : score >= 13 ? "una hoja de oro (a golden blade)" : score >= 9 ? "una hoja de plata (a silver blade)" : "una hoja de bronce (a bronze blade)";
            cue.text = "¡Forjaste " + made.Split('(')[0].Trim() + "!";
            english.text = made;
            GameState.Reward(score * 5, score / 4);
            GameState.AddXp(SkillType.Grammar, Lessons.Xp(rightPicks * 5));
            GameState.AddXp(SkillType.Crafting, Lessons.Xp(score));
            result.text = "Formas correctas: " + rightPicks + "/" + Rounds + "   Calidad: " + score + "/15";
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open) return;
            if (phase == Phase.Strike)
            {
                swing += Time.deltaTime * (3.2f + round * 0.5f);
                marker.anchoredPosition = new Vector2(Mathf.Sin(swing) * 174, 14);
                if (Input.GetKeyDown(KeyCode.Space)) Strike();
            }
            else if (phase == Phase.Pause)
            {
                pause -= Time.deltaTime;
                if (pause <= 0) Next();
            }
        }
    }

    /// <summary>
    /// Ser o estar (Sera, in the Academy): a 60-second rush. A sentence with a gap appears; choose the SER
    /// form (left, A or ←) or the ESTAR form (right, D or →). After each answer it says why
    /// (profession, place, mood ...). Streaks score more. Trains ser vs estar. XP: Grammar.
    /// </summary>
    public class SerEstarGame : GameWindow
    {
        static readonly (string text, string ser, string estar, bool isSer, string why)[] Bank =
        {
            ("Yo ___ estudiante.", "soy", "estoy", true, "profesión / qué eres"), ("La sopa ___ caliente.", "es", "está", false, "estado (ahora)"),
            ("Madrid ___ en España.", "es", "está", false, "lugar"), ("Mi madre ___ profesora.", "es", "está", true, "profesión"),
            ("Nosotros ___ cansados.", "somos", "estamos", false, "cómo te sientes"), ("Las flores ___ rojas.", "son", "están", true, "color / cómo es"),
            ("¿Dónde ___ el baño?", "es", "está", false, "lugar"), ("Tú ___ muy simpático.", "eres", "estás", true, "carácter"),
            ("Hoy ___ lunes.", "es", "está", true, "día y fecha"), ("El café ___ frío.", "es", "está", false, "estado (ahora)"),
            ("Ellos ___ de México.", "son", "están", true, "origen"), ("Yo ___ feliz hoy.", "soy", "estoy", false, "cómo te sientes"),
            ("___ las tres de la tarde.", "Son", "Están", true, "la hora"), ("Mi hermano ___ alto.", "es", "está", true, "cómo es"),
            ("El gato ___ debajo de la mesa.", "es", "está", false, "lugar"), ("La puerta ___ abierta.", "es", "está", false, "estado"),
            ("Este libro ___ de Ana.", "es", "está", true, "de quién es"), ("La mesa ___ de madera.", "es", "está", true, "material"),
            ("Mis padres ___ en casa.", "son", "están", false, "lugar"), ("¿Cómo ___ usted hoy?", "es", "está", false, "cómo te sientes"),
            ("Ella ___ médica.", "es", "está", true, "profesión"), ("Los niños ___ jugando.", "son", "están", false, "acción en este momento"),
            ("El examen ___ difícil.", "es", "está", true, "cómo es"), ("Yo ___ enfermo.", "soy", "estoy", false, "salud (estado)"),
            ("Nosotros ___ amigos.", "somos", "estamos", true, "relación"), ("La tienda ___ cerrada.", "es", "está", false, "estado"),
            ("Tú ___ en la plaza.", "eres", "estás", false, "lugar"), ("El cielo ___ azul.", "es", "está", true, "color / cómo es"),
            ("La fiesta ___ en mi casa.", "es", "está", true, "dónde ocurre un evento"), ("Estas manzanas ___ verdes todavía.", "son", "están", false, "estado (no maduras)"),
            ("Mi abuela ___ de Sevilla.", "es", "está", true, "origen"), ("El perro ___ contento.", "es", "está", false, "cómo se siente"),
            ("___ importante estudiar.", "Es", "Está", true, "opinión general"), ("Yo ___ comiendo.", "soy", "estoy", false, "acción en este momento"),
            ("Ellas ___ hermanas.", "son", "están", true, "relación"), ("El pan ___ en la mesa.", "es", "está", false, "lugar"),
        };

        Text sentence, score, feedback;
        Button left, right;
        Image timerBar;
        int index, correct, streak, best;
        float timer;
        bool playing;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(540, 280), "¿Ser o estar?", "Ser or estar?");
            score = UIFactory.Label("Score", W, new Vector2(0, 92), new Vector2(500, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            UIFactory.Box("TimerBack", W, new Vector2(0, 74), new Vector2(404, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", W, new Vector2(0, 74), new Vector2(400, 6), UIFactory.Green);
            sentence = UIFactory.Label("Sentence", W, new Vector2(0, 40), new Vector2(500, 30), "", UIFactory.Ink, TextAnchor.MiddleCenter, 24, true);
            left = UIFactory.Btn("Ser", W, new Vector2(-130, -14), new Vector2(230, 46), "SER", UIFactory.Blue, () => Answer(true));
            right = UIFactory.Btn("Estar", W, new Vector2(130, -14), new Vector2(230, 46), "ESTAR", UIFactory.Pink, () => Answer(false));
            UIFactory.LLabel("Keys", W, new Vector2(0, -52), new Vector2(500, 14), "A / ← = ser          D / → = estar", "A / ← = ser          D / → = estar", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            feedback = UIFactory.Label("Feedback", W, new Vector2(0, -78), new Vector2(500, 32), "", UIFactory.Ink);
            UIFactory.LBtn("Leave", W, new Vector2(0, -114), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            OpenWindow();
            correct = streak = best = 0;
            timer = 60;
            playing = true;
            feedback.text = "";
            Next();
        }

        void Next()
        {
            index = Random.Range(0, Bank.Length);
            var b = Bank[index];
            sentence.text = b.text;
            left.GetComponentInChildren<Text>().text = "SER:  " + b.ser;
            right.GetComponentInChildren<Text>().text = "ESTAR:  " + b.estar;
            score.text = "Aciertos " + correct + "   ·   Racha " + streak + "   ·   Mejor racha " + best;
        }

        void Answer(bool ser)
        {
            if (!playing) return;
            var b = Bank[index];
            string full = b.text.Replace("___", b.isSer ? b.ser : b.estar);
            if (ser == b.isSer)
            {
                correct++;
                streak++;
                best = Mathf.Max(best, streak);
                feedback.color = new Color32(40, 140, 60, 255);
                feedback.text = "✔ " + full + "   (" + (b.isSer ? "ser" : "estar") + ": " + b.why + ")";
            }
            else
            {
                streak = 0;
                timer -= 4;
                feedback.color = UIFactory.Red;
                feedback.text = "✘ " + full + "   (" + (b.isSer ? "ser" : "estar") + ": " + b.why + ")   -4 s";
            }
            Next();
        }

        void Finish()
        {
            playing = false;
            sentence.text = "¡Tiempo! Aciertos: " + correct;
            GameState.Reward(correct * 3 + best * 2, 1 + correct / 5);
            GameState.AddXp(SkillType.Grammar, Lessons.Xp(correct * 3));
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || !playing) return;
            timer -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / 60f), 6);
            if (timer <= 0) { Finish(); return; }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Answer(true);
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Answer(false);
        }
    }

    /// <summary>
    /// Tú o usted (Don Cortés, in the Academy): ten visitors arrive one by one. Greet each the right way:
    /// "tú" with friends, family and children; "usted" with strangers, elders and important people.
    /// Three slips and the lesson ends. Trains formal vs informal. XP: Conversation + Grammar.
    /// </summary>
    public class CourtesyGame : GameWindow
    {
        static readonly (string es, string en, bool formal)[] Visitors =
        {
            ("tu amigo Pablo", "your friend Pablo", false), ("tu hermana pequeña", "your little sister", false), ("un niño del pueblo", "a village child", false),
            ("tu primo", "your cousin", false), ("tu compañera de clase", "your classmate", false), ("tu mejor amiga", "your best friend", false),
            ("la alcaldesa", "the mayor", true), ("un señor mayor que no conoces", "an elderly man you don't know", true), ("la doctora", "the doctor", true),
            ("el profesor nuevo", "the new teacher", true), ("un cliente de la tienda", "a customer in the shop", true), ("la abuela de tu vecino", "your neighbour's grandmother", true),
            ("un policía", "a police officer", true), ("tu perro", "your dog", false),
        };
        static readonly (string tu, string usted, string en)[] Phrases =
        {
            ("¿Cómo estás?", "¿Cómo está usted?", "How are you?"), ("¿Cómo te llamas?", "¿Cómo se llama usted?", "What is your name?"),
            ("¿Quieres un café?", "¿Quiere usted un café?", "Would you like a coffee?"), ("¿De dónde eres?", "¿De dónde es usted?", "Where are you from?"),
            ("¿Puedes ayudarme?", "¿Puede usted ayudarme?", "Can you help me?"), ("Pasa, por favor.", "Pase, por favor.", "Come in, please."),
            ("¿Tienes hambre?", "¿Tiene usted hambre?", "Are you hungry?"), ("Siéntate aquí.", "Siéntese aquí.", "Sit here."),
            ("¿Qué quieres comer?", "¿Qué quiere comer usted?", "What would you like to eat?"), ("¿Dónde vives?", "¿Dónde vive usted?", "Where do you live?"),
            ("Te presento a mi madre.", "Le presento a mi madre.", "Let me introduce my mother to you."), ("¿Necesitas algo?", "¿Necesita usted algo?", "Do you need anything?"),
        };

        Text who, english, info, feedback;
        Transform options;
        int round, correct, lives;
        bool formal, playing;
        float pause = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(540, 280), "¿Tú o usted?", "Tú or usted?");
            info = UIFactory.Label("Info", W, new Vector2(0, 92), new Vector2(500, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            who = UIFactory.Label("Who", W, new Vector2(0, 62), new Vector2(500, 26), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            english = UIFactory.Label("English", W, new Vector2(0, 36), new Vector2(500, 18), "", UIFactory.Teal);
            options = UIFactory.Rect("Options", W, new Vector2(0, -12), Vector2.zero);
            feedback = UIFactory.Label("Feedback", W, new Vector2(0, -76), new Vector2(500, 32), "", UIFactory.Ink);
            UIFactory.LBtn("Leave", W, new Vector2(0, -114), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            OpenWindow();
            round = correct = 0;
            lives = 3;
            playing = true;
            feedback.text = "";
            Next();
        }

        void Next()
        {
            foreach (Transform c in options) Destroy(c.gameObject);
            if (round >= 10 || lives <= 0) { Finish(); return; }
            round++;
            var v = Visitors[Random.Range(0, Visitors.Length)];
            var p = Phrases[Random.Range(0, Phrases.Length)];
            formal = v.formal;
            who.text = "Llega " + v.es + ".";
            english.text = v.en + "  —  \"" + p.en + "\"";
            info.text = "Visita " + round + "/10   ·   Aciertos " + correct + "   ·   Vidas " + new string('♥', lives);
            bool usedFirst = Random.value < 0.5f;
            Option(usedFirst ? p.usted : p.tu, usedFirst, 14);
            Option(usedFirst ? p.tu : p.usted, !usedFirst, -22);
        }

        void Option(string text, bool isUsted, float y)
        {
            UIFactory.Btn("Phrase", options, new Vector2(0, y), new Vector2(380, 30), text, UIFactory.Teal, () =>
            {
                if (!playing || pause >= 0) return;
                bool right = isUsted == formal;
                if (right) correct++; else lives--;
                feedback.color = right ? (Color)new Color32(40, 140, 60, 255) : UIFactory.Red;
                feedback.text = (right ? "✔ " : "✘ ") + (formal ? "Se usa USTED: es una persona mayor, importante o desconocida." : "Se usa TÚ: es alguien cercano (amigo, familia, niño).");
                pause = right ? 0.9f : 1.8f;
            });
        }

        void Finish()
        {
            playing = false;
            who.text = lives > 0 ? "¡Qué educación! " + correct + "/10" : "Demasiados errores... " + correct + " aciertos";
            english.text = "";
            GameState.Reward(correct * 4, 1 + correct / 4);
            GameState.AddXp(SkillType.Conversation, Lessons.Xp(correct * 4));
            GameState.AddXp(SkillType.Grammar, Lessons.Xp(correct * 2));
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || pause < 0) return;
            pause -= Time.deltaTime;
            if (pause < 0) Next();
        }
    }

    /// <summary>
    /// El o la (the fruit stalls in the plaza): sort the market's crates. A noun appears; choose its
    /// article: el, la, los or las (buttons or keys 1-4). 45 seconds; streaks score more.
    /// Uses every noun of your lesson topic and level. Trains gender and number. XP: Vocabulary + Grammar.
    /// </summary>
    public class ArticleGame : GameWindow
    {
        static readonly string[] Articles = { "el", "la", "los", "las" };
        Text noun, english, score, feedback;
        Image timerBar;
        List<(string article, string word, string english)> pool;
        (string article, string word, string english) current;
        int correct, streak, best;
        float timer;
        bool playing;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -6), new Vector2(540, 270), "¿El o la?", "El or la?");
            score = UIFactory.Label("Score", W, new Vector2(0, 86), new Vector2(500, 16), "", UIFactory.Faded, TextAnchor.MiddleCenter, 12);
            UIFactory.Box("TimerBack", W, new Vector2(0, 68), new Vector2(404, 10), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", W, new Vector2(0, 68), new Vector2(400, 6), UIFactory.Green);
            noun = UIFactory.Label("Noun", W, new Vector2(0, 36), new Vector2(500, 30), "", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            english = UIFactory.Label("English", W, new Vector2(0, 10), new Vector2(500, 18), "", UIFactory.Teal);
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                UIFactory.Btn("Article", W, new Vector2(-186 + i * 124, -30), new Vector2(116, 40), (i + 1) + ".  " + Articles[i], i < 2 ? UIFactory.Blue : UIFactory.Pink, () => Answer(index));
            }
            feedback = UIFactory.Label("Feedback", W, new Vector2(0, -72), new Vector2(500, 20), "", UIFactory.Ink);
            UIFactory.LBtn("Leave", W, new Vector2(0, -108), new Vector2(140, 24), "Salir", "Leave", UIFactory.Red, Close);
        }

        public void Begin()
        {
            pool = new List<(string, string, string)>();
            foreach (var w in Lessons.Words(8)) Add(w);
            if (pool.Count < 12)
                foreach (var w in GameState.Pack.words)
                    if (w.tier <= Lessons.Level) Add(w);
            if (pool.Count == 0) { GameState.Say("No hay palabras con artículo en esta lección."); return; }
            OpenWindow();
            correct = streak = best = 0;
            timer = 45;
            playing = true;
            feedback.text = "";
            Next();
        }

        void Add(VocabEntry w)
        {
            string s = w.word.Trim();
            foreach (var a in Articles)
                if (s.StartsWith(a + " ") && s.Length - a.Length - 1 <= 22) { pool.Add((a, s.Substring(a.Length + 1), w.english)); return; }
        }

        void Next()
        {
            current = pool[Random.Range(0, pool.Count)];
            noun.text = "___  " + current.word;
            english.text = current.english;
            score.text = "Aciertos " + correct + "   ·   Racha " + streak + "   ·   Mejor racha " + best;
        }

        void Answer(int index)
        {
            if (!playing) return;
            bool right = Articles[index] == current.article;
            if (right) { correct++; streak++; best = Mathf.Max(best, streak); }
            else { streak = 0; timer -= 3; }
            feedback.color = right ? (Color)new Color32(40, 140, 60, 255) : UIFactory.Red;
            feedback.text = (right ? "✔ " : "✘ ") + current.article + " " + current.word + (right ? "" : "   -3 s");
            Next();
        }

        void Finish()
        {
            playing = false;
            noun.text = "¡Tiempo! Aciertos: " + correct;
            english.text = "";
            GameState.Reward(correct * 2 + best, 1 + correct / 6);
            GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(correct * 2));
            GameState.AddXp(SkillType.Grammar, Lessons.Xp(correct));
            GameState.Save();
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || !playing) return;
            timer -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / 45f), 6);
            if (timer <= 0) { Finish(); return; }
            for (int i = 0; i < 4; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Answer(i);
        }
    }
}
