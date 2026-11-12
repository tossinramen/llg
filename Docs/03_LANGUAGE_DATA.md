# Where the Language Data Comes From

Short version: **use Anki decks to see what a good word list looks like, but build the game's
data from openly licensed sources**, because you will be redistributing it inside a game.

## Why not just download an Anki deck?

Anki (apps.ankiweb.net) is a flashcard app, and AnkiWeb hosts thousands of shared decks.
They are great for study, but:

- Most shared decks have **no clear licence**, or were made from copyrighted textbooks.
  Putting them in a game you distribute (especially one you sell) is risky.
- Quality varies a lot between decks.
- Decks are `.apkg` files (a zipped database), so they need converting anyway.

Use them for inspiration and for your own learning. For the game, use the sources below.

## Recommended sources

**Check each licence yourself before release** – I am listing them from memory and licences
change. "CC BY" and "CC BY-SA" both require you to credit the source in the game's credits;
"SA" also asks that the data you derive from it is shared under the same licence.

### Any language

| Source | What you get | Licence (verify) |
|---|---|---|
| **Tatoeba** (tatoeba.org → Downloads) | Hundreds of thousands of example sentences with translations. Ideal for sentence building and fishing. | CC BY 2.0 FR (some CC0) |
| **Wiktionary via kaikki.org** | Dictionary entries as ready-made JSON: meanings, gender, conjugations. | CC BY-SA |
| **Frequency lists** (search "hermitdave FrequencyWords") | The most-used words in each language, in order. Use this to assign tiers. | CC BY-SA 4.0 for the lists |
| **Wikidata lexemes** | Words with grammar info. | CC0 (no restrictions) |
| **Mozilla Common Voice** | Recorded speech, for listening activities later. | CC0 |

### Spanish

- Frequency list for tiers + Wiktionary/kaikki for gender and meanings + Tatoeba for sentences
  covers everything needed.
- The **CEFR levels** (A1, A2, B1…) and the Instituto Cervantes curriculum are a good guide for
  *what order* to teach grammar in. Use them as a guide, do not copy their text.

### Japanese (later)

- **JMdict** – the standard free Japanese–English dictionary (CC BY-SA, by EDRDG).
- **KANJIDIC** – kanji data, same project.
- **JLPT word lists by level** (N5 → N1) – community lists exist, e.g. tanos.co.uk; these map
  nicely onto tiers 1–5.

### Korean (later)

- **Korean Learners' Dictionary (krdict.korean.go.kr)** – from the National Institute of Korean
  Language, with an open licence and downloads.
- **TOPIK vocabulary lists** by level for tiers.

## How data gets into the game

The game reads one file per language: `Assets/Resources/Languages/<code>/pack.json`.

```json
{
  "language": "es",
  "words": [
    { "id": "apple", "word": "la manzana", "english": "apple",
      "category": "food", "tier": 1, "icon": "food_apple", "hunger": 10 }
  ],
  "sentences": [
    { "id": "s001", "text": "Yo como una manzana", "english": "I eat an apple", "tier": 1 }
  ]
}
```

| Field | Meaning |
|---|---|
| `id` | Same for every language ("apple"). Lets pictures and food values be shared. |
| `word` | The word in the language being learned. Spanish nouns include the article so players learn the gender with the word. |
| `category` | Which activities can use it (food, animal, place, color, number, verb…). |
| `tier` | 1 = most common … 5 = hardest. Unlocked by skill level. |
| `icon` | Picture name in `Assets/Resources/Art/Icons/` (blank if no picture yet). |
| `hunger` | Food only: hunger restored. |

The Spanish pack currently has 43 hand-written starter words and 12 sentences – enough to build
and test every activity. It is a starting set, not the real content.

## The plan for real content

1. Build activities against the small starter pack.
2. Write an import script (Python, in `Tools/`) that takes a frequency list + Wiktionary +
   Tatoeba and produces `pack.json` automatically, with tiers from word frequency.
3. Hand-check tier 1 and 2 (the first hours of play matter most).
4. Repeat the same script for Japanese and Korean with their sources.

Target sizes: about 300 words / 100 sentences for a first playable demo,
1,500–2,000 words for a full game (roughly CEFR A1–A2).
