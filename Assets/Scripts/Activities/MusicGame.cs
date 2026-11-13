using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Music (at the music school, with Profesora Isabel or at the piano): build a Spanish verse by
    /// clicking its words in order. Every right word plays a note on your instrument; finish the verse
    /// and the whole melody plays. 3 verses per lesson. Better instruments give more time.
    /// Trains Music + Grammar. (Later: songs that stun monsters in battle.)
    /// </summary>
    public class MusicGame : GameWindow
    {
        static readonly (string es, string en)[] Verses =
        {
            ("La luna brilla en el cielo", "The moon shines in the sky"),
            ("Canto con mi guitarra", "I sing with my guitar"),
            ("El sol sale por la mañana", "The sun comes up in the morning"),
            ("Bailamos en la plaza", "We dance in the square"),
            ("Mi corazón está feliz", "My heart is happy"),
            ("Las flores son de colores", "The flowers are colourful"),
            ("Toca la flauta conmigo", "Play the flute with me"),
            ("El viento canta en el olivo", "The wind sings in the olive tree"),
            ("Vamos al río a cantar", "Let's go to the river to sing"),
            ("La música nos une", "Music brings us together"),
        };
        static readonly float[] Scale = { 261.6f, 293.7f, 329.6f, 392.0f, 440.0f, 523.3f, 587.3f, 659.3f };   // C major pentatonic-ish
        static readonly Dictionary<string, AudioClip> Notes = new Dictionary<string, AudioClip>();
        const int Rounds = 3;

        Transform tiles, answerRow;
        LocalizedText english, info;
        Image timerBar;
        Text instrumentLabel;
        string[] words;
        readonly List<int> order = new List<int>();
        int placed, round, wins;
        float timer, total, melodyTimer = -1;
        int melodyNote;
        string timbre = "piano";

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -4), new Vector2(560, 300), "La música", "Music");
            english = UIFactory.LLabel("English", W, new Vector2(0, 96), new Vector2(500, 22), "", "", UIFactory.Teal).GetComponent<LocalizedText>();
            instrumentLabel = UIFactory.Label("Instrument", W, new Vector2(0, 74), new Vector2(500, 18), "", UIFactory.Faded);
            UIFactory.Box("TimerBack", W, new Vector2(0, 54), new Vector2(404, 12), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", W, new Vector2(0, 54), new Vector2(400, 8), UIFactory.Purple);
            UIFactory.Box("Stave", W, new Vector2(0, 18), new Vector2(520, 40), new Color32(246, 240, 226, 255));
            for (int i = 0; i < 5; i++)
                UIFactory.Img("Line", W, new Vector2(0, 8 + i * 5), new Vector2(500, 1), new Color32(170, 160, 150, 255));
            answerRow = UIFactory.Rect("Answer", W, new Vector2(0, 18), Vector2.zero);
            tiles = UIFactory.Rect("Tiles", W, new Vector2(0, -40), Vector2.zero);
            info = UIFactory.LLabel("Info", W, new Vector2(0, -86), new Vector2(500, 36), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
        }

        /// <summary>Starts a music lesson. At the piano you don't need your own instrument.</summary>
        public void Begin(bool atPiano)
        {
            ItemDef best = null;
            foreach (var id in ItemDatabase.Music)
            {
                var item = ItemDatabase.Get(id);
                if (GameState.Has(id) && (best == null || item.power > best.power)) best = item;
            }
            if (best == null && !atPiano)
            {
                village.SayLine("", "Necesitas un instrumento. Cómpralo en la tienda de música, o toca el piano.",
                    "You need an instrument. Buy one at the music shop, or play the piano.");
                return;
            }
            timbre = atPiano || best == null ? "piano" : best.id;
            float bonus = best != null ? best.power * 2f : 3f;
            instrumentLabel.text = atPiano ? "El piano" : GameState.Capital(best.spanish);
            total = 12f + bonus;
            round = 0;
            wins = 0;
            OpenWindow();
            NextVerse();
        }

        void NextVerse()
        {
            if (round >= Rounds)
            {
                int reward = wins * 5;
                info.Set("¡Concierto terminado! Versos: " + wins + "/" + Rounds + "  +" + reward + " " + GameConfig.GoldName,
                    "Concert over! Verses: " + wins + "/" + Rounds + "  +" + reward + " " + GameConfig.GoldName);
                foreach (Transform c in tiles) Destroy(c.gameObject);
                if (wins > 0)
                {
                    GameState.Reward(reward, Mathf.Max(1, reward / 3));
                    GameState.AddXp(SkillType.Music, Lessons.Xp(wins * 8));
                    GameState.AddXp(SkillType.Grammar, Lessons.Xp(wins * 4));
                }
                timer = 0;
                return;
            }
            round++;
            // half the verses are songs written for the game, half are sentences from your lessons (topic and level)
            var verse = Verses[Random.Range(0, Verses.Length)];
            var lessons = Lessons.Sentences().FindAll(x => x.Tokens.Length >= 3 && x.Tokens.Length <= 8);
            if (lessons.Count > 0 && Random.value < 0.5f)
            {
                var pick = lessons[Random.Range(0, lessons.Count)];
                verse = (pick.text, pick.english);
            }
            words = verse.es.Split(' ');
            english.Set("\"" + verse.en + "\"", "\"" + verse.en + "\"");
            info.Set("Toca las palabras en orden.  Verso " + round + "/" + Rounds, "Click the words in order.  Verse " + round + "/" + Rounds);
            placed = 0;
            order.Clear();
            for (int i = 0; i < words.Length; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (order[i], order[j]) = (order[j], order[i]); }
            timer = total;
            Redraw();
        }

        void Redraw()
        {
            foreach (Transform c in tiles) Destroy(c.gameObject);
            foreach (Transform c in answerRow) Destroy(c.gameObject);
            float x = -Width(0, placed) / 2;
            for (int i = 0; i < placed; i++)
            {
                float w = words[i].Length * 8 + 22;
                var note = UIFactory.Box("Note", answerRow, new Vector2(x + w / 2, 0), new Vector2(w, 28), UIFactory.Gold);
                UIFactory.Label("Word", note.transform, new Vector2(0, 1), new Vector2(w - 8, 24), words[i], Color.white);
                x += w + 6;
            }
            float tx = 0;
            var remaining = order.FindAll(i => i >= placed);
            foreach (int i in remaining) tx += words[i].Length * 8 + 28;
            tx = -(tx - 6) / 2;
            foreach (int i in remaining)
            {
                int index = i;
                float w = words[i].Length * 8 + 22;
                UIFactory.Btn("Tile", tiles, new Vector2(tx + w / 2, 0), new Vector2(w, 30), words[i], UIFactory.Purple, () => Click(index));
                tx += w + 6;
            }
            translate.Set(translate.English);
        }

        float Width(int from, int to)
        {
            float w = 0;
            for (int i = from; i < to; i++) w += words[i].Length * 8 + 28;
            return w - 6;
        }

        void Click(int index)
        {
            if (timer <= 0 || melodyTimer >= 0) return;
            if (words[index] != words[placed])
            {
                PlayNote(-1);            // a wrong, low note
                info.Set("¡Nota falsa! Inténtalo otra vez.", "Wrong note! Try again.");
                return;
            }
            PlayNote(placed);
            // move the clicked tile's word into the next place
            int pos = order.IndexOf(index);
            int target = order.IndexOf(placed);
            (order[pos], order[target]) = (order[target], order[pos]);
            placed++;
            Redraw();
            if (placed >= words.Length)
            {
                wins++;
                info.Set("¡Precioso! Escucha la melodía...", "Beautiful! Listen to the melody...");
                melodyTimer = 0;
                melodyNote = 0;
            }
        }

        void PlayNote(int step)
        {
            float freq = step < 0 ? 146.8f : Scale[step % Scale.Length];
            AudioManager.PlaySfx(Note(timbre, freq));
        }

        /// <summary>Makes a short note in the sound of the instrument (created in code, no files needed).</summary>
        static AudioClip Note(string timbre, float freq)
        {
            string key = timbre + freq;
            if (Notes.TryGetValue(key, out var clip)) return clip;
            const int rate = 22050;
            float length = timbre == "tambourine" ? 0.25f : 0.6f;
            int n = (int)(rate * length);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1, t * 60) * Mathf.Exp(-t * (timbre == "violin" || timbre == "flute" ? 3f : 6f));
                float w = 2 * Mathf.PI * freq * t;
                float v = timbre switch
                {
                    "guitar" => Mathf.Sin(w) + 0.5f * Mathf.Sin(2 * w) + 0.25f * Mathf.Sin(3 * w),
                    "flute" => Mathf.Sin(w) + 0.1f * Mathf.Sin(2 * w),
                    "clarinet" => Mathf.Sin(w) + 0.4f * Mathf.Sin(3 * w) + 0.2f * Mathf.Sin(5 * w),
                    "trumpet" => Mathf.Sin(w) + 0.6f * Mathf.Sin(2 * w) + 0.4f * Mathf.Sin(3 * w) + 0.2f * Mathf.Sin(4 * w),
                    "violin" => Mathf.Sin(w + 0.3f * Mathf.Sin(2 * Mathf.PI * 5 * t)) + 0.4f * Mathf.Sin(2 * w),
                    "tambourine" => Random.Range(-1f, 1f) * 0.6f + 0.3f * Mathf.Sin(w * 4),
                    _ => Mathf.Sin(w) + 0.3f * Mathf.Sin(2 * w) * Mathf.Exp(-t * 4),
                };
                data[i] = v * env * 0.25f;
            }
            clip = AudioClip.Create(key, n, 1, rate, false);
            clip.SetData(data, 0);
            Notes[key] = clip;
            return clip;
        }

        protected override void Update()
        {
            base.Update();
            if (!Open) return;
            if (melodyTimer >= 0)
            {
                // play the finished verse back as a little tune
                melodyTimer -= Time.deltaTime;
                if (melodyTimer <= 0)
                {
                    if (melodyNote < words.Length) { PlayNote(melodyNote++); melodyTimer = 0.22f; }
                    else { melodyTimer = -1; NextVerse(); }
                }
                return;
            }
            if (timer <= 0) return;
            timer -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / total), 8);
            if (timer <= 0)
            {
                info.Set("¡Se acabó el tiempo!  " + string.Join(" ", words), "Out of time!  " + string.Join(" ", words));
                melodyTimer = 1.4f;
                melodyNote = words.Length;     // skip the melody, go to the next verse after a pause
            }
        }
    }
}
