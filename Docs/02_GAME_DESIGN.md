# Game Design

Single-player 16-bit style RPG where everything you do to survive and progress is done
in the language you are learning. Spanish first; Japanese and Korean later.

## 1. The idea in one paragraph

You arrive in a small town where nobody speaks English. To eat, earn money, cross rivers and
make friends you have to read, build and understand the local language. Activities are short
(10–60 seconds), each one trains a skill, and as skills level up the game gives you harder
words and longer sentences.

## 2. The core loop

1. **Explore** the town and its surroundings (top-down, tile based).
2. **Do an activity** – fishing, cooking, shopping, talking to someone.
3. **Earn** coins, items and skill XP.
4. **Spend** coins on food, tools and upgrades; eat to stay fed.
5. **Level up** → harder words, new areas, new activities.
6. **Sleep at the inn** → review the words you saw today → HP and mana refill.

## 3. Look and feel

- 16-bit SNES-era pixel art, 16×16 tiles, 16×24 characters.
- Internal resolution 320×180 (scales cleanly to 720p, 1080p, 4K).
- Small palette, dark outlines, soft outdoor colours. Calm music.
- No voice acting to start; word audio is a later feature.

## 4. Start screen (built – see `Assets/Scripts/UI/StartScreenController.cs`)

- Peaceful background with drifting clouds.
- The tagline under the title cycles through every language the game supports.
- Character creator: name, gender (♂ ♀ and non-binary symbols), 6 skin tones, 5 hair styles,
  8 hair colours, 8 shirt colours, 6 pants colours, a Randomize button.
- **Turn buttons rotate the character** so you can see the front, both sides and the back.
  (I read "rotate the screen" as rotating the character. If you meant something else – for
  example the game working in portrait and landscape on a phone – tell me.)
- Language picker: Spanish is selectable; Japanese and Korean show "soon".

## 5. Player stats

| Stat | What it does | Goes down | Goes up |
|---|---|---|---|
| **HP** | Your health. At 0 you wake up at the inn and lose a few coins. | Wrong answers in *risky* activities (combat, timed fishing, dangerous areas) | Eating, sleeping, potions |
| **Mana** | Powers **hints and spells**. | Using a hint: reveal a letter, remove two wrong answers, slow the timer, show the translation | Sleeping, the daily word review, mana potions |
| **Hunger** | Slowly drains while you play. Low hunger = XP penalty; empty = HP drains. | Time, running | Eating food (see activity 1) |
| **Verba** | The magical currency (words made solid). Buys food, rods, weapons and books now; pets, clothing and house upgrades later. | Shops, fainting | Most activities |
| **Intelligence** | Each book bought adds +1: +1 max mana and +5% skill XP. | – | Books from the store |
| **Attack** | Set by your best weapon (knife 1, dagger 2, bow 3, sword 5). Used by combat later. | – | Weapons from the store |

Mana as "hint fuel" is the important design choice: it lets a struggling player get help
without the game simply giving answers away for free.

## 6. Skills

Every activity gives XP to **one life skill and one language skill**. Levels run 1–50.
Every 10 levels in a skill unlocks the next word tier (tier 1 = most common words, tier 5 = hardest).

**Language skills**

| Skill | Trained by |
|---|---|
| Vocabulary | Recognising single words (picture match, food choice) |
| Grammar | Building sentences, choosing articles/endings |
| Reading | Signs, letters, quest boards |
| Listening | Hearing a word or sentence and acting on it |
| Spelling | Typing words yourself |
| Conversation | Choosing the right reply when talking to townspeople |

**Life skills**

| Skill | Trained by |
|---|---|
| Cooking | Cooking and eating activities |
| Fishing | Fishing |
| Farming | Planting and harvesting |
| Trading | Shopping, picture-match jobs |
| Crafting | Building and repairing things |
| Exploration | Reading signs, following directions, finding new areas |
| Magic | Combat spells |

## 7. Activities

### Your four

1. **Choose the food** *(Cooking + Vocabulary)* – You are hungry and a table shows several foods.
   The game asks for one by name ("el queso"). Pick correctly to eat it. Each food restores a
   different amount of hunger, so knowing more food words lets you eat better. Later tiers ask in
   full sentences ("Quiero la sopa caliente").
2. **Sentence building at obstacles** *(Crafting + Grammar)* – Something in the world is broken or
   blocked. Word tiles appear at the bottom of the screen; tap them in the right order to make
   the sentence that fixes it. Examples: relight a lighthouse, open a locked gate, wake a sleeping
   guard, repair a windmill, start a ferry.
3. **Fishing** *(Fishing + Grammar)* – Cast, wait for a bite, then build a sentence before the timer
   runs out. Bigger fish = longer sentence, shorter timer. Failing costs a little HP (the fish
   pulls you over).
