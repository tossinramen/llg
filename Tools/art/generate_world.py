"""Generates the village map, item icons and UI pieces.

Run:  python Tools/art/generate_world.py

The village is drawn as ONE picture (Assets/Resources/Art/World/village.png) plus a text grid
(Assets/Resources/Maps/village.txt) that tells the game which tiles are solid and which can be
interacted with. Both come from the layout in make_village(), so they always match.

Grid letters (walkable):  . grass   , path   = dock   ; archery range floor
Grid letters (solid):     T tree   u bush   N fence   F pond   A archery target   R rune stone
                          k bakery  q candy shop  s store  h your house  d other house
                          B C S H D doors of those buildings   1-5 villagers   P player start
"""
import math
import os
import random

from PIL import Image, ImageDraw, ImageFont

from generate_art import ART, INK, ROOT, icon, save

T = 16
MAP_W, MAP_H = 40, 24
O = INK[:3]
FONT = ImageFont.truetype(os.path.join(ROOT, "Assets", "Resources", "Fonts", "PressStart2P-Regular.ttf"), 8)


def dark(c, f=0.78):
    return tuple(int(v * f) for v in c[:3])


def light(c, f=0.35):
    return tuple(int(v + (255 - v) * f) for v in c[:3])


# ---------------------------------------------------------------- village pieces
def draw_tree(d, x, y, blossom, rnd):
    leaf, leaf_d, leaf_l = ((244, 140, 186), (214, 100, 156), (255, 196, 222)) if blossom else \
                           ((72, 168, 88), (48, 132, 76), (120, 204, 110))
    d.ellipse([x + 2, y + 26, x + 13, y + 31], fill=(84, 160, 84))             # ground shadow
    d.rectangle([x + 6, y + 18, x + 9, y + 29], fill=(128, 88, 60), outline=O)
    d.ellipse([x, y, x + 15, y + 21], fill=leaf_d, outline=O)
    d.ellipse([x + 1, y + 1, x + 12, y + 16], fill=leaf)
    d.ellipse([x + 3, y + 3, x + 8, y + 8], fill=leaf_l)
    for _ in range(5):
        px, py = x + rnd.randrange(3, 13), y + rnd.randrange(4, 18)
        d.point((px, py), fill=leaf_l if rnd.random() < 0.5 else leaf_d)


def draw_bush(d, x, y):
    d.ellipse([x + 1, y + 3, x + 14, y + 14], fill=(56, 144, 80), outline=O)
    d.ellipse([x + 3, y + 4, x + 10, y + 10], fill=(96, 188, 100))
    for px, py in ((5, 9), (10, 7), (9, 11)):
        d.point((x + px, y + py), fill=(240, 80, 96))


def draw_flower(d, x, y, c):
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        d.point((x + dx, y + dy), fill=c)
    d.point((x, y), fill=(255, 216, 72))


