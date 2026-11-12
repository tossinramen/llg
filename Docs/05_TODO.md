# To-Do List

Work top to bottom. `[x]` = done, `[ ]` = not started.

## Done so far
- [x] Unity project, folders, automatic pixel-art and pixel-font import
- [x] Name: **Runes of Speech**. Currency: **Lexa** (change either in `GameConfig.cs`)
- [x] Start screen: colourful background, pixel fonts, parchment panels, character creator,
      turnable character, Randomize, language picker, tagline cycling through languages
- [x] Village map with bakery, candy shop, store, pond, archery range, your house, rune stone
- [x] Walking (4 directions, animated), collisions, camera follow
- [x] Five villagers with Spanish + English dialogue
- [x] HUD: HP, mana, hunger, Lexa
- [x] Shops: buy food (refills hunger), fishing rods, knife, dagger, bow, sword, books (+intelligence)
- [x] Backpack window
- [x] Fishing: build the sentence against a timer; hints cost mana; mistakes cost HP
- [x] Archery: shoot the target with the correct word; 8 rounds, 3 arrows
- [x] Skill XP and level-ups for Fishing, Grammar, Archery, Vocabulary
- [x] Saving (profile, stats, skills, items)
- [x] Controls: E use, B backpack, left click reads a word, right click attacks / punches / opens doors
- [x] Equip weapons from the backpack; bow fires arrows; trees and bushes sometimes drop fruit
- [x] Start screen: drag the character to turn; language cards (Japanese/Korean locked)
- [x] Esc menu (Resume / Options / Exit Game), rebindable keys, music volume + mute
- [x] Original lofi background music, MMO-style cursor
- [x] Jump (Space), crouch (Shift), run (Ctrl); progress and position saved on exit + every 30 s
- [x] Village redrawn as a Spanish town (Valdeluz) with fake depth: side walls, cast shadows, walk-behind objects
- [x] Shaded characters (24x32) with outfits for villagers; hat and cloak you wear show on you
- [x] Translate (globe) buttons instead of English on every card
- [x] Profile (U) with 8 equipment slots + skills; 20-slot bag (B); weapon shop; pet shop with 4 pets that follow you
- [x] Minimap with shop icons, Esc and controls-page buttons above it

## Next up – make the village feel alive
- [ ] **You:** play it and tell me what feels wrong (speed, prices, difficulty, layout)
- [ ] Sound effects (footsteps, button clicks, correct/wrong) + an SFX volume slider
- [ ] Shop interiors with a shopkeeper character instead of a window opening at the door
- [ ] Shopkeepers ask for things by name ("Dame el pan") – the "choose the food" activity
- [ ] Villagers walk around; more lines of dialogue; a first small quest each
- [ ] Skills screen showing every skill, level and XP bar
- [ ] "Continue" on the start screen when a save exists; a way to start a new game
- [ ] Items you can see: hold the rod while fishing, show the weapon you own
- [ ] Animated water and a fishing bobber
- [ ] Walking behind trees and roofs (right now solid things simply block you)

## Spending Lexa (your list)
- [ ] Pets that follow you
- [ ] Clothing shop (new shirts, hats) using the character layers
- [ ] House interior and upgrades
- [ ] Sell fish for Lexa / cook them

## Learning engine
- [ ] Per-word mastery tracking (which words you keep getting wrong)
- [ ] Word selection that favours weak and not-recently-seen words
- [ ] Inn or bed: sleep + review words, refills HP and mana (the rune stone refill is a stand-in)
- [ ] Harder words and shorter timers as skills level up (tiers exist; needs more words)
- [ ] Introduce new words gently: show meaning the first time, quiz after

## Real language data
- [ ] Import script: frequency list + Wiktionary + Tatoeba → `pack.json`
- [ ] 300 Spanish words / 100 sentences, tiers 1–2 hand-checked
- [ ] Pictures for more words (archery and picture-matching are better with icons)
- [ ] Move item names from `ItemDatabase.cs` into the language pack
- [ ] Credits screen listing data sources and font licences

## More activities
- [ ] Picture match jobs for Lexa
- [ ] Sentence-building obstacles in the world (broken bridge, locked gate)
- [ ] Market shopping with numbers
- [ ] Recipe cooking
- [ ] Farming
- [ ] Mail delivery
- [ ] Spell combat + a forest area (weapons and attack are already tracked)
- [ ] Article sorting, scribe work, notice board quests, listening games

## More languages
- [ ] Sentence tiles defined in data (Japanese and Korean don't split on spaces)
- [ ] Reading aid toggle (romaji / romanisation)
- [ ] Japanese pack (JMdict + JLPT lists), Korean pack (krdict + TOPIK lists)
- [ ] Villager dialogue moved into the language packs

## Questions for you
1. "Rotate the screen" – I made the *character* turn. Is that what you meant?
2. PC only, or phones too? (Affects controls and screen layout.)
3. Should menus stay in English, or slowly switch to the language being learned?
4. Should punching/attacking villagers do anything beyond them saying "¡Ay!"?
