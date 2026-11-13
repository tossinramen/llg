# Runes of Speech — Game Guide (what exists right now)

A reference of everything currently in the game. Updated: 12 Nov 2026.
For ideas and the to-do list, see [PLANNER.md](PLANNER.md).

Contents: [Basics](#basics) · [Controls](#controls) · [Stats & XP](#stats--xp) · [Skills](#skills) ·
[Lessons & language](#lessons--language) · [Activities](#activities) · [World & places](#world--places) ·
[Buildings](#buildings) · [People](#people-npcs) · [Animals & monsters](#animals--monsters) ·
[Items](#items) · [Weapons](#weapons) · [Armour & gear](#armour--gear) · [Not built yet](#not-built-yet)

---

## Basics

| | |
|---|---|
| Name | **Runes of Speech** |
| Money | **Oro (gold)** buys everything (start 80); **Lexa** is for spells and enchantments (start 20). Shown short: 1.5k, 1m. *Testing: every load tops both up to 1m and puts you at level 500 (`GameConfig.GiveTestMoney`, `TestLevel`).* |
| Languages | Spanish (playable); Japanese and Korean shown as "coming soon" |
| Town | **Valdeluz**, a Spanish village |
| Style | 16-bit pixel art with fake depth: buildings have side walls and shadows, and you walk behind things |
| Translation | Everything is in Spanish; a **globe button** on each window, sign and dialogue shows the English |
| Sound | Original lofi music; **speaker buttons** read lines aloud (English recorded; Spanish needs a voice — see planner) |
| Saving | On Exit Game, on closing the game, and every 30 seconds (stats, items, skills, position) |

## Controls

All keys can be changed in **Esc → Opciones**.

| Key | Does |
|---|---|
| WASD / arrows | Walk |
| Ctrl (hold) | Run (hunger drops twice as fast) |
| Shift (hold) | Crouch (slow) |
| Space | Jump |
| E | Use: talk, go through doors, fish, shop, sleep, sit on a bench (E or any move key to get up) |
| Enter | Chat: type a message, Enter again to say it; it floats above your head in a white speech bubble (Esc cancels) |
| B | Bag (20 slots) |
| U | Profile: equipment, stats, skills |
| L | Lessons: choose topic and level |
| M | Big map (or click the minimap) |
| K | Spellbook: learn, upgrade, prepare (★) and cast (▶) spells |
| Left click | Read the Spanish name of anything (click the sign to close it); a shop's menu board opens the shop |
| Right click | Attack animals / chop trees (cursor becomes a fist); on a door: go in |
| Mouse wheel | Zoom in / out |
| Esc | Menu: Resume, Options (keys, music volume), Exit Game |
| 1–4 | Shoot targets in archery |

## Stats & XP

| Stat | Details |
|---|---|
| **HP** | Starts at 20; **+2 max HP per character level**. Lost to bites, boss attacks and fishing mistakes. After a bite you can't be hurt again for 0.5 s (so a pack can't bite all at once). The screen edges flash red when you're hit and pulse red below 25% HP. At 0 you faint, wake in the plaza and lose 10% of your gold. Bed, level-ups, Curación and food restore it; sitting on a bench gives 1 HP. |
| **Mana** | 10 **+1 per character level** + intelligence + jewellery bonuses. Spent on hints (2 each) and spells (3–12). Refills 1 every 20 s, fully at the rune fountain, in bed and on level-up. |
| **Hunger** | 0–100; drops 1 every 8 s (twice as fast when running). At 0 you walk slowly. Eat food from the bag. |
| **Intelligence** | +1 per book read: +1 max mana and +5% XP. |
| **Attack** | The damage of your weapon (fists = 1). |
| **Defence** | Sum of your armour; each 2 points blocks 1 damage per bite (minimum 1 damage). |
| **Oro (gold)** | Everyday money. Earned from activities, selling things, defeating animals and bosses. Spent in every shop (pets too). Lose 10% when you faint. |
| **Lexa** | Magical money (a grey rune stone). A little from every language activity (a third of the gold prize), a lot from bosses; Suerte runes add 10% per rank. Spent on learning and levelling up spells (K) and on enchantments at La Torre Rúnica. |

**Character level:** uncapped. **"Nv"** is short for **nivel** (level), e.g. "Nv 3" on your name plate = level 3.
XP needed for the next level = 80 × level^1.4. The XP bar near the bottom shows your level in the round badge, and
"Nivel 3 · 120 / 761 XP" inside the bar. The full table for levels 1–100 is in [XP for every level](#xp-for-every-level-1100).

**Per level you gain:** +2 max HP, +1 max mana, a full heal and full mana. Level also unlocks weapons, armour, spells and dungeon floors, and adds level ÷ 2 to your battle attacks.

| Level | Max HP | Max mana (no books or jewellery) |
|---|---|---|
| 1 | 20 | 10 |
| 10 | 38 | 19 |
| 25 | 68 | 34 |
| 50 | 118 | 59 |
| 100 | 218 | 109 |
| 500 | 1,018 | 509 |
Every XP gain goes to a **skill** and to your **character level**.

**Lesson multiplier:** Principiante ×1, Básico ×1.5, Intermedio ×2, Avanzado ×3, Experto ×4 (marked "× lesson" below).
**Intelligence:** every point adds +5% to all XP.

| Source | XP |
|---|---|
| Reading a word for the first time (left click) | 2 Reading |
| Archery hit | 5 Archery + 5 Vocabulary (× lesson) |
| Fishing catch | 6 + 6 × rarity Fishing (junk 6 … legendary 36) + 2 per word Grammar (× lesson) |
| El intruso correct answer | 4 Vocabulary (× lesson) |
| Music: each verse played | 8 Music + 4 Grammar (× lesson) |
| Planting a seed | 2 Farming |
| Watering (right answer) | 3 Farming (× lesson) |
| Harvesting | 4 + seed price ÷ 3 Farming (wheat 5 … olive 24) |
| Buying something | 2 Trading |
| Tree felled | 6 Woodcutting |
| Breaking a rock in the mine | 5 + 6 × rock (stone 5, ore vein 11, crystal 17) Mining, +15 for a relic; 1 Spelling per letter (× lesson) |
| Wild dog / wolf killed | 20 / 30 Combat |
| Battle: right answer on Atacar | 3 Vocabulary |
| Battle: casting a spell | 6 Magic |
| Boss beaten | 40 × floor Combat (floor 1: 40 … floor 10: 400) |
| Casting a spell outside battle | 5 Magic |
| Learning or upgrading a spell | 20 Magic |
| Enchanting an item | 10 Magic |

## Skills

18 skills, all uncapped. XP for next skill level = 40 × level^1.6. Shown in the profile (U).

| Language skills | Life skills |
|---|---|
| Vocabulario (Vocabulary) — archery, El intruso | Cocina (Cooking) — *not used yet* |
| Gramática (Grammar) — fishing | Pesca (Fishing) — fishing |
| Lectura (Reading) — left-click reading | Agricultura (Farming) — *not used yet* |
| Comprensión (Listening) — *not used yet* | Comercio (Trading) — buying |
| Ortografía (Spelling) — mining (spelling words) | Artesanía (Crafting) — *not used yet* |
| Conversación (Conversation) — *not used yet* | Exploración (Exploration) — *not used yet* |
| | Magia (Magic) — spells and enchanting |
| | Tiro con arco (Archery) — archery |
| | Combate (Combat) — killing animals |
| | Leñador (Woodcutting) — chopping trees |
| | Música (Music) — the music game |
| | Minería (Mining) — the mine (includes the archaeology finds) |

## Lessons & language

**How much Spanish is in the game:** **21,077 words and 48,640 sentences** in 21 topics, across 5 levels,
with no repeats (repeats are spotted ignoring capitals and punctuation, also between decks).

| Level | Words | Sentences |
|---|---|---|
| 1 Principiante | 306 | 2,138 |
| 2 Básico | 979 | 2,716 |
| 3 Intermedio | 3,709 | 6,351 |
| 4 Avanzado | 8,758 | 13,901 |
| 5 Experto | 7,325 | 23,534 |

Where they come from (all as plain text in `Tools/data/`; the Anki files themselves are not kept):

| File | From | Levels |
|---|---|---|
| `build_spanish_pack.py`, `words_extra.txt` | written for the game (~590 words, ~170 sentences) | by hand |
| `imported/10000_Spanish_Vocabulary.txt` | 10000 Spanish Vocabulary (parts 1–3): words + an example sentence each | by frequency rank |
| `imported/9000_Spanish_Sentences.txt` | 9000 Spanish sentences, difficulty sorted | easiest fifth = 1 … hardest = 5 |
| `imported/Spanish_7000_Sentences.txt` | Spanish 7000 Intermediate/Advanced sentences | 3–5 |
| `imported/550_Phrasal_Verbs.txt` | 550 Phrasal Verbs (parts 1–2) | 4–5 |
| `imported/575_Advanced_Verbs.txt` | 575 Advanced Spanish Verbs (+ usage sentences) | 4–5 |
| `imported/1001_High_Intermediate.txt` | 1001 High Intermediate Words & Sentences | 3–4 |
| `imported/Advanced_Spanish.txt` | Advanced Spanish (words and phrases) | 4–5 |
| `imported/Advanced_Spanish_Examples.txt` | Advanced Spanish Examples (10,500 sentences) | by how common the root word is |
| `imported/Advanced_Spanish_Phrases.txt` | Advanced Spanish Phrases and Vocabulary | 4 |
| `imported/Advanced_Spanish_Sentences.txt` | Advanced Spanish Sentences | 4 |
| `imported/Advanced_Spanish_Vocabulary.txt` | Advanced Spanish Vocabulary | 4 |
| `imported/Advanced_Spanish_Vocab_Phrases.txt` | Advanced Spanish Vocabulary and Phrases | 4 |
| `imported/Assimil_Using_Spanish.txt` | Assimil Using Spanish (main + exercises) | 4–5 |
| `imported/Beginner_to_Advanced.txt` | Beginner to Advanced Spanish (ranked words + examples) | by frequency rank |

Cleaning: Anki formatting, sounds, bullets, notes in brackets and trailing dashes removed; only the first English
meaning kept; sentences of 2–14 words; cards the wrong way round flipped (decided per card type over the whole
deck, then checked card by card); cards whose "English" was really Spanish skipped; missing ¿ and ¡ added;
decks that wrote the English twice cut to one translation. Left out: the **FSI Basic Course** (its English doesn't
match), the **C1/C2 Idioms** deck (explanations in Spanish only), and the conjugation drills in Beginner to
Advanced (they could become a verb-forms activity later). Words whose English meaning matches a topic go into it;
the rest are in **Más palabras**. `Tools/data/convert_decks.py` re-does all of this if the decks are put back in
the project folder; run `python Tools/data/build_spanish_pack.py` after editing any list.

**Adding more words: import a flashcard deck** (Anki or a spreadsheet):

1. Get a deck: in Anki use *File → Export → Anki Deck Package (.apkg)*, or download a shared deck from
   ankiweb.net (search "Spanish"), or make a spreadsheet with Spanish in one column and English in another.
2. Run `python Tools/data/import_deck.py "path/to/deck.apkg"` (or `.txt` / `.csv`). It finds the Spanish
   and English columns itself, strips Anki's formatting and sounds, sorts short cards into words and long ones
   into sentences, spreads them over the 5 levels by their order in the deck, and uses the deck's tags as topics
   when they match (otherwise they go in "Más palabras"). Options: `--category food`, `--level 2`,
   `--spanish 1 --english 2`, `--limit 1000`.
3. Open Unity: the new words are in every lesson, the mine, fishing sentences, El intruso and the battles.
   Delete the file in `Tools/data/imported/` to take a deck out again.

If a deck uses Anki's newest format the script will say so: export it as *Notes in Plain Text (.txt)* instead.

**Good free sources** (fine to use; check the licence before publishing the game):

| Source | What it gives | Licence |
|---|---|---|
| **Tatoeba** (tatoeba.org → Downloads) | tens of thousands of Spanish–English sentence pairs | CC BY 2.0 |
| **Frequency lists** (e.g. hermitdave/FrequencyWords on GitHub) | the most common Spanish words, in order, for the levels | CC BY-SA 4.0 |
| **Wiktionary data** (kaikki.org, Spanish) | meanings, genders and full verb conjugations | CC BY-SA |
| **Anki shared decks** (ankiweb.net) | ready-made decks (e.g. "Spanish Top 5000") | varies: fine for your own copy, often not for publishing |

Spoken audio for new words is made by `Tools/audio/generate_speech.py` (needs the Windows Spanish voice).

- **Word pack:** 152 words, 33 sentences (`Tools/data/build_spanish_pack.py` → `pack.json`).
- **12 topics:** La comida (Food), Los animales, Los colores, Los números, La casa (Home), El pueblo (Town), La ropa (Clothes), El cuerpo (Body), La familia, La naturaleza, Los verbos, Las herramientas (Tools & weapons). Plus "Todos".
- **5 levels:** 1 Principiante · 2 Básico · 3 Intermedio · 4 Avanzado · 5 Experto.
- Chosen at the **lesson board** in the plaza or with **L**. If a topic has too few words at a level, easier words fill in.
- **Spanish capitalisation rule followed:** signs capitalise only the first word ("La fuente rúnica"); English uses Title Case.

## Activities

| Activity | Where | How it works | Rewards |
|---|---|---|---|
| **Tiro con arco** (Archery) | Archery range (north-east), Don Felipe | English word shown; shoot the target with the Spanish word (an arrow flies to it). 8 rounds, 3 arrows, timer. Hint (2 mana) removes a wrong target. A bow equipped gives more time. | 4 gold per hit (+⅓ in Lexa), XP |
| **La pesca** (Fishing) | Pond (south) or river; needs a rod from the fishing shop | Wait, strike when the bobber dips, then reel the fish in word by word. See [Fishing](#fishing). | 17 kinds of fish, gold + Lexa, XP |
| **El intruso** (Odd one out) | Tomás the woodcutter, or the lessons window | Four words: three from a topic, one not. Click the intruder. 6 rounds. | 3 gold per correct (+⅓ in Lexa), XP |
| **Reading** | Anywhere | Left-click anything to see its Spanish name; globe for English; speaker to hear it. | Reading XP |
| **Tree chopping** | Any tree | Right-click: cracks appear, tree shakes, breaks after 6 damage (fist 1, hatchet 2, axe/battle axe 3 per hit). Regrows in 3 s. Orange trees sometimes drop oranges. | Wood (sell 4), XP |
| **Combat** | Forest (wolves), olive grove (wild dogs) | Right-click an animal to hit it; bows shoot arrows. They fight back once hit. | Gold, wolf pelts, XP |
| **Shopping** | 6 shops (inside) | Buy tab and Sell tab. | Trading XP |
| **La música** (Music) | Music school (Isabel or the piano); needs an instrument except at the piano | A verse in Spanish is shown in English; click its words in order. Each right word plays a note on your instrument; finish and the melody plays. 3 verses. Better instruments = more time. | 5 gold per verse (+⅓ in Lexa), Music + Grammar XP |
| **El huerto** (vegetable patch) | 18 plots next to your house | E on a plot: plant (uses seeds from your bag), water (pick the Spanish word — right = grows twice as fast for 45 s), harvest when ripe (2 crops). | Crops to sell, Farming XP |
| **Egg collecting** | Chicken coop at the farm | E at the coop; one egg per minute. | Egg (food) |
| **Sleeping** | Bed in your house | Restores HP and mana, saves. | — |
| **Rune fountain** | Plaza | Refills mana. | — |

## Fishing

Buy a rod at **La tienda de pesca** (Marina, on the road down to the pond), then press E at the pond or the river.

1. **Wait** — the bobber floats. Bread crumbs and the fishing hat make fish bite sooner.
2. **Strike** — when the bobber dips and "¡Pica!" shows, click **¡TIRA!** or press **Space** within 1.4 s (1.7 s with the hat). Too slow and it steals the bait.
3. **Reel** — the fish's shadow is out in the water. Build the Spanish sentence **one word at a time**: each right word pulls it closer. A wrong word strains the line (**Sedal ●●●**); three wrong and it snaps. The timer ends the fight too. Hint (2 mana) places the next word. Rarer fish bring **longer sentences**.
4. **Catch** — the fish goes to your bag, with its rarity and value shown. New kinds are counted (**Peces: x/17**).

**Your gear**

| Gear | Effect |
|---|---|
| Rod | Its **tier** is the rarest fish it can land: caña de pescar = common, bambú = uncommon, carbono = rare, mágica = epic, dorada = legendary. Better rods also add seconds to reel. Your best rod is used automatically. |
| Bait | Choose it with the **Cebo** button (it starts on your best bait). One is used each time you hook a fish. Migas de pan: bites sooner, more common fish · Lombriz: more uncommon, less junk · Gamba: ×2 rare · Calamar: ×2.5 epic, ×3 legendary. |
| La red de pesca (net) | Own it: 25% chance to catch a second fish of the same kind. |
| El sombrero de pescador (fishing hat) | Wear it: fish bite 40% sooner, longer strike window, +30% rare/epic/legendary. |
| Fishing skill | +2% rare+ chance per level. |

**The fish** (where they live, and how often they bite before bait and gear)

| Rarity | Chance | Fish (water) |
|---|---|---|
| Junk | ~6% | la bota vieja (any) |
| Common | ~55% | la sardina (any), el boquerón (pond), la carpa (pond), la trucha (river) |
| Uncommon | ~25% | la perca (pond), la lubina (any), el salmón (river), la dorada (pond) |
| Rare | ~10% | el cangrejo (river), el bacalao (any), el atún (pond) |
| Epic | ~3.5% | la anguila (river), el pulpo (pond), el pez espada (any) |
| Legendary | ~0.8% | la carpa koi (pond), el pez rúnico (river, +25 Lexa) |

Each catch also pays 2 + (words in the sentence) gold and 1 + 2 × rarity Lexa. Sell fish in any shop (values in [Every price](#every-price)), or eat them.

## Mining (La mina)

Buy a pickaxe from **Don Ernesto** outside the mine (north meadow, up the road from the bakery), go inside
and press **E** in front of a rock.

**How it works:** every swing of the pickaxe is a small Spanish task. The first one on each rock is always
spelling; after that they're mixed at random:

| Task | What you do |
|---|---|
| **Deletrea** (spell) | An English word appears: click the letters of the Spanish word **in order** (3 decoy letters). Each right letter is a hit. |
| **¿El, la, los o las?** (article) | Choose the right article for the noun: practises gender. |
| **Completa la frase** (gap) | A Spanish sentence with one word missing (English shown): pick the missing word. |
| **Elige la palabra** (pick) | Pick the Spanish for an English word from four. |

Finish all of a rock's tasks to break it. A wrong answer blunts the pickaxe (**Pico ●●●**); three and the rock
holds. Each task has a timer. Hint (2 mana) places the next letter or removes a wrong option. **No mistakes at
all = a perfect strike** ("¡Golpe perfecto!"): double find, +2 Lexa, +5 XP. Broken rocks turn to rubble and come
back after 45 seconds.

| Rock | Words to break | Pickaxe needed | Can give |
|---|---|---|---|
| La roca (grey) | 2 tasks (short words) | el pico | stone, coal, copper (iron with an iron pickaxe) |
| La veta de mineral (orange specks) | 3 tasks | el pico de hierro | copper, iron, silver (gold nuggets with steel) |
| La roca de cristal (gems) | 4 tasks (long words) | el pico de acero | silver, emerald, ruby (sapphire and diamond with diamond) |

**Archaeology:** any rock can hide a **relic** (6%, doubled by the miner's helmet, +0.2% per Mining level):
la punta de flecha (flint arrowhead), el fósil, la moneda romana, el ánfora antigua. They're worth a lot and are
counted in **Reliquias x/4**. Mining and archaeology are one skill here, **Minería**; relics are its rare finds.

**Gear:** better pickaxes break harder rocks and give more seconds per word. **El casco de minero** (miner's helmet,
hat slot): its lamp shows the first letter of each rock and doubles the relic chance. Every Mining level gives a 1%
chance of a double find. Each rock also pays 2 + 2 per word in gold and 1 + rock level in Lexa.

## World & places

The world is 112 × 72 tiles, plus 7 interiors.

| Area | What's there |
|---|---|
| **La plaza** (town square) | Rune fountain, lesson board, two fruit stalls, benches, street lamps |
| North street | Neighbour's house, Panadería, Iglesia (church), Ayuntamiento (town hall), Dulcería |
| West street | Your house, neighbour's house, Tienda, well |
| East street | Armería, Herrería, crates, barrels |
| South street | Mascotas, Taberna (tavern), a house |
| **Farm** (by your house) | Wheat, cabbage and tomato fields, scarecrow, barn, haystacks, chicken yard and coop |
| North-west hill | Windmill (sails turn) |
| North-east | Archery range |
| South | Pond with a dock, vineyard (south-east), olive grove (south-west) |
| East | River with a wooden bridge; signpost warning about wolves |
| **El bosque** (forest) | Dense stone-pine and cypress forest with 5 clearings where wolves live |

## Buildings

| Building | Inside? | Notes |
|---|---|---|
| La plaza | — | The town square. In the middle is a **raised stone terrace** with the rune fountain on top; stairs go up at its west and east ends. Two park benches you can sit on (E). |
| Panadería (bakery) | Yes | Carmen; oven; 11 baked goods |
| Dulcería (candy shop) | Yes | Paco; 11 sweets |
| Tienda (general store) | Yes | Doña Elena; rods, book, clothes, jewellery, basic food |
| Armería (weapon shop) | Yes | Rodrigo; 15 weapons |
| Herrería (blacksmith) | Yes | Bruno; forge and anvil; 8 armour pieces |
| Tienda de mascotas (pet shop) | Yes | Lucía; animals in a pen; 6 pets |
| Semillería (seed shop) | Yes | Don Ramón; 7 kinds of seeds; buys your crops (west street, next to your house) |
| Tienda de música (music shop) | Yes | Marta; 6 instruments (north street, next to the bakery) |
| La mina (the mine) | Yes (a cave) | North meadow, up the road from the bakery. Don Ernesto outside sells pickaxes and the miner's helmet; inside are 7 rocks to mine and a cart |
| Tienda de pesca (fishing shop) | Yes | Marina; rods, bait, the net, the fishing hat and fresh fish (by the road down to the pond) |
| Escuela de música (music school) | Yes | Profesora Isabel and a piano: the music game (north street, east end) |
| La Torre Rúnica (Rune Tower) | Yes | Round stone tower with glowing rune windows (south street, east end). Doña Runa and **el altar rúnico**: enchant your weapons and armour |
| Tu casa (your house) | Yes | Bed (sleep), fireplace, table, shelf, rug |
| Iglesia (church) | No | "Closed today" |
| Ayuntamiento (town hall) | No | "Closed, come back tomorrow" |
| Taberna (tavern) | No | "Opens at night" |
| Neighbours' houses (×2) | No | "Nobody is home" |
| Granero (barn), Molino (windmill), Gallinero (chicken coop) | No | Farm buildings |

## People (NPCs)

| Name | Role | Where | What they do |
|---|---|---|---|
| Abuela Rosa | la abuela (grandmother) | North street, near the bakery | Welcomes you, tips about shops and food |
| Mateo | el pescador (fisherman) | Pond dock | Tells you to buy a fishing rod |
| Luna | la niña (girl) | Near the candy shop | Loves sweets and the village cats |
| Don Felipe | el arquero (archer) | Archery range | Starts archery |
| Sabia | la sabia (wise woman) | Plaza, by the fountain | Explains runes, Lexa and the lesson board |
| Tomás | el leñador (woodcutter) | By the bridge | Warns about wolves, starts El intruso |
| Juana | la granjera (farmer) | By the farm | Talks about crops and eggs |
| Carmen | la panadera (baker) | Inside the bakery | Bakery shop |
| Paco | el dulcero (sweet seller) | Inside the candy shop | Candy shop |
| Doña Elena | la tendera (shopkeeper) | Inside the store | General store |
| Rodrigo | el armero (weaponsmith) | Inside the Armería | Weapon shop |
| Bruno | el herrero (blacksmith) | Inside the Herrería | Armour shop |
| Lucía | la cuidadora (animal keeper) | Inside the pet shop | Pet shop |
| Don Ramón | el semillero (seed seller) | Inside the Semillería | Seed shop; farming tips |
| Marta | la vendedora de música (music seller) | Inside the music shop | Music shop |
| Profesora Isabel | la profesora de música (music teacher) | Inside the music school | Starts the music game |
| Don Ernesto | el minero (miner) | Outside the mine | Sells pickaxes and the miner's helmet; explains mining |
| Marina | la pescadera (fishmonger) | Inside the fishing shop | Fishing shop; tips about rods and bait |
| Doña Runa | la encantadora (enchantress) | Inside La Torre Rúnica | Opens the enchanting altar |

People cannot be attacked.

## Animals & monsters

Only **two kinds of animal fight**: wolves and wild dogs. Everything else is harmless.

| Creature | Where | HP | Damage per bite | Bites | Speed | Behaviour | Drops |
|---|---|---|---|---|---|---|---|
| **El lobo** (wolf) | Forest beyond the bridge: 5 wolves in the clearings | 14 | **3** | 1 every 1.2 s each | 3.3 (you walk 4.5, run 7.5) | Neutral until you hit one; the rest of the pack within 6 tiles joins in; gives up past 14 tiles from you or 18 from home | 6–12 gold, 60% wolf pelt, 30 Combat XP |
| **El perro salvaje** (wild dog) | Olive grove (south-west): 3 dogs | 9 | **2** | 1 every 1.2 s each | 3.5 | Same, its own pack | 3–8 gold, 20 Combat XP |

**How much a bite really hurts:** damage = bite − (your defence ÷ 2), never below 1. After any bite you are safe for 0.5 s.
At level 1 (20 HP, no armour) one wolf needs 7 bites (about 8 s) to knock you out, but the **whole pack of 5** joins in,
so a fight there can be over in a few seconds. That's why it can feel like one-shotting: start with the dogs,
wear armour (Herrería) and level up first. Escudo de luz blocks a bite completely.

| Your defence | Wolf bite | Dog bite |
|---|---|---|
| 0 | 3 | 2 |
| 2 | 2 | 1 |
| 4+ | 1 | 1 |

**Bosses** (La Ciudadela del Sol) hit once per turn: floor 1 hits for 4, and each floor hits 2 more (floor 10: 22), minus defence ÷ 2.
| Village cats & dogs | Streets (5) | — | — | Wander; harmless | — |
| Chickens | Chicken yard (4) | — | — | Wander and peck | — |
| Shop animals | Pet shop | — | — | Wander in the pen | — |

Killed animals return after 30 seconds. **Pets** you buy follow you (choose one in the bag).

## Every price

All prices are in **gold** (pets too). "Sells for" is what any shop pays you. Items with a level are locked
(greyed out, with a padlock) until your character reaches it. *This list is generated from the game's item
data, so it always matches what's in the shops.*

**Panadería (bakery) — Carmen**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el pan | bread | 8 | 4 | — | +20 hunger |
| la barra de pan | baguette | 10 | 5 | — | +25 hunger |
| el cruasán | croissant | 12 | 6 | — | +30 hunger |
| la magdalena | muffin | 9 | 4 | — | +18 hunger |
| los churros | churros | 7 | 3 | — | +15 hunger |
| la ensaimada | ensaimada (sweet spiral bun) | 14 | 7 | — | +28 hunger |
| la empanada | empanada (filled pastry) | 15 | 7 | — | +35 hunger |
| la tarta de manzana | apple tart | 18 | 9 | — | +40 hunger |
| la rosquilla | doughnut | 8 | 4 | — | +16 hunger |
| el bizcocho | sponge cake | 16 | 8 | — | +32 hunger |
| el pastel | cake | 20 | 10 | — | +50 hunger |

**Dulcería (candy shop) — Paco**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el caramelo | sweet | 3 | 1 | — | +5 hunger |
| la piruleta | lollipop | 5 | 2 | — | +8 hunger |
| el chocolate | chocolate | 10 | 5 | — | +18 hunger |
| la galleta | cookie | 6 | 3 | — | +12 hunger |
| el chicle | chewing gum | 2 | 1 | — | +2 hunger |
| las gominolas | gummy sweets | 4 | 2 | — | +6 hunger |
| el turrón | turrón (nougat) | 12 | 6 | — | +20 hunger |
| el algodón de azúcar | candy floss | 6 | 3 | — | +8 hunger |
| el helado | ice cream | 9 | 4 | — | +14 hunger |
| la nube | marshmallow | 4 | 2 | — | +6 hunger |
| el regaliz | liquorice | 3 | 1 | — | +5 hunger |

**Tienda (general store) — Doña Elena**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el libro | book | 60 | 30 | — | +1 intelligence (+1 mana, +5% XP) |
| el sombrero | hat | 45 | 22 | — | +1 defence |
| el collar | necklace | 90 | 45 | — | +3 mana |
| la capa | cloak | 70 | 35 | — | +1 defence |
| el chaleco | vest | 60 | 30 | — | +2 defence |
| los pantalones | trousers | 50 | 25 | — | +1 defence |
| la pulsera | bracelet | 55 | 27 | — | +2 mana |
| la manzana | apple | 5 | 2 | — | +10 hunger |
| el queso | cheese | 9 | 4 | — | +15 hunger |
| el huevo | egg | 6 | 3 | — | +12 hunger |

**Armería (weapons) — Rodrigo: Blades**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el palo | stick | 25 | 12 | 5 | 2 damage |
| el cuchillo de cocina | kitchen knife | 60 | 30 | 10 | 3 damage |
| la daga | dagger | 150 | 75 | 15 | 4 damage |
| la espada corta | short sword | 300 | 150 | 20 | 6 damage |
| la espada | sword | 550 | 275 | 25 | 8 damage |
| el sable | sabre | 900 | 450 | 30 | 10 damage |
| el estoque | rapier | 1400 | 700 | 35 | 12 damage |
| la espada toledana | Toledo sword | 2200 | 1100 | 40 | 15 damage |
| el mandoble | greatsword | 3200 | 1600 | 45 | 18 damage |
| la Tizona | Tizona (El Cid's sword) | 5000 | 2500 | 50 | 22 damage |
| la Colada | Colada (El Cid's second sword) | 8000 | 4000 | 60 | 27 damage |

**Armería (weapons) — Rodrigo: Axes**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el hacha de mano | hatchet | 120 | 60 | 10 | 4 damage, chops ×2 |
| el hacha | axe | 450 | 225 | 20 | 7 damage, chops ×3 |
| el hacha de guerra | battle axe | 1300 | 650 | 35 | 12 damage, chops ×3 |
| el hacha doble | double axe | 4000 | 2000 | 50 | 20 damage, chops ×5 |

**Armería (weapons) — Rodrigo: Ranged**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la honda | sling | 50 | 25 | 5 | 2 damage, ranged |
| el arco corto | short bow | 160 | 80 | 10 | 3 damage, ranged |
| el arco | bow | 300 | 150 | 15 | 5 damage, ranged |
| el arco largo | longbow | 700 | 350 | 25 | 8 damage, ranged |
| la ballesta | crossbow | 1100 | 550 | 30 | 11 damage, ranged |
| la ballesta pesada | heavy crossbow | 2000 | 1000 | 40 | 14 damage, ranged |
| el arcabuz | arquebus | 3500 | 1750 | 50 | 18 damage, ranged |
| la pistola de chispa | flintlock pistol | 5500 | 2750 | 60 | 22 damage, ranged |
| el mosquete | musket | 8000 | 4000 | 70 | 27 damage, ranged |
| el revólver | revolver | 12000 | 6000 | 80 | 32 damage, ranged |
| el rifle | rifle | 18000 | 9000 | 90 | 40 damage, ranged |

**Herrería (armour) — Bruno: Cloth**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la capucha de tela | Cloth helmet | 20 | 10 | 1 | +1 defence |
| la túnica de tela | Cloth chest armour | 40 | 20 | 1 | +2 defence |
| los pantalones de tela | Cloth leg armour | 30 | 15 | 1 | +2 defence |
| las muñequeras de tela | Cloth bracers | 16 | 8 | 1 | +1 defence |
| el escudo de madera | Cloth shield | 30 | 15 | 1 | +2 defence |

**Herrería (armour) — Bruno: Leather**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el gorro de cuero | Leather helmet | 60 | 30 | 5 | +2 defence |
| el chaleco de cuero | Leather chest armour | 120 | 60 | 5 | +4 defence |
| los pantalones de cuero | Leather leg armour | 90 | 45 | 5 | +3 defence |
| los brazales de cuero | Leather bracers | 48 | 24 | 5 | +1 defence |
| el escudo de cuero | Leather shield | 90 | 45 | 5 | +3 defence |

**Herrería (armour) — Bruno: Chainmail**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la capucha de malla | Chainmail helmet | 180 | 90 | 15 | +3 defence |
| la cota de malla | Chainmail chest armour | 360 | 180 | 15 | +6 defence |
| las calzas de malla | Chainmail leg armour | 270 | 135 | 15 | +4 defence |
| los brazales de malla | Chainmail bracers | 144 | 72 | 15 | +2 defence |
| el escudo reforzado | Chainmail shield | 270 | 135 | 15 | +4 defence |

**Herrería (armour) — Bruno: Iron**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el casco de hierro | Iron helmet | 450 | 225 | 25 | +5 defence |
| la coraza de hierro | Iron chest armour | 900 | 450 | 25 | +10 defence |
| las grebas de hierro | Iron leg armour | 675 | 337 | 25 | +8 defence |
| los brazales de hierro | Iron bracers | 360 | 180 | 25 | +3 defence |
| el escudo de hierro | Iron shield | 675 | 337 | 25 | +8 defence |

**Herrería (armour) — Bruno: Toledo steel**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el yelmo de Toledo | Toledo steel helmet | 1200 | 600 | 40 | +8 defence |
| la coraza de Toledo | Toledo steel chest armour | 2400 | 1200 | 40 | +16 defence |
| las grebas de Toledo | Toledo steel leg armour | 1800 | 900 | 40 | +12 defence |
| los brazales de Toledo | Toledo steel bracers | 960 | 480 | 40 | +5 defence |
| el escudo de Toledo | Toledo steel shield | 1800 | 900 | 40 | +12 defence |

**Herrería (armour) — Bruno: Rune**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el yelmo rúnico | Rune helmet | 3500 | 1750 | 60 | +12 defence |
| la coraza rúnica | Rune chest armour | 7000 | 3500 | 60 | +24 defence |
| las grebas rúnicas | Rune leg armour | 5250 | 2625 | 60 | +18 defence |
| los brazales rúnicos | Rune bracers | 2800 | 1400 | 60 | +7 defence |
| el escudo rúnico | Rune shield | 5250 | 2625 | 60 | +18 defence |

**Tienda de mascotas (pets) — Lucía**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el perro | dog | 150 | 75 | — | follows you |
| el gato | cat | 150 | 75 | — | follows you |
| el conejo | rabbit | 120 | 60 | — | follows you |
| el loro | parrot | 250 | 125 | — | follows you |
| la tortuga | turtle | 140 | 70 | — | follows you |
| el hámster | hamster | 90 | 45 | — | follows you |

**Semillería (seeds) — Don Ramón**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| las semillas de trigo | wheat seeds | 5 | 2 | — | grows in 40 s into wheat (sells 9) |
| las semillas de lechuga | lettuce seeds | 8 | 4 | — | grows in 50 s into lettuce (sells 15) |
| las semillas de tomate | tomato seeds | 12 | 6 | — | grows in 90 s into tomato (sells 24) |
| las semillas de zanahoria | carrot seeds | 15 | 7 | — | grows in 100 s into carrot (sells 30) |
| las semillas de fresa | strawberry seeds | 25 | 12 | — | grows in 180 s into strawberry (sells 55) |
| las semillas de uva | grape seeds | 40 | 20 | — | grows in 200 s into grape (sells 90) |
| las semillas de aceituna | olive seeds | 60 | 30 | — | grows in 300 s into olive (sells 140) |

**Tienda de música (instruments) — Marta**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la pandereta | tambourine | 80 | 40 | — | +2 s in the music game |
| la flauta | flute | 120 | 60 | — | +3 s in the music game |
| la guitarra | guitar | 200 | 100 | — | +4 s in the music game |
| el clarinete | clarinet | 300 | 150 | — | +5 s in the music game |
| la trompeta | trumpet | 380 | 190 | — | +6 s in the music game |
| el violín | violin | 450 | 225 | — | +7 s in the music game |

**Tienda de pesca (fishing) — Marina: Rods**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la caña de pescar | fishing rod | 30 | 15 | — | lands up to common fish |
| la caña de bambú | bamboo rod | 90 | 45 | 3 | lands up to uncommon fish, +3 s to reel |
| la caña de carbono | carbon rod | 260 | 130 | 12 | lands up to rare fish, +6 s to reel |
| la caña mágica | magic rod | 600 | 300 | 25 | lands up to epic fish, +10 s to reel |
| la caña dorada | golden rod | 1500 | 750 | 40 | lands up to legendary fish, +14 s to reel |

**Tienda de pesca (fishing) — Marina: Bait & gear**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| las migas de pan | bread crumbs | 2 | 1 | — | fish bite sooner; more common fish |
| la lombriz | worm | 4 | 2 | — | more uncommon fish, less junk |
| la gamba | shrimp | 9 | 4 | 10 | ×2 rare fish |
| el calamar | squid | 20 | 10 | 20 | ×2.5 epic, ×3 legendary fish |
| la red de pesca | fishing net | 180 | 90 | 8 | 25% chance of a second fish |
| el sombrero de pescador | fishing hat | 120 | 60 | 5 | +1 defence; fish bite 40% sooner, more rare fish |

**Tienda de pesca (fishing) — Marina: Fish**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la sardina | sardine | 10 | 5 | — | common; eat: +15 hunger |
| el boquerón | anchovy | 10 | 5 | — | common, pond; eat: +12 hunger |
| la carpa | carp | 16 | 8 | — | common, pond; eat: +22 hunger |
| la trucha | trout | 20 | 10 | — | common, river; eat: +25 hunger |
| la lubina | sea bass | 32 | 16 | — | uncommon; eat: +30 hunger |
| el salmón | salmon | 40 | 20 | — | uncommon, river; eat: +35 hunger |

**La mina (mining) — Don Ernesto, outside the mine**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| el pico | pickaxe | 40 | 20 | — | breaks stone |
| el pico de hierro | iron pickaxe | 220 | 110 | 8 | breaks ore veins, +3 s per word |
| el pico de acero | steel pickaxe | 700 | 350 | 20 | breaks crystal rocks, +6 s per word |
| el pico de diamante | diamond pickaxe | 2500 | 1250 | 40 | breaks everything, finds sapphires and diamonds, +10 s per word |
| el casco de minero | miner's helmet | 150 | 75 | 5 | +2 defence; lamp shows the first letter of each rock, ×2 relics |

**Found, caught or grown (sell them in any shop; fish marked with a price are also sold by Marina)**

| Item | English | Price | Sells for | Level | What it does |
|---|---|---|---|---|---|
| la bota vieja | old boot | — | 1 | — | junk |
| la sardina | sardine | 10 | 5 | — | common; eat: +15 hunger |
| el boquerón | anchovy | 10 | 5 | — | common, pond; eat: +12 hunger |
| la carpa | carp | 16 | 8 | — | common, pond; eat: +22 hunger |
| la trucha | trout | 20 | 10 | — | common, river; eat: +25 hunger |
| la perca | perch | 28 | 14 | — | uncommon, pond; eat: +28 hunger |
| la lubina | sea bass | 32 | 16 | — | uncommon; eat: +30 hunger |
| el salmón | salmon | 40 | 20 | — | uncommon, river; eat: +35 hunger |
| la dorada | gilt-head bream | 36 | 18 | — | uncommon, pond; eat: +32 hunger |
| el cangrejo | crab | 60 | 30 | — | rare, river; eat: +30 hunger |
| el bacalao | cod | 64 | 32 | — | rare; eat: +40 hunger |
| el atún | tuna | 90 | 45 | — | rare, pond; eat: +50 hunger |
| la anguila | eel | 140 | 70 | — | epic, river; eat: +45 hunger |
| el pulpo | octopus | 160 | 80 | — | epic, pond; eat: +55 hunger |
| el pez espada | swordfish | 180 | 90 | — | epic; eat: +60 hunger |
| la carpa koi | koi carp | 400 | 200 | — | legendary, pond; eat: +40 hunger |
| el pez rúnico | rune fish | 600 | 300 | — | legendary, river; eat: +80 hunger |
| la piedra | stone | — | 1 | — | sell it |
| el carbón | coal | — | 4 | — | sell it |
| el cobre | copper ore | — | 8 | — | sell it |
| el hierro | iron ore | — | 15 | — | sell it |
| la plata | silver | — | 30 | — | sell it |
| la pepita de oro | gold nugget | — | 60 | — | sell it |
| la esmeralda | emerald | — | 90 | — | sell it |
| el rubí | ruby | — | 120 | — | sell it |
| el zafiro | sapphire | — | 150 | — | sell it |
| el diamante | diamond | — | 300 | — | sell it |
| la punta de flecha | flint arrowhead (relic) | — | 60 | — | sell it |
| el fósil | fossil (relic) | — | 80 | — | sell it |
| la moneda romana | Roman coin (relic) | — | 120 | — | sell it |
| el ánfora antigua | ancient amphora (relic) | — | 200 | — | sell it |
| el pez | fish | — | 6 | — | +30 hunger |
| la naranja | orange | — | 2 | — | +10 hunger |
| la madera | wood | — | 4 | — | sell it |
| la piel de lobo | wolf pelt | — | 15 | — | sell it |
| el trigo | wheat | — | 9 | — | +5 hunger |
| la lechuga | lettuce | — | 15 | — | +5 hunger |
| el tomate | tomato | — | 24 | — | +6 hunger |
| la zanahoria | carrot | — | 30 | — | +7 hunger |
| la fresa | strawberry | — | 55 | — | +9 hunger |
| la uva | grape | — | 90 | — | +12 hunger |
| la aceituna | olive | — | 140 | — | +16 hunger |

## Weapons

All sold in the **Armería** (east street, next to the Herrería — go inside, talk to Rodrigo or click the
menu board). The shop has three tabs: **Espadas**, **Hachas**, **A distancia**. Each weapon needs a
**character level**; locked ones are greyed out with a padlock and show the level needed.
Damage = attack; chop = tree damage per hit (fists 1).

**Espadas (blades)**

| Lv | Weapon | Price | Damage |
|---|---|---|---|
| 1 | Puños (fists) | — | 1 |
| 5 | el palo (stick) | 25 | 2 |
| 10 | el cuchillo de cocina (kitchen knife) | 60 | 3 |
| 15 | la daga (dagger) | 150 | 4 |
| 20 | la espada corta (short sword) | 300 | 6 |
| 25 | la espada (sword) | 550 | 8 |
| 30 | el sable (sabre) | 900 | 10 |
| 35 | el estoque (rapier) | 1,400 | 12 |
| 40 | la espada toledana (Toledo sword) | 2,200 | 15 |
| 45 | el mandoble (greatsword) | 3,200 | 18 |
| 50 | la Tizona (El Cid's sword) | 5,000 | 22 |
| 60 | la Colada (El Cid's second sword) | 8,000 | 27 |

**Hachas (axes — also chop trees)**

| Lv | Weapon | Price | Damage | Chop |
|---|---|---|---|---|
| 10 | el hacha de mano (hatchet) | 120 | 4 | 2 |
| 20 | el hacha (axe) | 450 | 7 | 3 |
| 35 | el hacha de guerra (battle axe) | 1,300 | 12 | 3 |
| 50 | el hacha doble (double axe) | 4,000 | 20 | 5 |

**A distancia (ranged — 8-tile reach; bows/crossbows shoot arrows, the sling stones, guns bullets)**

| Lv | Weapon | Price | Damage |
|---|---|---|---|
| 5 | la honda (sling) | 50 | 2 |
| 10 | el arco corto (short bow) | 160 | 3 |
| 15 | el arco (bow) | 300 | 5 |
| 25 | el arco largo (longbow) | 700 | 8 |
| 30 | la ballesta (crossbow) | 1,100 | 11 |
| 40 | la ballesta pesada (heavy crossbow) | 2,000 | 14 |
| 50 | el arcabuz (arquebus) | 3,500 | 18 |
| 60 | la pistola de chispa (flintlock pistol) | 5,500 | 22 |
| 70 | el mosquete (musket) | 8,000 | 27 |
| 80 | el revólver (revolver) | 12,000 | 32 |
| 90 | el rifle | 18,000 | 40 |

There is **no durability or upgrading yet** (in the planner). Enchanting is at La Torre Rúnica.

## Armour & gear

Equip from the bag; take off by clicking the slot in the profile. Hat/helmet and cloak show on your character.

| Slot | Item | Shop | Price | Bonus |
|---|---|---|---|---|
| Cabeza (head) | el sombrero | Tienda | 45 | DEF 1 |
| | el casco (helmet) | Herrería | 90 | DEF 2 |
| Cuello (neck) | el collar | Tienda | 90 | +3 mana |
| Espalda (back) | la capa | Tienda | 70 | DEF 1 |
| Pecho (chest) | el chaleco | Tienda | 60 | DEF 2 |
| | la cota de malla (chainmail) | Herrería | 150 | DEF 3 |
| | el peto (breastplate) | Herrería | 260 | DEF 5 |
| Piernas (legs) | los pantalones | Tienda | 50 | DEF 1 |
| | las grebas (greaves) | Herrería | 110 | DEF 2 |
| Muñecas (wrists) | la pulsera | Tienda | 55 | +2 mana |
| | los brazales (bracers) | Herrería | 70 | DEF 1 |
| Mano secundaria (off hand) | el escudo de madera | Herrería | 60 | DEF 2 |
| | el escudo | Herrería | 120 | DEF 3 |
| | el escudo de hierro | Herrería | 220 | DEF 5 |
| Mano principal (main hand) | any weapon | Armería | | |

**Armour tiers (Herrería).** Six tiers, each with 5 pieces (head, chest, legs, wrists, shield), locked by level. The Herrería has one tab per tier.

| Tier | Level | Head | Chest | Legs | Wrists | Shield | Pieces |
|---|---|---|---|---|---|---|---|
| Tela (cloth) | 1 | DEF 1 · 20 | DEF 2 · 40 | DEF 2 · 30 | DEF 1 · 16 | DEF 2 · 30 | capucha, túnica, pantalones, muñequeras, escudo de madera |
| Cuero (leather) | 5 | 2 · 60 | 4 · 120 | 3 · 90 | 1 · 48 | 3 · 90 | gorro, chaleco, pantalones, brazales, escudo |
| Malla (chainmail) | 15 | 3 · 180 | 6 · 360 | 4 · 270 | 2 · 144 | 4 · 270 | capucha, cota de malla, calzas, brazales, escudo reforzado |
| Hierro (iron) | 25 | 5 · 450 | 10 · 900 | 8 · 675 | 3 · 360 | 8 · 675 | casco, coraza, grebas, brazales, escudo |
| Toledo (Toledo steel) | 40 | 8 · 1,200 | 16 · 2,400 | 12 · 1,800 | 5 · 960 | 12 · 1,800 | yelmo, coraza, grebas, brazales, escudo |
| Rúnico (rune) | 60 | 12 · 3,500 | 24 · 7,000 | 18 · 5,250 | 7 · 2,800 | 18 · 5,250 | yelmo, coraza, grebas, brazales, escudo |

(Numbers are defence · price.) Upgrades, durability and repairs are not built yet.

## XP for every level (1–100)

Levels keep going after 100 (the formula is 80 × level^1.4).

| Level | XP needed | Total XP to reach the start of this level |
|---|---|---|
| 1 → 2 | 80 | 0 |
| 2 → 3 | 211 | 80 |
| 3 → 4 | 372 | 291 |
| 4 → 5 | 557 | 663 |
| 5 → 6 | 761 | 1,220 |
| 6 → 7 | 983 | 1,981 |
| 7 → 8 | 1,220 | 2,964 |
| 8 → 9 | 1,470 | 4,184 |
| 9 → 10 | 1,734 | 5,654 |
| 10 → 11 | 2,010 | 7,388 |
| 11 → 12 | 2,296 | 9,398 |
| 12 → 13 | 2,594 | 11,694 |
| 13 → 14 | 2,901 | 14,288 |
| 14 → 15 | 3,219 | 17,189 |
| 15 → 16 | 3,545 | 20,408 |
| 16 → 17 | 3,880 | 23,953 |
| 17 → 18 | 4,224 | 27,833 |
| 18 → 19 | 4,576 | 32,057 |
| 19 → 20 | 4,936 | 36,633 |
| 20 → 21 | 5,303 | 41,569 |
| 21 → 22 | 5,678 | 46,872 |
| 22 → 23 | 6,060 | 52,550 |
| 23 → 24 | 6,449 | 58,610 |
| 24 → 25 | 6,845 | 65,059 |
| 25 → 26 | 7,248 | 71,904 |
| 26 → 27 | 7,657 | 79,152 |
| 27 → 28 | 8,072 | 86,809 |
| 28 → 29 | 8,494 | 94,881 |
| 29 → 30 | 8,922 | 103,375 |
| 30 → 31 | 9,355 | 112,297 |
| 31 → 32 | 9,795 | 121,652 |
| 32 → 33 | 10,240 | 131,447 |
| 33 → 34 | 10,691 | 141,687 |
| 34 → 35 | 11,147 | 152,378 |
| 35 → 36 | 11,609 | 163,525 |
| 36 → 37 | 12,076 | 175,134 |
| 37 → 38 | 12,548 | 187,210 |
| 38 → 39 | 13,025 | 199,758 |
| 39 → 40 | 13,508 | 212,783 |
| 40 → 41 | 13,995 | 226,291 |
| 41 → 42 | 14,487 | 240,286 |
| 42 → 43 | 14,984 | 254,773 |
| 43 → 44 | 15,486 | 269,757 |
| 44 → 45 | 15,993 | 285,243 |
| 45 → 46 | 16,504 | 301,236 |
| 46 → 47 | 17,020 | 317,740 |
| 47 → 48 | 17,540 | 334,760 |
| 48 → 49 | 18,065 | 352,300 |
| 49 → 50 | 18,594 | 370,365 |
| 50 → 51 | 19,127 | 388,959 |
| 51 → 52 | 19,665 | 408,086 |
| 52 → 53 | 20,207 | 427,751 |
| 53 → 54 | 20,753 | 447,958 |
| 54 → 55 | 21,303 | 468,711 |
| 55 → 56 | 21,857 | 490,014 |
| 56 → 57 | 22,416 | 511,871 |
| 57 → 58 | 22,978 | 534,287 |
| 58 → 59 | 23,544 | 557,265 |
| 59 → 60 | 24,115 | 580,809 |
| 60 → 61 | 24,689 | 604,924 |
| 61 → 62 | 25,267 | 629,613 |
| 62 → 63 | 25,849 | 654,880 |
| 63 → 64 | 26,434 | 680,729 |
| 64 → 65 | 27,024 | 707,163 |
| 65 → 66 | 27,617 | 734,187 |
| 66 → 67 | 28,213 | 761,804 |
| 67 → 68 | 28,813 | 790,017 |
| 68 → 69 | 29,417 | 818,830 |
| 69 → 70 | 30,025 | 848,247 |
| 70 → 71 | 30,636 | 878,272 |
| 71 → 72 | 31,250 | 908,908 |
| 72 → 73 | 31,868 | 940,158 |
| 73 → 74 | 32,489 | 972,026 |
| 74 → 75 | 33,114 | 1,004,515 |
| 75 → 76 | 33,742 | 1,037,629 |
| 76 → 77 | 34,374 | 1,071,371 |
| 77 → 78 | 35,009 | 1,105,745 |
| 78 → 79 | 35,647 | 1,140,754 |
| 79 → 80 | 36,288 | 1,176,401 |
| 80 → 81 | 36,933 | 1,212,689 |
| 81 → 82 | 37,581 | 1,249,622 |
| 82 → 83 | 38,232 | 1,287,203 |
| 83 → 84 | 38,887 | 1,325,435 |
| 84 → 85 | 39,544 | 1,364,322 |
| 85 → 86 | 40,205 | 1,403,866 |
| 86 → 87 | 40,868 | 1,444,071 |
| 87 → 88 | 41,535 | 1,484,939 |
| 88 → 89 | 42,205 | 1,526,474 |
| 89 → 90 | 42,878 | 1,568,679 |
| 90 → 91 | 43,554 | 1,611,557 |
| 91 → 92 | 44,233 | 1,655,111 |
| 92 → 93 | 44,915 | 1,699,344 |
| 93 → 94 | 45,600 | 1,744,259 |
| 94 → 95 | 46,288 | 1,789,859 |
| 95 → 96 | 46,979 | 1,836,147 |
| 96 → 97 | 47,673 | 1,883,126 |
| 97 → 98 | 48,369 | 1,930,799 |
| 98 → 99 | 49,069 | 1,979,168 |
| 99 → 100 | 49,771 | 2,028,237 |
| 100 → 101 | 50,477 | 2,078,008 |

## The dungeon: La Ciudadela del Sol

A Machu Picchu-style stone citadel at the end of the forest path (east, across the bridge).
Press E at the doorway to see the ten floors. Each floor needs a character level and the floor below
it beaten. Battles are turn-based: pick a move, then answer the Spanish question within 12 seconds
(keys 1–4 work). **No translations or hints.** Wrong or late = the move fails. Then the boss strikes
(armour softens it). Lose = 10% of your gold and you leave with half HP.

| Move | Effect |
|---|---|
| Atacar | (Weapon damage + rune bonus) × 2 + level ÷ 2 + a little luck. Vampiro heals, Hielo may freeze the boss |
| Hechizo | Your prepared spell (★ in the spellbook): spell its word letter by letter before the timer ends. A wrong letter fizzles |
| Defender | Halves the boss's next hit, +3 HP |
| Comer | Eat your best food (no question): heals half its value |
| Huir | Leave the battle |

| Floor | Level | Boss | HP | Hits for | Tests | Reward |
|---|---|---|---|---|---|---|
| 1 | 5 | El Guardián de Piedra | 58 | 4 | Basic words | 40 gold + 60 Lexa |
| 2 | 10 | La Reina Araña | 86 | 6 | Food and animals | 120 |
| 3 | 15 | El Rey Trol | 114 | 8 | Ser or estar | 180 |
| 4 | 20 | El Brujo de la Niebla | 142 | 10 | Questions and replies | 240 |
| 5 | 25 | El Rey de Hielo | 170 | 12 | Past tense | 300 |
| 6 | 30 | La Serpiente Emplumada | 198 | 14 | Future tense | 360 |
| 7 | 35 | El Gigante de Lava | 226 | 16 | Which sentence is in the right order | 420 |
| 8 | 40 | La Bruja del Espejo | 254 | 18 | English sentence → Spanish | 480 |
| 9 | 45 | El Caballero Sin Nombre | 282 | 20 | Everything | 540 |
| 10 | 50 | El Rey Demonio | 310 | 22 | Everything (final) | 600 |

Bosses also give Combat XP (40 × floor).

## Magic (K)

Press **K** for the spellbook. Spells unlock by character level and are learned and levelled up (ranks I–V)
with **Lexa**. Learning costs level × 20 Lexa; each rank after that costs more. **★** chooses the spell the
battle "Hechizo" move uses; **▶** casts it now, at the nearest animal. To cast, click the letters of the
spell's word **in order**; one wrong letter and it fizzles ("¡Fsss!").

| Lv | Spell | Word | Mana | Effect (rank I → V) |
|---|---|---|---|---|
| 25 | Chispa (spark) | chispa | 3 | 16 → 40 fire damage |
| 30 | Escudo de luz (light shield) | escudo | 4 | Blocks the next hit |
| 35 | Rayo (lightning) | rayo | 6 | 35 → 75 damage to one enemy |
| 40 | Hielo (ice) | hielo | 5 | Freezes the enemy (3–7 s; one boss turn) |
| 45 | Curación (heal) | curar | 5 | Heals 18 → 50 HP |
| 55 | Terremoto (earthquake) | terremoto | 8 | 28 → 60 damage to every enemy nearby |
| 70 | Meteoro (meteor) | meteoro | 12 | 70 → 150 damage; 60 s cooldown (4 turns in battle) |

## Enchanting: La Torre Rúnica

Talk to **Doña Runa** or use **el altar rúnico**. Pick something you are wearing, then an enchantment.
One enchantment per item (a different one replaces it); ranks I–III cost **100 / 400 / 1k Lexa**.

| Encantamiento | On | Effect per rank |
|---|---|---|
| Afilado (sharpness) | Weapons | +2 damage |
| Fuego (fire) | Weapons | +1 damage (burns) |
| Hielo (ice) | Weapons | Hits slow animals for 1.5 s; 15% chance to freeze a boss for a turn |
| Protección | Armour | +1 defence |
| Vampiro | Weapons | Heal 1 HP per hit (2 in battle) |
| Suerte (luck) | Weapons or armour | +10% Lexa from everything |

## Not built yet

Quests, a story, the gym and strength, gear upgrades (+1…+10),
durability, repairs, music in battle, conversations, and Spanish voice recordings.
These are all in [PLANNER.md](PLANNER.md).
