"""Converts the Spanish Anki decks we started from into the game's word lists (Tools/data/imported/*.txt).

Each deck has its own card layout, so each has its own small reader below. Everything is cleaned the
same way: Anki formatting and sounds removed, bullets/quotes stripped, only the first English meaning
kept, junk skipped, and repeats removed (also across decks, ignoring capitals and punctuation).

Run once with the .apkg files in the project folder:  python Tools/data/convert_decks.py
(the decks themselves are not needed after that - the game only uses the .txt files).
For other decks use import_deck.py, which guesses the layout.
"""
import glob
import json
import os
import re
import sqlite3
import subprocess
import sys
import tempfile
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(HERE, "imported")
sys.path.insert(0, HERE)
from import_deck import clean, level_for, topic_for   # noqa: E402

ENGLISH_MARK = re.compile(r"(the|a|an|to|of|is|are|and|you|i|it|in|for|with|my|this|that|he|she|we|they|was|do|not|have)", re.I)
SPANISH_MARK = re.compile(r"[áéíóúñü¿¡]|\b(el|la|los|las|que|de|y|es|en|no|un|una|por|para|con|se|lo|me|te)\b", re.I)


def notes_of(path):
    """(fields by name, tags) for every note in an Anki package."""
    with zipfile.ZipFile(path) as z:
        names = z.namelist()
        db = next((n for n in ("collection.anki21", "collection.anki2") if n in names), None)
        tmp = tempfile.mkdtemp()
        z.extract(db, tmp)
    con = sqlite3.connect(os.path.join(tmp, db))
    models = json.loads(con.execute("select models from col").fetchone()[0])
    fnames = {int(k): (m["name"], [f["name"] for f in m["flds"]]) for k, m in models.items()}
    out = []
    for flds, tags, mid in con.execute("select flds, tags, mid from notes order by id"):
        model, names_ = fnames.get(mid, ("", []))
        vals = flds.split("\x1f")
        out.append((model, {n: vals[i] if i < len(vals) else "" for i, n in enumerate(names_)}, tags))
    con.close()
    return out


def tidy(text):
    """Clean a field and strip bullets, quotes and dashes at the ends."""
    t = clean(text)
    t = re.sub(r"^[\s•·\-–—*\"“”'«»]+", "", t)
    t = re.sub(r"[\s\"“”'«»\-–—]+$", "", t)
    t = re.sub(r"([¿¡])\s+", r"\1", t)
    t = re.sub(r"\s+([?!.,;:])", r"\1", t)
    return re.sub(r"\s+", " ", t).strip()


def sentence(es, en):
    """A cleaned (Spanish, English) sentence pair, or None if it isn't a good one for the game."""
    es, en = tidy(es), tidy(en)
    es = re.sub(r"[.…]+$", "", es).strip()          # keep ? and ! (they match the opening ¿ and ¡)
    en = re.sub(r"[.…]+$", "", en).strip()
    if not es or not en or es.lower() == en.lower():
        return None
    if re.search(r"[áéíóúñ¿¡]", en) or len(SPANISH_MARK.findall(en)) > len(ENGLISH_MARK.findall(en)) + 1:
        return None                                    # the "English" side is really Spanish
    if re.search(r"[|/\[\]{}=<>_]|\d{3,}|^[A-ZÁÉÍÓÚ][a-záéíóú]+:", es):      # drill blanks, labels, ids, alternatives
        return None
    n = len(es.split())
    if n < 2 or n > 14 or len(es) > 90 or len(en) > 110:
        return None
    if not SPANISH_MARK.search(es) and SPANISH_MARK.search(en):              # the sides are swapped
        es, en = en, es
    if es.endswith("?") and "¿" not in es and es.count("?") == 1:
        es = "¿" + es
    if es.endswith("!") and "¡" not in es and es.count("!") == 1:
        es = "¡" + es
    return es, en


def word(es, en):
    es, en = re.sub(r"\s*\([^)]*\)", "", tidy(es)).strip(), re.sub(r"\s*\([^)]*\)", "", tidy(en)).strip()
    en = re.split(r"\s*[,;/]\s*", en)[0].strip()                             # first meaning only
    es = re.sub(r"^(El|La|Los|Las|Un|Una) ", lambda m: m.group(1).lower() + " ", es)
    english_es = bool(re.match(r"(to|the|a|an) \w", es, re.I)) and not re.search(r"[áéíóúñ]", es)
    spanish_en = bool(re.match(r"(el|la|los|las) \w", en, re.I)) or bool(re.search(r"[ñ¿¡]", en))
    if english_es or (spanish_en and not re.search(r"[áéíóúñ¿¡]", es)):
        es, en = en, es
        es = re.sub(r"^(El|La|Los|Las|Un|Una) ", lambda m: m.group(1).lower() + " ", es)
    if not SPANISH_MARK.search(es) and SPANISH_MARK.search(en) and not re.search(r"[áéíóúñ]", es):
        es, en = en, es                                  # only the "English" side looks Spanish: swap
    if re.match(r"(to|the) \w", es, re.I):
        return None
    if es.endswith("?") and "¿" not in es: es = "¿" + es
    if es.endswith("!") and "¡" not in es: es = "¡" + es
    if not es or not en or len(es) > 40 or len(en) > 50 or re.search(r"[|\[\]{}=<>_\d]", es + en):
        return None
    return es, en


