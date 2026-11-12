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
| Currency | **Lexa** (starts at 60) |
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
| E / Enter | Use: talk, go through doors, fish, shop, sleep |
| B | Bag (20 slots) |
| U | Profile: equipment, stats, skills |
| L | Lessons: choose topic and level |
| M | Big map (or click the minimap) |
| Left click | Read the Spanish name of anything (click the sign to close it); click a shop's menu board to shop |
| Right click | Attack animals / chop trees (cursor becomes a fist); on a door: go in |
| Mouse wheel | Zoom in / out |
| Esc | Menu: Resume, Options (keys, music volume), Exit Game |
| 1–4 | Shoot targets in archery |

## Stats & XP

| Stat | Details |
|---|---|
| **HP** | Starts at 20; +2 max per character level. Lost to wolf/dog bites and fishing mistakes. At 0 you faint, wake in the plaza and lose 10% of your Lexa. Bed and level-ups restore it. |
| **Mana** | 10 + intelligence + jewellery bonuses. Spent on hints (2 each). Refills 1 every 20 s, fully at the rune fountain or in bed. |
| **Hunger** | 0–100; drops 1 every 8 s (twice as fast when running). At 0 you walk slowly. Eat food from the bag. |
| **Intelligence** | +1 per book read: +1 max mana and +5% XP. |
| **Attack** | The damage of your weapon (fists = 1). |
| **Defence** | Sum of your armour; each 2 points blocks 1 damage per bite (minimum 1 damage). |
| **Lexa** | Earned from activities, kills, selling. Spent in shops. |

**Character level:** uncapped. XP needed for the next level = 80 × level^1.4 (level 1→2: 80, 5→6: ~761, 10→11: ~2,009). XP bar along the bottom of the screen.
Every XP gain goes to a **skill** and to your **character level**.

**Lesson multiplier:** Principiante ×1, Básico ×1.5, Intermedio ×2, Avanzado ×3, Experto ×4 (applies to archery, fishing, El intruso).

| Source | XP |
|---|---|
| Reading a word for the first time (left click) | 2 Reading |
| Archery hit | 5 Archery + 5 Vocabulary (× lesson) |
| Fishing catch | 10 Fishing + 10 Grammar (× lesson) |
| El intruso correct answer | 4 Vocabulary (× lesson) |
| Buying something | 2 Trading |
| Tree felled | 6 Woodcutting |
| Wild dog / wolf killed | 20 / 30 Combat |

## Skills

16 skills, all uncapped. XP for next skill level = 40 × level^1.6. Shown in the profile (U).

| Language skills | Life skills |
|---|---|
| Vocabulario (Vocabulary) — archery, El intruso | Cocina (Cooking) — *not used yet* |
| Gramática (Grammar) — fishing | Pesca (Fishing) — fishing |
| Lectura (Reading) — left-click reading | Agricultura (Farming) — *not used yet* |
| Comprensión (Listening) — *not used yet* | Comercio (Trading) — buying |
| Ortografía (Spelling) — *not used yet* | Artesanía (Crafting) — *not used yet* |
| Conversación (Conversation) — *not used yet* | Exploración (Exploration) — *not used yet* |
| | Magia (Magic) — *not used yet* |
| | Tiro con arco (Archery) — archery |
| | Combate (Combat) — killing animals |
| | Leñador (Woodcutting) — chopping trees |

## Lessons & language

- **Word pack:** 152 words, 33 sentences (`Tools/data/build_spanish_pack.py` → `pack.json`).
- **12 topics:** La comida (Food), Los animales, Los colores, Los números, La casa (Home), El pueblo (Town), La ropa (Clothes), El cuerpo (Body), La familia, La naturaleza, Los verbos, Las herramientas (Tools & weapons). Plus "Todos".
- **5 levels:** 1 Principiante · 2 Básico · 3 Intermedio · 4 Avanzado · 5 Experto.
- Chosen at the **lesson board** in the plaza or with **L**. If a topic has too few words at a level, easier words fill in.
- **Spanish capitalisation rule followed:** signs capitalise only the first word ("La fuente rúnica"); English uses Title Case.

## Activities

