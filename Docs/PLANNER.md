# Runes of Speech — Planner

Your ideas, organised, with a to-do list to tackle one at a time.
**When a task is finished it gets ~~struck through~~ and marked ✅.**
What already exists is described in [GAME_GUIDE.md](GAME_GUIDE.md).

Contents: [To-do list](#to-do-list) · [Design notes](#design-notes) (weapon trees, armour tiers, magic,
dungeon, villains, battles, crops, music) · [Activity ideas](#activity-ideas)

---

## To-do list

### Done
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

### 1. Language content and sound
- [ ] **Add Spanish voices / sound** — install the Spanish voice in Windows, record every Spanish line, check it plays everywhere
- [ ] Much bigger word and sentence pack (target: 1,500+ words, 500+ sentences) from open sources (see notes)
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
- [ ] Level requirements for weapons (fists → stick lv 5 → kitchen knife lv 10 → dagger lv 15 → sword lv 25 …; see weapon trees)
- [ ] Add the missing weapons from the trees (stick, sling, Toledo sword, Tizona, arquebus, pistols…)
- [ ] Armour tiers like Minecraft but our own (cloth → leather → chainmail → iron → Toledo steel → rune)
- [ ] **Durability** on weapons and armour
- [ ] **Repair** at the blacksmith (costs Lexa)
- [ ] **Upgrade** weapons, armour and skills with Lexa (+1 … +10)
- [ ] **Enchant** at the blacksmith / armoury (fire, ice, sharpness, protection…)
- [ ] Strength stat, trained by boxing (adds fist and melee damage)
- [ ] Gym (el gimnasio) with activities: boxing bag, weights, running track (each with a language twist)
- [ ] Magic spells from level 25 (see magic notes)
- [ ] Music skill: music school + music store; instruments; music can stun monsters (and later players)
- [ ] Music activity: click words in order to build a verse for guitar / flute / clarinet

### 5. Dungeons, bosses and villains
- [ ] Dungeon hall building (inspired by Machu Picchu — see notes), outside the village
- [ ] Battle screen like Pokémon Showdown: turn-based, your moves are language answers
- [ ] **No hints or translations in battles**
- [ ] Monsters are fought with words; people are fought with clicking (real-time)
- [ ] One boss floor every 5 levels, climbing up the hall
- [ ] Villains: Rey Trol, Rey de Hielo, Rey Demonio (and the others in the notes)
- [ ] Bosses drop Lexa and rare gear
- [ ] Lexa redesign: grey stones with blue carvings (new icon and art)
- [ ] "Levelling up Lexa" (needs deciding what it means — see notes)

### 6. Farming
- [ ] Plant seeds in the farm field; crops grow over time
- [ ] Different crops sell for different amounts; buy pricier seeds at the store
- [ ] Watering screen: a language exercise that makes crops grow faster
- [ ] Use the Farming skill

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

### Weapon trees (unlock levels are suggestions)

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

### Armour tiers (Minecraft-like, our own)
Tela (cloth) → Cuero (leather) → Cota de malla (chainmail) → Hierro (iron) → Acero de Toledo (Toledo steel) → Armadura rúnica (rune armour, glows blue).
Each tier has head, chest, legs, wrists and a shield. Upgrades +1…+10 cost Lexa; durability drops with hits; the blacksmith repairs.

**Enchantments (encantamientos):** Afilado (sharpness), Fuego (fire), Hielo (ice, slows), Protección, Vampiro (steal HP), Suerte (more Lexa drops).

### Magic (level 25+)
Spells are cast by **saying or building the Spanish word correctly** — a wrong answer fizzles.

| Lv | Spell | Effect |
|---|---|---|
| 25 | Chispa (spark) | Small fire damage |
| 30 | Escudo de luz (light shield) | Blocks the next hit |
| 35 | Rayo (lightning) | Big single-target damage |
| 40 | Hielo (ice) | Freezes an enemy for a turn |
| 45 | Curación (heal) | Restores HP |
| 55 | Terremoto (earthquake) | Hits all enemies |
| 70 | Meteoro (meteor) | Huge damage, long cooldown |

### The dungeon: La Ciudadela del Sol (the Citadel of the Sun)
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

### Lexa
- New look: **grey stones with blue rune carvings** (matches the fountain's runes).
- "Levelling up Lexa" — options to choose from: (a) Lexa stones can be fused into higher-value stones,
  (b) Lexa itself has a level that raises how much activities pay, or (c) you spend Lexa to level skills.

### Story idea (to pick from)
You arrive in Valdeluz knowing no Spanish. The **Rey Demonio** has stolen the village's words, sealing
them into runes inside the Ciudadela del Sol. Without words, the fountain is fading. Sabia teaches you
that every word you learn re-carves a rune. Your goal is to climb the citadel, beat the kings and
bring the language back. (Quest text in English, everything else in Spanish.)

### Crops (farming)

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

### Music (idea)
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
