"""The dungeon (La Ciudadela del Sol), its 10 bosses, better dogs and cats, and farm gates.

Run:  python Tools/art/generate_dungeon.py      (after generate_items.py; before generate_village.py)
"""
import math

from PIL import Image, ImageDraw

from generate_art import save
from generate_items import finish


def rgba(c, a=255):
    return tuple(c[:3]) + (a,)


# ---------------------------------------------------------------- bosses (32 x 32, shown big in battle)
def boss_canvas():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def bosses():
    out = {}
    img, d = boss_canvas()                                    # 1 El Guardián de Piedra
    d.rectangle([7, 12, 24, 29], fill=(130, 130, 136))
    d.rectangle([10, 3, 21, 12], fill=(150, 150, 156))
    d.rectangle([2, 13, 7, 25], fill=(120, 120, 126)); d.rectangle([24, 13, 29, 25], fill=(120, 120, 126))
    d.rectangle([12, 6, 14, 8], fill=(110, 230, 255)); d.rectangle([17, 6, 19, 8], fill=(110, 230, 255))
    d.line([(15, 15), (15, 24)], fill=(110, 230, 255)); d.line([(15, 16), (19, 19)], fill=(110, 230, 255)); d.line([(19, 19), (15, 21)], fill=(110, 230, 255))
    out["guardian"] = img

    img, d = boss_canvas()                                    # 2 La Reina Araña
    for k in range(4):
        y = 12 + k * 4
        d.line([(15, 16), (2, y - 4), (0, y + 2)], fill=(60, 40, 70), width=2)
        d.line([(16, 16), (29, y - 4), (31, y + 2)], fill=(60, 40, 70), width=2)
    d.ellipse([8, 12, 23, 28], fill=(80, 50, 100)); d.ellipse([10, 5, 21, 15], fill=(100, 60, 120))
    for x in (12, 14, 17, 19):
        d.point((x, 9), fill=(255, 60, 60))
    d.polygon([(11, 5), (13, 1), (15, 4), (16, 0), (17, 4), (19, 1), (21, 5)], fill=(232, 190, 80))
    d.line([(12, 22), (19, 22)], fill=(200, 40, 60))
    out["spider"] = img

    img, d = boss_canvas()                                    # 3 El Rey Trol
    d.ellipse([5, 10, 26, 31], fill=(96, 140, 80))
    d.ellipse([9, 3, 22, 15], fill=(110, 156, 90))
    d.polygon([(9, 4), (11, 0), (14, 3), (16, -1), (18, 3), (21, 0), (23, 4)], fill=(232, 190, 80))
    d.point((13, 8), fill=(255, 220, 60)); d.point((18, 8), fill=(255, 220, 60))
    d.polygon([(12, 12), (13, 15), (14, 12)], fill=(250, 250, 240)); d.polygon([(17, 12), (18, 15), (19, 12)], fill=(250, 250, 240))
    d.line([(26, 30), (31, 12)], fill=(110, 76, 48), width=3); d.ellipse([27, 8, 32, 14], fill=(110, 76, 48))
    out["troll"] = img

    img, d = boss_canvas()                                    # 4 El Brujo de la Niebla
    for cx, cy, r in ((6, 26, 5), (26, 27, 5), (16, 29, 4)):
        d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(210, 210, 230, 160))
    d.polygon([(16, 4), (26, 30), (6, 30)], fill=(100, 70, 150))
    d.ellipse([11, 6, 21, 16], fill=(70, 50, 110))
    d.point((14, 11), fill=(180, 255, 200)); d.point((18, 11), fill=(180, 255, 200))
    d.line([(26, 30), (28, 6)], fill=(120, 90, 60), width=2); d.ellipse([25, 2, 31, 8], fill=(160, 120, 255))
    out["sorcerer"] = img

    img, d = boss_canvas()                                    # 5 El Rey de Hielo
    d.polygon([(4, 30), (16, 12), (28, 30)], fill=(150, 200, 240))
    d.rectangle([10, 14, 21, 30], fill=(200, 230, 250))
    d.ellipse([10, 5, 21, 16], fill=(220, 240, 255))
    for x in (10, 13, 16, 19, 22):
        d.polygon([(x - 1, 6), (x, 0), (x + 1, 6)], fill=(170, 230, 255))
    d.point((13, 10), fill=(40, 80, 160)); d.point((18, 10), fill=(40, 80, 160))
    d.line([(12, 13), (19, 13)], fill=(240, 250, 255))
    out["iceking"] = img

    img, d = boss_canvas()                                    # 6 La Serpiente Emplumada
    pts = [(6 + 20 * (t / 20), 28 - 6 * math.sin(t / 3.0) - t * 0.6) for t in range(21)]
    for i, (x, y) in enumerate(pts):
        r = 4 if i < 18 else 5
        d.ellipse([x - r, y - r, x + r, y + r], fill=(60, 160, 90) if i % 3 else (240, 200, 60))
    hx, hy = pts[-1]
    for k, c in enumerate(((220, 60, 50), (240, 200, 60), (60, 140, 220))):
        d.polygon([(hx, hy - 3), (hx - 6 + k * 4, hy - 12), (hx - 2 + k * 4, hy - 4)], fill=c)
    d.point((int(hx) + 1, int(hy) - 1), fill=(20, 20, 20))
    out["serpent"] = img

    img, d = boss_canvas()                                    # 7 El Gigante de Lava
    d.polygon([(6, 30), (3, 14), (10, 6), (22, 6), (29, 14), (26, 30)], fill=(60, 44, 40))
    for line in (((8, 26), (12, 18), (10, 12)), ((22, 28), (20, 20), (24, 14)), ((14, 30), (16, 22))):
        d.line(line, fill=(255, 130, 30), width=1)
    d.rectangle([11, 10, 13, 12], fill=(255, 220, 80)); d.rectangle([18, 10, 20, 12], fill=(255, 220, 80))
    d.line([(12, 16), (20, 16)], fill=(255, 130, 30))
    out["lava"] = img

    img, d = boss_canvas()                                    # 8 La Bruja del Espejo
    d.polygon([(16, 10), (27, 31), (5, 31)], fill=(170, 176, 196))
    d.ellipse([11, 8, 21, 18], fill=(236, 220, 210))
    d.polygon([(8, 10), (24, 10), (16, -1)], fill=(70, 70, 90))
    d.point((14, 13), fill=(120, 40, 140)); d.point((18, 13), fill=(120, 40, 140))
    d.ellipse([23, 14, 31, 24], fill=(200, 220, 240), outline=(232, 190, 80))
    d.line([(25, 16), (27, 18)], fill=(255, 255, 255))
    out["witch"] = img

    img, d = boss_canvas()                                    # 9 El Caballero Sin Nombre
    d.rectangle([9, 13, 22, 29], fill=(56, 56, 66))
    d.rectangle([10, 3, 21, 13], fill=(70, 70, 82))
    d.rectangle([12, 7, 19, 8], fill=(220, 40, 40))
    d.polygon([(4, 14), (9, 13), (9, 24), (5, 22)], fill=(46, 46, 56))
    d.line([(24, 30), (29, 4)], fill=(190, 196, 210), width=2); d.line([(22, 22), (28, 23)], fill=(150, 120, 60), width=2)
    out["knight"] = img

    img, d = boss_canvas()                                    # 10 El Rey Demonio
    d.polygon([(0, 8), (8, 16), (6, 26)], fill=(60, 20, 30)); d.polygon([(31, 8), (23, 16), (25, 26)], fill=(60, 20, 30))
    d.ellipse([8, 12, 23, 31], fill=(170, 40, 40))
    d.ellipse([10, 4, 21, 15], fill=(196, 50, 50))
    d.polygon([(10, 6), (6, -1), (12, 4)], fill=(30, 20, 20)); d.polygon([(21, 6), (25, -1), (19, 4)], fill=(30, 20, 20))
    d.polygon([(12, 5), (14, 1), (16, 4), (18, 1), (20, 5)], fill=(232, 190, 80))
    d.point((13, 9), fill=(255, 230, 80)); d.point((18, 9), fill=(255, 230, 80))
    d.line([(13, 12), (18, 12)], fill=(60, 10, 10))
    out["demon"] = img

    for name, im in out.items():
        save(finish(im, outline=(30, 24, 30), light=0.25, dark=0.75), "Bosses", name)


