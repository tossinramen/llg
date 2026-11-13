# Runes of Speech — Planner

Your ideas, organised, with a to-do list to tackle one at a time.
**Open tasks are at the top. When a task is finished it is ~~struck through~~, marked ✅ and moved to [Done](#done-) at the bottom.**
What already exists is described in [GAME_GUIDE.md](GAME_GUIDE.md).

Contents: [To-do list](#to-do-list) · [Design notes](#design-notes) (weapon trees, armour tiers, magic,
dungeon, villains, battles, crops, music) · [Activity ideas](#activity-ideas) · [Done](#done-)

---

## To-do list

### 1. Language content and sound
- [ ] **Add Spanish voices / sound** — install the Spanish voice in Windows, record every Spanish line, check it plays everywhere
- [ ] Sort more of the imported words into topics (about 17,700 are in "Más palabras")
- [ ] Verb-forms activity (conjugation drills: the Beginner to Advanced deck has 3,695)
- [ ] Verb conjugation data (present, past, future…) for the tense activities
- [ ] Speaker buttons on more places (shop items, archery words, fishing sentences, bag)

### 2. New lesson activities
- [ ] Fill in the blank with the right tense
- [ ] Pick the correct form of the verb
- [ ] Put the sentence in the correct order (as its own lesson, not only fishing)
- [ ] Match the picture to the word (easier levels; fruits etc.)
- [ ] Choose the correct reply to a sentence (harder levels: pick lines to build your reply)
- [ ] English → Spanish translation by clicking words to complete the sentence
- [ ] Word Rain — catch falling words that match the prompt
- [ ] Rune Lock — spell the word on dials to open a chest or door
- [ ] Cooking orders — serve "dos huevos y pan" before the customer leaves
- [ ] Whack-a-word — hit moles in the farm field holding words from the topic
- [ ] Translation bridge — step only on planks with the correct translation
- [ ] Market haggling — match spoken numbers ("treinta y cinco") for discounts
- [ ] Echo game — hear a word, click the matching picture (needs Spanish voice)

### 3. Conversation, charisma and persuasion
- [ ] Talk to villagers by clicking words in order to build your reply
- [ ] Conversation skill that raises **Charisma** and **Persuasion** (new stats)
- [ ] Uses for persuasion: better prices, unlocking dialogue, quests
- [ ] Radio station (listening practice: short Spanish "broadcasts" with questions)
- [ ] Coffee shop / café (a place to practise conversations; sells drinks)

### 4. Combat, gear and progression
- [ ] Damage and XP mechanics pass (crits, attack speed per weapon type, enemy scaling)
- [ ] **Durability** on weapons and armour
- [ ] **Repair** at the blacksmith (costs gold)
- [ ] **Upgrade** weapons, armour and skills with Lexa (+1 … +10)
- [ ] Strength stat, trained by boxing (adds fist and melee damage)
- [ ] Gym (el gimnasio) with activities: boxing bag, weights, running track (each with a language twist)
- [ ] Music in battle: a song can stun a monster for a turn (needs the battle system)

### 5. Dungeons, bosses and villains
- [ ] People (later PvP) are fought by clicking in real time (monsters are already fought with words)
- [ ] Bosses also drop rare gear (they already give gold and Lexa)
- [ ] More things to spend Lexa on: weapon/armour upgrades (+1 … +10), skill boosts
- [ ] **Remove the test money and level 500** before release (`GameConfig.GiveTestMoney = false`)

### 6. Farming
- [ ] More garden plots (buy extra land with gold), and use the big farm field too

### 7. Quests and story
- [ ] Quest system (quest text in English); quest log window
- [ ] First quests from the villagers (fetch, deliver, defeat wolves, learn a topic)
- [ ] A story / plot at the beginning (see notes)

### 8. Later
- [ ] PvP with other players using upgraded gear (multiplayer — last, after single player is finished)
- [ ] Japanese and Korean packs

---

## Design notes

### Where to get more Spanish words (resource)
- **Anki** shared decks are good to *study from* and to see what a good list looks like, but most have no
  clear licence, so they shouldn't be copied into the game.
- Better (free, licensed for reuse; credit them in the game):
  - **Frequency lists** (e.g. "hermitdave FrequencyWords" for Spanish) — the most common words in order, to set levels.
  - **Wiktionary data** via kaikki.org — meanings, gender, and full **verb conjugations** (needed for the tense games).
  - **Tatoeba** — tens of thousands of Spanish–English sentence pairs, for sentence order, translation and replies.
- Plan: a script that combines these into `pack.json` automatically, then hand-check levels 1–2.

### Weapon trees — ✅ DONE (all three are in the Armería, see GAME_GUIDE.md)

**Melee (blades)**

| Lv | Weapon | Notes |
|---|---|---|
| 1 | Puños (fists) | Starting weapon; boxing trains Strength |
| 5 | El palo (stick) | First weapon |
| 10 | El cuchillo de cocina (kitchen knife) | |
| 15 | La daga (dagger) | Fast |
| 20 | La espada corta (short sword) | |
| 25 | La espada (sword) | |
| 30 | El sable (sabre) | Curved, fast |
| 35 | El estoque (rapier) | High crits |
| 40 | La espada toledana (Toledo sword) | Toledo steel, famous in Spain |
| 45 | El mandoble (greatsword) | Slow, big damage |
| 50 | La Tizona | Legendary sword of El Cid |
| 60 | La Colada | El Cid's second legendary sword |

**Axes** (also chop trees): hacha de mano lv 10 → hacha lv 20 → hacha de guerra lv 35 → hacha doble (double axe) lv 50.

**Ranged** (your order: bows → crossbows → stronger bows → medieval pistols → real guns)

| Lv | Weapon |
|---|---|
| 5 | La honda (sling) |
| 10 | El arco corto (short bow) |
| 15 | El arco (bow) |
| 25 | El arco largo (longbow) |
| 30 | La ballesta (crossbow) |
| 40 | La ballesta pesada (heavy crossbow) |
| 50 | El arcabuz (arquebus — medieval gun) |
| 60 | La pistola de chispa (flintlock pistol) |
| 70 | El mosquete (musket) |
| 80 | El revólver (revolver) |
| 90 | El rifle |

### Armour tiers — ✅ DONE (sold in the Herrería; upgrades, durability and repairs are still open tasks)
Tela (cloth) → Cuero (leather) → Cota de malla (chainmail) → Hierro (iron) → Acero de Toledo (Toledo steel) → Armadura rúnica (rune armour, glows blue).
Each tier has head, chest, legs, wrists and a shield. Upgrades +1…+10 cost Lexa; durability drops with hits; the blacksmith repairs.

### Enchantments (encantamientos) — ✅ DONE (La Torre Rúnica, el altar rúnico, Doña Runa)
Afilado (sharpness), Fuego (fire), Hielo (ice, slows), Protección, Vampiro (steal HP), Suerte (more Lexa drops).
One enchantment per item, ranks I–III, paid in Lexa (100 / 400 / 1k).

### Magic (level 25+) — ✅ DONE (K opens the spellbook; learn and upgrade spells with Lexa)
Spells are cast by **building the Spanish word letter by letter** — a wrong letter fizzles.

| Lv | Spell | Effect |
|---|---|---|
| 25 | Chispa (spark) | Small fire damage |
| 30 | Escudo de luz (light shield) | Blocks the next hit |
| 35 | Rayo (lightning) | Big single-target damage |
| 40 | Hielo (ice) | Freezes an enemy for a turn |
| 45 | Curación (heal) | Restores HP |
| 55 | Terremoto (earthquake) | Hits all enemies |
| 70 | Meteoro (meteor) | Huge damage, long cooldown |

### The dungeon: La Ciudadela del Sol (the Citadel of the Sun) — ✅ DONE (in the forest, end of the path)
Inspired by **Machu Picchu**: stone terraces climbing a green mountain, with each terrace a floor. You climb
up to fight the next boss, **one boss every 5 levels**. Each floor tests a topic, **with no hints or translations**.

| Floor | Level | Boss (villain) | Tests |
|---|---|---|---|
| 1 | 5 | El Guardián de Piedra (Stone Guardian) | Basic words |
| 2 | 10 | La Reina Araña (Spider Queen) | Food, animals |
| 3 | 15 | **El Rey Trol (Troll King)** | Ser vs estar |
| 4 | 20 | El Brujo de la Niebla (Fog Sorcerer) | Questions, replies |
| 5 | 25 | **El Rey de Hielo (Ice King)** | Past tense ("frozen in the past") |
| 6 | 30 | La Serpiente Emplumada (Feathered Serpent) | Future tense |
| 7 | 35 | El Gigante de Lava (Lava Giant) | Long sentences |
| 8 | 40 | La Bruja del Espejo (Mirror Witch) | Translation both ways |
| 9 | 45 | El Caballero Sin Nombre (Nameless Knight) | Everything so far |
| 10 | 50 | **El Rey Demonio (Demon King)** | Final test |

**Battles (Pokémon Showdown-style):** turn-based. Each turn you pick a move (attack, spell, item, defend),
and its power depends on answering a language question correctly and fast. Bosses have phases.
Monsters are fought this way, with words; people (later PvP) are fought by clicking, in real time.
Bosses drop Lexa and gear.

### Gold and Lexa — ✅ DONE
- **Gold (oro)** buys everything in the shops, pets included. Activities, animals, selling and bosses pay gold.
- **Lexa** (grey stones with blue rune carvings) is for magic: learning and levelling up spells, and enchantments.
  Activities give a little Lexa on top of the gold; bosses give a lot. Suerte runes give more.
- Big numbers are shortened: 1.5k, 20k, 1m.

### Story idea (to pick from)
You arrive in Valdeluz knowing no Spanish. The **Rey Demonio** has stolen the village's words, sealing
them into runes inside the Ciudadela del Sol. Without words, the fountain is fading. Sabia teaches you
that every word you learn re-carves a rune. Your goal is to climb the citadel, beat the kings and
bring the language back. (Quest text in English, everything else in Spanish.)

### Crops — ✅ DONE (seeds at the Semillería, grow them in your patch)

| Crop | Seed price | Sells for | Grows in |
|---|---|---|---|
| El trigo (wheat) | 5 | 9 | short |
| La lechuga (lettuce) | 8 | 15 | short |
| El tomate | 12 | 24 | medium |
| La zanahoria | 15 | 30 | medium |
| La fresa | 25 | 55 | long |
| La uva (grape) | 40 | 90 | long |
| La aceituna (olive) | 60 | 140 | very long |

The watering screen asks a quick language question per plot; correct answers make that plot grow faster.

### Music — ✅ DONE (school, store, instruments, skill, verse game; battle stun is still an open task)
Music school (escuela de música) and music store (tienda de música). Instruments: la guitarra, la flauta,
el clarinete, la pandereta. Music skill. Playing = clicking words in order to build a verse.
In battle, a song can stun a monster for a turn.

---

## Activity ideas
From the brainstorm, each also listed as a to-do in section 2:
1. **Word Rain (Lluvia de palabras)** — words fall; catch only the ones that match ("catch the fruits").
2. **Rune Lock** — spin dials to spell the word; opens chests and doors.
3. **Cooking orders (Pedidos)** — a customer orders "dos huevos y pan"; serve it before they leave.
4. **Wolf duel (Duelo)** — click the correct verb form to land each hit.
5. **Whack-a-word (Topos)** — moles pop up with words; hit the ones in the topic.
6. **Translation bridge** — cross on planks with the correct translation.
7. **Market haggling (Regateo)** — match spoken numbers for discounts.
8. **Echo game** — hear a word, pick the picture.

---

## Done ✅

Finished tasks, struck through.

- ~~Speaker buttons to hear sentences, in Spanish and English~~ ✅ (English recorded; Spanish needs the task below)
- ~~Lessons by topic with levels you can choose (Principiante → Experto); harder gives more XP~~ ✅
- ~~XP bar at the bottom; uncapped levels; different activities give different XP~~ ✅
- ~~Farm next to your house~~ ✅ (to look at; planting is a separate task below)
- ~~Cats and dogs walking around~~ ✅
- ~~Go inside buildings; talk to the shopkeeper or click the menu board to shop~~ ✅
- ~~Many more items in every shop~~ ✅
- ~~WoW-style unit frame: portrait, HP with hit animation, mana, hunger, Lexa beside it~~ ✅
- ~~Wolves / wild dogs that fight back when punched, with HP bars; you can kill them~~ ✅
- ~~Fist cursor over things you can attack~~ ✅
- ~~Much bigger world with typical village things~~ ✅
- ~~Only animals can be attacked, not people~~ ✅
- ~~Archery: an arrow flies into the target you click~~ ✅
- ~~Brainstorm more archery-style activities~~ ✅ (see [Activity ideas](#activity-ideas))
- ~~Weapon shop with swords, axes, bows and knives~~ ✅
- ~~Zoom with the mouse wheel~~ ✅
- ~~Move the X on windows slightly left~~ ✅
- ~~Cut trees (cracks animation, axes faster, regrow, wood to pick up and sell)~~ ✅
- ~~"Remove the word that doesn't fit the category" game~~ ✅ (El intruso)
- ~~Armoury and blacksmith buildings~~ ✅ (enchant / repair are tasks below)
- ~~Smaller unit frame; name in its own box with space before the HP bar; hunger bar inside the frame~~ ✅
- ~~Lexa moved below the portrait in a smaller box that fits the number~~ ✅
- ~~Big map (M key or click the minimap) with every building and area labelled~~ ✅
- ~~Smaller fist (punch and cursor)~~ ✅
- ~~Smaller nose on the side view~~ ✅
- ~~Blade tree in the Armería, locked by level, expensive (palo lv 5 … la Colada lv 60)~~ ✅
- ~~Axe tree in the Armería (hacha de mano lv 10, hacha lv 20, hacha de guerra lv 35, hacha doble lv 50)~~ ✅
- ~~Ranged tree in the Armería (honda lv 5 … arco … ballesta … arcabuz … pistola … rifle lv 90); guns fire bullets~~ ✅
- ~~Text never spills out of any box (every label shrinks to fit; buttons have inner padding)~~ ✅
- ~~"Your progress is saved…" fits the menu when translated~~ ✅
- ~~Lexa box under the portrait fits the number~~ ✅
- ~~Map names never overlap, sit on plates, and hovering one shows its translation~~ ✅
- ~~Icons have a margin inside their slots and cards~~ ✅
- ~~Shop cards: names, prices and levels inside the cards; no overlap with the tabs; space above the bottom border~~ ✅
- ~~Music school (Profesora Isabel, piano) and music store (Marta) with 6 instruments~~ ✅
- ~~Music skill and the verse game (click words in order; each word plays a note)~~ ✅
- ~~Seed shop (Semillería, Don Ramón) with 7 crops at different prices~~ ✅
- ~~Vegetable patch: plant, water (language question = grows faster), harvest, sell~~ ✅
- ~~Farming skill used~~ ✅
- ~~Armour tiers in the Herrería: tela, cuero, malla, hierro, Toledo, rúnico (5 pieces each, level-locked)~~ ✅
- ~~Dialogue box redesign: name plate, text with room to breathe, buttons in a strip on the right, key hint inside the box~~ ✅
- ~~More space between the shop tabs and the item cards~~ ✅
- ~~Punch facing away only shows the arm reaching past the shoulder~~ ✅
- ~~Smaller signs when you click simple things (trees, roads...)~~ ✅
- ~~Options on the title screen are in English~~ ✅
- ~~Translate and speaker buttons: icons padded inside the button~~ ✅
- ~~Bag slots: icons and stack numbers padded~~ ✅
- ~~Lexa in a box under the portrait and in the bag~~ ✅
- ~~New Lexa icon: grey rune stone with glowing blue carvings (used everywhere)~~ ✅
- ~~The dungeon: La Ciudadela del Sol (Machu Picchu-style citadel) at the end of the forest path~~ ✅
- ~~10 boss floors, one every 5 levels, each needs the level and the floor below~~ ✅
- ~~Villains / bosses: Guardián de Piedra, Reina Araña, Rey Trol, Brujo de la Niebla, Rey de Hielo, Serpiente Emplumada, Gigante de Lava, Bruja del Espejo, Caballero Sin Nombre, Rey Demonio~~ ✅
- ~~Showdown-style battles: Atacar / Hechizo / Defender / Comer / Huir; moves work only if you answer the Spanish question~~ ✅
- ~~No hints or translations in battles~~ ✅
- ~~Bosses give Lexa and Combat XP~~ ✅
- ~~Hold the left button (a moment) to read something~~ ✅ (later switched back to a plain left click)
- ~~More space between the neighbour's house, the music shop and the bakery~~ ✅
- ~~Nicer dog and cat sprites~~ ✅
- ~~Farm and chicken-yard gates you can open and close; you can walk between the crops~~ ✅
- ~~Finished design notes are marked ✅ DONE in their headings~~ ✅
- ~~Two currencies: gold (oro) for the shops and Lexa from learning; gold box above the Lexa box, both smaller~~ ✅
- ~~Pets cost Lexa; selling and animals give gold; bosses give both; 10 Lexa → 20 gold exchange in the bag~~ ✅
- ~~Punches come from the front in every direction (above your head facing north, from the shoulder edge sideways)~~ ✅
- ~~Punching while walking north: the punch keeps its direction and the fist moves with you~~ ✅
- ~~Plain left click to read signs and labels again~~ ✅
- ~~Gold buys everything (pets too); Lexa is for spells and enchantments; activities pay gold + a little Lexa; exchange button removed~~ ✅
- ~~Money shortened to 1k / 1.5k / 1m everywhere~~ ✅
- ~~Testing: 1m gold and 1m Lexa~~ ✅
- ~~Spellbook on K: Chispa, Escudo de luz, Rayo, Hielo, Curación, Terremoto, Meteoro (level-locked, learn and upgrade to rank V with Lexa)~~ ✅
- ~~Casting = spell the word letter by letter; a wrong letter fizzles (in the world and in battle)~~ ✅
- ~~Battle "Hechizo" uses your prepared spell (★ in the spellbook)~~ ✅
- ~~La Torre Rúnica (south street, east end) with Doña Runa and el altar rúnico~~ ✅
- ~~Enchantments: Afilado, Fuego, Hielo, Protección, Vampiro, Suerte (ranks I–III, paid in Lexa)~~ ✅
- ~~Sharper gold and Lexa boxes (full-size pixel font)~~ ✅
- ~~Raised terrace in the plaza round the fountain, with stairs at both ends~~ ✅
- ~~Testing: level 500~~ ✅
- ~~Realistic park benches you can sit on with E~~ ✅
- ~~Nicer messages: coloured tags (green for wins, gold for gold, blue for Lexa, red for damage) that pop in and stack~~ ✅
- ~~Red screen-edge flash when you get hurt (and a pulse at low HP)~~ ✅
- ~~Game guide: every monster with its damage, stats gained per level, every XP source~~ ✅
- ~~+1 max mana per level (spells were impossible with 10 mana); 0.5 s safety after a bite~~ ✅
- ~~A super cute cat (chibi orange tabby)~~ ✅
- ~~Fishing shop (La tienda de pesca, Marina) by the pond: rods, bait, net, fishing hat, fresh fish; buys fish~~ ✅
- ~~Fishing is a real little game: wait, strike in time, reel the fish in word by word; the line can snap~~ ✅
- ~~17 kinds of fish from junk to legendary, pond vs river, with a collection counter~~ ✅
- ~~Fishing gear: 5 rods (tier = rarest fish), 4 baits, the net (double catch), the fishing hat (lures fish)~~ ✅
- ~~Every price in the game guide (generated from the item data)~~ ✅
- ~~Enter to chat: your words appear in a white bubble above your head~~ ✅
- ~~Left-click any animal (cats, dogs, chickens, pets, wolves) to read its name~~ ✅
- ~~XP bar moved up and redesigned (framed bar, level badge, "Nivel x · xp / need" inside)~~ ✅
- ~~XP needed for every level 1–100 in the game guide~~ ✅
- ~~The mine (La mina) with Don Ernesto, a cave with stone, ore and crystal rocks, and the spelling mining game~~ ✅
- ~~Minería skill (mining + archaeology): ores, gems, relics, 4 pickaxes, the miner's helmet~~ ✅
- ~~Plaza stairs drawn like real steps (six narrow steps, each lower, brick faces forming the diagonal)~~ ✅
- ~~Swords stab straight out (no swinging, no circle facing north); axes chop in a short arc, mirrored left/right~~ ✅
- ~~Bows and guns held out in front, bigger than the arms, pointing at the target; guns kick back with a muzzle flash; shots leave a trail~~ ✅
- ~~New double axe icon (two crescent blades)~~ ✅
- ~~Sharp map names (16 px text on bigger plates)~~ ✅
- ~~Mining with four kinds of task (spell, article, sentence gap, pick the word), more tasks per rock, perfect strikes~~ ✅
- ~~590 words in 21 topics and 171 sentences (was 152 / 33)~~ ✅
- ~~Deck importer for Anki (.apkg) and spreadsheets (.txt/.csv)~~ ✅
- ~~Much bigger word pack: 21,077 words and 48,640 sentences from 17 Anki decks, cleaned, de-duplicated and sorted into levels~~ ✅
