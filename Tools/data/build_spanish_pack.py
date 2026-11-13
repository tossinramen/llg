"""Builds Assets/Resources/Languages/es/pack.json: Spanish words and sentences, sorted into lesson
topics and five levels.

Run:  python Tools/data/build_spanish_pack.py

Levels (the "tier" of a word):  1 Principiante (Beginner)  2 Básico (Elementary)
                                3 Intermedio (Intermediate)  4 Avanzado (Advanced)  5 Experto (Expert)
Level 1 = short, very common words. Higher levels = longer and less common words.
Nouns include their article (el / la / los / las) so the gender is learned with the word.
"""
import json
import os
import re

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "Resources", "Languages", "es", "pack.json")

# category, level, Spanish, English, icon (optional), hunger (food only)
WORDS = """
food|1|el pan|bread|food_bread|20
food|1|la manzana|apple|food_apple|10
food|1|el agua|water||2
food|1|la leche|milk||8
food|1|el huevo|egg|food_egg|12
food|1|el queso|cheese|food_cheese|15
food|2|el pescado|fish (food)|food_fish|30
food|2|el arroz|rice||18
food|2|el pollo|chicken (food)||35
food|2|la sopa|soup||22
food|2|la naranja|orange|food_orange|10
food|2|la carne|meat||40
food|3|la fresa|strawberry||8
food|3|el plátano|banana||12
food|3|la galleta|cookie|food_cookie|12
food|3|el pastel|cake|food_cake|50
food|3|la mantequilla|butter||6
food|4|la berenjena|aubergine||10
food|4|el melocotón|peach||10
food|4|la zanahoria|carrot||8
food|5|el pimiento|pepper (vegetable)||8
food|5|la alcachofa|artichoke||8
animal|1|el perro|dog|pet_dog
animal|1|el gato|cat|pet_cat
animal|1|el pez|fish (animal)|food_fish
animal|1|la vaca|cow
animal|2|el caballo|horse
animal|2|el pájaro|bird
animal|2|el conejo|rabbit|pet_rabbit
animal|2|el cerdo|pig
animal|3|el lobo|wolf
animal|3|la oveja|sheep
animal|3|el loro|parrot|pet_parrot
animal|3|la gallina|hen
animal|4|la tortuga|turtle
animal|4|el ratón|mouse
animal|4|la ardilla|squirrel
animal|5|el murciélago|bat
animal|5|la mariposa|butterfly
color|1|rojo|red
color|1|azul|blue
color|1|verde|green
color|1|blanco|white
color|2|negro|black
color|2|amarillo|yellow
color|2|rosa|pink
color|3|marrón|brown
color|3|gris|grey
color|3|morado|purple
color|4|naranja (color)|orange (colour)
color|5|plateado|silver
number|1|uno|one
number|1|dos|two
number|1|tres|three
number|1|cuatro|four
number|1|cinco|five
number|2|seis|six
number|2|siete|seven
number|2|ocho|eight
number|2|nueve|nine
number|2|diez|ten
number|3|veinte|twenty
number|3|cincuenta|fifty
number|4|cien|one hundred
number|5|mil|one thousand
home|1|la casa|house
home|1|la cama|bed
home|1|la mesa|table
home|2|la silla|chair
home|2|la puerta|door
home|2|la ventana|window
home|3|la cocina|kitchen
home|3|el baño|bathroom
home|3|el espejo|mirror
home|4|la almohada|pillow
home|4|la estantería|bookshelf
home|5|el armario|wardrobe
town|1|el pueblo|village
town|1|la calle|street
town|2|la plaza|town square
town|2|la tienda|shop
town|2|el mercado|market
town|3|la panadería|bakery
town|3|la iglesia|church
town|3|el puente|bridge
town|3|la fuente|fountain
town|4|el ayuntamiento|town hall
town|4|la herrería|blacksmith's
town|4|el molino|windmill
town|5|la armería|armoury
clothes|1|la camisa|shirt
clothes|1|los zapatos|shoes
clothes|2|el sombrero|hat|gear_hat
clothes|2|los pantalones|trousers|gear_pants
clothes|2|la falda|skirt
clothes|3|la capa|cloak|gear_cloak
clothes|3|el vestido|dress
clothes|3|la chaqueta|jacket
clothes|4|el collar|necklace|gear_necklace
clothes|4|la pulsera|bracelet|gear_bracelet
clothes|5|el chaleco|vest|gear_vest
body|1|la mano|hand
body|1|el pie|foot
body|1|la boca|mouth
body|2|la cabeza|head
body|2|el ojo|eye
body|2|la nariz|nose
body|3|el brazo|arm
body|3|la pierna|leg
body|3|la oreja|ear
body|4|el hombro|shoulder
body|5|la rodilla|knee
family|1|la madre|mother
family|1|el padre|father
family|2|el hermano|brother
family|2|la hermana|sister
family|3|la abuela|grandmother
family|3|el abuelo|grandfather
family|4|el primo|cousin (male)
family|4|la tía|aunt
family|5|el sobrino|nephew
nature|1|el sol|sun
nature|1|el árbol|tree
nature|1|la flor|flower
nature|2|el río|river
nature|2|el bosque|forest
nature|2|la luna|moon
nature|3|la montaña|mountain
nature|3|el estanque|pond
nature|3|la hierba|grass
nature|4|el olivo|olive tree
nature|4|el naranjo|orange tree
nature|5|el ciprés|cypress
verb|1|comer|to eat
verb|1|beber|to drink
verb|1|ir|to go
verb|2|tener|to have
verb|2|querer|to want
verb|2|hablar|to speak
verb|3|comprar|to buy
verb|3|pescar|to fish
verb|3|correr|to run
verb|4|cocinar|to cook
verb|4|aprender|to learn
verb|5|construir|to build
tool|2|el hacha|axe|item_axe
tool|2|la espada|sword|item_sword
tool|3|el arco|bow|item_bow
tool|3|el cuchillo|knife|item_knife
tool|4|el escudo|shield|gear_shield
tool|4|la madera|wood|item_wood
tool|5|la ballesta|crossbow|item_crossbow
"""