# ---------------------------------------------------------------- the citadel (dungeon entrance)
def citadel():
    w, h = 176, 140
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # the green peak behind (like Huayna Picchu)
    d.polygon([(30, 70), (88, 2), (100, 10), (150, 70)], fill=(70, 120, 70))
    d.polygon([(88, 2), (100, 10), (150, 70), (110, 70)], fill=(52, 96, 56))
    for k in range(14):
        x = 50 + k * 7
        d.line([(x, 70 - (k % 3) * 4), (x + 6, 60 - (k % 4) * 3)], fill=(90, 140, 80))
    # three stone terraces, each with grass on top
    stone, stone_d, stone_l, grass = (156, 148, 132), (118, 110, 98), (190, 182, 164), (110, 160, 80)
    for i, (x0, x1, y0, y1) in enumerate(((8, 168, 108, 136), (22, 154, 84, 108), (38, 138, 62, 84))):
        d.rectangle([x0, y0, x1, y1], fill=stone)
        for yy in range(y0 + 2, y1, 7):                         # stone blocks
            off = ((yy - y0) // 7) % 2 * 6
            for xx in range(x0 + off, x1, 12):
                d.rectangle([xx, yy, xx + 11, yy + 6], outline=stone_d)
                d.line([xx + 1, yy + 1, xx + 9, yy + 1], fill=stone_l)
        d.rectangle([x0, y0 - 3, x1, y0], fill=grass)
        d.line([x0, y0 - 3, x1, y0 - 3], fill=(150, 200, 110))
    # stairs up the middle and the trapezoid doorway at the bottom (classic Inca shape)
    for k in range(10):
        y = 136 - k * 7
        d.rectangle([80 - k, y - 6, 96 + k, y], fill=stone_l if k % 2 else stone)
    d.polygon([(76, 136), (100, 136), (96, 110), (80, 110)], fill=(30, 26, 24))
    d.polygon([(74, 136), (76, 136), (80, 110), (78, 108)], fill=stone_d)
    d.polygon([(102, 136), (100, 136), (96, 110), (98, 108)], fill=stone_d)
    d.rectangle([78, 106, 98, 109], fill=stone_l)
    # a sun disc above the door and small stone huts on the top terrace
    d.ellipse([80, 88, 96, 104], fill=(232, 190, 80), outline=(170, 120, 40))
    d.ellipse([85, 93, 91, 99], fill=(250, 220, 120))
    for hx in (46, 112):
        d.rectangle([hx, 44, hx + 18, 62], fill=stone, outline=stone_d)
        d.polygon([(hx - 2, 44), (hx + 20, 44), (hx + 9, 32)], fill=(170, 140, 80))
        d.polygon([(hx + 6, 62), (hx + 12, 62), (hx + 11, 52), (hx + 7, 52)], fill=(30, 26, 24))
    # glowing runes on the doorway (it's the gate of the word-thieves)
    for x, y in ((70, 122), (104, 122)):
        d.line([(x, y - 5), (x, y + 5)], fill=(110, 230, 255))
        d.line([(x, y - 3), (x + 3, y - 1)], fill=(110, 230, 255))
    shadow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(shadow).ellipse([4, 128, 172, 140], fill=(20, 28, 16, 70))
    shadow.alpha_composite(img)
    save(shadow, "World/Objects", "citadel")


# ---------------------------------------------------------------- dogs and cats, redrawn
def pets():
    def draw(rows, pal, name):
        img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch != ".":
                    img.putpixel((x, y), pal[ch] + (255,))
        img = finish(img, outline=(52, 36, 32), light=0.2, dark=0.8)
        save(img, "Icons", name)
        hop = Image.new("RGBA", img.size, (0, 0, 0, 0))
        hop.paste(img, (0, -1))
        save(hop, "Icons", name + "_hop")

    dog = [
        "................",
        ".........dd.....",
        "........dbbbb...",
        "........dbbbbbb.",
        "........dbebbbbn",
        "........ddbbmmmm",
        "t.......ddbbmmm.",
        "tt.......bbbb...",
        ".tbbbbbbbbbbb...",
        "..bbbbbbbbbbbl..",
        "..bbbbbbbbbbll..",
        "..bbbbbbbbbbl...",
        "..bb.bb...bb.bb.",
        "..bb.bb...bb.bb.",
        "..ff.ff...ff.ff.",
        "................",
    ]
    draw(dog, {"b": (176, 120, 72), "d": (110, 70, 42), "e": (20, 16, 16), "n": (30, 20, 20), "m": (226, 190, 146),
               "t": (176, 120, 72), "l": (214, 170, 120), "f": (120, 80, 50)}, "pet_dog")
    cat = [
        "................",
        "..........k...k.",
        ".........kgk.kgk",
        ".........kggggg.",
        ".........ggegeg.",
        "..........gpgg..",
        "t.........gwwg..",
        "tg.......ggggg..",
        ".tg.ggggggggg...",
        "..tggsgsgsggg...",
        "...ggggggggggw..",
        "...ggggggggggw..",
        "...gg.gg..gg.gg.",
        "...gg.gg..gg.gg.",
        "...ww.ww..ww.ww.",
        "................",
    ]
    draw(cat, {"g": (170, 170, 180), "k": (110, 110, 122), "e": (110, 190, 80), "p": (236, 140, 150),
               "w": (240, 240, 244), "s": (110, 110, 122), "t": (150, 150, 160)}, "pet_cat")


# ---------------------------------------------------------------- farm gates (closed / open)
def gates():
    post, rail, rail_d = (120, 86, 56), (178, 132, 86), (110, 78, 50)
    closed = Image.new("RGBA", (16, 22), (0, 0, 0, 0))
    d = ImageDraw.Draw(closed)
    for px in (0, 13):
        d.rectangle([px, 4, px + 2, 21], fill=post, outline=(70, 50, 34))
        d.rectangle([px - 1, 3, px + 3, 5], fill=(150, 108, 70))
    d.rectangle([3, 8, 12, 10], fill=rail); d.rectangle([3, 15, 12, 17], fill=rail)
    d.line([(3, 17), (12, 8)], fill=rail_d, width=2)
    d.rectangle([10, 11, 11, 13], fill=(70, 66, 70))                              # latch
    save(closed, "World/Objects", "gate_closed")
    opened = Image.new("RGBA", (16, 22), (0, 0, 0, 0))
    d = ImageDraw.Draw(opened)
    for px in (0, 13):
        d.rectangle([px, 4, px + 2, 21], fill=post, outline=(70, 50, 34))
        d.rectangle([px - 1, 3, px + 3, 5], fill=(150, 108, 70))
    d.rectangle([3, 6, 4, 19], fill=rail)                                          # the gate, swung open
    d.line([(4, 6), (4, 19)], fill=rail_d)
    save(opened, "World/Objects", "gate_open")


if __name__ == "__main__":
    bosses()
    citadel()
    pets()
    gates()
    print("dungeon, pets and gates written")
