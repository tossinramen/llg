# To-Do List

Work top to bottom. Each phase ends with something you can play.
`[x]` = done, `[ ]` = not started.

## Phase 0 – Project setup
- [x] Unity project folder with organised subfolders
- [x] Automatic pixel-art import settings
- [x] Starter Spanish word pack (43 words, 12 sentences)
- [x] Design docs
- [ ] **You:** open the project in Unity and press Play (see `04_UNITY_SETUP.md`)
- [ ] **You:** pick a name (`01_NAME_IDEAS.md`)
- [ ] Put the project under version control (git) so nothing can be lost

## Phase 1 – Start screen
- [x] Peaceful background with drifting clouds
- [x] Tagline cycling through supported languages
- [x] Character creator: name, gender, skin, hair style/colour, shirt, pants
- [x] Turn the character to see all four sides
- [x] Randomize button
- [x] Language picker (Spanish available, Japanese/Korean "soon")
- [x] Save the profile to disk
- [ ] Pixel font (the screen currently uses Unity's built-in smooth font)
- [ ] Pixel-art buttons and panels instead of flat colours
- [ ] Title music and button sounds
- [ ] "Continue" option when a saved game exists

## Phase 2 – Walking around
- [ ] Walking animation frames for the character (4 directions)
- [ ] Player movement on a tile grid, camera follow, pixel-perfect camera
- [ ] Tile art: grass, path, water, trees, houses
- [ ] First map: the town square
- [ ] Collisions (can't walk through houses/water)
- [ ] Start button on the title screen loads the town

## Phase 3 – Core systems on screen
- [ ] HUD: HP hearts, mana, hunger bar, coins
- [ ] Hunger drains over time
- [ ] Save/load of stats, skills and position
- [ ] Interact button + signs you can read
- [ ] Dialogue box

## Phase 4 – First activity: picture match (simplest one)
- [ ] Shopkeeper NPC starts the activity
- [ ] Match words to pictures, pays coins
- [ ] Gives Trading + Vocabulary XP, level-up message
- [ ] More item pictures (need ~30 to make it interesting)

## Phase 5 – Food and hunger
- [ ] "Choose the food" activity
- [ ] Foods restore different hunger amounts
- [ ] Bakery/market where food is bought

## Phase 6 – Sentence activities
- [ ] Reusable word-tile sentence builder
- [ ] First obstacle (the broken bridge by the river)
- [ ] Fishing with timer; HP loss on failure
- [ ] Mana hints: reveal next word, show translation, slow timer

## Phase 7 – Learning engine
- [ ] Per-word mastery tracking
- [ ] Word selection that favours weak and not-recently-seen words
- [ ] Inn: sleep + journal review, refills HP and mana
- [ ] Skill screen showing all skills and levels
- [ ] Tier unlocks by skill level

## Phase 8 – Real language data
- [ ] Import script: frequency list + Wiktionary + Tatoeba → `pack.json`
- [ ] 300 Spanish words / 100 sentences, tiers 1–2 hand-checked
- [ ] Credits screen listing data sources and licences

## Phase 9 – More activities and world
- [ ] Market shopping (numbers)
- [ ] Conversations and friendship
- [ ] Farming
- [ ] Mail delivery
- [ ] Recipe cooking
- [ ] Spell combat + forest area
- [ ] Article sorting at the mine
- [ ] Scribe work (typing)
- [ ] Notice board quests

## Phase 10 – Polish
- [ ] Music and sound effects
- [ ] Word audio + listening activities
- [ ] Settings (volume, text speed, window size)
- [ ] Tutorial for the first ten minutes
- [ ] Playtest with someone who knows no Spanish

## Phase 11 – More languages
- [ ] Font with Japanese and Korean characters
- [ ] Sentence tiles defined in data (those languages don't split on spaces)
- [ ] Reading aid toggle (romaji / romanisation)
- [ ] Japanese pack (JMdict + JLPT lists)
- [ ] Korean pack (krdict + TOPIK lists)

## Questions for you
1. "Rotate the screen" – I made the *character* turn. Is that what you meant?
2. PC only, or phones too? (Affects controls and screen layout.)
3. Should the game's menus be in English, or gradually switch to the language being learned?
4. Combat – do you want it, or should the game stay peaceful?
