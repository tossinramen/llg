using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// "El intruso" (the odd one out): four words appear - three belong to a topic and one does not.
    /// Click the one that doesn't fit. Started by Tomás the woodcutter or from the lesson board.
    /// Trains Vocabulary; harder lesson levels give more XP.
    /// </summary>
    public class IntrusoGame : GameWindow
    {
        const int Rounds = 6;
        readonly Button[] options = new Button[4];
        readonly LocalizedText[] optionText = new LocalizedText[4];
        LocalizedText question, result;
        Text score;
        VocabEntry intruder;
        readonly VocabEntry[] shown = new VocabEntry[4];
        int round, right;
        float nextTimer = -1;

        public void Build(Transform canvas, VillageController owner)
        {
            CreateWindow(canvas, owner, new Vector2(0, -4), new Vector2(520, 300), "El intruso", "The Odd One Out");
            question = UIFactory.LLabel("Question", W, new Vector2(0, 92), new Vector2(480, 22), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                options[i] = UIFactory.Btn("Option", W, new Vector2(-116 + (i % 2) * 232, 40 - (i / 2) * 56), new Vector2(220, 46), "", UIFactory.Teal, () => Choose(index));
                var label = UIFactory.LLabel("Word", options[i].transform, new Vector2(0, 1), new Vector2(210, 40), "", "", Color.white);
                UIFactory.Outline(label);
                optionText[i] = label.GetComponent<LocalizedText>();
                UIFactory.ListenButton(options[i].transform, new Vector2(94, 12), () => shown[index]?.word, "es");
            }
            result = UIFactory.LLabel("Result", W, new Vector2(0, -70), new Vector2(480, 40), "", "", UIFactory.Ink).GetComponent<LocalizedText>();
            score = UIFactory.Label("Score", W, new Vector2(0, -116), new Vector2(300, 20), "", UIFactory.Faded);
        }

        public void Begin()
        {
            round = 0;
            right = 0;
            OpenWindow();
            NextRound();
        }

        void NextRound()
        {
            nextTimer = -1;
            if (round >= Rounds)
            {
                int reward = right * 3;
                question.Set("¡Terminado!", "Finished!");
                result.Set("Aciertos: " + right + " / " + Rounds + "   +" + reward + " " + GameConfig.GoldName,
                    "Correct: " + right + " / " + Rounds + "   +" + reward + " " + GameConfig.GoldName);
                foreach (var o in options) o.gameObject.SetActive(false);
                if (right > 0)
                {
                    GameState.Reward(reward, Mathf.Max(1, reward / 3));
                    GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(right * 4));
                }
                GameState.Save();
                return;
            }
            round++;
            var pack = GameState.Pack;

            // pick a topic with at least three words at this level, then one word from another topic
            // "Más palabras" is a mixed bag, not a real topic, so it can't have an odd one out
            var topics = pack.categories.FindAll(c => c.id != "imported");
            if (!string.IsNullOrEmpty(Lessons.Category) && Lessons.Category != "imported") topics = topics.FindAll(c => c.id == Lessons.Category);
            List<VocabEntry> inTopic = null;
            CategoryEntry topic = null;
            for (int tries = 0; tries < 20 && (inTopic == null || inTopic.Count < 3); tries++)
            {
                topic = topics[Random.Range(0, topics.Count)];
                inTopic = pack.words.FindAll(w => w.category == topic.id && w.tier <= Mathf.Max(2, Lessons.Level));
                if (inTopic.Count < 3) topics = pack.categories.FindAll(c => c.id != "imported");
            }
            var others = pack.words.FindAll(w => w.category != topic.id && w.tier <= Mathf.Max(2, Lessons.Level));
            Shuffle(inTopic);
            intruder = others[Random.Range(0, others.Count)];
            var four = new List<VocabEntry> { inTopic[0], inTopic[1], inTopic[2], intruder };
            Shuffle(four);
            for (int i = 0; i < 4; i++)
            {
                shown[i] = four[i];
                options[i].gameObject.SetActive(true);
                options[i].interactable = true;
                options[i].GetComponent<Image>().color = UIFactory.Teal;
                optionText[i].Set(four[i].word, four[i].english);
            }
            question.Set("¿Cuál no es de: " + topic.name.ToLower() + "?", "Which one is not: " + topic.english.ToLower() + "?");
            result.Set("", "");
            score.text = "Ronda " + round + " / " + Rounds + "     Aciertos: " + right;
            translate.Set(translate.English);
        }

        void Choose(int i)
        {
            if (nextTimer >= 0) return;
            bool correct = shown[i] == intruder;
            if (correct) right++;
            for (int k = 0; k < 4; k++)
            {
                options[k].interactable = false;
                if (shown[k] == intruder) options[k].GetComponent<Image>().color = UIFactory.Green;
                else if (k == i) options[k].GetComponent<Image>().color = UIFactory.Red;
            }
            result.Set((correct ? "¡Muy bien! " : "¡Casi! ") + GameState.Capital(intruder.word) + " = " + intruder.english,
                (correct ? "Well done! " : "Almost! ") + GameState.Capital(intruder.word) + " = " + intruder.english);
            nextTimer = 1.6f;
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!Open || nextTimer < 0) return;
            nextTimer -= Time.deltaTime;
            if (nextTimer < 0) NextRound();
        }
    }
}
