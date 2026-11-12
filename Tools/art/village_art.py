"""Drawing helpers for the village (buildings, trees, props, textures).
Used by generate_village.py, which lays out the world. Run that one, not this.

(Original notes, still accurate for how the pieces fit together:)
Draws the Spanish village "Valdeluz": ground, buildings, trees, props, water and the minimap.

Run:  python Tools/art/generate_village.py

How the village is built in the game:
  * village_ground.png  - grass, roads, the cobbled plaza and the pond (flat, drawn first)
  * Objects/*.png       - buildings, trees, lamps, the fountain... each a separate sprite placed
                          by Maps/village_objects.json. They are sorted by how far down the
                          screen their base is, so you can walk behind them.
  * water_0..2.png      - sparkles on the pond, cycled to animate the water
  * Maps/village.txt    - one letter per 16x16 tile: what is solid and what you can use.
                          Only the base of each object is solid (e.g. a tree's trunk, not its leaves).
  * UI/minimap.png      - the small map in the corner of the screen

Tile letters
  walkable : . grass   , road   : plaza cobbles   ; archery range   = dock
  solid    : F pond   R rune fountain   A archery target   N fence
             Z orange tree   O olive tree   Y cypress   u bush   f street lamp   p flower pot
             b barrels   e bench   x fruit stall   c cart
  buildings: k bakery  q candy shop  s store  w weapons shop  m pet shop  h your house
             d neighbour's house  i church     (their doors: B C S W M H D I)
  people   : 1-5 villagers   P player start
"""
import json
import math
import os

import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy.ndimage import binary_dilation, distance_transform_edt, gaussian_filter
from scipy.spatial import cKDTree

from generate_art import ART, ROOT

T = 16
MW, MH = 48, 32
PW, PH = MW * T, MH * T
FONT = ImageFont.truetype(os.path.join(ROOT, "Assets", "Resources", "Fonts", "DotGothic16-Regular.ttf"), 16)
rng = np.random.default_rng(21)
BAYER = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0 - 0.47


# ---------------------------------------------------------------- general helpers
def noise(w, h, cell, seed):
    r = np.random.default_rng(seed)
    gw, gh = w // cell + 3, h // cell + 3
    g = (r.random((gh, gw)) * 255).astype(np.uint8)
    big = Image.fromarray(g).resize((gw * cell, gh * cell), Image.BICUBIC)
    return np.asarray(big, np.float32)[cell:cell + h, cell:cell + w] / 255.0


def fractal(w, h, seed, cells=(32, 12, 4), weights=(0.55, 0.3, 0.15)):
    return sum(noise(w, h, c, seed + i) * wt for i, (c, wt) in enumerate(zip(cells, weights)))