SENTENCES = """
1|food|Yo como pan|I eat bread
1|food|Quiero agua|I want water
1|animal|El gato es negro|The cat is black
1|animal|El perro bebe agua|The dog drinks water
1|color|La casa es blanca|The house is white
1|number|Tengo dos huevos|I have two eggs
1|home|La mesa es azul|The table is blue
1|family|Mi madre come|My mother eats
2|food|Quiero pan y queso|I want bread and cheese
2|food|El pollo está rico|The chicken is tasty
2|animal|El caballo es grande|The horse is big
2|animal|Tengo un conejo blanco|I have a white rabbit
2|town|Voy a la tienda|I am going to the shop
2|town|El mercado está aquí|The market is here
2|home|La ventana está abierta|The window is open
2|nature|El sol es amarillo|The sun is yellow
3|food|Me gusta la fresa roja|I like the red strawberry
3|town|La panadería abre temprano|The bakery opens early
3|town|El puente cruza el río|The bridge crosses the river
3|nature|El pez vive en el estanque|The fish lives in the pond
3|animal|El lobo vive en el bosque|The wolf lives in the forest
3|verb|Quiero comprar tres naranjas|I want to buy three oranges
3|clothes|Mi capa es muy bonita|My cloak is very pretty
3|body|Me duele la pierna|My leg hurts
4|town|El molino está en la colina|The windmill is on the hill
4|verb|Mi abuela cocina con aceite de oliva|My grandmother cooks with olive oil
4|nature|Los olivos crecen cerca del pueblo|The olive trees grow near the village
4|family|Mi tía vive en la ciudad|My aunt lives in the city
4|tool|Corto madera con mi hacha|I cut wood with my axe
5|town|El herrero trabaja en la herrería|The blacksmith works in the forge
5|verb|Vamos a construir una casa nueva|We are going to build a new house
5|nature|Las mariposas vuelan sobre las flores|The butterflies fly over the flowers
5|tool|La ballesta es más fuerte que el arco|The crossbow is stronger than the bow
"""

CATEGORIES = [
    ("food", "La comida", "Food"), ("animal", "Los animales", "Animals"), ("color", "Los colores", "Colours"),
    ("number", "Los números", "Numbers"), ("home", "La casa", "Home"), ("town", "El pueblo", "Town"),
    ("clothes", "La ropa", "Clothes"), ("body", "El cuerpo", "Body"), ("family", "La familia", "Family"),
    ("nature", "La naturaleza", "Nature"), ("verb", "Los verbos", "Verbs"), ("tool", "Las herramientas", "Tools & weapons"),
    ("time", "El tiempo y los días", "Days & time"), ("weather", "El clima", "Weather"), ("school", "La escuela", "School"),
    ("job", "Los trabajos", "Jobs"), ("adjective", "Cómo es", "Describing words"), ("feeling", "Los sentimientos", "Feelings"),
    ("travel", "Los viajes", "Travel"), ("phrase", "Frases útiles", "Useful phrases"), ("imported", "Más palabras", "More words"),
]

HERE = os.path.dirname(os.path.abspath(__file__))


