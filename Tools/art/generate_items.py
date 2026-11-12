"""Shop goods, weapons, armour, extra pets, mobs (wolves), chickens, wood, tree cracks, the fist cursor.

Run:  python Tools/art/generate_items.py      (after generate_world.py and generate_icons.py)

Icons are drawn from simple shapes, then finish() adds a dark outline and a light/shadow edge so
they look rounded (lit from the top-left) instead of flat.
"""
import math
import os

import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import binary_dilation

from generate_art import ART, save

O = (52, 34, 34)


def canvas(w=16, h=16):
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def finish(img, outline=O, light=0.35, dark=0.7):
    """Shades edge pixels (top-left lighter, bottom-right darker) and adds an outline."""
    a = np.array(img)
    m = a[..., 3] > 0
    out = a.copy()
    H, W = m.shape
    for y in range(H):
        for x in range(W):
            if not m[y, x]:
                continue
            c = a[y, x, :3].astype(float)
            up_left = (y == 0 or not m[y - 1, x]) or (x == 0 or not m[y, x - 1])
            down_right = (y == H - 1 or not m[y + 1, x]) or (x == W - 1 or not m[y, x + 1])
            if down_right:
                c = c * dark
            elif up_left:
                c = c + (255 - c) * light
            out[y, x, :3] = c.astype(np.uint8)
    ring = binary_dilation(m) & ~m
    out[ring] = outline + (255,)
    return Image.fromarray(out)


def icon(name, img, folder="Icons", shade=True):
    save(finish(img) if shade else img, folder, name)


# ---------------------------------------------------------------- food
def bakery():
    img, d = canvas()
    d.polygon([(1, 11), (12, 2), (14, 4), (3, 13)], fill=(214, 160, 90))       # baguette
    for k in range(3):
        d.line([(4 + k * 3, 9 - k * 2), (6 + k * 3, 10 - k * 2)], fill=(240, 200, 140))
    icon("food_baguette", img)

    img, d = canvas()
    d.rectangle([4, 8, 11, 13], fill=(196, 140, 210))                          # muffin cup
    for x in range(5, 12, 2):
        d.line([(x, 8), (x, 13)], fill=(160, 100, 180))
    d.ellipse([3, 3, 12, 10], fill=(150, 96, 58))
    d.point((6, 5), fill=(90, 50, 30)); d.point((9, 6), fill=(90, 50, 30))
    icon("food_muffin", img)

    img, d = canvas()
    for i, x in enumerate((3, 7, 11)):                                         # churros
        d.rounded_rectangle([x, 2 + i, x + 2, 14], radius=1, fill=(214, 150, 80))
        for y in range(4 + i, 14, 2):
            d.point((x + 1, y), fill=(240, 200, 130))
    icon("food_churros", img)

    img, d = canvas()
    d.ellipse([1, 4, 14, 12], fill=(232, 186, 110))                            # ensaimada (spiral)
    d.arc([3, 5, 12, 11], 0, 300, fill=(200, 140, 70))
    d.arc([5, 6, 10, 10], 40, 330, fill=(200, 140, 70))
    for x, y in ((4, 6), (9, 5), (11, 9), (6, 10)):
        d.point((x, y), fill=(255, 255, 255))
    icon("food_ensaimada", img)

    img, d = canvas()
    d.pieslice([1, 3, 14, 17], 180, 360, fill=(222, 166, 90))                  # empanada
    for x in range(3, 13, 2):
        d.point((x, 10), fill=(180, 120, 60))
    icon("food_empanada", img)

    img, d = canvas()
    d.polygon([(2, 9), (13, 9), (12, 13), (3, 13)], fill=(214, 160, 90))       # apple tart
    d.ellipse([2, 6, 13, 11], fill=(232, 190, 120))
    for x in range(3, 13, 3):
        d.line([(x, 7), (x + 2, 10)], fill=(200, 80, 50))
    icon("food_tart", img)

    img, d = canvas()
    d.ellipse([2, 3, 13, 13], fill=(214, 150, 90))                             # donut
    d.ellipse([2, 3, 13, 11], fill=(240, 130, 170))
    d.ellipse([6, 6, 9, 9], fill=(0, 0, 0, 0))
    for x, y in ((4, 5), (10, 5), (5, 9), (11, 8)):
        d.point((x, y), fill=(255, 240, 120))
    icon("food_donut", img)

    img, d = canvas()
    d.rectangle([2, 6, 13, 13], fill=(240, 210, 130))                          # sponge cake
    d.rectangle([2, 5, 13, 6], fill=(200, 150, 80))
    for x, y in ((4, 9), (8, 11), (11, 8), (6, 12)):
        d.point((x, y), fill=(250, 230, 170))
    icon("food_sponge", img)


