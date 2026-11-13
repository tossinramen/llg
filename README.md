# Runes of Speech

A single-player, 16-bit style RPG for learning languages. Spanish first; Japanese and Korean planned.

## Read these in order

- **[Docs/STORY.md](Docs/STORY.md) – the storyline (acts, characters, the quests it will become)**
- **[Docs/GAME_GUIDE.md](Docs/GAME_GUIDE.md) – everything in the game right now (buildings, people, items, weapons, skills, XP)**
- **[Docs/PLANNER.md](Docs/PLANNER.md) – ideas and the to-do list (finished tasks get struck through)**

1. [Docs/04_UNITY_SETUP.md](Docs/04_UNITY_SETUP.md) – open the project in Unity, press Play, controls
2. [Docs/02_GAME_DESIGN.md](Docs/02_GAME_DESIGN.md) – the whole game plan: stats, skills, activities, the village
3. [Docs/01_NAME_IDEAS.md](Docs/01_NAME_IDEAS.md) – game name and currency name
4. [Docs/03_LANGUAGE_DATA.md](Docs/03_LANGUAGE_DATA.md) – where vocabulary and grammar data comes from
5. [Docs/05_TODO.md](Docs/05_TODO.md) – the to-do list, in build order

## What exists so far

- A Unity 2022.3 project with organised folders
- Start screen: character creator, turnable character, language picker
- The Spanish village of Valdeluz: a cobbled plaza with a rune fountain, a church, a bakery,
  candy shop, general store, weapon shop (Armería), pet shop (Mascotas), your house, a pond and an
  archery range, with five villagers to talk to. Buildings and trees are separate sprites, so you
  walk behind them, and only their base is solid
- Minimap, profile/character sheet with equipment slots and skills, a slotted bag, pets that follow you
- Lexa (the magical currency), shops, a backpack, HP / mana / hunger / intelligence
- Two activities: fishing (build the sentence) and archery (shoot the correct word)
- Pixel art for all of the above, drawn by the scripts in `Tools/art/`
- Starter Spanish word pack (43 words, 12 sentences)

## Controls

All keys can be changed in **Esc → Opciones (Options)**.

| Key | Action |
|---|---|
| WASD or arrow keys | Walk |
| Ctrl (hold) | Run (makes you hungry faster) |
| Shift (hold) | Crouch (move slowly) |
| Space | Jump |
| E (or Enter) | Talk, open a shop, fish, use what you are facing |
| Left click | Read: shows the Spanish name of whatever you click. Click the sign to close it |
| Right click | Attack an animal or chop a tree (the cursor turns into a fist). Axes chop fastest. On a door: go in |
| B | Bag: 20 slots. Click to eat, read, wear or call your pet |
| U | Profile: equipment slots (head, neck, back, chest, legs, wrists, both hands), stats and skills |
| L | Lessons: choose a topic and a level (Principiante to Experto); harder = more XP |
| Mouse wheel | Zoom in and out |
| Speaker button | Hear the Spanish (or, once translated, the English) read aloud |
| Globe button | On any window, sign or dialogue: show the English translation |
| Esc | Game menu: Resume, Options (keys + music volume), Exit Game (saves, back to title) |
| 1–4 | Shoot a target in archery (or click it) |
| Start screen: hold left click on the character | Drag sideways to turn them |

Progress (stats, items, skills and where you are standing) saves when you choose Exit Game,
when you close the game, and automatically every 30 seconds.

## Credits

Fonts: "Jacquard 12" (Sarah Cadigan-Fried) and "DotGothic16" (Fontworks), both under the SIL Open
Font License. Keep this credit if you release the game.

Music: "lofi_village" is original, composed by `Tools/audio/generate_music.py` for this project,
so there are no licence restrictions. Replace `Assets/Resources/Audio/lofi_village.wav` with any
track (same name) to change it.
