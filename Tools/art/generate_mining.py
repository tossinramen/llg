"""La mina: the mine entrance, rocks to mine (and their rubble), a mine cart, ores, gems, relics,
pickaxes and the miner's helmet.

Run:  python Tools/art/generate_mining.py      (after generate_items.py; before generate_village.py)
"""
import random

from PIL import Image, ImageDraw

from generate_art import save
from generate_items import canvas, icon


def lump(name, base, light, dark, specks=None, shape=None):
    """A rough nugget/ore lump with highlights and coloured specks."""
    img, d = canvas()
    pts = shape or [(3, 9), (5, 4), (10, 3), (13, 7), (12, 12), (6, 13)]
    d.polygon(pts, fill=base)
    d.line([pts[0], pts[1], pts[2]], fill=light)
    d.line([pts[3], pts[4], pts[5]], fill=dark)
    for x, y, c in specks or []:
        d.point((x, y), fill=c)
    icon(name, img)


def gem(name, base, light, dark):
    img, d = canvas()
    d.polygon([(4, 5), (8, 2), (12, 5), (8, 14)], fill=base)
    d.polygon([(4, 5), (8, 2), (8, 6)], fill=light)
    d.polygon([(8, 6), (12, 5), (8, 14)], fill=dark)
    d.line([(4, 5), (12, 5)], fill=light)
    d.point((6, 4), fill=(255, 255, 255))
    icon(name, img)


def ores():
    lump("ore_stone", (140, 134, 128), (180, 176, 170), (96, 90, 86))
    lump("ore_coal", (50, 48, 52), (100, 98, 104), (26, 24, 28), [(7, 7, (120, 120, 130)), (10, 9, (90, 90, 100))])
    lump("ore_copper", (120, 110, 100), (160, 150, 140), (80, 72, 66),
         [(6, 6, (220, 120, 60)), (9, 8, (230, 140, 70)), (11, 6, (210, 110, 50)), (7, 10, (220, 130, 60))])
    lump("ore_iron", (110, 104, 100), (150, 146, 140), (70, 66, 64),
         [(6, 7, (190, 130, 110)), (9, 5, (200, 140, 120)), (10, 10, (180, 120, 100))])
    lump("ore_silver", (190, 196, 206), (236, 240, 246), (130, 136, 150), [(7, 6, (255, 255, 255)), (10, 9, (255, 255, 255))])
    lump("ore_gold", (230, 186, 60), (255, 230, 120), (170, 120, 30), [(7, 6, (255, 250, 200))],
         shape=[(4, 9), (6, 5), (10, 4), (12, 8), (10, 12), (6, 12)])
    gem("gem_emerald", (60, 180, 90), (150, 236, 160), (30, 110, 60))
    gem("gem_ruby", (210, 40, 60), (255, 130, 140), (130, 20, 40))
    gem("gem_sapphire", (50, 90, 220), (140, 170, 255), (30, 50, 140))
    gem("gem_diamond", (210, 236, 250), (255, 255, 255), (140, 180, 210))


def relics():
    img, d = canvas()                                                       # fossil: a spiral shell in stone
    d.ellipse([2, 3, 14, 14], fill=(176, 160, 130))
    for r in (5, 3, 1):
        d.arc([8 - r, 8 - r, 8 + r, 8 + r], 0, 300, fill=(110, 96, 70))
    icon("relic_fossil", img)
    img, d = canvas()                                                       # Roman coin
    d.ellipse([3, 3, 13, 13], fill=(200, 160, 70))
    d.ellipse([5, 5, 11, 11], outline=(150, 110, 40))
    d.line([(7, 6), (7, 10)], fill=(150, 110, 40)); d.line([(9, 6), (9, 10)], fill=(150, 110, 40))
    icon("relic_coin", img)
    img, d = canvas()                                                       # ancient vase (ánfora)
    d.polygon([(6, 2), (10, 2), (10, 4), (13, 8), (11, 14), (5, 14), (3, 8), (6, 4)], fill=(190, 110, 70))
    d.line([(4, 8), (12, 8)], fill=(60, 40, 30)); d.line([(5, 10), (11, 10)], fill=(240, 220, 180))
    d.arc([1, 3, 6, 8], 90, 270, fill=(160, 90, 60)); d.arc([10, 3, 15, 8], 270, 90, fill=(160, 90, 60))
    icon("relic_vase", img)
    img, d = canvas()                                                       # flint arrowhead
    d.polygon([(8, 1), (12, 9), (10, 9), (9, 14), (7, 14), (6, 9), (4, 9)], fill=(120, 110, 120))
    d.line([(8, 2), (8, 12)], fill=(170, 160, 172))
    icon("relic_arrowhead", img)


