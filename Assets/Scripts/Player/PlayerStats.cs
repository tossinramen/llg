using System;
using UnityEngine;

namespace LLG
{
    /// <summary>
    /// HP, mana, hunger and money. Not wired into gameplay yet (see Docs/05_TODO.md, phase 3).
    /// HP   - lost on mistakes in risky activities; 0 = wake up at the inn.
    /// Mana - spent on hints and spells; refilled by sleeping / reviewing words.
    /// Hunger - drains over time; food chosen by its name in the target language refills it.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        public int maxHp = 20;
        public int hp = 20;
        public int maxMana = 10;
        public int mana = 10;
        public int maxHunger = 100;
        public int hunger = 100;
        public int coins;

        public event Action Changed;

        public bool IsDown => hp <= 0;

        public void Damage(int amount) { hp = Mathf.Clamp(hp - amount, 0, maxHp); Changed?.Invoke(); }
        public void Heal(int amount) { hp = Mathf.Clamp(hp + amount, 0, maxHp); Changed?.Invoke(); }
        public void RestoreMana(int amount) { mana = Mathf.Clamp(mana + amount, 0, maxMana); Changed?.Invoke(); }
        public void Eat(int hungerRestored) { hunger = Mathf.Clamp(hunger + hungerRestored, 0, maxHunger); Changed?.Invoke(); }
        public void GetHungrier(int amount) { hunger = Mathf.Clamp(hunger - amount, 0, maxHunger); Changed?.Invoke(); }
        public void AddCoins(int amount) { coins = Mathf.Max(0, coins + amount); Changed?.Invoke(); }

        public bool TrySpendMana(int amount)
        {
            if (mana < amount) return false;
            mana -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