def looks_like_sentence(es):
    return len(es.split()) >= 3 or es.rstrip()[-1:] in ".!?" or es[:1] in "¿¡"


# ---------------------------------------------------------------- one reader per deck
def deck_1001(path):
    """High intermediate: a word card (English front, Spanish back) followed by its example sentences."""
    notes = notes_of(path)
    out = []
    for i, (_, f, _) in enumerate(notes):
        en, es = f.get("Front", ""), f.get("Back", "")
        level = 3 if i < len(notes) / 2 else 4
        if looks_like_sentence(tidy(es)):
            s = sentence(es, en)
            if s: out.append(("S", "imported", level) + s)
        else:
            w = word(es, en)
            if w: out.append(("W", topic_for(*w), level) + w)
    return out


def deck_phrasal(path):
    """Phrasal verbs: English sentences on the front, Spanish on the back. Advanced."""
    out = []
    for _, f, _ in notes_of(path):
        s = sentence(f.get("Back", ""), f.get("Front", ""))
        if s: out.append(("S", "verb", 4 if len(s[0].split()) <= 8 else 5) + s)
    return out


def deck_575(path):
    """Advanced verbs: a verb card (Spanish front) followed by usage sentences (Spanish front)."""
    notes = notes_of(path)
    out = []
    for i, (_, f, _) in enumerate(notes):
        es, en = f.get("Front", ""), f.get("Back", "")
        level = 4 if i < len(notes) / 2 else 5
        if looks_like_sentence(tidy(es)):
            s = sentence(es, en)
            if s: out.append(("S", "verb", level) + s)
        else:
            w = word(es, en)
            if w: out.append(("W", "verb" if w[1].lower().startswith("to ") else topic_for(*w), level) + w)
    return out


