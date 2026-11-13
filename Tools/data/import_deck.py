"""Imports Spanish words and sentences from a flashcard deck into the game.

Works with:
  * Anki decks:            .apkg / .colpkg   (File > Export in Anki, "Anki Deck Package")
  * Anki plain-text export: .txt             (File > Export > "Notes in Plain Text", tab-separated)
  * Spreadsheets:          .csv / .tsv       (one card per row: Spanish and English in two columns)

Run (from the project folder):
  python Tools/data/import_deck.py  "C:\\Users\\you\\Downloads\\Spanish Top 5000.apkg"
  python Tools/data/import_deck.py  part1.apkg part2.apkg part3.apkg     (several parts = one ordered list)
  python Tools/data/import_deck.py  mywords.csv  --category food --level 2

Decks whose note type names its fields (e.g. "Spanish", "English", "Spanish Sentence", "English Sentence")
give both the word and its example sentence. Big frequency decks are levelled by rank:
words 1-300 = level 1, up to 1,000 = 2, up to 2,500 = 3, up to 5,000 = 4, the rest = 5.
Words are put into topics (food, animals, verbs ...) by their English meaning when it is clear.

Options:
  --category ID   put everything in one lesson topic (food, animal, verb, ...). Default: "imported"
                  ("Más palabras"), or the card's Anki tag if it matches a topic.
  --level N       give everything one lesson level (1-5). Default: spread by order in the deck
                  (first fifth = level 1 ... last fifth = level 5; frequency decks are in order).
  --spanish N --english N   which fields hold the Spanish and English (counting from 1) if the
                  guess is wrong. By default the script guesses which field is Spanish.
  --limit N       only take the first N cards.

It writes Tools/data/imported/<deck name>.txt and then rebuilds the language pack, so the new words
appear in every lesson, the mine, fishing sentences and so on. Delete that .txt to remove them.
Words already in the game are skipped.

About licences: decks you download are fine for your own copy of the game. If you ever publish
the game, only include decks whose licence allows it (or the open sources listed in the guide).
"""
import argparse
import csv
import html
import io
import os
import re
import sqlite3
import subprocess
import sys
import tempfile
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.join(HERE, "imported")
TOPICS = {"food", "animal", "color", "number", "home", "town", "clothes", "body", "family", "nature", "verb", "tool",
          "time", "weather", "school", "job", "adjective", "feeling", "travel", "phrase", "imported"}
TAG_TOPICS = {"food": "food", "comida": "food", "animal": "animal", "animals": "animal", "colour": "color", "color": "color",
              "colors": "color", "number": "number", "numbers": "number", "house": "home", "home": "home", "town": "town",
              "city": "town", "clothes": "clothes", "clothing": "clothes", "body": "body", "family": "family", "nature": "nature",
              "verb": "verb", "verbs": "verb", "time": "time", "weather": "weather", "school": "school", "job": "job",
              "jobs": "job", "work": "job", "adjective": "adjective", "adjectives": "adjective", "feelings": "feeling",
              "emotions": "feeling", "travel": "travel", "phrases": "phrase", "phrase": "phrase"}

KEYWORDS = {
    "food": "food bread apple water milk egg cheese fish meat rice chicken soup orange fruit salt tea coffee juice salad tomato potato "
            "onion sugar ham grape lemon oil garlic lettuce pear cherry honey breakfast lunch dinner watermelon melon pineapple "
            "mushroom cucumber flour avocado pumpkin seafood pepper vinegar butter cake cookie banana strawberry carrot beef pork "
            "sausage wine beer bean beans corn pasta sandwich dessert chocolate candy meal vegetable vegetables nut nuts olive",
    "animal": "animal dog cat horse cow pig sheep goat duck hen chicken mouse bear lion donkey fox squirrel elephant monkey snake "
              "bee ant frog tiger giraffe dolphin whale owl spider bull bat crocodile eagle snail deer bird rabbit wolf insect fly "
              "butterfly shark turtle parrot",
    "color": "red blue green yellow black white brown pink purple grey gray orange golden colour color",
    "number": "one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen "
              "eighteen nineteen twenty thirty forty fifty sixty seventy eighty ninety hundred thousand million first second third",
    "home": "house home bed chair door window table kitchen bathroom bedroom lamp sofa key plate glass spoon fork knife cup "
            "mirror rug wardrobe stairs ceiling fridge oven pillow blanket garden fireplace basement balcony roof wall floor room",
    "town": "street park square school hospital bank restaurant pharmacy library museum station church castle fountain village "
            "city neighbourhood neighborhood market shop store bakery bridge cathedral town building office hotel",
    "clothes": "shirt shoes skirt dress socks jacket coat scarf gloves boots cap jumper sweater belt pyjamas glasses pocket umbrella "
               "hat trousers pants clothes clothing tie",
    "body": "body head hand arm leg foot eye eyes mouth nose ear hair finger back neck shoulder knee tooth teeth tongue stomach "
            "heart elbow wrist forehead ankle eyebrow bone face skin blood lip",
    "family": "family mother father brother sister son daughter grandfather grandmother uncle aunt cousin husband wife baby "
              "grandson granddaughter parents nephew niece mom dad",
    "nature": "sea moon star mountain beach sky cloud leaf lake island stone sand countryside grass rock volcano cave jungle desert "
              "wave branch waterfall rainbow valley shore tree flower river forest earth world nature plant",
    "time": "day night today tomorrow yesterday monday tuesday wednesday thursday friday saturday sunday week month year hour "
            "afternoon evening morning january february march april may june july august september october november december "
            "minute second spring summer autumn fall winter always never often time",
    "weather": "sun rain wind snow heat cold storm fog thunder lightning ice weather temperature hail",
    "school": "book pencil notebook backpack teacher student pupil class blackboard pen eraser ruler word question answer exam "
              "lesson dictionary subject language school university",
    "job": "doctor baker farmer cook fisherman blacksmith police officer firefighter waiter carpenter nurse musician miner soldier "
           "seller architect lawyer archaeologist painter job work worker engineer driver",
    "feeling": "happy sad tired angry pleased nervous bored fear hunger thirst surprised worried proud joy love feeling",
    "travel": "car train boat road plane bicycle ticket suitcase map journey trip hotel border port airport luggage travel bus",
}
KEYWORD_TOPIC = {}
for _topic, _words in KEYWORDS.items():
    for _w in _words.split():
        KEYWORD_TOPIC.setdefault(_w, _topic)