def extra_lines():
    """words_extra.txt, then every deck imported with import_deck.py (Tools/data/imported/*.txt)."""
    files = [os.path.join(HERE, "words_extra.txt")]
    imported = os.path.join(HERE, "imported")
    if os.path.isdir(imported):
        files += [os.path.join(imported, f) for f in sorted(os.listdir(imported)) if f.endswith(".txt")]
    for path in files:
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#"):
                    yield line, os.path.basename(path)


def slug(text):
    return re.sub(r"[^a-z0-9]+", "_", text.lower()).strip("_")


ENGLISHY = re.compile(r"\b(the|to|of|is|and|you|it|with|my|this|that|in|at|on)\b", re.I)
SPANISHY = re.compile(r"[áéíóúñ¿¡]|\b(el|la|los|las|que|de|y|es|en|por|para|con|a|al|del)\b|(ar|er|ir|ción|mente|aste|ando|iendo)$", re.I)


def tidy_pair(kind, es, en):
    """Last clean-up for every imported line: trailing dashes, missing ¿ ¡, backwards cards, same on both sides."""
    es = re.sub(r"[\s\-–—]+$", "", es).strip()
    en = re.sub(r"[\s\-–—]+$", "", en).strip()
    if kind == "W" and ENGLISHY.search(es) and not SPANISHY.search(es) and (
            SPANISHY.search(en) or (len(en.split()) == 1 and not ENGLISHY.search(en))):
        es, en = en, es                                   # the card was the wrong way round
    if kind == "S":
        if es.endswith("?") and "¿" not in es and es.count("?") == 1:
            es = "¿" + es
        if es.endswith("!") and "¡" not in es and es.count("!") == 1:
            es = "¡" + es
    same = re.sub(r"\W", "", es.lower()) == re.sub(r"\W", "", en.lower())
    if not es or not en or (same and (kind == "S" or ENGLISHY.search(es))):     # words like "piano" = "piano" are fine
        return "", ""
    return es, en


# ---------------------------------------------------------------- one rule for every level
MANUAL = ("words_extra.txt",)                      # hand-made lists keep the levels they were given
FREQ_LIST = "10000_Spanish_Vocabulary.txt"         # a frequency-ordered deck: its order is the word's rank
WORD_LEVELS = (500, 1500, 3500, 7000)              # rank below each = level 1, 2, 3, 4; the rest = 5
SENTENCE_SHARE = (0.12, 0.18, 0.24, 0.24, 0.22)    # how the sentences are shared out over levels 1-5
SMALL = {"del", "con", "por", "para", "que", "una", "los", "las", "uno", "sin", "sus", "mis", "tus", "nos", "les", "muy", "mas", "más"}
ARTICLES = ("el ", "la ", "los ", "las ", "un ", "una ")


def head(word):
    """The main word of an entry: "la casa" -> "casa", "dar su brazo a torcer" -> "brazo"."""
    w = word.lower().strip("¿¡?!.,; ")
    for a in ARTICLES:
        if w.startswith(a):
            w = w[len(a):]
    parts = [t for t in re.findall(r"[a-záéíóúñü]+", w) if len(t) > 2] or re.findall(r"[a-záéíóúñü]+", w) or [w]
    return max(parts, key=len)


def level_of(rank, steps):
    for lv, top in enumerate(steps, start=1):
        if rank < top:
            return lv
    return 5