def tools():
    for name, head, light in (("item_pickaxe", (130, 130, 136), (180, 180, 186)), ("item_pickaxe_iron", (150, 150, 160), (210, 210, 220)),
                              ("item_pickaxe_steel", (100, 140, 170), (170, 210, 236)), ("item_pickaxe_diamond", (150, 220, 240), (240, 255, 255))):
        img, d = canvas()
        d.line([(3, 14), (11, 4)], fill=(140, 96, 60), width=2)                 # handle
        d.arc([2, 0, 15, 12], 200, 340, fill=head, width=3)                     # head
        d.line([(4, 3), (12, 2)], fill=light)
        icon(name, img)
    img, d = canvas()                                                       # miner's helmet with a lamp
    d.chord([2, 4, 14, 16], 180, 360, fill=(230, 190, 60))
    d.rectangle([1, 9, 15, 11], fill=(200, 160, 40))
    d.ellipse([6, 4, 10, 8], fill=(255, 250, 200), outline=(120, 100, 40))
    d.line([(4, 7), (5, 6)], fill=(255, 236, 140))
    icon("gear_miner_helmet", img)


def rock(kind, seed):
    """A boulder to mine. kind: stone, rich (orange-brown veins) or crystal (glinting gems)."""
    rng = random.Random(seed)
    img = Image.new("RGBA", (28, 24), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([2, 18, 26, 23], fill=(10, 8, 6, 80))
    base = {"stone": (128, 122, 116), "rich": (120, 100, 84), "crystal": (96, 100, 120)}[kind]
    light = tuple(min(255, c + 40) for c in base)
    dark = tuple(max(0, c - 40) for c in base)
    d.polygon([(2, 20), (4, 9), (10, 3), (18, 2), (25, 8), (26, 20)], fill=base)
    d.polygon([(4, 9), (10, 3), (18, 2), (14, 9)], fill=light)
    d.polygon([(25, 8), (26, 20), (16, 20), (18, 12)], fill=dark)
    d.line([(10, 10), (14, 15), (12, 19)], fill=dark)
    if kind == "rich":
        for _ in range(9):
            x, y = rng.randint(6, 22), rng.randint(6, 18)
            d.point((x, y), fill=(230, 140, 60)); d.point((x + 1, y), fill=(250, 200, 90))
    if kind == "crystal":
        for x, y, c in ((8, 12, (80, 220, 120)), (17, 7, (230, 60, 80)), (20, 14, (90, 140, 255)), (12, 6, (230, 250, 255))):
            d.polygon([(x, y - 3), (x + 2, y), (x, y + 3), (x - 2, y)], fill=c)
            d.point((x, y - 2), fill=(255, 255, 255))
    save(img, "World/Objects", "rock_" + kind)


def rubble():
    img = Image.new("RGBA", (28, 24), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([3, 18, 25, 23], fill=(10, 8, 6, 60))
    for x, y, r in ((6, 18, 3), (12, 19, 4), (19, 18, 3), (15, 15, 2), (9, 15, 2), (22, 19, 2)):
        d.ellipse([x - r, y - r, x + r, y + r], fill=(110, 104, 98), outline=(70, 66, 62))
    save(img, "World/Objects", "rock_rubble")


def mine_entrance():
    """A rocky hillside with a timber-framed tunnel, rails coming out and a lantern."""
    w, h = 144, 80
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rng = random.Random(7)
    d.ellipse([0, 64, 143, 79], fill=(20, 24, 14, 70))
    d.polygon([(0, 74), (10, 40), (30, 18), (56, 6), (84, 4), (112, 14), (132, 34), (143, 74)], fill=(122, 112, 100))
    d.polygon([(10, 40), (30, 18), (56, 6), (60, 30), (30, 50)], fill=(150, 140, 126))
    d.polygon([(112, 14), (132, 34), (143, 74), (110, 74), (104, 40)], fill=(96, 88, 80))
    for _ in range(70):                                                     # boulders and cracks on the hill
        x, y = rng.randint(8, 136), rng.randint(14, 70)
        if img.getpixel((x, y))[3] == 0:
            continue
        r = rng.randint(2, 5)
        c = rng.choice([(140, 130, 118), (108, 100, 90), (160, 150, 136)])
        d.ellipse([x - r, y - r // 2 - 1, x + r, y + r // 2 + 1], fill=c)
    for _ in range(26):                                                     # grass tufts on top
        x = rng.randint(20, 124)
        y = 10 + abs(x - 70) // 3
        d.line([(x, y), (x + 1, y - 3)], fill=(80, 130, 60))
    # the tunnel with a timber frame
    d.rectangle([52, 34, 92, 74], fill=(24, 18, 16))
    d.polygon([(52, 34), (72, 26), (92, 34)], fill=(24, 18, 16))
    for x in (48, 92):
        d.rectangle([x, 30, x + 4, 74], fill=(130, 90, 56), outline=(80, 54, 34))
    d.rectangle([44, 26, 100, 31], fill=(150, 104, 64), outline=(80, 54, 34))
    d.line([(46, 27), (98, 27)], fill=(180, 134, 90))
    # rails coming out
    for x in (62, 80):
        d.line([(x, 52), (x - (4 if x < 70 else -4), 76)], fill=(120, 120, 130), width=2)
    for y in range(56, 78, 5):
        d.line([(58 - (y - 52) // 6, y), (86 + (y - 52) // 6, y)], fill=(110, 76, 50), width=2)
    # a lantern by the door and a little sign
    d.line([(100, 36), (106, 36)], fill=(60, 50, 40))
    d.rectangle([104, 37, 108, 43], fill=(255, 210, 100), outline=(60, 50, 40))
    d.rectangle([20, 54, 42, 64], fill=(170, 124, 80), outline=(90, 62, 40))
    d.line([(31, 64), (31, 74)], fill=(110, 76, 50), width=2)
    d.line([(24, 58), (38, 58)], fill=(70, 50, 34)); d.line([(24, 61), (35, 61)], fill=(70, 50, 34))
    save(img, "World/Objects", "mine_entrance")


def cart():
    img = Image.new("RGBA", (24, 20), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([2, 16, 22, 19], fill=(10, 8, 6, 70))
    d.polygon([(2, 6), (22, 6), (20, 15), (4, 15)], fill=(110, 76, 50), outline=(70, 48, 32))
    d.line([(3, 9), (21, 9)], fill=(140, 100, 66))
    for x, y in ((6, 4), (11, 3), (16, 4), (9, 5), (14, 5)):
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(120, 112, 104))
    d.point((11, 2), fill=(240, 196, 70))
    for x in (6, 17):
        d.ellipse([x - 2, 14, x + 2, 18], fill=(50, 46, 50))
    save(img, "World/Objects", "mine_cart")


if __name__ == "__main__":
    ores()
    relics()
    tools()
    for i, k in enumerate(("stone", "rich", "crystal")):
        rock(k, i)
    rubble()
    mine_entrance()
    cart()
    print("mining art written")