def draw_building(d, tx, ty, w, h, wall, roof, text=None, awning=None, chimney=False):
    x0, y0, W, H = tx * T, ty * T, w * T, h * T
    roof_h = H // 2 + 2
    cx = x0 + W // 2
    if chimney:
        d.rectangle([x0 + W - 18, y0 - 4, x0 + W - 11, y0 + 6], fill=(170, 110, 96), outline=O)
    d.rectangle([x0 + 2, y0 + roof_h - 2, x0 + W - 3, y0 + H - 1], fill=wall, outline=O)
    d.rectangle([x0 + 3, y0 + H - 4, x0 + W - 4, y0 + H - 2], fill=dark(wall, 0.86))
    d.rectangle([x0, y0 + 1, x0 + W - 1, y0 + roof_h], fill=roof, outline=O)
    row = 0
    for ry in range(y0 + 6, y0 + roof_h - 1, 5):                               # shingles
        d.line([x0 + 1, ry, x0 + W - 2, ry], fill=dark(roof))
        for rx in range(x0 + 4 + (row % 2) * 4, x0 + W - 2, 8):
            d.line([rx, ry - 4, rx, ry - 1], fill=dark(roof))
        row += 1
    d.line([x0 + 1, y0 + 2, x0 + W - 2, y0 + 2], fill=light(roof))
    if awning:
        ay = y0 + roof_h + 1
        for i in range(x0 + 1, x0 + W - 1):
            k = (i - x0) % 8
            d.line([i, ay, i, ay + 4 + (1 if k % 4 in (1, 2) else 0)], fill=awning[k // 4])
        d.line([x0 + 1, ay - 1, x0 + W - 2, ay - 1], fill=O)
    d.rectangle([cx - 6, y0 + H - 17, cx + 5, y0 + H - 1], fill=(150, 96, 60), outline=O)   # door
    d.line([cx - 4, y0 + H - 15, cx - 4, y0 + H - 2], fill=(176, 120, 76))
    d.point((cx + 3, y0 + H - 8), fill=(255, 216, 72))
    for wx in (x0 + 7, x0 + W - 17):                                           # windows
        d.rectangle([wx, y0 + H - 16, wx + 9, y0 + H - 7], fill=(140, 208, 248), outline=O)
        d.rectangle([wx - 1, y0 + H - 6, wx + 10, y0 + H - 5], fill=(250, 250, 250), outline=O)
        d.line([wx + 2, y0 + H - 14, wx + 4, y0 + H - 14], fill=(230, 246, 255))
        d.line([wx + 5, y0 + H - 15, wx + 5, y0 + H - 8], fill=O)
    if text:
        tw = len(text) * 8
        d.rectangle([cx - tw // 2 - 3, y0 + 9, cx + tw // 2 + 2, y0 + 22], fill=(252, 240, 204), outline=O)
        d.text((cx - tw // 2, y0 + 12), text, font=FONT, fill=O)


def draw_fence(d, x, y, vertical):
    post, rail = (150, 100, 64), (196, 144, 92)
    if vertical:
        d.rectangle([x + 6, y, x + 9, y + 15], fill=rail)
        d.line([x + 6, y, x + 6, y + 15], fill=post)
        d.rectangle([x + 5, y + 5, x + 10, y + 9], fill=post, outline=O)
    else:
        for ry in (6, 10):
            d.rectangle([x, y + ry, x + 15, y + ry + 1], fill=rail)
            d.line([x, y + ry + 2, x + 15, y + ry + 2], fill=post)
        for pxo in (2, 11):
            d.rectangle([x + pxo, y + 3, x + pxo + 2, y + 14], fill=post, outline=O)


def draw_target(d, x, y):
    d.line([x + 4, y + 10, x + 2, y + 15], fill=(128, 88, 60), width=2)
    d.line([x + 11, y + 10, x + 13, y + 15], fill=(128, 88, 60), width=2)
    for r, c in ((7, O), (6, (250, 250, 250)), (4.5, (232, 64, 80)), (3, (250, 250, 250)), (1.5, (255, 208, 64))):
        d.ellipse([x + 7.5 - r, y + 7 - r, x + 7.5 + r, y + 7 + r], fill=c)


def draw_rune_stone(d, x, y):
    for bx, by, r in ((6, 27, 5), (25, 27, 5), (16, 29, 4)):
        d.ellipse([x + bx - r, y + by - 3, x + bx + r, y + by + 3], fill=(130, 130, 150), outline=O)
    d.rounded_rectangle([x + 8, y + 1, x + 23, y + 29], radius=6, fill=(160, 160, 186), outline=O)
    d.rectangle([x + 19, y + 6, x + 22, y + 28], fill=(124, 124, 156))
    glow = (120, 244, 255)
    for seg in ([12, 6, 12, 11], [12, 8, 15, 6], [12, 8, 15, 10],
                [17, 14, 12, 14], [14, 14, 14, 19], [12, 19, 17, 19],
                [12, 23, 16, 26], [16, 23, 12, 26]):
        d.line([x + seg[0], y + seg[1], x + seg[2], y + seg[3]], fill=glow)
    for gx, gy in ((4, 8), (27, 12), (3, 18), (28, 4)):
        d.point((x + gx, y + gy), fill=glow)


# ---------------------------------------------------------------- village
def make_village():
    rnd = random.Random(11)
    img = Image.new("RGBA", (MAP_W * T, MAP_H * T), (112, 196, 104, 255))
    d = ImageDraw.Draw(img)
    d.fontmode = "1"                                   # no anti-aliasing on sign text
    grid = [["." for _ in range(MAP_W)] for _ in range(MAP_H)]

    def block(x, y, w=1, h=1, ch="#"):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                grid[yy][xx] = ch

    # grass texture
    for _ in range(2600):
        x, y = rnd.randrange(MAP_W * T - 1), rnd.randrange(1, MAP_H * T)
        c = (88, 172, 92) if rnd.random() < 0.6 else (150, 220, 124)
        d.point((x, y), fill=c)
        d.point((x + 1, y - 1), fill=c)

    # paths
    paths = set()

    def path(x0, y0, x1, y1):
        for yy in range(y0, y1 + 1):
            for xx in range(x0, x1 + 1):
                paths.add((xx, yy))

    path(2, 11, 37, 12)        # main street
    path(16, 9, 23, 14)        # plaza
    path(19, 15, 20, 20)       # south road
    path(8, 20, 20, 20)        # lower street
    path(8, 8, 8, 10)          # bakery
    path(14, 8, 14, 10)        # neighbour's house
    path(31, 8, 31, 10)        # candy shop
    path(24, 9, 25, 10)        # archery range
    path(28, 13, 28, 13)       # dock
    path(8, 19, 8, 19)         # store
    path(14, 18, 14, 19)       # your house
    for (tx, ty) in paths:
        grid[ty][tx] = ","
    sand, sand_d, sand_e = (232, 204, 148), (210, 178, 122), (190, 152, 100)
    for (tx, ty) in paths:
        x, y = tx * T, ty * T
        d.rectangle([x, y, x + 15, y + 15], fill=sand)
        for _ in range(5):
            d.point((x + rnd.randrange(16), y + rnd.randrange(16)), fill=sand_d)
        if (tx, ty - 1) not in paths: d.line([x, y, x + 15, y], fill=sand_e)
        if (tx, ty + 1) not in paths: d.line([x, y + 15, x + 15, y + 15], fill=sand_e)
        if (tx - 1, ty) not in paths: d.line([x, y, x, y + 15], fill=sand_e)
        if (tx + 1, ty) not in paths: d.line([x + 15, y, x + 15, y + 15], fill=sand_e)

    # pond
    pcx, pcy, prx, pry = 30.5 * T, 16.5 * T, 5.2 * T, 2.8 * T
    px = img.load()

    def pond_d(x, y):
        return math.hypot((x - pcx) / prx, (y - pcy) / pry)

    for y in range(int(pcy - pry * 1.3), int(pcy + pry * 1.3)):
        for x in range(int(pcx - prx * 1.2), int(pcx + prx * 1.2)):
            dist = pond_d(x, y)
            if dist <= 0.86:
                px[x, y] = (72, 152, 236, 255)
            elif dist <= 1.0:
                px[x, y] = (120, 196, 248, 255)
            elif dist <= 1.035:
                px[x, y] = (250, 250, 255, 255)
            elif dist <= 1.13:
                px[x, y] = (240, 222, 164, 255)
    for _ in range(46):                                 # ripples
        x, y = rnd.randrange(int(pcx - prx), int(pcx + prx)), rnd.randrange(int(pcy - pry), int(pcy + pry))
        if pond_d(x, y) < 0.8 and pond_d(x + 4, y) < 0.8:
            d.line([x, y, x + 3, y], fill=(150, 210, 250))
    for lx, ly in ((33, 17.2), (26.6, 16.8), (31.5, 18.2)):   # lily pads
        x, y = int(lx * T), int(ly * T)
        d.ellipse([x, y, x + 8, y + 5], fill=(72, 176, 96), outline=(40, 120, 72))
        d.point((x + 4, y + 2), fill=(255, 150, 190))
        d.point((x + 5, y + 2), fill=(255, 220, 236))
    for ty in range(MAP_H):
        for tx in range(MAP_W):
            if pond_d(tx * T + 8, ty * T + 8) <= 1.02:
                grid[ty][tx] = "F"
    for ty in (14, 15):                                 # dock
        x, y = 28 * T, ty * T
        d.rectangle([x + 1, y, x + 14, y + 15], fill=(196, 144, 92), outline=O)
        for py in range(y + 3, y + 16, 4):
            d.line([x + 2, py, x + 13, py], fill=(150, 100, 64))
        grid[ty][28] = "="
    d.rectangle([28 * T + 1, 13 * T + 12, 28 * T + 14, 14 * T], fill=(196, 144, 92), outline=O)

    # archery range
    for ty in range(4, 8):
        for tx in range(23, 27):
            x, y = tx * T, ty * T
            d.rectangle([x, y, x + 15, y + 15], fill=(226, 206, 150) if ty % 2 else (236, 218, 166))
            grid[ty][tx] = ";"
    for tx in range(22, 28):
        draw_fence(d, tx * T, 3 * T, False)
        block(tx, 3, ch="N")
        if tx not in (24, 25):
            draw_fence(d, tx * T, 8 * T, False)
            block(tx, 8, ch="N")
    for ty in range(4, 8):
        for tx in (22, 27):
            draw_fence(d, tx * T, ty * T, True)
            block(tx, ty, ch="N")
    for tx in range(23, 27):
        draw_target(d, tx * T, 4 * T)
        block(tx, 4, ch="A")

    # buildings
    draw_building(d, 6, 4, 5, 4, (252, 232, 190), (236, 104, 72), "PANADERÍA", ((240, 80, 80), (255, 250, 240)))
    block(6, 4, 5, 4, ch="k"); grid[7][8] = "B"
    draw_building(d, 29, 4, 5, 4, (255, 200, 224), (190, 96, 220), "DULCERÍA", ((255, 120, 180), (255, 250, 240)))
    block(29, 4, 5, 4, ch="q"); grid[7][31] = "C"
    draw_building(d, 6, 15, 5, 4, (206, 214, 230), (72, 120, 220), "TIENDA", ((72, 190, 170), (255, 250, 240)))
    block(6, 15, 5, 4, ch="s"); grid[18][8] = "S"
    draw_building(d, 12, 5, 5, 3, (250, 240, 214), (96, 190, 120), chimney=True)
    block(12, 5, 5, 3, ch="d"); grid[7][14] = "D"
    draw_building(d, 12, 15, 5, 3, (255, 236, 180), (250, 176, 64), chimney=True)
    block(12, 15, 5, 3, ch="h"); grid[17][14] = "H"

    # rune stone in the plaza
    draw_rune_stone(d, 19 * T, 11 * T)
    block(19, 11, 2, 2, ch="R")

    # trees: border + a few inside
    trees = [(x, 0) for x in range(MAP_W)] + [(x, 22) for x in range(MAP_W)]
    trees += [(0, y) for y in range(2, 22, 2)] + [(39, y) for y in range(2, 22, 2)]
    trees += [(3, 4), (4, 8), (18, 5), (36, 5), (36, 9), (37, 14), (3, 14), (4, 18), (23, 16), (36, 19), (17, 3), (2, 20)]
    for (tx, ty) in sorted(trees, key=lambda t: t[1]):
        draw_tree(d, tx * T, ty * T, rnd.random() < 0.22, rnd)
        block(tx, ty, 1, 2, ch="T")
    for (tx, ty) in ((6, 8), (10, 8), (29, 8), (33, 8), (6, 19), (10, 19), (12, 8), (16, 8), (12, 18), (16, 18)):
        draw_bush(d, tx * T, ty * T)
        block(tx, ty, ch="u")

    # flowers on free grass
    petals = [(255, 232, 96), (255, 140, 170), (255, 255, 255), (190, 150, 255), (255, 120, 90), (120, 200, 255)]
    for _ in range(150):
        tx, ty = rnd.randrange(1, MAP_W - 1), rnd.randrange(2, MAP_H - 2)
        if grid[ty][tx] != "." or (tx, ty) in paths:
            continue
        x, y = tx * T + rnd.randrange(2, 14), ty * T + rnd.randrange(2, 14)
        if pond_d(x, y) > 1.2:
            draw_flower(d, x, y, rnd.choice(petals))

    # villagers and the player's starting tile
    for ch, (tx, ty) in {"1": (10, 9), "2": (27, 13), "3": (33, 9), "4": (26, 7), "5": (22, 10), "P": (19, 14)}.items():
        assert grid[ty][tx] in ".,;", (ch, grid[ty][tx])
        grid[ty][tx] = ch

    save(img, "World", "village")
    maps = os.path.join(ROOT, "Assets", "Resources", "Maps")
    os.makedirs(maps, exist_ok=True)
    with open(os.path.join(maps, "village.txt"), "w", newline="\n") as f:
        f.write("\n".join("".join(r) for r in grid))


# ---------------------------------------------------------------- items
def make_items():
    wood, grip, steel, steel_d, gold = (150, 100, 64), (96, 64, 48), (214, 222, 236), (150, 160, 184), (250, 204, 72)
    rod = [
        ".............o..",
        "............obo.",
        "...........obo.l",
        "..........obo..l",
        ".........obo...l",
        "........obo....l",
        ".......obo.....l",
        "......obo......l",
        ".....obo.......l",
        "....obo........l",
        "...obgo........h",
        "..obgo..........",
        ".obgo...........",
        ".ooo............",
    ]
    icon("item_rod", rod, {"o": O, "b": wood, "g": grip, "l": (236, 240, 248), "h": (120, 130, 150)})
    icon("item_rod_fine", rod, {"o": O, "b": (80, 170, 200), "g": gold, "l": (236, 240, 248), "h": gold})
    sword = [
        "............ooo.",
        "...........osso.",
        "..........osdso.",
        ".........osdso..",
        "........osdso...",
        ".......osdso....",
        "......osdso.....",
        "..o..osdso......",
        ".oyoosdso.......",
        "..oyysso........",
        "...oyyo.........",
        "..obyyyo........",
        ".obbo.oyo.......",
        ".obo...o........",
        "..o.............",
    ]
    icon("item_sword", sword, {"o": O, "s": steel, "d": steel_d, "y": gold, "b": grip})
    icon("item_dagger", [
        "..........oo....",
        ".........osso...",
        "........osdso...",
        ".......osdso....",
        "......osdso.....",
        "...o.osdso......",
        "..oyoosso.......",
        "...oyyso........",
        "...obyyo........",
        "..obbooyo.......",
        "..obo..o........",
        "...o............",
    ], {"o": O, "s": steel, "d": steel_d, "y": (200, 110, 220), "b": grip})
    icon("item_knife", [
        "............oo..",
        "..........oosso.",
        "........oosssso.",
        "......oossssdo..",
        "....oossssddo...",
        "...obbosddoo....",
        "..obbbooo.......",
        ".obbbo..........",
        ".obbo...........",
        "..oo............",
    ], {"o": O, "s": steel, "d": steel_d, "b": wood})
    icon("item_bow", [
        "...oo...........",
        "..obbo..........",
        "..olobbo........",
        "...l.obbo.......",
        "...l..obbo......",
        "...l...obo......",
        "...l...obbo.....",
        "...l...obbo.....",
        "...l...obo......",
        "...l..obbo......",
        "...l.obbo.......",
        "..olobbo........",
        "..obbo..........",
        "...oo...........",
    ], {"o": O, "b": wood, "l": (236, 240, 248)})
    icon("item_book", [
        "..oooooooooo....",
        ".orrrrrrrrrro...",
        ".orryyyyyyrro...",
        ".orryrrrryrro...",
        ".orryryyryrro...",
        ".orryrrrryrro...",
        ".orryyyyyyrro...",
        ".orrrrrrrrrro...",
        ".orrrrrrrrrro...",
        ".owwwwwwwwwwo...",
        ".owwwwwwwwwro...",
        "..oooooooooo....",
    ], {"o": O, "r": (120, 90, 220), "y": gold, "w": (250, 244, 228)})
    icon("item_fist", [
        "...oooooooo.....",
        "..owwowwowwo....",
        "..owwowwowwo....",
        "..owwowwowwoo...",
        "..owwwwwwwwwwo..",
        "..oggggggwwwwo..",
        "..owwwwwwwwwo...",
        "...owwwwwwwo....",
        "....oooooooo....",
    ], {"o": O, "w": (255, 255, 255), "g": (200, 200, 200)})
    icon("item_arrow", [
        "ff..........o...",
        ".ffwwwwwwwwwsso.",
        "ff..........o...",
    ], {"o": O, "f": (232, 64, 80), "w": (196, 144, 92), "s": steel})
    icon("food_croissant", [
        ".....oooooo.....",
        "...oobbBbbboo...",
        "..obbBbbbBbbbo..",
        ".obbBbbbbbBbbbo.",
        ".obBbboooobbBbo.",
        ".obbbo....obbbo.",
        ".obbo......obbo.",
        "..oo........oo..",
    ], {"o": O, "b": (240, 184, 96), "B": (190, 124, 56)})
    icon("food_cake", [
        ".......oo.......",
        "......orro......",
        "......orro......",
        "...oooooooooo...",
        "..owwwwwwwwwwo..",
        "..owpwwwpwwwpo..",
        "..oppppppppppo..",
        "..obbbbbbbbbbo..",
        "..oppppppppppo..",
        "..obbbbbbbbbbo..",
        "..obbbbbbbbbbo..",
        "...oooooooooo...",
    ], {"o": O, "r": (232, 56, 80), "w": (255, 250, 240), "p": (255, 150, 190), "b": (244, 214, 150)})
    icon("food_candy", [
        ".oo..........oo.",
        ".opo..oooo..opo.",
        ".oppoocwwcooppo.",
        ".opppocwccopppo.",
        ".opppoccwcopppo.",
        ".oppoocwwcooppo.",
        ".opo..oooo..opo.",
        ".oo..........oo.",
    ], {"o": O, "p": (255, 130, 180), "c": (120, 200, 255), "w": (255, 255, 255)})
    icon("food_lollipop", [
        ".....oooooo.....",
        "....oppwwppo....",
        "...opwwppwwpo...",
        "...opwppppwpo...",
        "...opwpwwpwpo...",
        "...oppwwppwpo...",
        "...opwppppwpo...",
        "....oppwwppo....",
        ".....oooooo.....",
        ".......ss.......",
        ".......ss.......",
        ".......ss.......",
        ".......ss.......",
        ".......ss.......",
    ], {"o": O, "p": (240, 90, 150), "w": (255, 236, 120), "s": (250, 250, 250)})
    icon("food_chocolate", [
        "..ooooooooooooo.",
        "..ohccohccohcco.",
        "..occcocccoccco.",
        "..occcocccoccco.",
        "..ooooooooooooo.",
        "..ohccohccohcco.",
        "..occcocccoccco.",
        "..occcocccoccco.",
        "..ooooooooooooo.",
        "..orrrrrrrrrrro.",
        "..orrrrrrrrrrro.",
        "..ooooooooooooo.",
    ], {"o": O, "c": (130, 80, 50), "h": (180, 124, 84), "r": (230, 70, 90)})
    icon("food_cookie", [
        ".....oooooo.....",
        "...oobbbbbboo...",
        "..obbbkbbbbbbo..",
        ".obbbbbbbbkbbbo.",
        ".obkbbbbbbbbbbo.",
        ".obbbbbkbbbbbbo.",
        ".obbbbbbbbbkbbo.",
        ".obbkbbbbbbbbbo.",
        "..obbbbbkbbbbo..",
        "...oobbbbbboo...",
        ".....oooooo.....",
    ], {"o": O, "b": (228, 176, 104), "k": (110, 70, 48)})


# ---------------------------------------------------------------- UI
def make_ui():
    # parchment panel with a wooden frame (9-sliced by the game, border = 6)
    p = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    d = ImageDraw.Draw(p)
    d.rounded_rectangle([0, 0, 23, 23], radius=3, fill=(206, 150, 90), outline=O)
    d.rectangle([2, 2, 21, 21], outline=(232, 184, 120))
    d.rectangle([3, 3, 20, 20], outline=(150, 100, 64))
    d.rectangle([4, 4, 19, 19], fill=(254, 244, 214), outline=O)
    d.line([5, 19 - 1, 18, 19 - 1], fill=(240, 224, 184))
    save(p, "UI", "panel")

    # button, greyscale so the game can tint it any colour (border = 5)
    b = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(b)
    d.rounded_rectangle([0, 0, 15, 15], radius=3, fill=(255, 255, 255), outline=(70, 60, 80))
    d.rectangle([2, 10, 13, 13], fill=(214, 214, 214))
    d.line([3, 14, 12, 14], fill=(150, 150, 150))
    d.line([3, 1, 12, 1], fill=(255, 255, 255))
    d.line([2, 2, 4, 2], fill=(255, 255, 255))
    save(b, "UI", "button")

    # sparkle + three rune shapes for the title screen
    s = Image.new("RGBA", (7, 7), (0, 0, 0, 0))
    for i in range(7):
        s.putpixel((3, i), (255, 255, 255, 255))
        s.putpixel((i, 3), (255, 255, 255, 255))
    for x, y in ((2, 2), (4, 2), (2, 4), (4, 4)):
        s.putpixel((x, y), (255, 255, 255, 255))
    save(s, "UI", "sparkle")
    runes = [
        [[1, 0, 1, 10], [1, 2, 6, 0], [1, 5, 6, 3]],
        [[0, 0, 3, 5], [6, 0, 3, 5], [3, 5, 3, 10]],
        [[1, 0, 1, 10], [1, 0, 6, 3], [6, 3, 1, 6], [1, 6, 6, 10]],
    ]
    for i, segs in enumerate(runes):
        r = Image.new("RGBA", (8, 12), (0, 0, 0, 0))
        d = ImageDraw.Draw(r)
        for seg in segs:
            d.line([seg[0] + 0, seg[1] + 0, seg[2], seg[3]], fill=(255, 255, 255, 255))
        save(r, "UI", f"rune{i}")

    icon("lock", ["....." + r for r in [
        "..oooo..",
        ".o....o.",
        ".o....o.",
        "oooooooo",
        "oyyyyyyo",
        "oyyooyyo",
        "oyyooyyo",
        "oyyyyyyo",
        "oooooooo",
    ]], {"o": O, "y": (250, 204, 72)}, "UI")

    # big archery target
    t = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(t)
    for r, c in ((15.5, O), (14.5, (250, 250, 250)), (11.5, (232, 64, 80)), (8.5, (250, 250, 250)),
                 (5.5, (80, 150, 240)), (2.5, (255, 208, 64))):
        d.ellipse([15.5 - r, 15.5 - r, 15.5 + r, 15.5 + r], fill=c)
    save(t, "UI", "target")


if __name__ == "__main__":
    make_village()
    make_items()
    make_ui()
    print("World art written to", ART)