def topic_for(spanish, english):
    """Guess a lesson topic from the English meaning (and verbs from "to ...")."""
    e = english.lower().strip()
    if e.startswith("to ") and len(spanish.split()) == 1 and spanish.lower().endswith(("ar", "er", "ir", "se")):
        return "verb"
    core = re.sub(r"^(the|a|an)\s+", "", re.sub(r"\(.*?\)", "", e)).strip()
    if core in KEYWORD_TOPIC:
        return KEYWORD_TOPIC[core]
    if core.endswith("s") and core[:-1] in KEYWORD_TOPIC:
        return KEYWORD_TOPIC[core[:-1]]
    return "imported"


def level_for(rank, total):
    """Big frequency decks: by rank. Small decks: spread evenly by order."""
    if total >= 1500:
        for lv, top in enumerate((300, 1000, 2500, 5000), start=1):
            if rank < top:
                return lv
        return 5
    return min(5, 1 + rank * 5 // max(1, total))


SPANISH_HINTS = re.compile(r"[áéíóúñü¿¡]|\b(el|la|los|las|un|una|de|que|y|es|en|por|para|con|no|muy|yo|tú|está|son)\b", re.I)
ENGLISH_HINTS = re.compile(r"\b(the|a|an|to|of|is|are|and|you|i|it|in|for|with|my|this|that)\b", re.I)


def clean(text):
    """Strips Anki's HTML, sound tags, cloze marks and extra spaces."""
    text = re.sub(r"\[sound:[^\]]*\]", " ", text)
    text = re.sub(r"\{\{c\d+::(.*?)(::[^}]*)?\}\}", r"\1", text)
    text = re.sub(r"<br\s*/?>|</div>|</p>", " ", text, flags=re.I)
    text = re.sub(r"<[^>]+>", "", text)
    text = html.unescape(text).replace("\u00a0", " ")
    return re.sub(r"\s+", " ", text).strip()


def read_apkg(path):
    """Notes from an Anki package: list of (fields, tags, field names)."""
    with zipfile.ZipFile(path) as z:
        names = z.namelist()
        db_name = next((n for n in ("collection.anki21", "collection.anki2") if n in names), None)
        if db_name is None:
            if "collection.anki21b" in names:
                sys.exit("This deck uses Anki's newest compressed format. In Anki: File > Export > "
                         "'Notes in Plain Text (.txt)', then import that .txt file instead.")
            sys.exit("No Anki collection found inside " + path)
        tmp = tempfile.mkdtemp()
        z.extract(db_name, tmp)
    con = sqlite3.connect(os.path.join(tmp, db_name))
    try:
        import json as _json
        models = _json.loads(con.execute("select models from col").fetchone()[0])
        names = {str(k): [f["name"].strip().lower() for f in m["flds"]] for k, m in models.items()}
    except Exception:
        names = {}
    rows = con.execute("select flds, tags, mid from notes order by id").fetchall()
    con.close()
    notes = [([clean(f) for f in flds.split("\x1f")], tags.lower().split(), names.get(str(mid), [])) for flds, tags, mid in rows]
    # very new decks keep a dummy "please update Anki" note in the old file
    return [n for n in notes if not any("please update to the latest anki" in f.lower() for f in n[0])]


def read_table(path):
    with io.open(path, encoding="utf-8-sig", errors="replace") as f:
        lines = [l for l in f.read().splitlines() if l.strip() and not l.startswith("#")]
    delim = "\t" if sum(l.count("\t") for l in lines[:20]) >= sum(l.count(",") for l in lines[:20]) or path.endswith((".txt", ".tsv")) else ","
    return [([clean(c) for c in row], [], []) for row in csv.reader(lines, delimiter=delim)]


def guess_fields(notes):
    """Which field is Spanish and which is English, by counting telltale words and letters."""
    width = max(len(n[0]) for n in notes)
    es = [0] * width
    en = [0] * width
    for fields, _, _ in notes[:300]:
        for i, f in enumerate(fields):
            es[i] += len(SPANISH_HINTS.findall(f))
            en[i] += len(ENGLISH_HINTS.findall(f))
    s = max(range(width), key=lambda i: es[i] - en[i])
    e = max((i for i in range(width) if i != s), key=lambda i: en[i] - es[i] + (1 if i < 3 else 0))
    return s, e


def main():
    ap = argparse.ArgumentParser(description="Import a flashcard deck into Runes of Speech")
    ap.add_argument("deck", nargs="+")
    ap.add_argument("--category", default=None)
    ap.add_argument("--level", type=int, default=None)
    ap.add_argument("--spanish", type=int, default=None)
    ap.add_argument("--english", type=int, default=None)
    ap.add_argument("--limit", type=int, default=None)
    a = ap.parse_args()
    if a.category and a.category not in TOPICS:
        sys.exit("Unknown topic '" + a.category + "'. Choose one of: " + ", ".join(sorted(TOPICS)))

    notes = []
    for path in a.deck:
        part = read_apkg(path) if path.lower().endswith((".apkg", ".colpkg")) else read_table(path)
        print(os.path.basename(path) + ":", len(part), "cards")
        notes += part
    path = a.deck[0]
    notes = [n for n in notes if len(n[0]) >= 2]
    if not notes:
        sys.exit("No cards with at least two fields were found.")
    names = notes[0][2]
    named = "spanish" in names and "english" in names
    s_i, e_i = (names.index("spanish"), names.index("english")) if named else guess_fields(notes)
    if a.spanish: s_i = a.spanish - 1
    if a.english: e_i = a.english - 1
    print("Spanish field:", s_i + 1, " English field:", e_i + 1, " (use --spanish/--english to change)")
    print("  e.g.", notes[0][0][s_i], "=", notes[0][0][e_i])
    if a.limit: notes = notes[:a.limit]

    lines, words, sentences = [], 0, 0
    total = len(notes)
    seen = set()
    for i, (fields, tags, fnames) in enumerate(notes):
        if max(s_i, e_i) >= len(fields):
            continue
        es, en = fields[s_i].strip(" ;"), fields[e_i].strip(" ;")
        if not es or not en or "|" in es + en or len(es) > 90:
            continue
        level = a.level or level_for(i, total)
        en = re.split(r"\s*[;/]\s*", en)[0][:60]                    # first meaning only
        cat = a.category or next((TAG_TOPICS[t] for t in tags if t in TAG_TOPICS), None) or topic_for(es, en)
        # the card's example sentence, if the deck has one (e.g. "Spanish Sentence" / "English Sentence")
        if "spanish sentence" in fnames and "english sentence" in fnames:
            ss, se = fields[fnames.index("spanish sentence")], fields[fnames.index("english sentence")]
            ss = re.sub(r"[.…]+$", "", ss).strip()
            se = re.sub(r"[.…]+$", "", se).strip()
            if ss and se and "|" not in ss + se and 2 <= len(ss.split()) <= 12 and ss.lower() not in seen:
                seen.add(ss.lower())
                lines.append("S|%s|%d|%s|%s" % (cat, level, ss, se))
                sentences += 1
        if es.lower() in seen:
            continue
        seen.add(es.lower())
        es = re.sub(r"^(El|La|Los|Las|Un|Una) ", lambda m: m.group(1).lower() + " ", es)     # "La casa" -> "la casa"
        if "spanish sentence" in fnames:                        # this deck has real sentences: the headword is a word/phrase
            lines.append("W|%s|%d|%s|%s" % (cat, level, es, en))
            words += 1
            continue
        if len(es.split()) >= 4 or (len(es.split()) >= 2 and es[-1] in ".!?"):     # a sentence: becomes word tiles
            text = re.sub(r"[.…]+$", "", es).strip()
            lines.append("S|%s|%d|%s|%s" % (cat, level, text, re.sub(r"[.…]+$", "", en)))
            sentences += 1
        else:
            lines.append("W|%s|%d|%s|%s" % (cat, level, es, en))
            words += 1
    os.makedirs(OUT_DIR, exist_ok=True)
    name = re.sub(r"[^A-Za-z0-9_-]+", "_", os.path.splitext(os.path.basename(path))[0]).strip("_") or "deck"
    if len(a.deck) > 1:
        name = re.sub(r"_?Part_\d+_of_\d+$", "", name, flags=re.I) or name     # one file for all the parts
    out = os.path.join(OUT_DIR, name + ".txt")
    with io.open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("# imported from " + os.path.basename(path) + "\n" + "\n".join(lines) + "\n")
    print("Read %d words and %d sentences -> %s" % (words, sentences, out))
    subprocess.call([sys.executable, os.path.join(HERE, "build_spanish_pack.py")])


if __name__ == "__main__":
    main()