def deck_9000(path):
    """9000 Tatoeba sentences sorted from easiest to hardest: levels by position."""
    notes = notes_of(path)
    out = []
    for i, (_, f, _) in enumerate(notes):
        s = sentence(f.get("SpanishSentence", ""), f.get("EnglishSentence", ""))
        if s: out.append(("S", "imported", 1 + i * 5 // len(notes)) + s)
    return out


def deck_fsi(path):
    """FSI Basic Course drills: the full answer sentence; its English is the last sentence of 'extras'."""
    out = []
    for model, f, tags in notes_of(path):
        if "Replacement" not in model:
            continue
        es = tidy(f.get("Answer", ""))
        parts = [p.strip() for p in re.split(r"(?<=[.?!])(?=\s*[A-Z¿¡\"])", tidy(f.get("extras", ""))) if p.strip()]
        if len(parts) != 2:
            continue
        vol = re.search(r"Volume(\d)", tags)
        level = {1: 1, 2: 2, 3: 2, 4: 3}.get(int(vol.group(1)) if vol else 2, 2)
        s = sentence(es, parts[1])
        if s: out.append(("S", "phrase" if level == 1 else "imported", level) + s)
    return out


def deck_7000(path):
    """Intermediate/advanced sentences: levels 3-5 by position."""
    notes = notes_of(path)
    out = []
    for i, (_, f, _) in enumerate(notes):
        s = sentence(f.get("sentence", ""), f.get("english translation", ""))
        if s: out.append(("S", "imported", 3 + i * 3 // len(notes)) + s)
    return out


# ---------------------------------------------------------------- the second batch of decks
def no_parens(text):
    return re.sub(r"\s*\([^)]*\)", "", text).strip()


def undouble(text):
    """Some decks write the English twice ("Why? Why?"): keep one."""
    t = text.strip()
    half = len(t) // 2
    for cut in range(half - 2, half + 3):
        a, b = t[:cut].strip(), t[cut:].strip()
        if a and a == b:
            return a
    return t


def sides(a, b):
    """(Spanish, English) from two fields, whichever way round the card has them."""
    ta, tb = tidy(a), tidy(b)
    score = lambda t: len(SPANISH_MARK.findall(t)) * 2 - len(ENGLISH_MARK.findall(t)) * 2 + len(re.findall(r"[áéíóúñ¿¡]", t)) * 3
    return (a, b) if score(ta) >= score(tb) else (b, a)


def card(es, en, level, cat="imported"):
    """A word or a sentence, whichever the Spanish looks like."""
    es, en = tidy(no_parens(tidy(es))), tidy(en)          # notes in brackets out first, then trim the ends
    if not es:
        return None
    n = len(es.split())
    if n >= 5 or (n >= 2 and (es[-1:] in ".!?" or es[:1] in "¿¡")):
        s_ = sentence(es, en)
        return ("S", cat, level) + s_ if s_ else None
    w = word(es, no_parens(en))
    if not w:
        return None
    adj = "(adj" in tidy(en) or "(adj" in tidy(es)
    return ("W", "adjective" if adj else (cat if cat != "imported" else topic_for(*w)), level) + w


def orientation(notes, a_name, b_name):
    """For each note type: True if field a holds the Spanish (decided by the whole deck, not card by card)."""
    votes = {}
    score = lambda t: len(SPANISH_MARK.findall(t)) * 2 - len(ENGLISH_MARK.findall(t)) * 2 + len(re.findall(r"[áéíóúñ¿¡]", t)) * 3 \
        - (4 if re.match(r"to \w", t) else 0)
    for model, f, _ in notes:
        a, b = tidy(f.get(a_name, "")), tidy(f.get(b_name, ""))
        votes[model] = votes.get(model, 0) + (1 if score(a) > score(b) else -1 if score(b) > score(a) else 0)
    return {m: v >= 0 for m, v in votes.items()}


def deck_advanced(path):
    """Advanced Spanish: words and phrases (Spanish front)."""
    notes = notes_of(path)
    way = orientation(notes, "Front", "Back")
    out = []
    for i, (model, f, _) in enumerate(notes):
        if "Spanish" in f:
            es, en = f.get("Spanish", ""), f.get("English", "")
        else:
            es, en = (f.get("Front", ""), f.get("Back", "")) if way.get(model, True) else (f.get("Back", ""), f.get("Front", ""))
        c = card(es, en, 4 if i < len(notes) / 2 else 5)
        if c: out.append(c)
    return out


def deck_examples(path):
    """Advanced Spanish Examples: sentences grouped by a root word with its frequency per million (FPM)."""
    out = []
    for _, f, _ in notes_of(path):
        try:
            fpm = float(tidy(f.get("FPM", "")) or 0)
        except ValueError:
            fpm = 0
        level = 3 if fpm >= 50 else 4 if fpm >= 10 else 5
        s_ = sentence(f.get("Spanish", ""), f.get("English", ""))
        if s_: out.append(("S", "imported", level) + s_)
    return out


def deck_mixed(path, level=4):
    """Decks with Front/Back (either way round), cloze Text/Extra, or Spanish/English fields."""
    notes = notes_of(path)
    way = orientation(notes, "Front", "Back")
    out = []
    for model, f, tags in notes:
        if "Text" in f:                                  # cloze: Spanish text (answers filled in), English in Extra
            es, en = f.get("Text", ""), f.get("Extra", "")
        elif "Spanish" in f:
            es, en = f.get("Spanish", ""), f.get("English", "")
        else:
            es, en = (f.get("Front", ""), f.get("Back", "")) if way.get(model, True) else (f.get("Back", ""), f.get("Front", ""))
        cat = "verb" if "verbs" in tags.split() else "imported"
        c = card(es, en, level, cat)
        if c: out.append(c)
    return out


def deck_assimil(path):
    """Assimil Using Spanish: Spanish sentences with the English written twice. Advanced."""
    out = []
    for _, f, _ in notes_of(path):
        es_t = tidy(f.get("Front", ""))
        n = max(1, len(re.findall(r"[.!?…]+(?=\s|$)", es_t)))
        parts = re.split(r"(?<=[.!?…])\s+", undouble(tidy(f.get("Back", ""))))
        s_ = sentence(es_t, " ".join(parts[:n]))
        if s_: out.append(("S", "imported", 5 if len(s_[0].split()) > 8 else 4) + s_)
    return out


def deck_beginner_advanced(path):
    """Beginner to Advanced: ranked words (with example sentences), two-sided cards with sentences,
    and Flashcard Space cards. The conjugation drills and picture cards are skipped."""
    out = []
    for model, f, tags in notes_of(path):
        if "Ranking" in f and "Spanish" in f:
            try:
                rank = int(re.sub(r"\D", "", tidy(f.get("Ranking", ""))) or 5000)
            except ValueError:
                rank = 5000
            level = level_for(rank, 10000)
            c = card(f["Spanish"], f.get("English", ""), level)
            if c: out.append(c)
            s_ = sentence(f.get("Example-Spanish", ""), f.get("Example-English", ""))
            if s_: out.append(("S", c[1] if c else "imported", level) + s_)
        elif "Sentence-Front" in f:
            es, en = sides(f.get("Front", ""), f.get("Back", ""))
            c = card(es, en, 3)
            if c: out.append(c)
            for a, b in (("Sentence-Front", "Sentence-Back"), ("Sentence2-Front", "Sentence2-Back")):
                se, sn = sides(f.get(a, ""), f.get(b, ""))
                s_ = sentence(se, sn)
                if s_: out.append(("S", "imported", 3) + s_)
        elif "FrontText" in f:
            es, en = sides(f.get("FrontText", ""), f.get("BackText", ""))
            c = card(es, en, 2)
            if c: out.append(c)
            s_ = sentence(f.get("SentenceExample", ""), f.get("SentenceExampleTranslation", ""))
            if s_: out.append(("S", "imported", 2) + s_)
    return out


READERS = [
    ("1001_High_Intermediate*.apkg", "1001_High_Intermediate", deck_1001),
    ("550_Phrasal_Verbs*.apkg", "550_Phrasal_Verbs", deck_phrasal),
    ("575_Advanced_Spanish_Verbs*.apkg", "575_Advanced_Verbs", deck_575),
    ("9000_Spanish_sentences*.apkg", "9000_Spanish_Sentences", deck_9000),
    # FSI_Spanish_Basic_Course: skipped - its drill cards' English doesn't line up with the Spanish
    ("Spanish_7000*.apkg", "Spanish_7000_Sentences", deck_7000),
    ("Advanced_Spanish.apkg", "Advanced_Spanish", deck_advanced),
    ("Advanced_Spanish_Examples*.apkg", "Advanced_Spanish_Examples", deck_examples),
    ("Advanced_Spanish_Phrases_and_Vocabulary*.apkg", "Advanced_Spanish_Phrases", lambda p: deck_mixed(p, 4)),
    ("Advanced_Spanish_Sentences*.apkg", "Advanced_Spanish_Sentences", lambda p: deck_mixed(p, 4)),
    ("Advanced_Spanish_Vocabulary.apkg", "Advanced_Spanish_Vocabulary", lambda p: deck_mixed(p, 4)),
    ("Advanced_Spanish_Vocabulary_and_Phrases*.apkg", "Advanced_Spanish_Vocab_Phrases", lambda p: deck_mixed(p, 4)),
    ("Assimil_Using_Spanish*.apkg", "Assimil_Using_Spanish", deck_assimil),
    ("Beginner_to_Advanced_Spanish*.apkg", "Beginner_to_Advanced", deck_beginner_advanced),
    # C1C2 idioms: skipped - its explanations are in Spanish only (the game needs the English)
]


def key(text):
    """Text for spotting repeats: no capitals, punctuation or extra spaces."""
    return re.sub(r"\s+", " ", re.sub(r"[^\w\s]", "", text.lower())).strip()


def main():
    os.makedirs(OUT, exist_ok=True)
    # what the game already has (built-in lists and earlier imports), so nothing is added twice
    pack = json.load(open(os.path.join(ROOT, "Assets", "Resources", "Languages", "es", "pack.json"), encoding="utf-8"))
    have_w = {key(w["word"]) for w in pack["words"]}
    have_s = {key(s["text"]) for s in pack["sentences"]}
    mine = {name for pattern, name, _ in READERS if glob.glob(os.path.join(ROOT, pattern))}
    for f in glob.glob(os.path.join(OUT, "*.txt")):          # re-running replaces our own files, not other imports
        if os.path.splitext(os.path.basename(f))[0] in mine:
            for line in open(f, encoding="utf-8"):
                p = line.rstrip("\n").split("|")
                if len(p) >= 5:
                    (have_w if p[0] == "W" else have_s).discard(key(p[3]))
    total_w = total_s = 0
    for pattern, name, reader in READERS:
        paths = sorted(glob.glob(os.path.join(ROOT, pattern)))
        if not paths:
            continue
        rows, raw = [], 0
        for p in paths:
            got = reader(p)
            raw += len(got)
            rows += got
        lines, nw, ns = [], 0, 0
        for kind, cat, level, es, en in rows:
            k = key(es)
            seen = have_w if kind == "W" else have_s
            if not k or k in seen:
                continue
            seen.add(k)
            lines.append("%s|%s|%d|%s|%s" % (kind, cat, level, es, en))
            if kind == "W": nw += 1
            else: ns += 1
        with open(os.path.join(OUT, name + ".txt"), "w", encoding="utf-8", newline="\n") as f:
            f.write("# converted from " + ", ".join(os.path.basename(p) for p in paths) + "\n" + "\n".join(lines) + "\n")
        print("%-26s %6d good cards -> %5d new words, %5d new sentences (repeats removed)" % (name, raw, nw, ns))
        total_w += nw
        total_s += ns
    print("TOTAL NEW: %d words, %d sentences" % (total_w, total_s))
    subprocess.call([sys.executable, os.path.join(HERE, "build_spanish_pack.py")])


if __name__ == "__main__":
    main()