| Activity | Where | How it works | Rewards |
|---|---|---|---|
| **Tiro con arco** (Archery) | Archery range (north-east), Don Felipe | English word shown; shoot the target with the Spanish word (an arrow flies to it). 8 rounds, 3 arrows, timer. Hint (2 mana) removes a wrong target. A bow equipped gives more time. | 4 Lexa per hit, XP |
| **La pesca** (Fishing) | Pond (south) or river; needs a fishing rod | Wait for a bite, then click word tiles in order to build the Spanish sentence. Hint (2 mana) places the next word. Mistakes cost 1 HP. Magic rod gives more time. | Fish into the bag, Lexa, XP |
| **El intruso** (Odd one out) | Tomás the woodcutter, or the lessons window | Four words: three from a topic, one not. Click the intruder. 6 rounds. | 3 Lexa per correct, XP |
| **Reading** | Anywhere | Left-click anything to see its Spanish name; globe for English; speaker to hear it. | Reading XP |
| **Tree chopping** | Any tree | Right-click: cracks appear, tree shakes, breaks after 6 damage (fist 1, hatchet 2, axe/battle axe 3 per hit). Regrows in 3 s. Orange trees sometimes drop oranges. | Wood (sell 4), XP |
| **Combat** | Forest (wolves), olive grove (wild dogs) | Right-click an animal to hit it; bows shoot arrows. They fight back once hit. | Lexa, wolf pelts, XP |
| **Shopping** | 6 shops (inside) | Buy tab and Sell tab. | Trading XP |
| **Egg collecting** | Chicken coop at the farm | E at the coop; one egg per minute. | Egg (food) |
| **Sleeping** | Bed in your house | Restores HP and mana, saves. | — |
| **Rune fountain** | Plaza | Refills mana. | — |

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
| Panadería (bakery) | Yes | Carmen; oven; 11 baked goods |
| Dulcería (candy shop) | Yes | Paco; 11 sweets |
| Tienda (general store) | Yes | Doña Elena; rods, book, clothes, jewellery, basic food |
| Armería (weapon shop) | Yes | Rodrigo; 15 weapons |
| Herrería (blacksmith) | Yes | Bruno; forge and anvil; 8 armour pieces |
| Tienda de mascotas (pet shop) | Yes | Lucía; animals in a pen; 6 pets |
| Tu casa (your house) | Yes | Bed (sleep), fireplace, table, shelf, rug |
| Iglesia (church) | No | "Closed today" |
| Ayuntamiento (town hall) | No | "Closed, come back tomorrow" |
| Taberna (tavern) | No | "Opens at night" |
| Neighbours' houses (×3) | No | "Nobody is home" |
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

People cannot be attacked.

## Animals & monsters

| Creature | Where | HP | Bite | Behaviour | Drops |
|---|---|---|---|---|---|
| **El lobo** (wolf) | Forest clearings (5) | 14 | 3 | Neutral until hit; the pack joins in; gives up past 14 tiles | 6–12 Lexa, 60% wolf pelt, 30 XP |
| **El perro salvaje** (wild dog) | Olive grove (3) | 9 | 2 | Same | 3–8 Lexa, 20 XP |
| Village cats & dogs | Streets (5) | — | — | Wander; harmless | — |
| Chickens | Chicken yard (4) | — | — | Wander and peck | — |
| Shop animals | Pet shop | — | — | Wander in the pen | — |

Killed animals return after 30 seconds. **Pets** you buy follow you (choose one in the bag).

## Items

**Bakery (Panadería)** — price / hunger restored

el pan 8/20 · la barra de pan 10/25 · el cruasán 12/30 · la magdalena 9/18 · los churros 7/15 · la ensaimada 14/28 · la empanada 15/35 · la tarta de manzana 18/40 · la rosquilla 8/16 · el bizcocho 16/32 · el pastel 20/50

**Candy shop (Dulcería)**

el caramelo 3/5 · la piruleta 5/8 · el chocolate 10/18 · la galleta 6/12 · el chicle 2/2 · las gominolas 4/6 · el turrón 12/20 · el algodón de azúcar 6/8 · el helado 9/14 · la nube 4/6 · el regaliz 3/5

**General store (Tienda)**

la caña de pescar 30 · la caña mágica 120 (+10 s fishing) · el libro 60 (+1 INT when read) · la manzana 5/10 · el queso 9/15 · el huevo 6/12 · plus the clothes in [Armour & gear](#armour--gear)

**Pets (Mascotas)**

el perro 150 · el gato 150 · el conejo 120 · el loro 250 · la tortuga 140 · el hámster 90

**Found (sell only)** — sell price

el pez 6 (food 30) · la naranja 2 (food 10) · la madera 4 · la piel de lobo 15

Shops buy anything at **half price** (found items at the prices above).

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

There is **no durability, upgrading or enchanting yet** (in the planner).

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

## Not built yet

Quests, dungeons, bosses, villains, a story, magic spells, farming crops you plant, the gym and strength,
gear upgrades, durability, repairs and enchanting, music, conversations, and Spanish voice recordings.
These are all in [PLANNER.md](PLANNER.md).