4. **Picture match jobs** *(Trading + Vocabulary)* – A shopkeeper pays you to label stock. Match each
   word to its picture. Streaks pay bonus coins.

### New ones

5. **Market shopping** *(Trading + Listening/Reading)* – The seller states a price and quantity in the
   target language; hand over the right coins or pick the right number of items. Teaches numbers.
6. **Recipe cooking** *(Cooking + Reading)* – Read a short recipe and add ingredients in the right
   order. Result is better food than you can buy.
7. **Farming** *(Farming + Vocabulary)* – Seed packets are labelled only in the target language.
   Plant what the farmer asks for; teaches plants, colours, seasons, weather.
8. **Mail delivery** *(Exploration + Reading)* – Deliver letters by reading addresses and
   directions ("the blue house next to the river"). Teaches places and prepositions.
9. **Conversations** *(Conversation)* – Townspeople ask you things; choose the reply that makes
   sense. Friendship levels unlock gifts, quests and story.
10. **Spell combat** *(Magic + Grammar)* – Turn-based fights in the wild. Each spell is a small
    language task (conjugate the verb, pick the right article). Costs mana; mistakes cost HP.
11. **Article sorting at the mine** *(Crafting + Grammar)* – Ore carts roll past with nouns on them;
    switch each into the *el* or *la* track. (For Japanese/Korean the same activity teaches particles.)
12. **Scribe work at the library** *(Spelling)* – Type the word for a picture or copy a heard word.
    The main place to practise accents now, and kana/hangul later.
13. **Bard's songs** *(Listening)* – Hear a word and tap the matching picture. Needs audio, so it
    comes later.
14. **Notice board quests** *(Reading)* – Short written requests ("Bring three eggs to the baker").
    Understanding the note *is* the quest.
15. **Journal review at the inn** *(all language skills)* – Before sleeping, quickly review words
    you got wrong or haven't seen for a while. This refills mana and is the game's spaced
    repetition system (below).

## 8. How the game decides what to teach

- Every word has a **tier** (1–5) and a **category** (food, animal, place…).
- An activity asks for words from its category at or below the tier your skill has unlocked.
- The game keeps a **mastery score per word** (0–5). Right answer +1, wrong answer −1.
- Words with low mastery, and words you haven't seen for a while, are picked more often.
  This is the same idea Anki uses (spaced repetition) and it is what makes the learning stick.
- New words are introduced a few at a time and always shown with their meaning the first time.

## 9. Supporting more languages

- All language content lives in data files, one folder per language
  (`Assets/Resources/Languages/es/`, later `ja/`, `ko/`). No words are written in code.
- Every word has a language-independent `id` ("apple"), so pictures, food values and
  activities are shared across languages.
- Japanese and Korean need extra things planned from the start: a font that contains their
  characters, an optional reading aid (romaji / romanisation toggle), and sentence tiles that
  are split by a list in the data rather than by spaces.

## 10. The village (built)

The game starts here after the start screen. Everything is labelled in Spanish.

| Place | What happens |
|---|---|
| **Panadería** (bakery) | Buy bread, croissants, cake. Food is eaten straight away and refills hunger. |
| **Dulcería** (candy shop) | Buy candy, lollipops, cookies, chocolate. Cheap, small hunger refills. |
| **Tienda** (store) | Fishing rod, magic rod (more time when fishing), knife, dagger, bow (more time in archery), sword, books (+intelligence). |
| **Pond** | Face the water and press E to fish. Needs a rod. |
| **Archery range** | Talk to Don Felipe or face a target. Shoot the target showing the right word. |
| **Rune stone** (plaza) | Touch it to refill mana. |
| **Your house** | Placeholder for house upgrades later. |

Villagers (each speaks Spanish with the English shown underneath):

- **Abuela Rosa** – welcomes you, talks about the bakery and eating.
- **Mateo** the fisherman – explains you need a rod from the store.
- **Luna** – a child who loves candy and the colour pink.
- **Don Felipe** – the archery master; talking to him starts archery.
- **Sabia** – the rune sage; explains runes and Verba.

Rules currently in the game: hunger drops 1 every 8 seconds (at 0 you walk slowly); mana
returns 1 every 20 seconds; a wrong sentence or an escaped fish costs 1 HP; at 0 HP you faint,
wake in the plaza and lose 10% of your Verba; hints cost 2 mana.

## 11. World outline (later areas)

- **The town** – inn (sleep/review), market (shopping, picture jobs), bakery (food), library (scribe).
- **The river** – fishing, a broken bridge (first sentence-building obstacle).
- **The farm** – planting and harvesting.
- **The forest** – first combat area, gated by level.
- Later: mine, lighthouse, harbour (ships to the next language's island).
