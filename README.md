# Runes of Speech

A single-player, 16-bit style RPG for learning languages. Spanish first; Japanese and Korean planned.

## Read these in order

1. [Docs/04_UNITY_SETUP.md](Docs/04_UNITY_SETUP.md) – open the project in Unity, press Play, controls
2. [Docs/02_GAME_DESIGN.md](Docs/02_GAME_DESIGN.md) – the whole game plan: stats, skills, activities, the village
3. [Docs/01_NAME_IDEAS.md](Docs/01_NAME_IDEAS.md) – game name and currency name
4. [Docs/03_LANGUAGE_DATA.md](Docs/03_LANGUAGE_DATA.md) – where vocabulary and grammar data comes from
5. [Docs/05_TODO.md](Docs/05_TODO.md) – the to-do list, in build order

## What exists so far

- A Unity 2022.3 project with organised folders
- Start screen: character creator, turnable character, language picker
- A village you can walk around: bakery, candy shop, store, pond, archery range, your house,
  a rune stone and five villagers to talk to
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
| Right click | Attack with your equipped weapon, or punch. The bow fires arrows. On a door: open it |
| B | Backpack (equip weapons here) |
| Esc | Game menu: Resume, Options (keys + music volume), Exit Game (saves, back to title) |
| 1–4 | Shoot a target in archery (or click it) |
| Start screen: hold left click on the character | Drag sideways to turn them |

Progress (stats, items, skills and where you are standing) saves when you choose Exit Game,
when you close the game, and automatically every 30 seconds.

## Credits

Fonts: "Press Start 2P" (CodeMan38) and "DotGothic16" (Fontworks), both under the SIL Open Font
License. Keep this credit if you release the game.

Music: "lofi_village" is original, composed by `Tools/audio/generate_music.py` for this project,
so there are no licence restrictions. Replace `Assets/Resources/Audio/lofi_village.wav` with any
track (same name) to change it.