def relevel(words, sentences):
    """Words: by how common they are (the frequency deck's order, otherwise how often the word turns up in
    all the sentences). Sentences: the level of their hardest word, one more if long (9+ words), two if
    very long (12+). Built-in and hand-made entries keep their levels. Also sorts more words into topics."""
    from collections import Counter
    from import_deck import topic_for
    tokens = Counter()
    for x in sentences:
        tokens.update(t for t in re.findall(r"[a-záéíóúñü]+", x["text"].lower()))
    token_rank = {t: i for i, (t, _) in enumerate(tokens.most_common())}
    freq_rank = {}
    for w in words:
        if w.get("_src") == FREQ_LIST:
            freq_rank.setdefault(head(w["word"]), len(freq_rank))
    for w in words:
        src = w.pop("_src", None)
        w.pop("_order", None)
        if src is None or src in MANUAL:
            continue
        def rank_of(t):
            if t in freq_rank:
                return freq_rank[t]
            if t in token_rank and tokens[t] >= 3:
                return int(token_rank[t] * 1.6)        # word forms in sentences are a bit more spread out than headwords
            return 20000
        whole = w["word"].lower().strip("¿¡?!.,; ")
        for art in ARTICLES:
            if whole.startswith(art):
                whole = whole[len(art):]
        parts = [t for t in re.findall(r"[a-záéíóúñü]+", whole) if len(t) > 2 and t not in SMALL] or [head(w["word"])]
        if whole in freq_rank:                         # the whole entry is ranked (e.g. "a pesar de")
            rank = freq_rank[whole]
        else:                                          # a phrase is as hard as its hardest word
            rank = max(rank_of(t) for t in parts)
        w["tier"] = level_of(rank, WORD_LEVELS)
        if w["category"] == "imported":                # second pass at topics: the English head noun
            en = re.sub(r"\(.*?\)", "", w["english"].lower()).strip()
            last = en.split()[-1] if en.split() else ""
            guess = topic_for(w["word"], en)
            if guess == "imported" and last:
                guess = topic_for(w["word"], last)
            w["category"] = guess
    # sentences: difficulty = the rank of the hardest word (names ignored), a little harder when long;
    # then the easiest SENTENCE_SHARE[0] are level 1, and so on, so the levels climb smoothly
    scored = []
    for x in sentences:
        src = x.pop("_src", None)
        if src is None or src in MANUAL:
            continue
        raw = x["text"].split()
        toks = []
        for i, t in enumerate(raw):
            clean_t = re.sub(r"[^a-záéíóúñüA-ZÁÉÍÓÚÑÜ]", "", t)
            if not clean_t or (i > 0 and clean_t[0].isupper()):
                continue                               # names like Tom, Madrid
            toks.append(clean_t.lower())
        def tr(t):                                     # the frequency list when it knows the word; otherwise counts in the sentences
            if t in freq_rank:
                return freq_rank[t]
            return token_rank[t] * 2 if tokens.get(t, 0) >= 3 else 99999
        hardest = max((tr(t) for t in toks), default=0)
        scored.append((hardest * (1 + 0.07 * max(0, len(raw) - 5)), x))
    scored.sort(key=lambda p: p[0])
    bounds, acc = [], 0
    for share in SENTENCE_SHARE:
        acc += share
        bounds.append(acc * len(scored))
    for i, (_, x) in enumerate(scored):
        x["tier"] = next(lv for lv, b in enumerate(bounds, start=1) if i < b)


def main():
    words, seen = [], set()
    for line in WORDS.strip().splitlines():
        p = line.split("|")
        cat, tier, es, en = p[0], int(p[1]), p[2], p[3]
        icon = p[4] if len(p) > 4 else ""
        hunger = int(p[5]) if len(p) > 5 and p[5] else 0
        wid = slug(en)
        while wid in seen:
            wid += "_2"
        seen.add(wid)
        words.append({"id": wid, "word": es, "english": en, "category": cat, "tier": tier, "icon": icon, "hunger": hunger})
    sentences = []
    for i, line in enumerate(SENTENCES.strip().splitlines()):
        tier, cat, es, en = line.split("|")
        sentences.append({"id": "s%03d" % (i + 1), "text": es, "english": en, "tier": int(tier), "category": cat})

    # the bigger lists: skip anything already in the game (same Spanish)
    def key(text):        # repeats ignore capitals, punctuation and extra spaces
        return re.sub(r"\s+", " ", re.sub(r"[^\w\s]", "", text.lower())).strip()

    known_words = {key(w["word"]) for w in words}
    known_sentences = {key(x["text"]) for x in sentences}
    for line, source in extra_lines():
        p = line.split("|")
        if len(p) < 5:
            continue
        kind, cat, tier, es, en = p[0], p[1], int(p[2]), p[3].strip(), p[4].strip()
        es, en = tidy_pair(kind, es, en)
        if not es:
            continue
        if kind == "W" and key(es) not in known_words:
            known_words.add(key(es))
            wid = slug(en) or slug(es)
            while wid in seen:
                wid += "_2"
            seen.add(wid)
            words.append({"id": wid, "word": es, "english": en, "category": cat, "tier": tier, "icon": "", "hunger": 0,
                          "_src": source, "_order": len(words)})
        elif kind == "S" and key(es) not in known_sentences:
            known_sentences.add(key(es))
            sentences.append({"id": "s%04d" % (len(sentences) + 1), "text": es, "english": en, "tier": tier, "category": cat,
                              "_src": source})
    relevel(words, sentences)
    pack = {"language": "es", "categories": [{"id": c, "name": n, "english": e} for c, n, e in CATEGORIES],
            "words": words, "sentences": sentences}
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(pack, f, ensure_ascii=False, indent=1)
    print(len(words), "words,", len(sentences), "sentences ->", OUT)


if __name__ == "__main__":
    main()
