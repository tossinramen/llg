using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Fishing: wait for a bite, then click the word tiles in the right order to build the sentence
    /// before the time runs out. Trains Fishing + Grammar. The magic rod gives extra time.
    /// </summary>
    public class FishingGame : MonoBehaviour
    {
        const int HintCost = 2;
        enum Phase { Waiting, Building, Result }

        GameObject window;
        Transform poolRow, answerRow;
        Text heading, english, result;
        Image timerBar;
        Button hintButton, againButton;
        SentenceEntry sentence;
        string[] tokens;
        readonly List<int> answer = new List<int>();        // indexes into tokens, in the order clicked
        readonly List<int> shuffled = new List<int>();
        Phase phase;
        float timer, totalTime;
        VillageController village;

        public bool Open => window != null && window.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            window = UIFactory.Panel("Fishing", canvas, new Vector2(0, -8), new Vector2(600, 300)).gameObject;
            var w = window.transform;
            UIFactory.Label("Title", w, new Vector2(0, 126), new Vector2(560, 20), "LA PESCA", UIFactory.Purple, TextAnchor.MiddleCenter, 16, true);
            UIFactory.Img("Fish", w, new Vector2(-250, 122), new Vector2(32, 32), Color.white, UIFactory.LoadSprite("Art/Icons/food_fish"));
            heading = UIFactory.Label("Heading", w, new Vector2(0, 98), new Vector2(560, 22), "", UIFactory.Ink);
            english = UIFactory.Label("English", w, new Vector2(0, 72), new Vector2(560, 22), "", UIFactory.Teal);
            UIFactory.Box("TimerBack", w, new Vector2(0, 50), new Vector2(404, 12), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", w, new Vector2(0, 50), new Vector2(400, 8), UIFactory.Green);
            UIFactory.Box("AnswerLine", w, new Vector2(0, 14), new Vector2(560, 36), new Color32(236, 222, 184, 255));
            answerRow = UIFactory.Rect("Answer", w, new Vector2(0, 14), Vector2.zero);
            poolRow = UIFactory.Rect("Pool", w, new Vector2(0, -34), Vector2.zero);
            result = UIFactory.Label("Result", w, new Vector2(0, -78), new Vector2(560, 40), "", UIFactory.Ink);

            hintButton = UIFactory.Btn("Hint", w, new Vector2(-180, -122), new Vector2(200, 26), "Pista / Hint (2 mana)", UIFactory.Blue, Hint);
            againButton = UIFactory.Btn("Again", w, new Vector2(10, -122), new Vector2(160, 26), "Otra vez / Again", UIFactory.Green, Cast);
            UIFactory.Btn("Leave", w, new Vector2(190, -122), new Vector2(160, 26), "Salir / Leave", UIFactory.Red, Close);
            window.SetActive(false);
        }

        public void Begin()
        {
            window.SetActive(true);
            Cast();
        }

        void Cast()
        {
            var options = GameState.Pack.Sentences(GameState.Data.skills.WordTierFor(SkillType.Grammar));
            if (options.Count == 0)
            {
                GameState.Say("No sentences in the language pack yet.");
                Close();
                return;
            }
            sentence = options[Random.Range(0, options.Count)];
            tokens = sentence.Tokens;
            phase = Phase.Waiting;
            timer = Random.Range(1.2f, 3f);
            heading.text = "Esperando...  (Waiting for a bite...)";
            english.text = "";
            result.text = "";
            answer.Clear();
            Clear(poolRow);
            Clear(answerRow);
            hintButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(false);
            timerBar.rectTransform.sizeDelta = new Vector2(400, 8);
        }

        void Bite()
        {
            phase = Phase.Building;
            totalTime = 10 + tokens.Length * 3 + (GameState.Has("rod_fine") ? 10 : 0);
            timer = totalTime;
            heading.text = "¡Un pez! Forma la frase:  (A fish! Build the sentence:)";
            english.text = "\"" + sentence.english + "\"";
            hintButton.gameObject.SetActive(true);

            shuffled.Clear();
            for (int i = 0; i < tokens.Length; i++) shuffled.Add(i);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            if (tokens.Length > 1 && InOrder(shuffled)) (shuffled[0], shuffled[1]) = (shuffled[1], shuffled[0]);
            Redraw();
        }

        static bool InOrder(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] != i) return false;
            return true;
        }

        // ------------------------------------------------------------ tiles

        static void Clear(Transform row)
        {
            foreach (Transform child in row) Destroy(child.gameObject);
        }

        void Redraw()
        {
            Clear(poolRow);
            Clear(answerRow);
            Layout(answerRow, answer, UIFactory.Gold, i => { answer.Remove(i); Redraw(); }, null);
            Layout(poolRow, shuffled, UIFactory.Teal, i => { answer.Add(i); Redraw(); CheckAnswer(); }, answer);
        }

        /// <summary>Lays word tiles out in a centred row. Tiles in "hidden" keep their gap but are not drawn.</summary>
        void Layout(Transform row, List<int> order, Color color, System.Action<int> onClick, List<int> hidden)
        {
            const float gap = 6;
            float total = -gap;
            foreach (int i in order) total += TileWidth(i) + gap;
            float x = -total / 2;
            foreach (int i in order)
            {
                float width = TileWidth(i);
                if (hidden == null || !hidden.Contains(i))
                {
                    int captured = i;
                    UIFactory.Btn("Tile", row, new Vector2(x + width / 2, 0), new Vector2(width, 28), tokens[i], color, () => onClick(captured));
                }
                x += width + gap;
            }
        }

        float TileWidth(int i) => tokens[i].Length * 8 + 20;

        void CheckAnswer()
        {
            if (phase != Phase.Building || answer.Count < tokens.Length) return;

            var built = new List<string>();
            foreach (int i in answer) built.Add(tokens[i]);
            if (string.Join(" ", built) == sentence.text)
            {
                int reward = 6 + tokens.Length * 2;
                GameState.Data.fishCaught++;
                GameState.AddCurrency(reward);
                GameState.AddXp(SkillType.Fishing, 10);
                GameState.AddXp(SkillType.Grammar, 10);
                End("¡Lo atrapaste!  (You caught it!)\n" + sentence.text);
            }
            else
            {
                GameState.Data.stats.Damage(1);
                result.text = "Casi...  (Not quite - try again.)  -1 HP";
                answer.Clear();
                Redraw();
            }
        }

        void Hint()
        {
            if (phase != Phase.Building) return;
            // keep the part that is right so far, then place the next correct word
            int correct = 0;
            while (correct < answer.Count && tokens[answer[correct]] == tokens[correct]) correct++;
            if (correct >= tokens.Length) return;
            if (!GameState.Data.stats.TrySpendMana(HintCost))
            {
                GameState.Say("No tienes maná. (Not enough mana.)");
                return;
            }
            answer.RemoveRange(correct, answer.Count - correct);
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == tokens[correct] && !answer.Contains(i))
                {
                    answer.Add(i);
                    break;
                }
            }
            Redraw();
            CheckAnswer();
        }

        void End(string message)
        {
            phase = Phase.Result;
            result.text = message;
            heading.text = "";
            Clear(poolRow);
            hintButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(true);
            GameState.Save();
        }

        public void Close()
        {
            if (!Open) return;
            window.SetActive(false);
            village.NoteWindowClosed();
        }

        void Update()
        {
            if (!Open) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }

            if (phase == Phase.Waiting)
            {
                timer -= Time.deltaTime;
                if (timer <= 0) Bite();
            }
            else if (phase == Phase.Building)
            {
                timer -= Time.deltaTime;
                timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timer / totalTime), 8);
                if (timer <= 0)
                {
                    GameState.Data.stats.Damage(1);
                    End("¡Se escapó!  (It got away!)  -1 HP\n" + sentence.text);
                }
            }
        }
    }
}
