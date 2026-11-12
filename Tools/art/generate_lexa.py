"""The Lexa currency icon: a grey rune stone with glowing blue carvings.

Run:  python Tools/art/generate_lexa.py
Writes Art/UI/lexa.png (16x16, used everywhere money is shown) and Art/UI/lexa_big.png (32x32).
"""
from PIL import Image, ImageDraw

from generate_art import save
from generate_items import finish


def lexa(size):
    k = size // 16
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # a rough, rounded stone
    d.polygon([(4, 1), (11, 1), (14, 4), (15, 10), (12, 15), (4, 15), (1, 11), (1, 5)], fill=(132, 132, 140))
    d.polygon([(4, 2), (10, 2), (12, 4), (6, 6), (2, 6)], fill=(170, 170, 178))       # lit top face
    for x, y in ((5, 9), (11, 7), (9, 13), (3, 12)):                                   # speckles in the rock
        d.point((x, y), fill=(104, 104, 112))
    # the rune carving (like the fountain's runes), glowing blue
    glow = (110, 220, 255)
    d.line([(7, 4), (7, 12)], fill=glow)
    d.line([(7, 5), (10, 7)], fill=glow)
    d.line([(10, 7), (7, 9)], fill=glow)
    d.line([(7, 9), (10, 12)], fill=glow)
    d.point((8, 5), fill=(200, 245, 255))
    img = finish(img, outline=(52, 48, 58), light=0.25, dark=0.8)
    return img.resize((16 * k, 16 * k), Image.NEAREST)


if __name__ == "__main__":
    save(lexa(16), "UI", "lexa")
    save(lexa(32), "UI", "lexa_big")
    print("lexa icon written")
