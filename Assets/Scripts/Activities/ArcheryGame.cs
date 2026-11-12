using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LLG
{
    /// <summary>
    /// Archery: an English word is shown and four moving targets each carry a word in the language
    /// being learned. Click (or press 1-4) to shoot the right one before the time runs out.
    /// Trains Archery + Vocabulary. Owning a bow gives more time per shot.
    /// </summary>
    public class ArcheryGame : MonoBehaviour
    {
        const int Rounds = 8, HintCost = 2;

        GameObject window, range;
        Text prompt, info, summary;
        Image promptIcon, timerBar;
        Button hintButton;
        readonly RectTransform[] targets = new RectTransform[4];
        readonly Text[] labels = new Text[4];
        readonly VocabEntry[] choices = new VocabEntry[4];
        List<VocabEntry> words;
        VocabEntry answer;
        int round, hits, arrows;
        float timeLeft, roundTime;
        bool playing;
        RectTransform arrow;
        int aimed;
        float flight = -1;
        VillageController village;

        public bool Open => window != null && window.activeSelf;

        public void Build(Transform canvas, VillageController owner)
        {
            village = owner;
            window = UIFactory.Panel("Archery", canvas, new Vector2(0, -8), new Vector2(600, 316)).gameObject;
            var w = window.transform;
            UIFactory.TranslateButton(w, new Vector2(-270, 134));
            UIFactory.LLabel("Title", w, new Vector2(0, 134), new Vector2(460, 28), "Tiro con arco", "Archery", UIFactory.Purple, TextAnchor.MiddleCenter, 24, true);
            prompt = UIFactory.LLabel("Prompt", w, new Vector2(0, 106), new Vector2(560, 22), "", "", UIFactory.Ink);
            promptIcon = UIFactory.Img("PromptIcon", w, new Vector2(-250, 112), new Vector2(32, 32), Color.white);
            UIFactory.Box("TimerBack", w, new Vector2(0, 84), new Vector2(404, 12), UIFactory.Faded);
            timerBar = UIFactory.Img("Timer", w, new Vector2(0, 84), new Vector2(400, 8), UIFactory.Green);

            range = UIFactory.Rect("Range", w, Vector2.zero, Vector2.zero).gameObject;
            var targetSprite = UIFactory.LoadSprite("Art/UI/target");
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                var button = UIFactory.Btn("Target" + i, range.transform, new Vector2(-207 + i * 138, 10), new Vector2(64, 64), "", Color.white, () => Shoot(index));
                var image = button.GetComponent<Image>();
                image.sprite = targetSprite;
                image.type = Image.Type.Simple;
                targets[i] = image.rectTransform;
                var tag = UIFactory.Box("Tag", button.transform, new Vector2(0, -48), new Vector2(128, 24), new Color32(254, 244, 214, 255));
                labels[i] = UIFactory.Label("Word", tag.transform, new Vector2(0, 1), new Vector2(124, 22), "", UIFactory.Ink);
                UIFactory.Label("Key", button.transform, new Vector2(0, 42), new Vector2(20, 16), (i + 1).ToString(), UIFactory.Faded);
            }

            summary = UIFactory.Label("Summary", w, new Vector2(0, 10), new Vector2(540, 120), "", UIFactory.Ink);
            info = UIFactory.Label("Info", w, new Vector2(0, -98), new Vector2(560, 20), "", UIFactory.Ink);
            hintButton = UIFactory.Btn("Hint", w, new Vector2(-110, -130), new Vector2(200, 26), "Pista (2 maná)", UIFactory.Blue, Hint);
            UIFactory.Btn("Leave", w, new Vector2(120, -130), new Vector2(160, 26), "Salir", UIFactory.Red, Close);
            window.SetActive(false);
        }

        public void Begin()
        {
            words = Lessons.Words();
            if (words.Count < 4)
            {
                GameState.Say("Not enough words in the language pack for archery.");
                return;
            }
            var weapon = GameState.EquippedWeapon();
            roundTime = weapon != null && weapon.ranged ? 14f : 10f;     // a bow in hand gives more time
            round = 0;
            hits = 0;
            arrows = 3;
            playing = true;
            window.SetActive(true);
            range.SetActive(true);
            hintButton.gameObject.SetActive(true);
            summary.text = "";
            NextRound();
        }

        void NextRound()
        {
            if (round >= Rounds || arrows <= 0)
            {
                Finish();
                return;
            }
            round++;
            var pool = new List<VocabEntry>(words);
            for (int i = 0; i < 4; i++)
            {
                int pick = Random.Range(0, pool.Count);
                choices[i] = pool[pick];
                pool.RemoveAt(pick);
                labels[i].text = choices[i].word;
                targets[i].gameObject.SetActive(true);
            }
            answer = choices[Random.Range(0, 4)];
            prompt.GetComponent<LocalizedText>().Set("Dispara a:  \"" + answer.english + "\"", "Shoot the Spanish word for:  \"" + answer.english + "\"");
            var icon = string.IsNullOrEmpty(answer.icon) ? null : UIFactory.LoadSprite("Art/Icons/" + answer.icon);
            promptIcon.sprite = icon;
            promptIcon.enabled = icon != null;
            timeLeft = roundTime;
            RefreshInfo();
        }

        void RefreshInfo()
        {
            info.text = "Ronda " + round + "/" + Rounds + "     Aciertos: " + hits + "     Flechas: " + arrows;
        }

        /// <summary>Fires an arrow at a target; the result is shown when it lands.</summary>
        void Shoot(int index)
        {
            if (!playing || flight >= 0 || !targets[index].gameObject.activeSelf) return;
            aimed = index;
            flight = 0;
            if (arrow == null)
            {
                arrow = UIFactory.Img("Arrow", range.transform, Vector2.zero, new Vector2(32, 32), Color.white,
                    UIFactory.LoadSprite("Art/Icons/item_arrow")).rectTransform;
            }
            arrow.gameObject.SetActive(true);
            arrow.SetAsLastSibling();
        }

        /// <summary>Moves the arrow from the bow (bottom of the window) to the target, then scores it.</summary>
        void FlyArrow()
        {
            const float flyTime = 0.22f, stickTime = 0.35f;
            flight += Time.deltaTime;
            var from = new Vector2(0, -96);
            var to = targets[aimed].anchoredPosition;
            float t = Mathf.Clamp01(flight / flyTime);
            var pos = Vector2.Lerp(from, to, t) + new Vector2(0, Mathf.Sin(t * Mathf.PI) * 24);   // a slight arc
            var dir = to - from;
            arrow.anchoredPosition = pos;
            arrow.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            if (flight >= flyTime + stickTime)
            {
                flight = -1;
                arrow.gameObject.SetActive(false);
                Score(aimed);
            }
        }

        void Score(int index)
        {
            if (choices[index] == answer)
            {
                hits++;
                GameState.Say("¡Bien!  " + answer.word + " = " + answer.english);
            }
            else
            {
                arrows--;
                GameState.Say("No...  " + answer.english + " = " + answer.word);
            }
            NextRound();
        }

        void Hint()
        {
            if (!playing || flight >= 0) return;
            var wrong = new List<int>();
            for (int i = 0; i < 4; i++)
                if (choices[i] != answer && targets[i].gameObject.activeSelf) wrong.Add(i);
            if (wrong.Count <= 1) return;
            if (!GameState.Data.stats.TrySpendMana(HintCost))
            {
                GameState.Say("No tienes maná. (Not enough mana.)");
                return;
            }
            targets[wrong[Random.Range(0, wrong.Count)]].gameObject.SetActive(false);
        }

        void Finish()
        {
            playing = false;
            range.SetActive(false);
            hintButton.gameObject.SetActive(false);
            promptIcon.enabled = false;
            prompt.GetComponent<LocalizedText>().Set("", "");
            int reward = hits * 4;
            summary.text = (hits >= Rounds / 2 ? "¡Buen trabajo!" : "¡Sigue practicando!")
                + "\n\nAciertos: " + hits + " / " + Rounds
                + "\n+" + reward + " " + GameConfig.CurrencyName;
            info.text = "";
            if (hits > 0)
            {
                GameState.AddCurrency(reward);
                GameState.AddXp(SkillType.Archery, Lessons.Xp(hits * 5));
                GameState.AddXp(SkillType.Vocabulary, Lessons.Xp(hits * 5));
            }
            GameState.Save();
        }

        public void Close()
        {
            if (!Open) return;
            playing = false;
            flight = -1;
            if (arrow != null) arrow.gameObject.SetActive(false);
            window.SetActive(false);
            village.NoteWindowClosed();
        }

        void Update()
        {
            if (!Open) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            if (!playing) return;

            for (int i = 0; i < 4; i++)
            {
                // targets bob up and down, each at its own pace
                var pos = targets[i].anchoredPosition;
                pos.y = 10 + Mathf.Sin(Time.time * (1.6f + i * 0.35f) + i * 1.7f) * 22;
                targets[i].anchoredPosition = pos;
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { Shoot(i); return; }
            }
            if (flight >= 0) { FlyArrow(); return; }      // the clock pauses while the arrow flies

            timeLeft -= Time.deltaTime;
            timerBar.rectTransform.sizeDelta = new Vector2(400 * Mathf.Clamp01(timeLeft / roundTime), 8);
            if (timeLeft <= 0)
            {
                arrows--;
                GameState.Say("¡Tiempo!  " + answer.english + " = " + answer.word);
                NextRound();
            }
        }
    }
}
