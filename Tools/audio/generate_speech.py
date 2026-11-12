"""Records every word, sentence and line of dialogue as speech, in Spanish and English.

Run:  python Tools/audio/generate_speech.py

It collects the text from the word pack, the map labels, the item list and the villagers'
dialogue, then uses the voices built into Windows (speak.ps1) to make one WAV per line in
Assets/Resources/Audio/Speech/es and /en. Files that already exist are skipped, so it is quick
to run again after adding new lines.

Spanish needs a Spanish voice installed in Windows:
  Settings > Time & language > Speech > Add voices > "Español (España)"
then run this script again. English uses the voices Windows already has.

Each file is named after the first 12 characters of the MD5 hash of its text; the game
(Assets/Scripts/Core/Speech.cs) works out the same name to find it.
"""
import glob
import hashlib
import json
import os
import re
import subprocess

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
OUT = os.path.join(ROOT, "Assets", "Resources", "Audio", "Speech")
SCRIPTS = os.path.join(ROOT, "Assets", "Scripts")
STR = r'"((?:[^"\\]|\\.)*)"'


def key(text):
    return hashlib.md5(text.strip().encode("utf-8")).hexdigest()[:12]


def collect():
    lines = set()      # (language, text)

    def add(es, en):
        if es and es.strip():
            lines.add(("es", es.strip()))
        if en and en.strip():
            lines.add(("en", en.strip()))

    pack = json.load(open(os.path.join(ROOT, "Assets", "Resources", "Languages", "es", "pack.json"), encoding="utf-8"))
    for w in pack["words"]:
        add(w["word"], w["english"])
    for s in pack["sentences"]:
        add(s["text"], s["english"])
    world = json.load(open(os.path.join(ROOT, "Assets", "Resources", "Maps", "village_objects.json"), encoding="utf-8"))
    for w in world.get("words", []):
        add(w["es"], w["en"])

    for path in glob.glob(os.path.join(SCRIPTS, "**", "*.cs"), recursive=True):
        src = open(path, encoding="utf-8").read()
        # dialogue lines: L("who", "es", "en") and new Line("who", "es", "en")
        for m in re.finditer(r'(?:\bL|new Line)\(\s*' + STR + r'\s*,\s*' + STR + r'\s*,\s*' + STR + r'\s*\)', src):
            add(m.group(2), m.group(3))
        # one-off messages: dialogue.Show("", "es", "en")
        for m in re.finditer(r'dialogue\.Show\(\s*""\s*,\s*' + STR + r'\s*,\s*' + STR + r'\s*\)', src):
            add(m.group(1), m.group(2))
        # items: new ItemDef("id", "es", "en", ...)
        for m in re.finditer(r'new ItemDef\(\s*' + STR + r'\s*,\s*' + STR + r'\s*,\s*' + STR, src):
            add(m.group(2), m.group(3))
        # villager roles: AddVillager('x', "Name", "role", "Role")
        for m in re.finditer(r"AddVillager\('.'\s*,\s*" + STR + r'\s*,\s*' + STR + r'\s*,\s*' + STR, src):
            add(m.group(2), m.group(3))
        # mob names: spanish = "El lobo"; english = "Wolf"
        for m in re.finditer(r'spanish = ' + STR + r'; m\.english = ' + STR, src):
            add(m.group(1), m.group(2))
    return sorted(lines)


def main():
    lines = collect()
    tsv = os.path.join(os.path.dirname(os.path.abspath(__file__)), "speech_lines.tsv")
    with open(tsv, "w", encoding="utf-8", newline="\n") as f:
        for lang, text in lines:
            f.write(f"{lang}\t{key(text)}\t{text}\n")
    print(len(lines), "lines to record")
    ps1 = os.path.join(os.path.dirname(os.path.abspath(__file__)), "speak.ps1")
    subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", ps1, "-List", tsv, "-OutDir", OUT], check=False)


if __name__ == "__main__":
    main()