def tones(values, palette, dither=0.9):
    """Maps 0..1 values onto a palette using ordered (Bayer) dithering - the classic 16-bit look."""
    h, w = values.shape
    b = np.tile(BAYER, (h // 4 + 1, w // 4 + 1))[:h, :w]
    idx = np.clip(np.round(values * (len(palette) - 1) + b * dither), 0, len(palette) - 1).astype(int)
    return np.array(palette, np.uint8)[idx]


def darker(c, f=0.75):
    return tuple(int(v * f) for v in c[:3])


def lighter(c, f=0.3):
    return tuple(int(v + (255 - v) * f) for v in c[:3])


def rgba(c, a=255):
    return tuple(c[:3]) + (a,)


def shadow_ellipse(img, cx, cy, rx, ry, alpha=70):
    d = ImageDraw.Draw(img)
    layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(layer).ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(30, 36, 24, alpha))
    img.alpha_composite(layer)


def save_png(img, *parts):
    path = os.path.join(ART, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)


# ---------------------------------------------------------------- layout (in tiles)
BUILDINGS = [
    # key, letter, door, x0, y0, width, depth, kind, sign, extras
    ("bakery", "k", "B", 5, 9, 6, 2, "shop", "Panadería", {"shutters": (78, 112, 84), "awning": (178, 64, 52), "pots": True}),
    ("neighbour", "d", "D", 12, 9, 5, 2, "house", None, {"shutters": (66, 100, 140), "two_story": True}),
    ("church", "i", "I", 20, 7, 8, 3, "church", None, {}),
    ("candy", "q", "C", 31, 9, 6, 2, "shop", "Dulcería", {"shutters": (164, 80, 110), "awning": (200, 110, 140), "wall": (246, 226, 216)}),
    ("store", "s", "S", 4, 21, 6, 2, "shop", "Tienda", {"shutters": (66, 100, 140), "awning": (70, 110, 150)}),
    ("weapons", "w", "W", 11, 21, 6, 2, "shop", "Armería", {"shutters": (90, 74, 60), "awning": (96, 90, 84), "wall": (226, 214, 192), "shield": True}),
    ("home", "h", "H", 19, 21, 5, 2, "house", None, {"shutters": (78, 112, 84), "pots": True}),
    ("pets", "m", "M", 25, 21, 6, 2, "shop", "Mascotas", {"shutters": (196, 140, 60), "awning": (210, 150, 70), "paw": True}),
]
PLAZA = (17, 11, 31, 20)              # x0, y0, x1, y1 inclusive
ROADS = [(2, 11, 45, 12), (2, 23, 36, 24), (17, 21, 18, 22), (31, 13, 32, 24), (41, 9, 42, 10), (24, 21, 24, 22), (10, 21, 10, 22)]
POND = (40.0, 24.5, 5.6, 4.2)        # centre x, y and radii in tiles
FOUNTAIN = (23, 15, 3, 2)
RANGE = (38, 2, 45, 9)                # fenced archery range
PEOPLE = {"1": (9, 13), "2": (33, 23), "3": (35, 13), "4": (41, 6), "5": (27, 17), "P": (24, 17)}


# ---------------------------------------------------------------- ground
GRASS = [(66, 96, 54), (82, 114, 60), (100, 132, 66), (120, 150, 74), (142, 166, 84), (164, 178, 98)]
DRY = [(150, 140, 86), (172, 160, 100), (190, 176, 116)]
DIRT = [(150, 116, 82), (170, 134, 94), (188, 152, 108), (204, 170, 124), (218, 188, 142)]
STONE = [(126, 116, 106), (150, 140, 126), (170, 160, 144), (188, 178, 160), (206, 196, 178)]
WATER = [(30, 62, 98), (38, 78, 116), (48, 98, 134), (62, 120, 148), (84, 146, 160), (112, 170, 170)]


def tile_mask(rects):
    m = np.zeros((PH, PW), bool)
    for x0, y0, x1, y1 in rects:
        m[y0 * T:(y1 + 1) * T, x0 * T:(x1 + 1) * T] = True
    return m


def make_ground(grid):
    img = np.zeros((PH, PW, 3), np.uint8)

    # grass: layered noise, dithered across six greens, with sun-dried patches
    n = fractal(PW, PH, 1)
    img[:] = tones(np.clip((n - 0.3) * 1.45 + 0.1, 0, 1), GRASS)
    dry = fractal(PW, PH, 9, (64, 20, 6)) > 0.72
    img[dry] = tones(np.clip(fractal(PW, PH, 4) * 1.2 - 0.2, 0, 1), DRY)[dry]

    # roads: soft irregular edges, packed earth with pebbles
    road = tile_mask(ROADS)
    edge = gaussian_filter(road.astype(float), 3) + (noise(PW, PH, 4, 7) - 0.5) * 0.5
    road = edge > 0.5
    d = fractal(PW, PH, 11, (16, 6, 2))
    img[road] = tones(np.clip(d * 1.3 - 0.15, 0, 1), DIRT)[road]

    # the pond: an organic blob, darker where deeper, with a muddy bank
    cx, cy, rx, ry = (POND[0] * T, POND[1] * T, POND[2] * T, POND[3] * T)
    yy, xx = np.mgrid[0:PH, 0:PW]
    wob = (noise(PW, PH, 24, 5) - 0.5) * 0.35
    rad = np.hypot((xx - cx) / rx, (yy - cy) / ry) + wob
    water = rad < 1.0
    bank = (rad >= 1.0) & (rad < 1.12)
    img[bank] = tones(np.clip(noise(PW, PH, 3, 8), 0, 1), [(92, 80, 58), (112, 98, 70), (134, 120, 88)])[bank]
    depth = distance_transform_edt(water)
    dv = 1 - np.clip(depth / 30.0, 0, 1)
    img[water] = tones(dv * 0.85 + noise(PW, PH, 6, 3) * 0.15, WATER)[water]
    # sky reflections: short horizontal light streaks
    for _ in range(70):
        x, y = int(cx + rng.normal(0, rx * 0.5)), int(cy + rng.normal(0, ry * 0.5))
        L = int(rng.integers(3, 9))
        if 0 <= y < PH and water[y, max(0, x - 1):x + L + 1].all():
            img[y, x:x + L] = (150, 196, 206)

    # the plaza: cobblestones (each stone shaded, dark mortar between them)
    plaza = tile_mask([PLAZA])
    px0, py0, px1, py1 = PLAZA[0] * T, PLAZA[1] * T, (PLAZA[2] + 1) * T, (PLAZA[3] + 1) * T
    seeds = []
    for sy in range(py0 - 6, py1 + 6, 6):
        for sx in range(px0 - 6, px1 + 6, 7):
            seeds.append((sx + rng.uniform(-2, 2) + (3 if (sy // 6) % 2 else 0), sy + rng.uniform(-1.5, 1.5)))
    seeds = np.array(seeds)
    tree = cKDTree(seeds)
    pts = np.stack([xx[plaza], yy[plaza]], 1)
    dist, idx = tree.query(pts, k=2)
    base = rng.uniform(0.25, 0.75, len(seeds))
    off = pts - seeds[idx[:, 0]]
    light = -(off[:, 0] * 0.6 + off[:, 1] * 0.8) / 4.0
    v = np.clip(base[idx[:, 0]] + light * 0.35, 0, 1)
    cols = np.array(STONE)[np.clip((v * 4.99).astype(int), 0, 4)]
    mortar = (dist[:, 1] - dist[:, 0]) < 1.3
    cols[mortar] = (92, 84, 78)
    img[plaza] = cols
    # curb of bigger stones around the plaza edge
    curb = plaza & ~binary_dilation(~plaza, iterations=2) ^ plaza
    img[curb & plaza] = (120, 110, 100)

    out = Image.fromarray(img).convert("RGBA")
    d = ImageDraw.Draw(out)

    # pebbles on roads, grass tufts and wildflowers on grass
    for _ in range(900):
        x, y = int(rng.integers(0, PW - 2)), int(rng.integers(0, PH - 2))
        if road[y, x] and not plaza[y, x]:
            d.point((x, y), fill=(120, 100, 80))
            d.point((x + 1, y), fill=(226, 208, 176))
    grass_ok = ~(road | plaza | water | bank)
    for _ in range(5200):
        x, y = int(rng.integers(1, PW - 2)), int(rng.integers(2, PH - 1))
        if not grass_ok[y, x]:
            continue
        r = rng.random()
        if r < 0.72:     # tuft: two dark blades and a light tip
            d.point((x, y), fill=(54, 84, 46))
            d.point((x + 1, y - 1), fill=(54, 84, 46))
            d.point((x - 1, y - 1), fill=(150, 176, 92))
        elif r < 0.80:
            d.point((x, y), fill=(206, 54, 44))           # poppy
            d.point((x, y + 1), fill=(60, 90, 50))
        elif r < 0.87:
            d.point((x, y), fill=(244, 240, 226))         # daisy
        elif r < 0.93:
            d.point((x, y), fill=(150, 120, 196))         # lavender
            d.point((x, y - 1), fill=(170, 140, 210))
        else:
            d.point((x, y), fill=(232, 196, 70))          # buttercup

    # reeds and lily pads on the pond edge
    for _ in range(46):
        ang = rng.uniform(0, 2 * math.pi)
        x = int(cx + math.cos(ang) * rx * rng.uniform(0.92, 1.05))
        y = int(cy + math.sin(ang) * ry * rng.uniform(0.92, 1.05))
        for k in range(int(rng.integers(2, 4))):
            hx = x + k * 2 - 2
            h = int(rng.integers(5, 10))
            d.line([hx, y, hx + rng.integers(-1, 2), y - h], fill=(70, 110, 58))
            if rng.random() < 0.4:
                d.rectangle([hx, y - h - 2, hx + 1, y - h], fill=(110, 76, 50))
    for _ in range(9):
        x, y = int(cx + rng.normal(0, rx * 0.45)), int(cy + rng.normal(0, ry * 0.45))
        if water[y, x]:
            d.ellipse([x, y, x + 7, y + 4], fill=(74, 130, 70), outline=(46, 92, 54))
            d.line([x + 3, y + 2, x + 6, y + 2], fill=(46, 92, 54))
            if rng.random() < 0.5:
                d.point((x + 2, y + 1), fill=(250, 200, 220))

    # mark the grid
    for ty in range(MH):
        for tx in range(MW):
            c = (tx * T + 8, ty * T + 8)
            if water[c[1], c[0]]:
                grid[ty][tx] = "F"
            elif plaza[c[1], c[0]]:
                grid[ty][tx] = ":"
            elif tile_mask(ROADS)[c[1], c[0]]:
                grid[ty][tx] = ","

    # wooden dock reaching into the pond from the west
    dock_y = int(POND[1])
    x = int(POND[0] - POND[2]) - 1
    while grid[dock_y][x + 1] != "F":
        x += 1
    for tx in range(x, x + 3):
        X, Y = tx * T, dock_y * T
        d.rectangle([X, Y + 2, X + 15, Y + 13], fill=(150, 108, 70))
        for k in range(0, 16, 4):
            d.line([X + k, Y + 2, X + k, Y + 13], fill=(112, 78, 52))
        d.line([X, Y + 13, X + 15, Y + 13], fill=(90, 62, 42))
        d.line([X, Y + 14, X + 15, Y + 14], fill=(40, 70, 96))
        grid[dock_y][tx] = "="
    d.rectangle([(x + 2) * T + 12, dock_y * T + 10, (x + 2) * T + 14, dock_y * T + 17], fill=(96, 66, 44))
    return out, water, (cx, cy, rx, ry)


def make_water_frames(water, bounds):
    """Three overlays of glinting light on the pond, cycled by the game to animate it."""
    ys, xs = np.nonzero(water)
    x0, y0, x1, y1 = xs.min(), ys.min(), xs.max() + 1, ys.max() + 1
    depth = distance_transform_edt(water)
    for f in range(3):
        img = Image.new("RGBA", (x1 - x0, y1 - y0), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        r = np.random.default_rng(100 + f)
        for _ in range(60):
            x, y = int(r.integers(x0, x1)), int(r.integers(y0, y1))
            if depth[y, x] > 3:
                L = int(r.integers(1, 4))
                d.line([x - x0, y - y0, x - x0 + L, y - y0], fill=(220, 240, 240, 200))
                if r.random() < 0.25:
                    d.point((x - x0 + 1, y - y0 - 1), fill=(255, 255, 255, 230))
        save_png(img, "World", f"water_{f}.png")
    return int(x0), int(y0), int(x1 - x0), int(y1 - y0)


# ---------------------------------------------------------------- building sprites
ROOF = [(124, 54, 38), (156, 70, 46), (182, 88, 56), (204, 112, 72), (222, 140, 92)]
WALL = (238, 230, 210)


def stucco(w, h, colour, seed):
    n = fractal(w, h, seed, (10, 4, 2), (0.5, 0.3, 0.2))
    pal = [darker(colour, 0.86), darker(colour, 0.93), colour, lighter(colour, 0.25)]
    return tones(np.clip(n * 1.3 - 0.15, 0, 1), pal, 0.7)


def barrel_roof(d, poly_pts, x0, x1, y_top, y_eave, depth_shift):
    """Terracotta barrel tiles: columns of curved tiles with staggered courses."""
    mask = Image.new("L", d.im.size if hasattr(d, "im") else (1, 1), 0)
    return mask


def draw_roof(img, left, right, eave_y, ridge_y, shift):
    """A front-facing sloped roof covered in curved clay tiles, with a ridge cap and tile ends."""
    w, h = img.size
    roof = Image.new("L", (w, h), 0)
    ImageDraw.Draw(roof).polygon([(left, eave_y), (right, eave_y), (right + shift, ridge_y), (left + shift, ridge_y)], fill=255)
    m = np.array(roof) > 0
    ys, xs = np.nonzero(m)
    arr = np.array(img)
    for y, x in zip(ys, xs):
        t = (eave_y - y) / max(1, eave_y - ridge_y)               # 0 at the eave, 1 at the ridge
        col = (x - left - shift * t) % 6                            # tile columns follow the slope
        course = int((eave_y - y) // 5)
        c = int(col)
        v = [1, 3, 4, 3, 2, 0][c]                                   # curved tile: dark gutter, bright crown
        if (eave_y - y) % 5 == 0:
            v = max(0, v - 2)                                       # course shadow
        if course % 2 and c in (0, 5):
            v = 1
        v = int(np.clip(v + (1 if t > 0.8 else 0) - (1 if t < 0.12 else 0), 0, 4))
        arr[y, x] = rgba(ROOF[v])
    img.paste(Image.fromarray(arr))
    d = ImageDraw.Draw(img)
    # ridge cap and rounded tile ends along the eave
    d.line([left + shift, ridge_y, right + shift, ridge_y], fill=rgba(ROOF[0]))
    d.line([left + shift, ridge_y - 1, right + shift, ridge_y - 1], fill=rgba(ROOF[3]))
    for x in range(left, right, 6):
        d.ellipse([x, eave_y - 2, x + 5, eave_y + 2], fill=rgba(ROOF[2]), outline=rgba(ROOF[0]))
    d.line([left, eave_y + 2, right, eave_y + 2], fill=rgba((64, 40, 32)))


def draw_window(d, x, y, shutters, grille=False, pots=False, w=10, h=12):
    d.rectangle([x - 1, y - 1, x + w, y + h], fill=rgba((196, 186, 166)))               # stone frame
    d.rectangle([x, y, x + w - 1, y + h - 1], fill=rgba((40, 46, 60)))                  # dark glass
    d.line([x + 1, y + 1, x + 3, y + 1], fill=rgba((120, 150, 170)))
    d.line([x + 1, y + 2, x + 1, y + 4], fill=rgba((96, 120, 140)))
    d.line([x + w // 2, y, x + w // 2, y + h - 1], fill=rgba((92, 70, 52)))
    for sx in (x - 5, x + w + 1):                                                         # open shutters
        d.rectangle([sx, y, sx + 3, y + h - 1], fill=rgba(shutters), outline=rgba(darker(shutters, 0.6)))
        for k in range(y + 2, y + h - 1, 3):
            d.line([sx + 1, k, sx + 2, k], fill=rgba(darker(shutters, 0.75)))
    if grille:
        for gx in range(x + 2, x + w - 1, 3):
            d.line([gx, y, gx, y + h - 1], fill=rgba((44, 38, 40)))
    d.rectangle([x - 2, y + h, x + w + 1, y + h + 1], fill=rgba((214, 204, 184)))     # sill
    if pots:                                                                              # geraniums
        d.rectangle([x, y + h + 2, x + w - 1, y + h + 4], fill=rgba((168, 84, 52)))
        for k in range(x, x + w, 2):
            d.point((k, y + h + 1), fill=rgba((70, 120, 60)))
            d.point((k + 1, y + h), fill=rgba((214, 48, 52)))
            d.point((k, y + h - 1), fill=rgba((236, 80, 84)))


def draw_door(d, x, y, w=14, h=22, arched=True):
    d.rectangle([x - 2, y - 2, x + w + 1, y + h], fill=rgba((176, 158, 128)))          # stone surround
    if arched:
        d.ellipse([x - 2, y - 8, x + w + 1, y + 6], fill=rgba((176, 158, 128)))
        d.ellipse([x, y - 6, x + w - 1, y + 4], fill=rgba((96, 60, 38)))
    d.rectangle([x, y, x + w - 1, y + h - 1], fill=rgba((112, 70, 44)))
    for k in range(x + 3, x + w - 1, 3):
        d.line([k, y - (4 if arched else 0), k, y + h - 1], fill=rgba((80, 48, 32)))
    d.line([x + 1, y, x + 1, y + h - 1], fill=rgba((140, 92, 58)))
    for sy in (y + 4, y + h - 6):
        d.line([x, sy, x + w - 1, sy], fill=rgba((60, 48, 44)))
    d.point((x + w - 4, y + h // 2), fill=rgba((226, 190, 100)))
    d.rectangle([x - 3, y + h, x + w + 2, y + h + 1], fill=rgba((150, 140, 126)))      # step


def azulejo_sign(d, cx, y, text):
    """A blue-and-white painted tile plaque, the classic Spanish shop sign."""
    tw = int(d.textlength(text, font=FONT))
    x0, x1 = cx - tw // 2 - 5, cx + tw // 2 + 5
    d.rectangle([x0, y, x1, y + 18], fill=rgba((246, 244, 236)), outline=rgba((40, 70, 140)))
    d.rectangle([x0 + 1, y + 1, x1 - 1, y + 17], outline=rgba((120, 150, 200)))
    for cx2 in (x0 + 2, x1 - 2):
        d.point((cx2, y + 2), fill=rgba((214, 160, 50)))
        d.point((cx2, y + 16), fill=rgba((214, 160, 50)))
    l, t, r_, b = d.textbbox((0, 0), text, font=FONT)
    d.text((cx - tw // 2, y + (19 - (b - t)) // 2 - t), text, font=FONT, fill=rgba((30, 56, 120)))


def make_building(spec):
    key, letter, door, x0, y0, wt, dt, kind, sign, ex = spec
    W_ = wt * T
    two = ex.get("two_story")
    wall_h = 64 if (two or kind == "church") else 62 if kind == "shop" else 46
    side = 10                              # side wall visible to the right (fake depth)
    rise = 26 if kind != "church" else 22
    lean = 6                               # how far the roof ridge leans back
    tower = 48 if kind == "church" else 0
    m = 6
    cw = W_ + side + m * 2 + 6
    ch = wall_h + rise + side // 2 + tower + 12
    img = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.fontmode = "1"
    base = ch - 5
    left, right = m, m + W_
    top = base - wall_h
    colour = ex.get("wall", WALL if kind != "church" else (214, 190, 146))

    # cast shadow on the ground (to the lower right)
    sh = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).polygon([(right, base), (right + side + 6, base - 4), (right + side + 6, base + 3), (left + 4, base + 4)],
                               fill=(20, 28, 16, 70))
    img.alpha_composite(sh)

    # side wall (the depth), darker because it faces away from the light
    side_poly = [(right, base), (right + side, base - side // 2), (right + side, top - side // 2), (right, top)]
    sw = Image.new("L", img.size, 0)
    ImageDraw.Draw(sw).polygon(side_poly, fill=255)
    tex = stucco(cw, ch, darker(colour, 0.78), 31 + x0)
    arr = np.array(img)
    msk = np.array(sw) > 0
    arr[msk, :3] = tex[msk]
    arr[msk, 3] = 255
    # front wall
    arr[top:base, left:right, :3] = stucco(W_, wall_h, colour, 17 + x0 * 3)
    arr[top:base, left:right, 3] = 255
    img = Image.fromarray(arr)
    d = ImageDraw.Draw(img)
    d.fontmode = "1"
    # grime near the ground and a stone plinth
    for k in range(6):
        g = Image.new("RGBA", img.size, (0, 0, 0, 0))
        ImageDraw.Draw(g).line([left, base - 6 - k, right - 1, base - 6 - k], fill=(90, 70, 50, 26 - k * 4))
        img.alpha_composite(g)
    d.rectangle([left, base - 5, right - 1, base - 1], fill=rgba((168, 150, 124)))
    d.line([left, base - 5, right - 1, base - 5], fill=rgba((196, 180, 152)))
    d.polygon([(right, base - 1), (right + side, base - 1 - side // 2), (right + side, base - 5 - side // 2), (right, base - 5)],
              fill=rgba((132, 118, 98)))
    d.line([left, top, left, base - 1], fill=rgba(darker(colour, 0.7)))
    d.line([right, top, right, base - 1], fill=rgba(darker(colour, 0.6)))

    door_x = left + (int(door_cols(spec)) - x0) * T + 1
    if kind == "church":
        # stone facade lines, a rose window and a bell tower
        for yy in range(top + 6, base - 6, 6):
            d.line([left + 1, yy, right - 2, yy], fill=rgba(darker(colour, 0.88)))
        cx = (left + right) // 2
        d.ellipse([cx - 8, top + 10, cx + 8, top + 26], fill=rgba((150, 120, 80)), outline=rgba((110, 86, 60)))
        d.ellipse([cx - 5, top + 13, cx + 5, top + 23], fill=rgba((96, 130, 190)))
        d.line([cx - 5, top + 18, cx + 5, top + 18], fill=rgba((214, 180, 90)))
        d.line([cx, top + 13, cx, top + 23], fill=rgba((214, 180, 90)))
        draw_door(d, cx - 11, base - 30, w=22, h=25)
        for wx in (left + 12, right - 22):
            draw_window(d, wx, top + 30, (110, 90, 70), grille=True, w=8, h=14)
    else:
        draw_door(d, door_x, base - 27)
        cols = [left + 8 + i * 22 for i in range(W_ // 22)]
        for wx in cols:
            if abs(wx + 5 - (door_x + 7)) < 16:
                continue
            draw_window(d, wx + 2, base - 30, ex.get("shutters", (78, 112, 84)), grille=kind == "shop", pots=ex.get("pots", False))
        if two:
            for i, wx in enumerate(cols):
                if sign and abs(wx + 5 - (left + right) // 2) < 30:
                    continue
                draw_window(d, wx + 2, top + 26, ex.get("shutters"), pots=i % 2 == 0)
            # wrought-iron balcony
            bx0, bx1 = door_x - 8, door_x + 22
            by = top + 26 if sign else top + 15
            d.rectangle([bx0, by + 7, bx1, by + 8], fill=rgba((150, 140, 126)))
            for k in range(bx0, bx1 + 1, 2):
                d.line([k, by, k, by + 6], fill=rgba((44, 38, 40)))
            d.line([bx0, by, bx1, by], fill=rgba((44, 38, 40)))
        if ex.get("awning"):
            aw = ex["awning"]
            ax0, ax1, ay = door_x - 6, door_x + 20, base - 40
            for k in range(ax0, ax1):
                c = aw if ((k - ax0) // 3) % 2 == 0 else (238, 228, 206)
                d.line([k, ay, k, ay + 4], fill=rgba(c))
                d.point((k, ay + 5 + ((k - ax0) % 3 == 1)), fill=rgba(darker(c, 0.8)))
            d.line([ax0, ay - 1, ax1 - 1, ay - 1], fill=rgba(darker(aw, 0.6)))
        if sign:
            azulejo_sign(d, (left + right) // 2, top + 4, sign)
        if ex.get("shield"):                   # a shield and crossed swords hang by the armoury door
            sx, sy = door_x + 19, base - 31
            d.polygon([(sx, sy), (sx + 10, sy), (sx + 10, sy + 8), (sx + 5, sy + 13), (sx, sy + 8)], fill=rgba((150, 40, 40)), outline=rgba((70, 30, 30)))
            d.line([sx + 5, sy + 1, sx + 5, sy + 11], fill=rgba((230, 190, 90)))
        if ex.get("paw"):                      # a paw print painted by the pet shop door
            px_, py_ = door_x + 20, base - 30
            d.ellipse([px_, py_ + 4, px_ + 7, py_ + 10], fill=rgba((110, 70, 40)))
            for k, (ox, oy) in enumerate(((-1, 1), (2, -1), (5, -1), (8, 1))):
                d.ellipse([px_ + ox, py_ + oy, px_ + ox + 2, py_ + oy + 2], fill=rgba((110, 70, 40)))

    # shadow under the eaves, then the roof itself
    for k in range(4):
        g = Image.new("RGBA", img.size, (0, 0, 0, 0))
        ImageDraw.Draw(g).line([left, top + k, right - 1, top + k], fill=(40, 30, 20, 90 - k * 22))
        img.alpha_composite(g)
    gable = [(right, top), (right + side, top - side // 2), (right + lean // 2 + side // 2, top - rise)]
    ImageDraw.Draw(img).polygon(gable, fill=rgba(darker(colour, 0.72)))
    if kind == "church":
        tx0 = (left + right) // 2 - 12
        tw = Image.new("RGBA", img.size, (0, 0, 0, 0))
        tdd = ImageDraw.Draw(tw)
        tower_top = top - rise - tower + 10
        tdd.rectangle([tx0, tower_top, tx0 + 24, top], fill=rgba(colour))
        tdd.rectangle([tx0 + 24, tower_top - 4, tx0 + 30, top - 3], fill=rgba(darker(colour, 0.76)))
        tdd.rectangle([tx0 + 6, tower_top + 8, tx0 + 18, tower_top + 22], fill=rgba((40, 34, 30)))
        tdd.ellipse([tx0 + 6, tower_top + 4, tx0 + 18, tower_top + 14], fill=rgba((40, 34, 30)))
        tdd.ellipse([tx0 + 8, tower_top + 10, tx0 + 16, tower_top + 20], fill=rgba((196, 150, 60)), outline=rgba((120, 84, 30)))
        tdd.polygon([(tx0 - 2, tower_top), (tx0 + 26, tower_top), (tx0 + 12, tower_top - 16)], fill=rgba(ROOF[2]), outline=rgba(ROOF[0]))
        tdd.polygon([(tx0 + 26, tower_top), (tx0 + 31, tower_top - 4), (tx0 + 12, tower_top - 16)], fill=rgba(ROOF[1]))
        tdd.line([tx0 + 12, tower_top - 22, tx0 + 12, tower_top - 16], fill=rgba((60, 50, 44)))
        tdd.line([tx0 + 9, tower_top - 19, tx0 + 15, tower_top - 19], fill=rgba((60, 50, 44)))
        for yy in range(tower_top + 26, top, 6):
            tdd.line([tx0 + 1, yy, tx0 + 23, yy], fill=rgba(darker(colour, 0.88)))
        img.alpha_composite(tw)
    draw_roof(img, left - 3, right + 3, top + 1, top - rise, lean + side // 2)
    if kind == "house" or key == "bakery":
        d2 = ImageDraw.Draw(img)
        cx2 = left + W_ - 16
        d2.rectangle([cx2, top - rise + 2, cx2 + 7, top - rise + 14], fill=rgba(colour), outline=rgba(darker(colour, 0.6)))
        d2.rectangle([cx2 - 1, top - rise + 1, cx2 + 8, top - rise + 3], fill=rgba(ROOF[1]))

    anchor = (left + W_ // 2, base)       # bottom-centre of the front wall = bottom-centre of the footprint
    return img, anchor


def door_cols(spec):
    key, letter, door, x0, y0, wt, dt, kind, sign, ex = spec
    return x0 + (wt // 2 if kind == "church" else 2)


# ---------------------------------------------------------------- nature and props
def blob_canopy(w, h, clumps, palette, seed, holes=0.0):
    """Leafy canopy: several overlapping clumps, lit from the top-left, with leaf speckle."""
    r = np.random.default_rng(seed)
    yy, xx = np.mgrid[0:h, 0:w]
    m = np.zeros((h, w), bool)
    shade = np.zeros((h, w))
    for cx, cy, rad in clumps:
        dd = np.hypot(xx - cx, yy - cy)
        inside = dd <= rad
        nx, ny = (xx - cx) / max(rad, 1), (yy - cy) / max(rad, 1)
        s = 0.55 - 0.45 * (nx * 0.6 + ny * 0.8) - 0.25 * (dd / rad) ** 3
        shade = np.where(inside, np.maximum(shade, s) if False else np.where(m, np.minimum(shade + 0.05, np.maximum(shade, s)), s), shade)
        m |= inside
    shade += (r.random((h, w)) - 0.5) * 0.25
    if holes:
        m &= ~((noise(w, h, 3, seed) > 1 - holes) & (distance_transform_edt(m) > 2))
    cols = tones(np.clip(shade, 0, 1), palette, 0.8)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    arr = np.array(img)
    arr[m, :3] = cols[m]
    arr[m, 3] = 255
    ring = binary_dilation(m) & ~m
    arr[ring] = rgba(darker(palette[0], 0.7))
    return Image.fromarray(arr), m


def orange_tree(seed):
    w, h = 40, 50
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 21, 45, 14, 4)
    d = ImageDraw.Draw(img)
    d.rectangle([18, 28, 22, 45], fill=rgba((108, 78, 56)))
    d.line([18, 28, 18, 45], fill=rgba((136, 100, 72)))
    d.line([22, 30, 22, 45], fill=rgba((80, 56, 42)))
    canopy, m = blob_canopy(w, 36, [(20, 18, 13), (11, 20, 9), (29, 20, 9), (16, 10, 9), (25, 11, 9)],
                            [(40, 74, 42), (56, 96, 50), (74, 120, 60), (98, 144, 70), (128, 170, 86)], seed)
    img.alpha_composite(canopy, (0, 2))
    r = np.random.default_rng(seed)
    d = ImageDraw.Draw(img)
    for _ in range(12):
        x, y = int(r.integers(6, 34)), int(r.integers(6, 30))
        if m[y - 2, x] if 0 <= y - 2 < 36 else False:
            d.rectangle([x, y, x + 1, y + 1], fill=rgba((232, 128, 34)))
            d.point((x, y), fill=rgba((252, 196, 96)))
    return img, (20, 45)


def olive_tree(seed):
    w, h = 46, 50
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 23, 45, 15, 4)
    d = ImageDraw.Draw(img)
    bark, bark_d = (120, 104, 84), (86, 72, 60)
    d.polygon([(19, 45), (27, 45), (25, 34), (29, 24), (26, 23), (23, 31), (20, 24), (17, 25), (21, 35)], fill=rgba(bark))
    d.line([19, 45, 21, 35, 17, 25], fill=rgba(lighter(bark, 0.2)))
    d.line([27, 45, 25, 34, 29, 24], fill=rgba(bark_d))
    canopy, m = blob_canopy(w, 34, [(13, 18, 10), (24, 13, 11), (34, 18, 9), (20, 22, 8), (30, 24, 7)],
                            [(82, 98, 66), (104, 120, 82), (128, 144, 100), (154, 168, 122), (184, 194, 150)], seed, holes=0.12)
    img.alpha_composite(canopy, (0, 0))
    return img, (23, 45)


def cypress(seed):
    w, h = 20, 60
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 10, 55, 7, 3)
    yy, xx = np.mgrid[0:56, 0:w]
    prof = np.clip(1 - ((yy - 30) / 28.0) ** 2, 0, 1) ** 0.6 * 8 + 0.5
    prof[yy < 8] *= (yy[yy < 8] / 8.0)
    m = np.abs(xx - 9.5) <= prof
    m[54:, :] = False
    light = 0.6 - (xx - 9.5) / 10 + ((xx * 7 + yy * 3) % 5 == 0) * 0.15 - ((xx + yy * 2) % 4 == 0) * 0.12
    cols = tones(np.clip(light, 0, 1), [(28, 54, 38), (38, 70, 46), (52, 90, 56), (70, 112, 68), (92, 134, 80)], 0.8)
    arr = np.array(img)
    sub = arr[:56]
    sub[m, :3] = cols[m]
    sub[m, 3] = 255
    ring = binary_dilation(m) & ~m
    sub[ring] = rgba((22, 40, 30))
    arr[:56] = sub
    arr[52:56, 9:11] = rgba((96, 70, 50))
    return Image.fromarray(arr), (10, 55)


def bush(kind, seed):
    w, h = 22, 18
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 11, 15, 9, 2)
    pal = [(46, 80, 46), (62, 102, 56), (82, 126, 64), (108, 150, 78), (136, 172, 92)]
    canopy, m = blob_canopy(w, 16, [(7, 9, 6), (14, 8, 6), (11, 6, 5)], pal, seed)
    img.alpha_composite(canopy, (0, 0))
    d = ImageDraw.Draw(img)
    r = np.random.default_rng(seed)
    flower = (150, 112, 200) if kind == "lavender" else (226, 64, 70)
    for _ in range(9):
        x, y = int(r.integers(3, 19)), int(r.integers(2, 12))
        if m[y, x]:
            d.point((x, y), fill=rgba(flower))
            d.point((x, y - 1), fill=rgba(lighter(flower, 0.3)))
    return img, (11, 15)


def flower_pot():
    img = Image.new("RGBA", (14, 18), (0, 0, 0, 0))
    shadow_ellipse(img, 7, 16, 6, 2)
    d = ImageDraw.Draw(img)
    d.polygon([(3, 9), (11, 9), (10, 16), (4, 16)], fill=rgba((176, 88, 54)), outline=rgba((112, 54, 36)))
    d.line([4, 10, 4, 15], fill=rgba((210, 120, 80)))
    d.rectangle([2, 8, 12, 9], fill=rgba((196, 104, 66)), outline=rgba((112, 54, 36)))
    for x, y in ((4, 5), (7, 3), (10, 5), (6, 6), (9, 7)):
        d.ellipse([x - 2, y - 1, x + 1, y + 2], fill=rgba((70, 120, 60)))
    for x, y in ((4, 3), (8, 2), (10, 4), (6, 5)):
        d.rectangle([x, y, x + 1, y + 1], fill=rgba((222, 50, 56)))
        d.point((x, y), fill=rgba((250, 110, 110)))
    return img, (7, 16)


def street_lamp():
    img = Image.new("RGBA", (14, 46), (0, 0, 0, 0))
    shadow_ellipse(img, 7, 43, 5, 2)
    d = ImageDraw.Draw(img)
    iron = (44, 38, 42)
    d.rectangle([6, 14, 7, 43], fill=rgba(iron))
    d.rectangle([4, 40, 9, 43], fill=rgba(iron))
    d.line([8, 15, 8, 42], fill=rgba((80, 72, 76)))
    d.polygon([(3, 4), (10, 4), (12, 13), (1, 13)], fill=rgba((250, 214, 120)), outline=rgba(iron))
    d.line([6, 4, 6, 13], fill=rgba(iron))
    d.line([2, 2, 11, 2], fill=rgba(iron))
    d.polygon([(4, 2), (9, 2), (6, -1)], fill=rgba(iron))
    d.point((4, 7), fill=rgba((255, 248, 210)))
    return img, (7, 43)


def barrels():
    img = Image.new("RGBA", (26, 22), (0, 0, 0, 0))
    shadow_ellipse(img, 13, 19, 12, 3)
    d = ImageDraw.Draw(img)
    for bx in (2, 13):
        d.rounded_rectangle([bx, 4, bx + 10, 19], radius=3, fill=rgba((140, 96, 60)), outline=rgba((78, 52, 36)))
        d.line([bx + 2, 5, bx + 2, 18], fill=rgba((170, 124, 82)))
        d.line([bx + 8, 5, bx + 8, 18], fill=rgba((110, 74, 48)))
        for yy in (7, 16):
            d.line([bx, yy, bx + 10, yy], fill=rgba((70, 66, 70)))
        d.ellipse([bx + 1, 2, bx + 9, 6], fill=rgba((162, 116, 74)), outline=rgba((78, 52, 36)))
    return img, (13, 19)


def bench():
    img = Image.new("RGBA", (34, 20), (0, 0, 0, 0))
    shadow_ellipse(img, 17, 17, 15, 3)
    d = ImageDraw.Draw(img)
    iron = (48, 42, 44)
    for k in (3, 9):
        d.rectangle([2, k, 31, k + 3], fill=rgba((150, 104, 66)), outline=rgba((92, 62, 40)))
    d.rectangle([2, 12, 31, 14], fill=rgba((120, 82, 52)))
    for lx in (4, 28):
        d.rectangle([lx, 3, lx + 1, 17], fill=rgba(iron))
    return img, (17, 17)


def fruit_stall():
    img = Image.new("RGBA", (40, 40), (0, 0, 0, 0))
    shadow_ellipse(img, 20, 36, 18, 3)
    d = ImageDraw.Draw(img)
    d.rectangle([4, 22, 35, 34], fill=rgba((130, 90, 58)), outline=rgba((80, 54, 36)))
    for k in range(6, 34, 4):
        d.line([k, 23, k, 33], fill=rgba((104, 72, 46)))
    for i, c in enumerate([(232, 128, 34), (210, 50, 50), (230, 210, 70), (120, 160, 60)]):
        x = 6 + i * 7
        d.rectangle([x, 18, x + 6, 22], fill=rgba((150, 108, 70)), outline=rgba((92, 62, 40)))
        for fx in range(x + 1, x + 6, 2):
            d.ellipse([fx - 1, 15, fx + 1, 18], fill=rgba(c))
    d.line([5, 6, 5, 22], fill=rgba((92, 62, 40)))
    d.line([34, 6, 34, 22], fill=rgba((92, 62, 40)))
    for k in range(2, 38):
        c = (190, 56, 48) if (k // 4) % 2 == 0 else (240, 228, 200)
        d.line([k, 3, k, 8], fill=rgba(c))
        d.point((k, 9 + (k % 4 == 1)), fill=rgba(darker(c, 0.8)))
    return img, (20, 36)


def cart():
    img = Image.new("RGBA", (36, 26), (0, 0, 0, 0))
    shadow_ellipse(img, 18, 23, 16, 3)
    d = ImageDraw.Draw(img)
    d.rectangle([4, 8, 30, 16], fill=rgba((150, 108, 70)), outline=rgba((86, 58, 38)))
    for k in range(8, 30, 5):
        d.line([k, 9, k, 15], fill=rgba((120, 84, 54)))
    d.line([30, 14, 35, 18], fill=rgba((86, 58, 38)), width=2)
    for wx in (9, 25):
        d.ellipse([wx - 5, 13, wx + 5, 23], fill=rgba((96, 66, 44)), outline=rgba((60, 42, 30)))
        d.ellipse([wx - 1, 17, wx + 1, 19], fill=rgba((60, 42, 30)))
    for hx in (8, 14, 20):                       # hay
        d.ellipse([hx, 3, hx + 8, 10], fill=rgba((214, 182, 90)), outline=rgba((160, 130, 60)))
    return img, (18, 23)


def fountain(frame):
    w, h = 60, 56
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 31, 48, 28, 6)
    d = ImageDraw.Draw(img)
    stone, stone_l, stone_d = (176, 164, 146), (208, 198, 178), (128, 118, 104)
    d.ellipse([2, 26, 58, 50], fill=rgba(stone_d))                     # front face of the basin
    d.rectangle([2, 34, 58, 42], fill=rgba(stone_d))
    d.ellipse([2, 20, 58, 42], fill=rgba(stone), outline=rgba((96, 88, 78)))   # rim (top surface)
    d.ellipse([6, 23, 54, 39], fill=rgba((44, 92, 128)))               # water
    d.ellipse([8, 24, 52, 36], fill=rgba((58, 116, 150)))
    d.arc([2, 20, 58, 42], 190, 330, fill=rgba(stone_l))
    for k in range(6, 56, 8):                                           # carved panels on the front
        d.line([k, 40, k, 47], fill=rgba((112, 102, 90)))
    d.rectangle([26, 12, 34, 32], fill=rgba(stone), outline=rgba((96, 88, 78)))   # pedestal with runes
    d.line([27, 13, 27, 31], fill=rgba(stone_l))
    glow = (120, 236, 255)
    for seg in ((29, 16, 29, 20), (29, 17, 31, 16), (31, 22, 29, 25), (29, 22, 31, 25), (29, 27, 31, 30)):
        d.line(seg, fill=rgba(glow))
    d.ellipse([20, 8, 40, 16], fill=rgba(stone), outline=rgba((96, 88, 78)))      # upper bowl
    d.ellipse([23, 9, 37, 13], fill=rgba((58, 116, 150)))
    r = np.random.default_rng(frame)
    for side in (-1, 1):                                                 # water arcs
        for k in range(10):
            t = k / 9
            x = 30 + side * (3 + t * 13)
            y = 9 - 6 * math.sin(math.pi * min(1, t * 1.1)) + t * 16
            d.point((int(x), int(y)), fill=rgba((200, 232, 246)))
    d.line([30, 2, 30, 9], fill=rgba((210, 236, 250)))
    for _ in range(14):
        x, y = int(r.integers(10, 50)), int(r.integers(25, 36))
        d.point((x, y), fill=rgba((214, 240, 250)))
    return img, (30, 47)


def target():
    img = Image.new("RGBA", (22, 30), (0, 0, 0, 0))
    shadow_ellipse(img, 11, 27, 9, 2)
    d = ImageDraw.Draw(img)
    d.line([5, 16, 3, 27], fill=rgba((110, 78, 52)), width=2)
    d.line([16, 16, 18, 27], fill=rgba((110, 78, 52)), width=2)
    d.line([11, 18, 13, 27], fill=rgba((86, 60, 40)), width=2)
    d.ellipse([1, 1, 20, 20], fill=rgba((206, 176, 100)), outline=rgba((140, 110, 60)))   # straw
    for r_, c in ((7.5, (236, 226, 200)), (5.5, (176, 56, 46)), (3.5, (236, 226, 200)), (1.6, (214, 170, 52))):
        d.ellipse([10.5 - r_, 10.5 - r_, 10.5 + r_, 10.5 + r_], fill=rgba(c))
    d.arc([1, 1, 20, 20], 200, 290, fill=rgba((232, 208, 140)))
    return img, (11, 27)


def fence(vertical):
    img = Image.new("RGBA", (16, 22), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    post, rail, rail_d = (120, 86, 56), (168, 124, 82), (110, 78, 50)
    if vertical:
        d.rectangle([6, 0, 9, 18], fill=rgba(rail))
        d.line([9, 0, 9, 18], fill=rgba(rail_d))
        d.rectangle([5, 8, 10, 20], fill=rgba(post), outline=rgba((70, 50, 34)))
    else:
        for ry in (9, 14):
            d.rectangle([0, ry, 15, ry + 2], fill=rgba(rail))
            d.line([0, ry + 2, 15, ry + 2], fill=rgba(rail_d))
        for px in (2, 12):
            d.rectangle([px, 6, px + 2, 20], fill=rgba(post), outline=rgba((70, 50, 34)))
            d.line([px, 6, px + 2, 6], fill=rgba((190, 150, 104)))
    return img, (8, 20)


# ---------------------------------------------------------------- putting it together
def make_village():
    grid = [["." for _ in range(MW)] for _ in range(MH)]
    ground, water, pond = make_ground(grid)
    objects = []
    saved = set()

    def place(name, img_anchor, bx, by, footprint=None, letter=None, sprite_name=None):
        """Adds a sprite whose anchor pixel sits at world pixel (bx, by)."""
        img, (ax, ay) = img_anchor
        sprite_name = sprite_name or name
        if sprite_name not in saved:
            save_png(img, "World", "Objects", sprite_name + ".png")
            saved.add(sprite_name)
        objects.append({"sprite": sprite_name, "x": bx, "y": by, "ax": ax, "ay": ay, "w": img.width, "h": img.height,
                        "letter": letter or ""})
        if footprint and letter:
            fx0, fy0, fw, fh = footprint
            for ty in range(fy0, fy0 + fh):
                for tx in range(fx0, fx0 + fw):
                    if 0 <= tx < MW and 0 <= ty < MH:
                        grid[ty][tx] = letter

    for spec in BUILDINGS:
        key, letter, door, x0, y0, wt, dt, kind, sign, ex = spec
        place(key, make_building(spec), x0 * T + wt * T // 2, (y0 + dt) * T, (x0, y0, wt, dt), letter, "building_" + key)
        if kind == "church":
            grid[y0 + dt - 1][x0 + wt // 2] = door
            grid[y0 + dt - 1][x0 + wt // 2 - 1] = door
        else:
            grid[y0 + dt - 1][door_cols(spec)] = door

    fx, fy, fw, fh = FOUNTAIN
    for f in range(3):
        img, anc = fountain(f)
        save_png(img, "World", "Objects", f"fountain_{f}.png")
    objects.append({"sprite": "fountain_0", "frames": ["fountain_0", "fountain_1", "fountain_2"], "fps": 4,
                    "x": fx * T + fw * T // 2, "y": (fy + fh) * T, "ax": anc[0], "ay": anc[1], "w": 60, "h": 56, "letter": "R"})
    for ty in range(fy, fy + fh):
        for tx in range(fx, fx + fw):
            grid[ty][tx] = "R"

    # archery range: fence, sandy floor, targets
    rx0, ry0, rx1, ry1 = RANGE
    gd = ImageDraw.Draw(ground)
    for ty in range(ry0 + 1, ry1):
        for tx in range(rx0 + 1, rx1):
            X, Y = tx * T, ty * T
            sand = tones(np.clip(noise(16, 16, 3, tx * 31 + ty) * 1.2 - 0.1, 0, 1), [(196, 170, 120), (210, 186, 136), (222, 200, 152)])
            ground.paste(Image.fromarray(sand).convert("RGBA"), (X, Y))
            grid[ty][tx] = ";"
    for tx in range(rx0, rx1 + 1):
        place("fence_h", fence(False), tx * T + 8, ry0 * T + 16, (tx, ry0, 1, 1), "N", "fence_h")
        if tx not in (41, 42):
            place("fence_h", fence(False), tx * T + 8, ry1 * T + 16, (tx, ry1, 1, 1), "N", "fence_h")
    for ty in range(ry0 + 1, ry1):
        for tx in (rx0, rx1):
            place("fence_v", fence(True), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_v")
    for tx in range(rx0 + 2, rx1 - 1):
        place("target", target(), tx * T + 8, (ry0 + 1) * T + 15, (tx, ry0 + 1, 1, 1), "A")

    # trees: a border of olives and cypresses, orange trees around the plaza, an olive grove south
    tree_kinds = {"orange": (orange_tree, "Z"), "olive": (olive_tree, "O"), "cypress": (cypress, "Y")}

    def tree(kind, tx, ty, variant=0):
        fn, letter = tree_kinds[kind]
        place(kind, fn(variant), tx * T + 8, ty * T + 14, (tx, ty, 1, 1), letter, f"{kind}_{variant}")

    r = np.random.default_rng(3)
    for ty in (0, 1, MH - 2, MH - 1):
        for tx in range(MW):
            grid[ty][tx] = "O" if grid[ty][tx] == "." else grid[ty][tx]
    for tx in range(MW):
        for ty in range(MH):
            if tx in (0, 1, MW - 2, MW - 1) and grid[ty][tx] in ".,":
                grid[ty][tx] = "O"
    for tx in range(0, MW, 2):
        tree("cypress" if r.random() < 0.35 else "olive", tx, 1, int(r.integers(0, 2)))
    for tx in range(1, MW, 2):
        tree("olive" if r.random() < 0.7 else "cypress", tx, MH - 1, int(r.integers(0, 2)))
    for ty in range(3, MH - 1, 2):
        tree("olive" if r.random() < 0.6 else "cypress", 0, ty, int(r.integers(0, 2)))
        tree("olive" if r.random() < 0.6 else "cypress", MW - 1, ty, int(r.integers(0, 2)))
    for tx, ty in ((17, 13), (31, 19), (17, 19), (2, 15), (15, 15), (35, 16), (44, 14), (3, 20)):
        tree("orange", tx, ty, (tx + ty) % 2)
    for tx, ty in ((19, 9), (28, 9)):
        tree("cypress", tx, ty)
    for tx, ty in ((6, 27), (10, 28), (14, 26), (19, 28), (24, 27), (29, 28), (34, 28), (3, 25)):
        tree("olive", tx, ty, (tx * 7 + ty) % 2)

    # props
    def prop(name, maker, tx, ty, letter, w=1):
        place(name, maker(), tx * T + w * 8, ty * T + 14, (tx, ty, w, 1), letter, name)

    for tx, ty in ((17, 11), (31, 11), (17, 20), (31, 20)):
        prop("lamp", street_lamp, tx, ty, "f")
    for tx, ty in ((5, 11), (10, 11), (19, 22), (23, 22), (12, 11), (16, 11)):
        if grid[ty][tx] in ".,:":
            prop("pot", flower_pot, tx, ty, "p")
    prop("barrels", barrels, 11, 10, "b", 1)
    prop("bench", bench, 26, 19, "e", 2)
    prop("stall", fruit_stall, 20, 18, "x", 2)
    prop("cart", cart, 3, 13, "c", 2)
    for i, (tx, ty) in enumerate(((4, 17), (8, 18), (13, 17), (36, 18), (44, 18), (37, 9), (29, 14), (36, 26), (43, 29))):
        if grid[ty][tx] == ".":
            prop("lavender" if i % 2 else "rosebush", lambda k=i: bush("lavender" if k % 2 else "rose", k), tx, ty, "u")

    for ch, (tx, ty) in PEOPLE.items():
        assert grid[ty][tx] in ".,:;", (ch, grid[ty][tx])
        grid[ty][tx] = ch

    save_png(ground, "World", "village_ground.png")
    wx, wy, ww, wh = make_water_frames(water, pond)
    anims = [{"frames": ["water_0", "water_1", "water_2"], "fps": 2, "x": wx, "y": wy, "w": ww, "h": wh}]

    maps = os.path.join(ROOT, "Assets", "Resources", "Maps")
    os.makedirs(maps, exist_ok=True)
    with open(os.path.join(maps, "village.txt"), "w", newline="\n") as f:
        f.write("\n".join("".join(row) for row in grid))
    with open(os.path.join(maps, "village_objects.json"), "w", newline="\n") as f:
        json.dump({"objects": objects, "overlays": anims}, f)
    make_minimap(ground, grid)
    return grid


def make_minimap(ground, grid):
    """4 pixels per tile; buildings as roof-coloured blocks, trees as dark dots."""
    mm = ground.resize((MW * 4, MH * 4), Image.BOX)
    d = ImageDraw.Draw(mm)
    for ty in range(MH):
        for tx in range(MW):
            c = grid[ty][tx]
            X, Y = tx * 4, ty * 4
            if c in "kqswmhdiBCSWMHDI":
                d.rectangle([X, Y - 4, X + 3, Y + 3], fill=(176, 82, 54) if c != "i" and c != "I" else (196, 170, 120))
            elif c in "ZOY":
                d.ellipse([X, Y, X + 3, Y + 3], fill=(44, 78, 44))
            elif c in "N":
                d.rectangle([X + 1, Y + 1, X + 2, Y + 2], fill=(120, 86, 56))
    save_png(mm, "UI", "minimap.png")


if __name__ == "__main__":
    g = make_village()
    print("\n".join("".join(r) for r in g))