def candy():
    img, d = canvas()
    for i, c in enumerate(((240, 80, 100), (90, 200, 120), (250, 200, 60), (120, 140, 240))):   # gummies
        x, y = 2 + (i % 2) * 6, 3 + (i // 2) * 6
        d.ellipse([x, y, x + 5, y + 5], fill=c)
        d.ellipse([x + 1, y - 2, x + 2, y + 1], fill=c)
        d.ellipse([x + 3, y - 2, x + 4, y + 1], fill=c)
    icon("food_gummies", img)

    img, d = canvas()
    d.rectangle([2, 5, 13, 11], fill=(232, 214, 170))                          # turrón
    for x, y in ((4, 7), (7, 9), (10, 6), (11, 9), (5, 10)):
        d.point((x, y), fill=(170, 120, 70))
    d.rectangle([2, 5, 13, 6], fill=(250, 240, 220))
    icon("food_turron", img)

    img, d = canvas()
    d.line([(8, 9), (8, 15)], fill=(240, 230, 200), width=1)                   # cotton candy
    d.ellipse([2, 1, 14, 11], fill=(250, 170, 210))
    d.ellipse([4, 2, 10, 7], fill=(255, 210, 230))
    icon("food_cotton_candy", img)

    img, d = canvas()
    d.polygon([(5, 8), (11, 8), (8, 15)], fill=(214, 160, 90))                 # ice cream
    for k in range(9, 14, 2):
        d.line([(6, k - 1), (10, k + 1)], fill=(180, 120, 60))
    d.ellipse([4, 2, 12, 9], fill=(250, 200, 220))
    d.ellipse([5, 0, 10, 5], fill=(140, 90, 60))
    icon("food_ice_cream", img)

    img, d = canvas()
    for x, y in ((2, 6), (7, 4), (9, 9)):                                       # marshmallows
        d.rounded_rectangle([x, y, x + 5, y + 4], radius=2, fill=(250, 236, 240))
    d.rounded_rectangle([4, 9, 9, 13], radius=2, fill=(250, 190, 210))
    icon("food_marshmallow", img)

    img, d = canvas()
    for k in range(3):                                                          # licorice
        d.line([(2 + k * 4, 14), (6 + k * 4, 2)], fill=(60, 30, 40), width=2)
    icon("food_licorice", img)

    img, d = canvas()
    d.rectangle([3, 5, 12, 11], fill=(110, 200, 180))                          # chewing gum
    d.rectangle([3, 7, 12, 8], fill=(240, 240, 240))
    icon("food_gum", img)


# ---------------------------------------------------------------- weapons
STEEL, STEEL_D, GOLD, WOOD, GRIP = (214, 222, 236), (150, 160, 184), (232, 190, 80), (150, 104, 62), (96, 64, 48)


def blade_icon(name, length, width, guard, blade=STEEL, hilt=GOLD, grip=GRIP, curve=0.0, point=True):
    """A sword/knife lying diagonally (handle bottom-left, tip top-right)."""
    img, d = canvas()
    hx, hy = 3, 13
    tip_x, tip_y = hx + length, hy - length
    pts = []
    for t in np.linspace(0, 1, 12):
        x = hx + 2 + (tip_x - hx - 2) * t
        y = hy - 2 - (hy - 2 - tip_y) * t - math.sin(t * math.pi) * curve
        pts.append((x, y))
    for (x, y) in pts:
        w = width * (0.4 if point and (x - hx) / max(1, length) > 0.88 else 1)
        d.ellipse([x - w / 2, y - w / 2, x + w / 2, y + w / 2], fill=blade)
    d.line([(hx + 3, hy - 3), (tip_x - 1, tip_y + 1)], fill=STEEL_D)
    if guard:
        d.line([(hx + 1 - guard // 2, hy - 3 - guard // 2), (hx + 3 + guard // 2, hy - 1 + guard // 2)], fill=hilt, width=2)
    d.line([(hx - 1, hy + 1), (hx + 2, hy - 2)], fill=grip, width=2)
    d.point((hx - 1, hy + 1), fill=hilt)
    icon(name, img)


def axe_icon(name, head_size, double=False, head=(170, 176, 190)):
    img, d = canvas()
    d.line([(3, 14), (11, 3)], fill=WOOD, width=2)                             # handle
    cx, cy = 10, 4
    s = head_size
    d.polygon([(cx - 1, cy - 1), (cx + s, cy - s + 1), (cx + s + 1, cy + s - 1), (cx + 1, cy + 2)], fill=head)
    if double:
        d.polygon([(cx - 1, cy + 1), (cx - s, cy - s + 2), (cx - s - 1, cy + s), (cx - 2, cy + 3)], fill=head)
    d.line([(cx + s, cy - s + 1), (cx + s + 1, cy + s - 1)], fill=(240, 244, 250))
    icon(name, img)


def bow_icon(name, size, wood=WOOD, crossbow=False):
    img, d = canvas()
    if crossbow:
        d.line([(3, 13), (12, 4)], fill=wood, width=2)                         # stock
        d.arc([2, 0, 14, 12], 190, 340, fill=(90, 90, 100), width=2)           # steel bow
        d.line([(3, 5), (13, 9)], fill=(236, 240, 248))
        d.line([(9, 8), (13, 4)], fill=(232, 64, 80))
    else:
        r = size
        d.arc([8 - r, 8 - r, 8 + r, 8 + r], 110, 250, fill=wood, width=2)
        a1, a2 = math.radians(110), math.radians(250)
        d.line([(8 + r * math.cos(a1), 8 + r * math.sin(a1)), (8 + r * math.cos(a2), 8 + r * math.sin(a2))], fill=(236, 240, 248))
    icon(name, img)


def weapons():
    blade_icon("item_pocketknife", 7, 2, 0, blade=STEEL, grip=(150, 40, 40))
    blade_icon("item_shortsword", 9, 3, 4)
    blade_icon("item_sabre", 11, 3, 4, curve=2.5)
    blade_icon("item_rapier", 12, 2, 6, hilt=(200, 200, 210))
    blade_icon("item_greatsword", 12, 4, 6, blade=(196, 206, 222), hilt=(150, 40, 40))
    axe_icon("item_hatchet", 3)
    axe_icon("item_axe", 4)
    axe_icon("item_battleaxe", 4, double=True, head=(150, 156, 176))
    bow_icon("item_shortbow", 5)
    bow_icon("item_longbow", 7, wood=(120, 80, 48))
    bow_icon("item_crossbow", 0, crossbow=True)


# ---------------------------------------------------------------- armour (blacksmith)
IRON, IRON_D = (170, 176, 190), (120, 126, 140)


def armour():
    img, d = canvas()
    d.chord([2, 2, 13, 15], 180, 360, fill=IRON)                               # helmet
    d.rectangle([2, 8, 13, 10], fill=IRON_D)
    d.rectangle([7, 8, 8, 13], fill=IRON_D)
    icon("gear_helmet", img)

    img, d = canvas()
    d.polygon([(3, 2), (12, 2), (14, 6), (12, 14), (3, 14), (1, 6)], fill=IRON)   # chainmail
    for y in range(4, 14, 2):
        for x in range(3 + (y % 4 == 0), 13, 2):
            d.point((x, y), fill=IRON_D)
    icon("gear_chainmail", img)

    img, d = canvas()
    d.polygon([(3, 2), (12, 2), (14, 6), (12, 14), (3, 14), (1, 6)], fill=(200, 206, 220))  # breastplate
    d.line([(8, 3), (8, 13)], fill=(150, 156, 170))
    d.arc([3, 4, 12, 12], 200, 340, fill=(240, 244, 250))
    icon("gear_breastplate", img)

    img, d = canvas()
    for x in (3, 9):                                                            # greaves
        d.rounded_rectangle([x, 2, x + 4, 14], radius=2, fill=IRON)
        d.line([(x + 1, 4), (x + 1, 12)], fill=(220, 226, 236))
    icon("gear_greaves", img)

    img, d = canvas()
    for y in (3, 9):                                                            # bracers
        d.rounded_rectangle([2, y, 13, y + 4], radius=2, fill=(150, 100, 60))
        d.line([(3, y + 2), (12, y + 2)], fill=(232, 190, 80))
    icon("gear_bracers", img)

    img, d = canvas()
    d.ellipse([1, 1, 14, 14], fill=(150, 104, 62))                             # wooden shield
    for x in (5, 8, 11):
        d.line([(x, 2), (x, 13)], fill=(120, 80, 48))
    d.ellipse([6, 6, 9, 9], fill=IRON)
    icon("gear_wood_shield", img)

    img, d = canvas()
    d.polygon([(2, 2), (13, 2), (13, 8), (7, 14), (2, 8)], fill=IRON)          # iron shield (heater)
    d.polygon([(4, 4), (11, 4), (11, 8), (7, 12), (4, 8)], fill=(60, 90, 160))
    d.line([(7, 4), (7, 11)], fill=GOLD)
    d.line([(4, 7), (11, 7)], fill=GOLD)
    icon("gear_iron_shield", img)


# ---------------------------------------------------------------- pets, mobs, farm animals, wood
def animals():
    img, d = canvas()                                                           # turtle
    d.ellipse([2, 6, 13, 13], fill=(110, 150, 70))
    d.ellipse([12, 8, 15, 11], fill=(150, 180, 100))
    for x in (3, 10):
        d.rectangle([x, 12, x + 2, 14], fill=(150, 180, 100))
    d.line([(4, 9), (11, 9)], fill=(80, 110, 50))
    d.point((14, 9), fill=(20, 20, 20))
    icon("pet_turtle", img)
    img, d = canvas()                                                           # hamster
    d.ellipse([2, 5, 14, 14], fill=(226, 170, 100))
    d.ellipse([4, 9, 11, 14], fill=(250, 236, 214))
    d.ellipse([3, 4, 6, 7], fill=(236, 150, 150))
    d.ellipse([10, 4, 13, 7], fill=(236, 150, 150))
    d.point((6, 8), fill=(20, 20, 20)); d.point((10, 8), fill=(20, 20, 20))
    icon("pet_hamster", img)
    for name in ("pet_turtle", "pet_hamster"):
        src = Image.open(os.path.join(ART, "Icons", name + ".png"))
        hop = Image.new("RGBA", src.size, (0, 0, 0, 0))
        hop.paste(src, (0, -1))
        save(hop, "Icons", name + "_hop")

    # wolf: 24x16 side view (facing right), two walk frames
    for f in range(2):
        img, d = canvas(24, 16)
        fur, belly = (128, 128, 140), (196, 196, 204)
        d.ellipse([4, 4, 17, 11], fill=fur)                                     # body
        d.polygon([(15, 3), (21, 5), (23, 8), (19, 9), (15, 9)], fill=fur)      # head + snout
        d.polygon([(16, 3), (17, 0), (19, 3)], fill=fur)                        # ear
        d.polygon([(4, 6), (0, 3), (1, 7), (4, 9)], fill=fur)                   # tail
        d.ellipse([7, 8, 15, 11], fill=belly)
        legs = ((6, 9), (9, 9), (13, 9), (16, 9)) if f == 0 else ((5, 9), (10, 9), (12, 9), (17, 9))
        for i, (x, y) in enumerate(legs):
            d.rectangle([x, y + 1, x + 1, y + 5 - (1 if f and i % 2 else 0)], fill=(100, 100, 112))
        d.point((20, 5), fill=(240, 200, 60))
        d.point((23, 8), fill=(20, 20, 20))
        save(finish(img, outline=(40, 36, 44)), "Mobs", f"wolf_{f}")
    # wild dog: brown, smaller
    for f in range(2):
        img, d = canvas(24, 16)
        fur = (150, 100, 60)
        d.ellipse([5, 6, 16, 12], fill=fur)
        d.ellipse([14, 4, 20, 10], fill=fur)
        d.polygon([(19, 6), (23, 8), (19, 9)], fill=fur)
        d.polygon([(14, 4), (15, 1), (17, 4)], fill=(110, 70, 40))
        d.line([(5, 8), (2, 5)], fill=fur, width=2)
        legs = ((7, 11), (10, 11), (13, 11), (15, 11)) if f == 0 else ((6, 11), (11, 11), (12, 11), (16, 11))
        for x, y in legs:
            d.rectangle([x, y, x + 1, y + 4], fill=(110, 70, 40))
        d.point((18, 6), fill=(20, 20, 20))
        d.point((23, 8), fill=(20, 20, 20))
        save(finish(img), "Mobs", f"wilddog_{f}")
    # chicken 14x14, two frames (peck)
    for f in range(2):
        img, d = canvas(14, 14)
        d.ellipse([2, 5, 10, 12], fill=(246, 242, 232))
        hy = 3 if f == 0 else 6
        d.ellipse([7, hy, 12, hy + 5], fill=(246, 242, 232))
        d.polygon([(12, hy + 2), (14, hy + 3), (12, hy + 4)], fill=(240, 180, 50))
        d.rectangle([9, hy - 1, 10, hy], fill=(220, 50, 50))
        d.point((10, hy + 2), fill=(20, 20, 20))
        d.line([(5, 12), (5, 13)], fill=(240, 180, 50)); d.line([(8, 12), (8, 13)], fill=(240, 180, 50))
        save(finish(img, outline=(90, 80, 70)), "Mobs", f"chicken_{f}")

    img, d = canvas()                                                           # wood log
    d.rounded_rectangle([1, 5, 13, 11], radius=3, fill=(140, 96, 58))
    d.ellipse([10, 5, 15, 11], fill=(214, 176, 120))
    d.ellipse([11, 7, 13, 9], outline=(170, 130, 80))
    d.line([(3, 7), (9, 7)], fill=(110, 74, 44))
    icon("item_wood", img)
    img, d = canvas()                                                           # wolf pelt
    d.polygon([(3, 2), (12, 2), (14, 7), (12, 14), (3, 14), (1, 7)], fill=(128, 128, 140))
    d.polygon([(5, 4), (10, 4), (11, 8), (8, 12), (4, 8)], fill=(170, 170, 180))
    icon("item_pelt", img)


# ---------------------------------------------------------------- effects and cursor
def effects():
    # tree-breaking cracks: four stages, each adding more cracks (drawn over the tree)
    r = np.random.default_rng(4)
    lines = []
    for _ in range(14):
        x, y = r.integers(4, 28), r.integers(4, 28)
        pts = [(int(x), int(y))]
        for _ in range(4):
            x = int(np.clip(x + r.integers(-5, 6), 0, 31)); y = int(np.clip(y + r.integers(-5, 6), 0, 31))
            pts.append((x, y))
        lines.append(pts)
    for stage in range(4):
        img, d = canvas(32, 32)
        for pts in lines[: (stage + 1) * 3 + 2]:
            d.line(pts, fill=(30, 20, 16, 220), width=1)
        save(img, "World", f"crack_{stage}")
    # stump left behind for a moment after a tree falls
    img, d = canvas(20, 16)
    d.ellipse([3, 10, 17, 15], fill=(30, 36, 24, 60))
    d.rectangle([5, 6, 14, 12], fill=(120, 86, 56))
    d.ellipse([5, 3, 14, 9], fill=(206, 170, 116))
    d.ellipse([8, 5, 11, 7], outline=(160, 120, 76))
    save(img, "World", "stump")

    # fist cursor (shown when pointing at something you can attack)
    img, d = canvas()
    skin = (236, 190, 140)
    d.rounded_rectangle([2, 5, 13, 13], radius=2, fill=skin)
    for x in (3, 6, 9):
        d.line([(x + 2, 5), (x + 2, 8)], fill=(200, 150, 100))
    d.rounded_rectangle([1, 9, 7, 12], radius=1, fill=(246, 206, 160))
    d.rectangle([4, 13, 11, 15], fill=(176, 60, 60))
    cur = finish(img)
    save(cur.resize((24, 24), Image.NEAREST), "Cursor", "cursor_fist")     # a bit smaller than the arrow

    # small speaker icon for "listen" buttons
    img, d = canvas(12, 12)
    d.polygon([(1, 4), (4, 4), (7, 1), (7, 10), (4, 7), (1, 7)], fill=(255, 255, 255))
    d.arc([5, 2, 11, 9], 300, 60, fill=(255, 255, 255))
    save(img, "UI", "listen")


if __name__ == "__main__":
    bakery()
    candy()
    weapons()
    armour()
    animals()
    effects()
    print("Items written to", ART)
