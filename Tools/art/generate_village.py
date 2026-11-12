"""Builds the whole world: the village of Valdeluz, its farm, river and wolf forest, and the
insides of the shops and your house.

Run:  python Tools/art/generate_village.py   (after the other art scripts; uses their icons)

Outputs
  Art/World/village_ground.png, Art/World/Rooms/*.png   flat ground (drawn first)
  Art/World/Objects/*.png                              buildings, trees, props (sorted, walk-behind)
  Art/World/water_*.png                                pond sparkle frames
  Art/UI/minimap.png                                   the corner map
  Maps/village.txt                                     one letter per tile, village AND rooms
  Maps/village_objects.json                            object placements, rooms (areas), doors
                                                       (portals), spawn points, and the Spanish/
                                                       English name of every letter

Rooms sit to the right of the village in the same tile grid (the game only ever shows one area
at a time), which keeps collisions and doors simple.
"""
import json
import math
import os

import numpy as np
from PIL import Image, ImageDraw, ImageEnhance
from scipy.ndimage import binary_dilation, distance_transform_edt, gaussian_filter
from scipy.spatial import cKDTree

import village_art as va
from village_art import (T, DIRT, DRY, GRASS, STONE, WATER, darker, fractal, lighter, noise, rgba, save_png,
                         shadow_ellipse, tones)
from generate_art import ART, ROOT

VW, VH = 112, 72                 # the village area in tiles
GW, GH = 146, 72                 # the whole grid (village + rooms)
PW, PH = VW * T, VH * T
rng = np.random.default_rng(21)

# ---------------------------------------------------------------- what every letter means
WALKABLE = ".,:;=_X-\"ÇŠ"       # crops can be walked between; Ç = an open gate; Š = stairs
WORDS = {
    ".": ("La hierba", "Grass"), ",": ("El camino", "Road"), ":": ("La plaza", "Town Square"),
    ";": ("El campo de tiro", "Archery Range"), "=": ("El muelle", "Dock"), "-": ("El puente", "Bridge"),
    "_": ("El suelo", "Floor"), "X": ("La salida", "Exit"), "#": ("La pared", "Wall"),
    "F": ("El estanque", "Pond"), "V": ("El río", "River"), "R": ("La fuente rúnica", "Rune Fountain"),
    "A": ("La diana", "Target"), "N": ("La valla", "Fence"), "Z": ("El naranjo", "Orange Tree"),
    "O": ("El olivo", "Olive Tree"), "Y": ("El ciprés", "Cypress"), "}": ("El pino", "Pine Tree"),
    "u": ("El arbusto", "Bush"), "f": ("La farola", "Street Lamp"), "p": ("La maceta", "Flower Pot"),
    "b": ("Los barriles", "Barrels"), "e": ("El banco", "Bench"), "x": ("El puesto de fruta", "Fruit Stall"),
    "c": ("El carro", "Cart"), "l": ("El pozo", "Well"), "n": ("El tablón de lecciones", "Lesson Board"),
    "g": ("El letrero", "Signpost"), "T": ("Las cajas", "Crates"), "|": ("La paja", "Hay"),
    "?": ("El espantapájaros", "Scarecrow"), "o": ("El gallinero", "Chicken Coop"), '"': ("Los cultivos", "Crops"),
    "{": ("La viña", "Vineyard"),
    "k": ("La panadería", "Bakery"), "q": ("La dulcería", "Candy Shop"), "s": ("La tienda", "General Store"),
    "w": ("La armería", "Weapon Shop"), "j": ("La herrería", "Blacksmith"), "m": ("La tienda de mascotas", "Pet Shop"),
    "h": ("Tu casa", "Your House"), "d": ("La casa", "House"), "i": ("La iglesia", "Church"),
    "a": ("El ayuntamiento", "Town Hall"), "r": ("La taberna", "Tavern"), "v": ("El granero", "Barn"),
    "y": ("El molino", "Windmill"),
    "K": ("El mostrador", "Counter"), "Q": ("El menú", "Menu Board"), "L": ("La estantería", "Shelf"),
    "E": ("El horno", "Oven"), "U": ("El yunque", "Anvil"), "J": ("La cama", "Bed"), "t": ("La mesa", "Table"),
    "G": ("La chimenea", "Fireplace"),
    "ç": ("La puerta de la valla", "Gate"), "Ç": ("La puerta de la valla", "Gate"),
    "œ": ("La tienda de pesca", "Fishing Shop"),
    "Ŧ": ("El muro de la terraza", "Terrace Wall"), "Š": ("La escalera", "Stairs"),
    "þ": ("La torre rúnica", "Rune Tower"), "ø": ("El altar rúnico", "Rune Altar"),
    "ð": ("La Ciudadela del Sol", "Citadel of the Sun"), "Ð": ("La entrada de la Ciudadela", "Citadel Entrance"),
    "[": ("La semillería", "Seed Shop"), "(": ("La tienda de música", "Music Shop"),
    "`": ("La escuela de música", "Music School"), "+": ("El huerto", "Vegetable Patch"), "ñ": ("El piano", "Piano"),
}
for door in "BCSWMHDI^<>])'ÞŒ":
    WORDS[door] = ("La puerta", "Door")

# ---------------------------------------------------------------- the village layout (tiles)
PLAZA = (36, 20, 56, 34)
ROADS = [(6, 18, 74, 19), (6, 27, 35, 28), (57, 27, 79, 28), (10, 38, 62, 39), (34, 18, 35, 39), (57, 18, 58, 39),
         (66, 8, 67, 17), (8, 10, 9, 17), (52, 40, 53, 46), (84, 27, 100, 28), (100, 16, 101, 40), (9, 20, 10, 27)]
BUILDINGS = [
    # key, letter, door, x0, y0, width, depth, kind, sign, extras
    ("bakery", "k", "B", 26, 16, 6, 2, "shop", "Panadería", {"shutters": (78, 112, 84), "awning": (178, 64, 52), "pots": True}),
    ("house_n", "d", "D", 11, 16, 5, 2, "house", None, {"shutters": (66, 100, 140), "two_story": True}),
    ("church", "i", "I", 42, 14, 8, 4, "church", None, {}),
    ("hall", "a", "<", 51, 15, 7, 3, "shop", "Ayuntamiento", {"shutters": (66, 100, 140), "two_story": True, "wall": (232, 214, 180)}),
    ("candy", "q", "C", 60, 16, 6, 2, "shop", "Dulcería", {"shutters": (164, 80, 110), "awning": (200, 110, 140), "wall": (246, 226, 216)}),
    ("store", "s", "S", 27, 25, 6, 2, "shop", "Tienda", {"shutters": (66, 100, 140), "awning": (70, 110, 150)}),
    ("home", "h", "H", 12, 25, 5, 2, "house", None, {"shutters": (78, 112, 84), "pots": True}),
    ("seeds", "[", "]", 19, 25, 5, 2, "shop", "Semillas", {"shutters": (110, 150, 70), "awning": (120, 160, 70), "pots": True}),
    ("music_store", "(", ")", 18, 16, 6, 2, "shop", "Música", {"shutters": (120, 70, 150), "awning": (150, 90, 180), "wall": (240, 228, 214)}),
    ("music_school", "`", "'", 68, 16, 6, 2, "shop", "Escuela", {"shutters": (60, 80, 150), "two_story": True, "wall": (236, 224, 204)}),
    ("weapons", "w", "W", 60, 25, 6, 2, "shop", "Armería", {"shutters": (90, 74, 60), "awning": (96, 90, 84), "wall": (226, 214, 192), "shield": True}),
    ("smithy", "j", "^", 68, 25, 6, 2, "shop", "Herrería", {"shutters": (70, 70, 74), "awning": (80, 80, 86), "wall": (196, 190, 178)}),
    ("pets", "m", "M", 38, 36, 6, 2, "shop", "Mascotas", {"shutters": (196, 140, 60), "awning": (210, 150, 70), "paw": True}),
    ("tavern", "r", ">", 46, 36, 6, 2, "shop", "Taberna", {"shutters": (120, 70, 50), "two_story": True, "wall": (236, 222, 196)}),
    ("house_s", "d", "D", 54, 36, 5, 2, "house", None, {"shutters": (66, 100, 140), "pots": True}),
    ("fishing", "œ", "Œ", 54, 42, 6, 2, "shop", "Pesca", {"shutters": (50, 110, 150), "awning": (50, 120, 170), "wall": (236, 232, 222), "pots": True}),
]
FOUNTAIN = (45, 26, 3, 2)
TERRACE = (42, 23, 50, 29)         # the raised square round the fountain (top surface, tiles)
TERRACE_STAIRS = (26, 27)          # rows of the stairs that come down at the west and east ends
RANGE = (62, 4, 71, 12)
POND = (58.0, 52.0, 7.0, 5.0)
FARM = (12, 30, 24, 36)            # crop field (inside the fence)
YARD = (27, 31, 31, 36)            # chicken yard
RIVER_X = 80.5
BRIDGE_ROWS = (27, 28)
PEOPLE = {"1": (24, 20), "3": (64, 20), "4": (66, 9), "5": (49, 28), "%": (76, 29), "&": (9, 29), "P": (46, 31)}

# rooms: key, Spanish name, English name, shop id, keeper marker, grid position (x0, y0); size 14 x 10
ROOMS = [
    ("bakery", "La panadería", "Bakery", "bakery", "6", (114, 1)),
    ("candy", "La dulcería", "Candy Shop", "candy", "7", (114, 12)),
    ("store", "La tienda", "General Store", "store", "8", (114, 23)),
    ("weapons", "La armería", "Weapon Shop", "weapons", "9", (114, 34)),
    ("smithy", "La herrería", "Blacksmith", "smithy", "@", (130, 1)),
    ("pets", "La tienda de mascotas", "Pet Shop", "pets", "$", (130, 12)),
    ("home", "Tu casa", "Your House", "", "", (130, 23)),
    ("seeds", "La semillería", "Seed Shop", "seeds", "*", (130, 34)),
    ("music_store", "La tienda de música", "Music Shop", "music", "/", (114, 45)),
    ("music_school", "La escuela de música", "Music School", "", "!", (130, 45)),
    ("tower", "La torre rúnica", "Rune Tower", "", "Ø", (114, 56)),
    ("fishing", "La tienda de pesca", "Fishing Shop", "fishing", "§", (130, 56)),
]
RW, RH = 14, 10


def tile_mask(rects, w=PW, h=PH):
    m = np.zeros((h, w), bool)
    for x0, y0, x1, y1 in rects:
        m[y0 * T:(y1 + 1) * T, x0 * T:(x1 + 1) * T] = True
    return m


# ---------------------------------------------------------------- the ground
def make_ground(grid):
    img = np.zeros((PH, PW, 3), np.uint8)
    yy, xx = np.mgrid[0:PH, 0:PW]

    n = fractal(PW, PH, 1)
    img[:] = tones(np.clip((n - 0.3) * 1.45 + 0.1, 0, 1), GRASS)
    dry = fractal(PW, PH, 9, (64, 20, 6)) > 0.72
    img[dry] = tones(np.clip(fractal(PW, PH, 4) * 1.2 - 0.2, 0, 1), DRY)[dry]
    # the forest floor beyond the river is darker and mossier
    forest = xx > (RIVER_X + 3) * T
    dark = [(44, 70, 44), (56, 84, 50), (70, 98, 56), (86, 112, 62), (104, 126, 70)]
    img[forest] = tones(np.clip((n - 0.3) * 1.4 + 0.05, 0, 1), dark)[forest]

    # river: a wavy band running north to south
    wave = RIVER_X * T + np.sin(yy / 70.0) * 18 + np.sin(yy / 23.0) * 5
    rdist = np.abs(xx - wave)
    river = rdist < 30
    bank = (rdist >= 30) & (rdist < 36)
    img[bank] = tones(np.clip(noise(PW, PH, 3, 18), 0, 1), [(92, 80, 58), (112, 98, 70), (134, 120, 88)])[bank]
    rv = 1 - np.clip((30 - rdist) / 30, 0, 1)
    img[river] = tones(np.clip(rv * 0.9 + noise(PW, PH, 5, 19) * 0.1, 0, 1), WATER)[river]

    # roads with soft edges and pebbles
    road = tile_mask(ROADS)
    road = (gaussian_filter(road.astype(float), 3) + (noise(PW, PH, 4, 7) - 0.5) * 0.5) > 0.5
    road &= ~river
    img[road] = tones(np.clip(fractal(PW, PH, 11, (16, 6, 2)) * 1.3 - 0.15, 0, 1), DIRT)[road]

    # pond
    cx, cy, rx, ry = POND[0] * T, POND[1] * T, POND[2] * T, POND[3] * T
    rad = np.hypot((xx - cx) / rx, (yy - cy) / ry) + (noise(PW, PH, 24, 5) - 0.5) * 0.35
    water = rad < 1.0
    pbank = (rad >= 1.0) & (rad < 1.1)
    img[pbank] = tones(np.clip(noise(PW, PH, 3, 8), 0, 1), [(92, 80, 58), (112, 98, 70), (134, 120, 88)])[pbank]
    dv = 1 - np.clip(distance_transform_edt(water) / 30.0, 0, 1)
    img[water] = tones(dv * 0.85 + noise(PW, PH, 6, 3) * 0.15, WATER)[water]

    # plaza cobblestones
    plaza = tile_mask([PLAZA])
    px0, py0, px1, py1 = PLAZA[0] * T, PLAZA[1] * T, (PLAZA[2] + 1) * T, (PLAZA[3] + 1) * T
    seeds = []
    for sy in range(py0 - 6, py1 + 6, 6):
        for sx in range(px0 - 6, px1 + 6, 7):
            seeds.append((sx + rng.uniform(-2, 2) + (3 if (sy // 6) % 2 else 0), sy + rng.uniform(-1.5, 1.5)))
    seeds = np.array(seeds)
    pts = np.stack([xx[plaza], yy[plaza]], 1)
    dist, idx = cKDTree(seeds).query(pts, k=2)
    base = rng.uniform(0.25, 0.75, len(seeds))
    off = pts - seeds[idx[:, 0]]
    v = np.clip(base[idx[:, 0]] - (off[:, 0] * 0.6 + off[:, 1] * 0.8) / 4.0 * 0.35, 0, 1)
    cols = np.array(STONE)[np.clip((v * 4.99).astype(int), 0, 4)]
    cols[(dist[:, 1] - dist[:, 0]) < 1.3] = (92, 84, 78)
    img[plaza] = cols

    # farm: tilled rows of crops (wheat, cabbages, tomatoes)
    fx0, fy0, fx1, fy1 = FARM
    for ty in range(fy0, fy1 + 1):
        for tx in range(fx0, fx1 + 1):
            X, Y = tx * T, ty * T
            img[Y:Y + T, X:X + T] = (112, 80, 54)
            for k in range(0, 16, 4):
                img[Y + k:Y + k + 1, X:X + T] = (90, 64, 44)
    out = Image.fromarray(img).convert("RGBA")
    d = ImageDraw.Draw(out)
    for ty in range(fy0, fy1 + 1):
        for tx in range(fx0, fx1 + 1):
            X, Y = tx * T, ty * T
            crop = (tx - fx0) // 4
            for k in range(2, 16, 4):
                for j in range(1, 16, 4):
                    if crop == 0:      # wheat
                        d.line([X + j, Y + k + 2, X + j, Y + k - 2], fill=(214, 180, 80))
                        d.point((X + j, Y + k - 3), fill=(240, 210, 110))
                    elif crop == 1:    # cabbages
                        d.ellipse([X + j - 1, Y + k - 1, X + j + 2, Y + k + 2], fill=(110, 170, 80), outline=(70, 120, 60))
                    else:              # tomatoes
                        d.line([X + j, Y + k + 2, X + j, Y + k - 2], fill=(70, 120, 60))
                        d.point((X + j + 1, Y + k - 1), fill=(220, 60, 50))
            grid[ty][tx] = '"'

    # bridge over the river
    for ty in BRIDGE_ROWS:
        row_wave = RIVER_X * T + math.sin(ty * T / 70.0) * 18
        for tx in range(int(row_wave // T) - 3, int(row_wave // T) + 4):
            X, Y = tx * T, ty * T
            d.rectangle([X, Y, X + 15, Y + 15], fill=(150, 108, 70))
            for k in range(0, 16, 4):
                d.line([X + k, Y, X + k, Y + 15], fill=(112, 78, 52))
            if ty == BRIDGE_ROWS[0]:
                d.line([X, Y, X + 15, Y], fill=(86, 60, 40))
                d.rectangle([X, Y - 2, X + 15, Y - 1], fill=(120, 84, 54))
            else:
                d.line([X, Y + 15, X + 15, Y + 15], fill=(86, 60, 40))
            grid[ty][tx] = "-"

    # tufts and flowers on grass
    grass_ok = ~(road | plaza | water | pbank | river | bank)
    for _ in range(26000):
        x, y = int(rng.integers(1, PW - 2)), int(rng.integers(2, PH - 1))
        if not grass_ok[y, x]:
            continue
        r = rng.random()
        if x > (RIVER_X + 3) * T:
            if r < 0.85:
                d.point((x, y), fill=(36, 60, 38)); d.point((x + 1, y - 1), fill=(36, 60, 38))
            elif r < 0.92:
                d.point((x, y), fill=(200, 80, 60))          # mushroom
                d.point((x, y + 1), fill=(230, 220, 200))
            continue
        if r < 0.72:
            d.point((x, y), fill=(54, 84, 46)); d.point((x + 1, y - 1), fill=(54, 84, 46)); d.point((x - 1, y - 1), fill=(150, 176, 92))
        elif r < 0.80:
            d.point((x, y), fill=(206, 54, 44)); d.point((x, y + 1), fill=(60, 90, 50))
        elif r < 0.87:
            d.point((x, y), fill=(244, 240, 226))
        elif r < 0.93:
            d.point((x, y), fill=(150, 120, 196)); d.point((x, y - 1), fill=(170, 140, 210))
        else:
            d.point((x, y), fill=(232, 196, 70))
    for _ in range(4000):
        x, y = int(rng.integers(0, PW - 2)), int(rng.integers(0, PH - 2))
        if road[y, x] and not plaza[y, x]:
            d.point((x, y), fill=(120, 100, 80)); d.point((x + 1, y), fill=(226, 208, 176))
    # reeds round the pond
    for _ in range(60):
        ang = rng.uniform(0, 2 * math.pi)
        x = int(cx + math.cos(ang) * rx * rng.uniform(0.95, 1.05)); y = int(cy + math.sin(ang) * ry * rng.uniform(0.95, 1.05))
        for k in range(int(rng.integers(2, 4))):
            h = int(rng.integers(5, 10))
            d.line([x + k * 2 - 2, y, x + k * 2 - 2 + int(rng.integers(-1, 2)), y - h], fill=(70, 110, 58))
    for _ in range(12):
        x, y = int(cx + rng.normal(0, rx * 0.45)), int(cy + rng.normal(0, ry * 0.45))
        if 0 <= y < PH and 0 <= x < PW and water[y, x]:
            d.ellipse([x, y, x + 7, y + 4], fill=(74, 130, 70), outline=(46, 92, 54))

    # the grid
    for ty in range(VH):
        for tx in range(VW):
            c = (min(PH - 1, ty * T + 8), min(PW - 1, tx * T + 8))
            if grid[ty][tx] in '"-':
                continue
            if water[c]:
                grid[ty][tx] = "F"
            elif river[c]:
                grid[ty][tx] = "V"
            elif plaza[c]:
                grid[ty][tx] = ":"
            elif tile_mask(ROADS)[c]:
                grid[ty][tx] = ","

    raised_terrace(out, d, grid)

    # dock into the pond from the west
    dock_y = int(POND[1])
    x = int(POND[0] - POND[2]) - 2
    while grid[dock_y][x + 1] != "F":
        x += 1
    for tx in range(x, x + 3):
        X, Y = tx * T, dock_y * T
        d.rectangle([X, Y + 2, X + 15, Y + 13], fill=(150, 108, 70))
        for k in range(0, 16, 4):
            d.line([X + k, Y + 2, X + k, Y + 13], fill=(112, 78, 52))
        d.line([X, Y + 14, X + 15, Y + 14], fill=(40, 70, 96))
        grid[dock_y][tx] = "="
    return out, water, (x - 1, dock_y)


# ---------------------------------------------------------------- the raised terrace in the plaza
def raised_terrace(out, d, grid):
    """The middle of the plaza is a raised stone terrace (the fountain stands on it): a low kerb round
    the top, stone walls down its sides and front, and stairs coming down at the west and east ends."""
    tx0, ty0, tx1, ty1 = TERRACE
    sy0, sy1 = TERRACE_STAIRS
    X0, Y0, X1, Y1 = tx0 * T, ty0 * T, (tx1 + 1) * T, (ty1 + 1) * T
    kerb, kerb_l, kerb_d = (196, 188, 176), (226, 220, 208), (118, 110, 102)
    wall, wall_d, mortar = (126, 116, 110), (96, 88, 84), (74, 66, 62)
    L, R = X0 - T, X1 + T - 1              # outer edges of the side kerbs

    # shadows the terrace throws on the plaza (south and both sides), drawn first
    shadow = Image.new("RGBA", out.size, (0, 0, 0, 0))
    sd = ImageDraw.Draw(shadow)
    sd.rectangle([L - 3, Y0 - T + 4, L - 1, Y1 + T + 6], fill=(30, 22, 18, 80))
    sd.rectangle([R + 1, Y0 - T + 4, R + 4, Y1 + T + 6], fill=(30, 22, 18, 80))
    sd.rectangle([L - 3, Y1 + T, R + 4, Y1 + T + 7], fill=(30, 22, 18, 95))
    out.alpha_composite(shadow)

    # the top catches more light than the plaza below
    top = out.crop((L, Y0 - T, R + 1, Y1))
    top = ImageEnhance.Brightness(top).enhance(1.18)
    out.paste(top, (L, Y0 - T))

    def bricks(x0, y0, x1, y1, course=5, joint=10):
        d.rectangle([x0, y0, x1, y1], fill=wall)
        for i, y in enumerate(range(y0 + course, y1 + 1, course)):
            d.line([x0, y, x1, y], fill=mortar)
            for x in range(x0 + (joint // 2 if i % 2 else 0), x1, joint):
                d.line([x, y - course + 1, x, y], fill=mortar)
        d.line([x0, y1, x1, y1], fill=wall_d)

    # front wall (a full tile tall) with a lit coping stone along the top
    bricks(L, Y1, R, Y1 + T - 1)
    d.rectangle([L, Y1, R, Y1 + 2], fill=kerb_l)
    d.line([L, Y1 + 3, R, Y1 + 3], fill=kerb_d)

    # back kerb: a low lit edge
    d.rectangle([L, Y0 - T + 5, R, Y0 - 3], fill=kerb)
    d.line([L, Y0 - T + 5, R, Y0 - T + 5], fill=kerb_l)
    d.line([L, Y0 - 3, R, Y0 - 3], fill=kerb_d)
    for x in range(L + 8, R, 12):
        d.line([x, Y0 - T + 6, x, Y0 - 4], fill=kerb_d)

    # side kerbs: the lit top of the edge, then the outer wall face going down (darker)
    for side, X in ((-1, L), (1, X1)):
        ys = [(Y0 - T + 5, sy0 * T - 1), ((sy1 + 1) * T, Y1 - 1)]
        for ya, yb in ys:
            top_x = (X + 8, X + 15) if side < 0 else (X, X + 7)
            face_x = (X, X + 7) if side < 0 else (X + 8, X + 15)
            d.rectangle([top_x[0], ya, top_x[1], yb], fill=kerb)
            d.line([top_x[0] if side > 0 else top_x[1], ya, top_x[0] if side > 0 else top_x[1], yb], fill=kerb_l)
            bricks(face_x[0], ya, face_x[1], yb, course=4, joint=8)
            d.line([face_x[1] if side < 0 else face_x[0], ya, face_x[1] if side < 0 else face_x[0], yb], fill=kerb_d)

    # stairs at both ends: three steps going down and out, each with a lit tread and a dark riser
    Ys, Ye = sy0 * T, (sy1 + 1) * T
    for side in (-1, 1):
        edge = tx0 - 1 if side < 0 else tx1 + 1
        for k in range(3):                          # k = 0 at the top (next to the terrace)
            tx = edge + side * k
            X = tx * T
            tone = 1.06 - k * 0.09
            tread = tuple(min(255, int(c * tone)) for c in kerb)
            d.rectangle([X, Ys, X + T - 1, Ye - 1], fill=tread)
            rx = (X + T - 4, X + T - 1) if side > 0 else (X, X + 3)            # riser: the drop to the next step
            d.rectangle([rx[0], Ys, rx[1], Ye - 1], fill=tuple(int(c * tone) for c in wall_d))
            nx = X if side > 0 else X + T - 1
            d.line([nx, Ys, nx, Ye - 1], fill=kerb_l)                           # lit nose
            d.line([X, Ys + T, X + T - 1, Ys + T], fill=tuple(int(c * 0.8) for c in tread))
            face = 12 - k * 4                                                    # the stair's side wall gets lower
            bricks(X, Ye, X + T - 1, Ye + face, course=4, joint=8)
            for ty in (sy0, sy1):
                grid[ty][tx] = "Š"[0] if False else "Š"

    # walls are solid; the stairs are walkable
    for tx in range(tx0 - 1, tx1 + 2):
        grid[ty0 - 1][tx] = "Ŧ"
        grid[ty1 + 1][tx] = "Ŧ"
    for ty in range(ty0, ty1 + 1):
        for tx in (tx0 - 1, tx1 + 1):
            if not sy0 <= ty <= sy1:
                grid[ty][tx] = "Ŧ"


# ---------------------------------------------------------------- new props
def pine(seed):
    """Stone pine (pino piñonero): a tall bare trunk and a wide umbrella canopy."""
    w, h = 50, 62
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 25, 57, 16, 4)
    d = ImageDraw.Draw(img)
    d.polygon([(22, 57), (28, 57), (27, 24), (24, 24)], fill=rgba((120, 86, 60)))
    d.line([23, 56, 24, 25], fill=rgba((150, 112, 80)))
    d.line([27, 30, 34, 20], fill=rgba((110, 78, 54)), width=2)
    d.line([24, 32, 16, 22], fill=rgba((110, 78, 54)), width=2)
    canopy, m = va.blob_canopy(w, 30, [(12, 16, 9), (25, 12, 12), (38, 16, 9), (18, 20, 7), (32, 20, 7)],
                               [(30, 62, 40), (42, 80, 48), (58, 100, 58), (76, 120, 66), (98, 140, 78)], seed)
    img.alpha_composite(canopy, (0, 2))
    return img, (25, 57)


def windmill(frame):
    w, h = 64, 96
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    shadow_ellipse(img, 34, 90, 20, 5)
    d = ImageDraw.Draw(img)
    d.polygon([(18, 90), (46, 90), (42, 36), (22, 36)], fill=rgba((242, 236, 222)))         # white tower
    d.polygon([(38, 90), (46, 90), (42, 36), (36, 36)], fill=rgba((206, 198, 182)))
    d.polygon([(18, 90), (20, 90), (23, 36), (22, 36)], fill=rgba((255, 252, 244)))
    d.rectangle([28, 72, 35, 89], fill=rgba((112, 70, 44)))
    d.ellipse([28, 68, 35, 76], fill=rgba((112, 70, 44)))
    d.rectangle([30, 50, 34, 56], fill=rgba((40, 46, 60)))
    d.polygon([(18, 37), (46, 37), (32, 18)], fill=rgba((60, 56, 60)))                      # conical cap
    d.polygon([(32, 18), (46, 37), (38, 37)], fill=rgba((44, 40, 44)))
    cx, cy = 32, 30
    for k in range(4):                                                                       # sails
        ang = math.radians(frame * 22.5 + k * 90)
        ex, ey = cx + math.cos(ang) * 28, cy + math.sin(ang) * 28
        px, py = -math.sin(ang) * 5, math.cos(ang) * 5
        d.line([cx, cy, ex, ey], fill=rgba((96, 70, 50)), width=2)
        quad = [(cx + math.cos(ang) * 8, cy + math.sin(ang) * 8), (ex, ey), (ex + px, ey + py),
                (cx + math.cos(ang) * 8 + px, cy + math.sin(ang) * 8 + py)]
        d.polygon(quad, fill=rgba((236, 226, 200)), outline=rgba((120, 96, 70)))
    d.ellipse([cx - 3, cy - 3, cx + 3, cy + 3], fill=rgba((80, 60, 44)))
    return img, (32, 90)


def barn():
    w, h = 112, 92
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    sh = Image.new("RGBA", img.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).polygon([(98, 88), (110, 82), (110, 90), (12, 91)], fill=(20, 28, 16, 70))
    img.alpha_composite(sh)
    d.polygon([(98, 88), (108, 82), (108, 40), (98, 46)], fill=rgba((120, 46, 38)))         # side
    d.rectangle([6, 46, 97, 88], fill=rgba((164, 62, 48)))                                    # red planks
    for x in range(8, 97, 5):
        d.line([x, 47, x, 87], fill=rgba((134, 48, 38)))
    d.polygon([(4, 47), (52, 18), (100, 47)], fill=rgba((164, 62, 48)))                       # gable
    for x in range(10, 96, 5):
        d.line([x, 47, x, max(47 - (x - 4) * 0.6, 47 - (100 - x) * 0.6)], fill=rgba((134, 48, 38)))
    d.line([(2, 48), (52, 17), (102, 48)], fill=rgba((70, 60, 60)), width=3)                 # roof edge
    d.polygon([(52, 17), (102, 48), (110, 42), (60, 12)], fill=rgba((90, 80, 76)))
    d.rectangle([36, 58, 67, 88], fill=rgba((238, 228, 206)))                                 # big doors (white X)
    d.rectangle([38, 60, 65, 88], fill=rgba((150, 56, 44)))
    d.line([(38, 60), (65, 88)], fill=rgba((238, 228, 206)), width=2)
    d.line([(65, 60), (38, 88)], fill=rgba((238, 228, 206)), width=2)
    d.line([(51, 60), (51, 88)], fill=rgba((238, 228, 206)))
    d.rectangle([44, 30, 59, 42], fill=rgba((60, 40, 30)))                                    # hay loft
    for x in range(45, 59, 3):
        d.line([x, 38, x + 1, 42], fill=rgba((220, 190, 90)))
    return img, (52, 88)


def well():
    img = Image.new("RGBA", (32, 40), (0, 0, 0, 0))
    shadow_ellipse(img, 16, 35, 13, 3)
    d = ImageDraw.Draw(img)
    d.ellipse([4, 24, 28, 36], fill=rgba((128, 118, 104)))
    d.rectangle([4, 28, 28, 32], fill=rgba((128, 118, 104)))
    d.ellipse([4, 20, 28, 30], fill=rgba((176, 164, 146)), outline=rgba((96, 88, 78)))
    d.ellipse([8, 22, 24, 28], fill=rgba((30, 40, 56)))
    for x in (6, 25):
        d.rectangle([x, 4, x + 1, 25], fill=rgba((110, 78, 52)))
    d.polygon([(2, 6), (30, 6), (16, -2)], fill=rgba(va.ROOF[2]), outline=rgba(va.ROOF[0]))
    d.line([8, 10, 24, 10], fill=rgba((90, 64, 44)), width=2)
    d.line([16, 10, 16, 18], fill=rgba((200, 190, 160)))
    d.rectangle([14, 18, 18, 22], fill=rgba((120, 86, 56)))
    return img, (16, 35)


def notice_board():
    img = Image.new("RGBA", (30, 34), (0, 0, 0, 0))
    shadow_ellipse(img, 15, 31, 12, 2)
    d = ImageDraw.Draw(img)
    for x in (4, 24):
        d.rectangle([x, 8, x + 2, 31], fill=rgba((110, 78, 52)))
    d.rectangle([2, 4, 28, 22], fill=rgba((150, 108, 70)), outline=rgba((86, 60, 40)))
    for x0, y0, c in ((5, 7, (246, 240, 220)), (14, 6, (240, 226, 190)), (8, 14, (250, 246, 230)), (18, 13, (236, 230, 210))):
        d.rectangle([x0, y0, x0 + 7, y0 + 6], fill=rgba(c))
        d.line([x0 + 1, y0 + 2, x0 + 5, y0 + 2], fill=rgba((120, 100, 90)))
        d.point((x0 + 3, y0), fill=rgba((200, 40, 40)))
    d.polygon([(0, 5), (30, 5), (15, -1)], fill=rgba(va.ROOF[2]), outline=rgba(va.ROOF[0]))
    return img, (15, 31)


def signpost():
    img = Image.new("RGBA", (34, 34), (0, 0, 0, 0))
    shadow_ellipse(img, 16, 31, 6, 2)
    d = ImageDraw.Draw(img)
    d.rectangle([15, 4, 17, 31], fill=rgba((110, 78, 52)))
    d.polygon([(4, 6), (28, 6), (32, 10), (28, 14), (4, 14)], fill=rgba((176, 130, 84)), outline=rgba((86, 60, 40)))
    d.polygon([(30, 17), (6, 17), (2, 21), (6, 25), (30, 25)], fill=rgba((176, 130, 84)), outline=rgba((86, 60, 40)))
    d.text((6, 4), "Bosque", font=va.FONT, fill=rgba((60, 40, 30))) if False else None
    for x in range(7, 25, 3):
        d.point((x, 10), fill=rgba((80, 56, 40)))
        d.point((x + 2, 21), fill=rgba((80, 56, 40)))
    return img, (16, 31)


def crates():
    img = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    shadow_ellipse(img, 12, 21, 11, 2)
    d = ImageDraw.Draw(img)
    for x0, y0 in ((1, 9), (12, 9), (6, 0)):
        d.rectangle([x0, y0, x0 + 10, y0 + 11], fill=rgba((176, 130, 84)), outline=rgba((96, 66, 44)))
        d.line([x0, y0, x0 + 10, y0 + 11], fill=rgba((130, 92, 60)))
        d.line([x0 + 10, y0, x0, y0 + 11], fill=rgba((130, 92, 60)))
    return img, (12, 21)


def haystack():
    img = Image.new("RGBA", (28, 24), (0, 0, 0, 0))
    shadow_ellipse(img, 14, 21, 12, 2)
    d = ImageDraw.Draw(img)
    d.ellipse([2, 4, 26, 24], fill=rgba((214, 182, 90)))
    d.rectangle([2, 14, 26, 21], fill=rgba((214, 182, 90)))
    r = np.random.default_rng(2)
    for _ in range(40):
        x, y = int(r.integers(4, 24)), int(r.integers(6, 21))
        d.point((x, y), fill=rgba((180, 148, 64) if r.random() < 0.5 else (240, 214, 130)))
    return img, (14, 21)


def scarecrow():
    img = Image.new("RGBA", (24, 36), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([11, 10, 12, 35], fill=rgba((110, 78, 52)))
    d.rectangle([2, 15, 21, 16], fill=rgba((110, 78, 52)))
    d.polygon([(6, 14), (17, 14), (19, 26), (4, 26)], fill=rgba((80, 110, 160)))
    d.ellipse([7, 5, 16, 14], fill=rgba((230, 210, 160)))
    d.point((10, 9), fill=rgba((30, 30, 30))); d.point((13, 9), fill=rgba((30, 30, 30)))
    d.ellipse([2, 4, 21, 8], fill=rgba((200, 168, 108)))
    d.rectangle([7, 0, 16, 5], fill=rgba((200, 168, 108)))
    return img, (12, 34)


def coop():
    img = Image.new("RGBA", (36, 32), (0, 0, 0, 0))
    shadow_ellipse(img, 18, 29, 16, 3)
    d = ImageDraw.Draw(img)
    d.rectangle([4, 12, 32, 28], fill=rgba((176, 130, 84)), outline=rgba((96, 66, 44)))
    for x in range(6, 32, 4):
        d.line([x, 13, x, 27], fill=rgba((150, 108, 70)))
    d.polygon([(1, 13), (35, 13), (18, 1)], fill=rgba(va.ROOF[2]), outline=rgba(va.ROOF[0]))
    d.rectangle([14, 18, 21, 28], fill=rgba((60, 40, 30)))
    d.line([10, 28, 14, 31], fill=rgba((120, 86, 56)), width=2)
    return img, (18, 29)


def vine():
    img = Image.new("RGBA", (18, 26), (0, 0, 0, 0))
    shadow_ellipse(img, 9, 23, 7, 2)
    d = ImageDraw.Draw(img)
    d.rectangle([8, 6, 9, 23], fill=rgba((110, 78, 52)))
    canopy, m = va.blob_canopy(18, 16, [(5, 8, 5), (12, 8, 5), (9, 5, 4)],
                               [(46, 84, 44), (62, 104, 54), (84, 128, 64), (108, 150, 78), (134, 172, 92)], 9)
    img.alpha_composite(canopy, (0, 0))
    d = ImageDraw.Draw(img)
    for x, y in ((4, 10), (12, 11), (8, 13)):
        for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1), (0, 2)):
            d.point((x + dx, y + dy), fill=rgba((110, 50, 120)))
        d.point((x, y), fill=rgba((170, 110, 180)))
    return img, (9, 23)


# ---------------------------------------------------------------- shop and house interiors
ROOM_STYLE = {
    "bakery": {"wall": (240, 226, 200), "dado": (70, 110, 170), "floor": "tile", "floor_col": (176, 96, 60)},
    "candy": {"wall": (250, 220, 230), "dado": (200, 110, 150), "floor": "check", "floor_col": (240, 240, 240)},
    "store": {"wall": (226, 214, 190), "dado": (120, 96, 70), "floor": "wood", "floor_col": (150, 108, 70)},
    "weapons": {"wall": (170, 160, 150), "dado": (90, 80, 76), "floor": "stone", "floor_col": (130, 124, 116)},
    "smithy": {"wall": (120, 110, 104), "dado": (70, 64, 62), "floor": "stone", "floor_col": (96, 90, 86)},
    "pets": {"wall": (236, 230, 196), "dado": (110, 150, 80), "floor": "wood", "floor_col": (170, 126, 84)},
    "home": {"wall": (242, 234, 214), "dado": (70, 110, 170), "floor": "wood", "floor_col": (140, 96, 60)},
    "seeds": {"wall": (232, 226, 196), "dado": (110, 150, 70), "floor": "wood", "floor_col": (160, 120, 80)},
    "music_store": {"wall": (236, 226, 236), "dado": (120, 70, 150), "floor": "wood", "floor_col": (120, 80, 56)},
    "music_school": {"wall": (238, 230, 214), "dado": (60, 80, 150), "floor": "tile", "floor_col": (150, 120, 96)},
    "tower": {"wall": (120, 118, 140), "dado": (70, 130, 190), "floor": "stone", "floor_col": (90, 88, 110)},
    "fishing": {"wall": (214, 230, 236), "dado": (50, 110, 150), "floor": "wood", "floor_col": (150, 116, 80)},
}
SHELF_GOODS = {
    "bakery": ["food_bread", "food_baguette", "food_croissant", "food_muffin", "food_cake", "food_donut", "food_empanada", "food_tart"],
    "candy": ["food_candy", "food_lollipop", "food_chocolate", "food_gummies", "food_cookie", "food_ice_cream", "food_cotton_candy", "food_turron"],
    "store": ["item_rod", "item_book", "gear_hat", "gear_cloak", "gear_necklace", "gear_vest", "gear_pants", "gear_bracelet"],
    "weapons": ["item_sword", "item_sabre", "item_axe", "item_battleaxe", "item_bow", "item_longbow", "item_dagger", "item_crossbow"],
    "smithy": ["gear_helmet", "gear_chainmail", "gear_breastplate", "gear_greaves", "gear_bracers", "gear_iron_shield", "item_hatchet", "gear_wood_shield"],
    "pets": ["pet_dog", "pet_cat", "pet_rabbit", "pet_parrot", "pet_turtle", "pet_hamster", "food_fish", "food_cookie"],
    "seeds": ["seed_wheat", "seed_lettuce", "seed_tomato", "seed_carrot", "seed_strawberry", "seed_grape", "seed_olive", "crop_tomato"],
    "fishing": ["item_rod", "item_rod_bamboo", "item_rod_carbon", "item_net", "fish_salmon", "fish_tuna", "fish_trout", "bait_worm"],
    "tower": ["spell_spark", "spell_shield", "spell_lightning", "spell_ice", "spell_heal", "spell_quake", "spell_meteor", "ench_luck"],
    "music_store": ["item_guitar", "item_flute", "item_clarinet", "item_tambourine", "item_trumpet", "item_violin", "item_guitar", "item_flute"],
}


def icon_img(name):
    path = os.path.join(ART, "Icons", name + ".png")
    return Image.open(path).convert("RGBA") if os.path.exists(path) else Image.new("RGBA", (16, 16))


def room_ground(key):
    st = ROOM_STYLE[key]
    w, h = RW * T, RH * T
    img = Image.new("RGBA", (w, h), (20, 16, 14, 255))
    d = ImageDraw.Draw(img)
    # back wall (rows 0-2) with a tiled dado band, side walls, and the floor
    wall = va.stucco(w - 2 * T, 3 * T, st["wall"], hash(key) % 100)
    img.paste(Image.fromarray(wall).convert("RGBA"), (T, 0))
    for x in range(T, w - T, 8):
        d.rectangle([x, 3 * T - 14, x + 7, 3 * T - 7], fill=rgba(st["dado"]), outline=rgba(lighter(st["dado"], 0.3)))
        d.point((x + 3, 3 * T - 11), fill=rgba((246, 240, 220)))
    d.rectangle([T, 3 * T - 6, w - T - 1, 3 * T - 1], fill=rgba(darker(st["wall"], 0.7)))
    fl = st["floor_col"]
    for ty in range(3, RH - 1):
        for tx in range(1, RW - 1):
            X, Y = tx * T, ty * T
            kind = st["floor"]
            if kind == "tile":
                c = fl if (tx + ty) % 2 else darker(fl, 0.9)
                d.rectangle([X, Y, X + 15, Y + 15], fill=rgba(c), outline=rgba(darker(fl, 0.75)))
            elif kind == "check":
                c = fl if (tx + ty) % 2 else (200, 110, 150)
                d.rectangle([X, Y, X + 15, Y + 15], fill=rgba(c))
            elif kind == "stone":
                d.rectangle([X, Y, X + 15, Y + 15], fill=rgba(darker(fl, 0.9 + 0.1 * ((tx * 7 + ty * 3) % 3) / 2)), outline=rgba(darker(fl, 0.7)))
            else:  # wood planks
                d.rectangle([X, Y, X + 15, Y + 15], fill=rgba(fl))
                for k in (5, 11):
                    d.line([X, Y + k, X + 15, Y + k], fill=rgba(darker(fl, 0.8)))
                d.line([X + (ty * 5) % 16, Y, X + (ty * 5) % 16, Y + 4], fill=rgba(darker(fl, 0.8)))
    # shadow along the back wall, side walls, bottom wall with the doorway
    for k in range(5):
        d.line([T, 3 * T + k, w - T - 1, 3 * T + k], fill=(0, 0, 0, 70 - k * 14))
    d.rectangle([0, 0, T - 1, h], fill=rgba((52, 40, 34)))
    d.rectangle([w - T, 0, w, h], fill=rgba((52, 40, 34)))
    d.rectangle([0, h - T, w, h], fill=rgba((52, 40, 34)))
    d.rectangle([6 * T, h - T, 8 * T - 1, h], fill=rgba((150, 108, 70)))
    d.rectangle([6 * T + 2, h - T + 3, 8 * T - 3, h - 3], fill=rgba((176, 60, 50)))                 # doormat
    if key == "home":
        d.rectangle([4 * T, 5 * T, 10 * T - 1, 8 * T - 1], fill=rgba((150, 50, 50)))                # rug
        d.rectangle([4 * T + 3, 5 * T + 3, 10 * T - 4, 8 * T - 4], outline=rgba((230, 190, 90)))
    return img


def counter():
    img = Image.new("RGBA", (8 * T, 30), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 8 * T - 1, 9], fill=rgba((176, 130, 84)), outline=rgba((96, 66, 44)))        # top
    d.rectangle([0, 10, 8 * T - 1, 27], fill=rgba((130, 90, 58)), outline=rgba((86, 60, 40)))       # front
    for x in range(8, 8 * T, 16):
        d.rectangle([x, 13, x + 9, 24], outline=rgba((104, 72, 46)))
    d.line([1, 1, 8 * T - 2, 1], fill=rgba((206, 160, 110)))
    return img, (4 * T, 27)


def menu_board(key):
    img = Image.new("RGBA", (44, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.fontmode = "1"
    d.rectangle([0, 0, 43, 31], fill=rgba((120, 86, 56)))
    d.rectangle([2, 2, 41, 29], fill=rgba((40, 52, 46)))
    d.text((6, 0), "Menú", font=va.FONT, fill=rgba((240, 240, 230)))
    for y in (19, 23, 27):
        d.line([6, y, 37, y], fill=rgba((200, 210, 200)))
    return img, (22, 31)


def shelf(key):
    img = Image.new("RGBA", (4 * T, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 4 * T - 1, 39], fill=rgba((110, 76, 50)), outline=rgba((70, 48, 32)))
    for y in (3, 21):
        d.rectangle([2, y, 4 * T - 3, y + 15], fill=rgba((80, 56, 38)))
        d.line([2, y + 16, 4 * T - 3, y + 16], fill=rgba((150, 108, 70)))
    goods = SHELF_GOODS.get(key, [])
    for i, g in enumerate(goods):
        ic = icon_img(g).resize((14, 14), Image.NEAREST)
        img.alpha_composite(ic, (2 + (i % 4) * 15, 4 + (i // 4) * 18))
    return img, (2 * T, 39)


def oven():
    img = Image.new("RGBA", (36, 36), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([0, 0, 35, 30], fill=rgba((170, 80, 56)))
    d.rectangle([0, 15, 35, 35], fill=rgba((170, 80, 56)))
    for y in range(4, 34, 5):
        for x in range((y // 5) % 2 * 4, 36, 8):
            d.rectangle([x, y, x + 6, y + 3], outline=rgba((130, 56, 40)))
    d.ellipse([8, 14, 27, 30], fill=rgba((40, 20, 16)))
    d.rectangle([8, 22, 27, 30], fill=rgba((40, 20, 16)))
    d.ellipse([11, 22, 24, 30], fill=rgba((250, 150, 50)))
    d.ellipse([14, 24, 21, 30], fill=rgba((255, 220, 120)))
    return img, (18, 35)


def anvil():
    img = Image.new("RGBA", (26, 20), (0, 0, 0, 0))
    shadow_ellipse(img, 13, 18, 11, 2)
    d = ImageDraw.Draw(img)
    d.polygon([(0, 3), (22, 3), (26, 1), (22, 8), (4, 8)], fill=rgba((80, 80, 90)))
    d.line([1, 3, 22, 3], fill=rgba((150, 150, 164)))
    d.rectangle([9, 8, 16, 14], fill=rgba((66, 66, 74)))
    d.rectangle([5, 14, 20, 17], fill=rgba((110, 78, 52)))
    return img, (13, 17)


def forge():
    img = Image.new("RGBA", (36, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([2, 10, 33, 39], fill=rgba((110, 100, 96)), outline=rgba((70, 64, 62)))
    for y in range(12, 38, 6):
        for x in range(4 + (y // 6) % 2 * 4, 32, 8):
            d.rectangle([x, y, x + 6, y + 4], outline=rgba((86, 78, 74)))
    d.rectangle([8, 18, 27, 30], fill=rgba((40, 24, 18)))
    d.ellipse([10, 22, 25, 32], fill=rgba((250, 120, 40)))
    d.ellipse([13, 24, 22, 30], fill=rgba((255, 220, 110)))
    d.rectangle([12, 0, 23, 10], fill=rgba((96, 88, 84)))
    return img, (18, 39)


def bed():
    img = Image.new("RGBA", (34, 44), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 33, 8], fill=rgba((110, 76, 50)))
    d.rectangle([1, 6, 32, 41], fill=rgba((246, 240, 228)), outline=rgba((150, 140, 126)))
    d.rectangle([4, 9, 29, 16], fill=rgba((255, 255, 250)), outline=rgba((200, 196, 186)))
    d.rectangle([1, 18, 32, 41], fill=rgba((70, 110, 170)))
    for y in range(20, 40, 5):
        d.line([2, y, 31, y], fill=rgba((90, 130, 190)))
    d.rectangle([0, 40, 33, 43], fill=rgba((110, 76, 50)))
    return img, (17, 43)


def table():
    img = Image.new("RGBA", (34, 26), (0, 0, 0, 0))
    shadow_ellipse(img, 17, 23, 15, 3)
    d = ImageDraw.Draw(img)
    d.rectangle([1, 4, 32, 12], fill=rgba((150, 104, 66)), outline=rgba((92, 62, 40)))
    for x in (3, 28):
        d.rectangle([x, 12, x + 2, 22], fill=rgba((110, 76, 50)))
    d.ellipse([12, 0, 20, 6], fill=rgba((240, 236, 226)))
    d.ellipse([14, 1, 18, 4], fill=rgba((232, 128, 34)))
    return img, (17, 22)


def fireplace():
    img = Image.new("RGBA", (40, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 6, 39, 39], fill=rgba((176, 160, 140)), outline=rgba((110, 100, 90)))
    d.rectangle([0, 2, 39, 8], fill=rgba((150, 104, 66)))
    d.rectangle([8, 16, 31, 39], fill=rgba((40, 24, 18)))
    d.ellipse([12, 26, 27, 38], fill=rgba((250, 120, 40)))
    d.ellipse([15, 29, 24, 38], fill=rgba((255, 220, 110)))
    for x in (4, 34):
        d.rectangle([x, 0, x + 2, 2], fill=rgba((200, 60, 60)))
    return img, (20, 39)


def pen():
    img = Image.new("RGBA", (4 * T, 22), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for ry in (9, 14):
        d.rectangle([0, ry, 4 * T - 1, ry + 2], fill=rgba((168, 124, 82)))
    for px in range(1, 4 * T, 10):
        d.rectangle([px, 5, px + 2, 21], fill=rgba((120, 86, 56)))
    return img, (2 * T, 20)


# ---------------------------------------------------------------- world assembly
def make_world():
    grid = [["#" for _ in range(GW)] for _ in range(GH)]
    for y in range(VH):
        for x in range(VW):
            grid[y][x] = "."
    ground, water, dock_start = make_ground(grid)
    objects, saved = [], set()
    spawns = []
    portals = []
    areas = [{"key": "village", "es": "Valdeluz", "en": "Valdeluz", "x": 0, "y": 0, "w": VW, "h": VH, "interior": False, "shop": "", "keeper": ""}]
    grounds = [{"sprite": "village_ground", "x": 0, "y": 0, "w": PW, "h": PH}]

    def place(img_anchor, bx, by, footprint=None, letter=None, name=None, frames=None, fps=0):
        img, (ax, ay) = img_anchor
        if name not in saved:
            save_png(img, "World", "Objects", name + ".png")
            saved.add(name)
        o = {"sprite": name, "x": int(bx), "y": int(by), "ax": ax, "ay": ay, "w": img.width, "h": img.height, "letter": letter or ""}
        if frames:
            o["frames"], o["fps"] = frames, fps
        objects.append(o)
        if footprint and letter:
            fx0, fy0, fw, fh = footprint
            for ty in range(fy0, fy0 + fh):
                for tx in range(fx0, fx0 + fw):
                    if 0 <= tx < GW and 0 <= ty < GH:
                        grid[ty][tx] = letter

    def free(tx, ty):
        return 0 <= tx < VW and 0 <= ty < VH and grid[ty][tx] == "."

    # buildings
    doors = {}
    for spec in BUILDINGS:
        key, letter, door, x0, y0, wt, dt, kind, sign, ex = spec
        place(va.make_building(spec), x0 * T + wt * T // 2, (y0 + dt) * T, (x0, y0, wt, dt), letter, "building_" + key)
        dx = x0 + (wt // 2 if kind == "church" else 2)
        grid[y0 + dt - 1][dx] = door
        if kind == "church":
            grid[y0 + dt - 1][dx - 1] = door
        doors[key] = (dx, y0 + dt - 1)
    place(barn(), 7 * T, 34 * T, (3, 32, 7, 2), "v", "barn")
    tower_img = Image.open(os.path.join(ART, "World", "Objects", "tower.png")).convert("RGBA")
    place((tower_img, (40, 144)), 62 * T + 8, 38 * T, (61, 36, 3, 2), "þ", "tower")
    grid[37][62] = "Þ"
    doors["tower"] = (62, 37)
    citadel_img = Image.open(os.path.join(ART, "World", "Objects", "citadel.png")).convert("RGBA")
    place((citadel_img, (88, 136)), 100 * T + 8, 16 * T, (95, 13, 11, 3), "ð", "citadel")
    grid[15][100] = "Ð"
    for f in range(4):
        img, anc = windmill(f)
        save_png(img, "World", "Objects", f"windmill_{f}.png")
    objects.append({"sprite": "windmill_0", "frames": [f"windmill_{f}" for f in range(4)], "fps": 3, "x": 8 * T, "y": 10 * T,
                    "ax": 32, "ay": 90, "w": 64, "h": 96, "letter": "y"})
    for ty in (8, 9):
        for tx in (6, 7, 8, 9):
            grid[ty][tx] = "y"

    # fountain
    fx, fy, fw, fh = FOUNTAIN
    for f in range(3):
        img, anc = va.fountain(f)
        save_png(img, "World", "Objects", f"fountain_{f}.png")
    objects.append({"sprite": "fountain_0", "frames": ["fountain_0", "fountain_1", "fountain_2"], "fps": 4,
                    "x": fx * T + fw * T // 2, "y": (fy + fh) * T, "ax": 30, "ay": 47, "w": 60, "h": 56, "letter": "R"})
    for ty in range(fy, fy + fh):
        for tx in range(fx, fx + fw):
            grid[ty][tx] = "R"

    # archery range
    rx0, ry0, rx1, ry1 = RANGE
    for ty in range(ry0 + 1, ry1):
        for tx in range(rx0 + 1, rx1):
            sand = tones(np.clip(noise(16, 16, 3, tx * 31 + ty) * 1.2 - 0.1, 0, 1), [(196, 170, 120), (210, 186, 136), (222, 200, 152)])
            ground.paste(Image.fromarray(sand).convert("RGBA"), (tx * T, ty * T))
            grid[ty][tx] = ";"
    for tx in range(rx0, rx1 + 1):
        place(va.fence(False), tx * T + 8, ry0 * T + 16, (tx, ry0, 1, 1), "N", "fence_h")
        if tx not in (66, 67):
            place(va.fence(False), tx * T + 8, ry1 * T + 16, (tx, ry1, 1, 1), "N", "fence_h")
    for ty in range(ry0 + 1, ry1):
        for tx in (rx0, rx1):
            place(va.fence(True), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_v")
    for tx in range(rx0 + 2, rx1 - 1):
        place(va.target(), tx * T + 8, (ry0 + 1) * T + 15, (tx, ry0 + 1, 1, 1), "A", "target")

    # farm fence and chicken yard
    fx0, fy0, fx1, fy1 = FARM
    gate_img = (Image.open(os.path.join(ART, "World", "Objects", "gate_closed.png")).convert("RGBA"), (8, 20))
    for tx in range(fx0 - 1, fx1 + 2):
        for ty in (fy0 - 1, fy1 + 1):
            if (tx, ty) == (18, fy0 - 1):
                place(gate_img, tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "ç", "gate_closed")
            elif grid[ty][tx] == ".":
                place(va.fence(False), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_h")
    for ty in range(fy0, fy1 + 1):
        for tx in (fx0 - 1, fx1 + 1):
            place(va.fence(True), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_v")
    place(scarecrow(), 18 * T + 8, 33 * T + 14, None, "?", "scarecrow")
    yx0, yy0, yx1, yy1 = YARD
    for tx in range(yx0 - 1, yx1 + 2):
        for ty in (yy0 - 1, yy1 + 1):
            if (tx, ty) == (29, yy0 - 1):
                place(gate_img, tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "ç", "gate_closed")
            elif free(tx, ty):
                place(va.fence(False), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_h")
    for ty in range(yy0, yy1 + 1):
        for tx in (yx0 - 1, yx1 + 1):
            if free(tx, ty):
                place(va.fence(True), tx * T + 8, ty * T + 16, (tx, ty, 1, 1), "N", "fence_v")
    place(coop(), 30 * T, 32 * T + 14, (29, 32, 2, 1), "o", "coop")
    for i in range(4):
        spawns.append({"kind": "chicken", "x": 28 + i % 3, "y": 34 + i // 3})
    for tx, ty in ((4, 36), (7, 37), (26, 29)):
        if free(tx, ty):
            place(haystack(), tx * T + 8, ty * T + 14, (tx, ty, 1, 1), "|", "haystack")

    # props in and around the plaza
    def prop(maker, tx, ty, letter, name, w=1):
        if all(free(tx + k, ty) or grid[ty][tx + k] in ",:" for k in range(w)):
            place(maker(), tx * T + w * 8, ty * T + 14, (tx, ty, w, 1), letter, name)

    for tx, ty in ((36, 20), (56, 20), (36, 34), (56, 34), (74, 26), (77, 31), (33, 22)):
        prop(va.street_lamp, tx, ty, "f", "lamp")
    prop(notice_board, 40, 22, "n", "notice_board")
    prop(va.fruit_stall, 40, 31, "x", "stall", 2)
    prop(va.fruit_stall, 51, 31, "x", "stall", 2)
    prop(va.bench, 52, 24, "e", "bench", 2)
    prop(va.bench, 37, 24, "e", "bench", 2)
    prop(well, 20, 22, "l", "well")
    prop(signpost, 77, 30, "g", "signpost")
    prop(crates, 74, 29, "T", "crates")
    prop(va.barrels, 33, 16, "b", "barrels")
    prop(va.barrels, 65, 29, "b", "barrels")
    prop(va.cart, 3, 20, "c", "cart", 2)
    for b in BUILDINGS:
        key, letter, door, x0, y0, wt, dt, kind, sign, ex = b
        if ex.get("pots"):
            for tx in (x0, x0 + wt - 1):
                prop(va.flower_pot, tx, y0 + dt, "p", "pot")

    # vineyard (south-east)
    for ty in range(44, 57, 3):
        for tx in range(66, 77, 2):
            if free(tx, ty):
                place(vine(), tx * T + 8, ty * T + 14, (tx, ty, 1, 1), "{", "vine")

    # trees: orange trees in town, olives in the south-west grove, cypresses by the church,
    # a pine forest beyond the river, and a border all round
    kinds = {"orange": (va.orange_tree, "Z"), "olive": (va.olive_tree, "O"), "cypress": (va.cypress, "Y"), "pine": (pine, "}")}

    def tree(kind, tx, ty, variant=0):
        if not free(tx, ty):
            return False
        fn, letter = kinds[kind]
        place(fn(variant), tx * T + 8, ty * T + 14, (tx, ty, 1, 1), letter, f"{kind}_{variant}")
        return True

    r = np.random.default_rng(3)
    for tx, ty in ((31, 22), (25, 30), (32, 31), (63, 31), (70, 31), (75, 22), (44, 41), (60, 40), (3, 24), (24, 33)):
        tree("orange", tx, ty, (tx + ty) % 2)
    for tx, ty in ((41, 12), (51, 12), (59, 13)):
        tree("cypress", tx, ty)
    for _ in range(70):
        tree("olive", int(r.integers(4, 32)), int(r.integers(43, 66)), int(r.integers(0, 2)))
    for _ in range(40):
        tree("olive", int(r.integers(34, 50)), int(r.integers(44, 68)), int(r.integers(0, 2)))
    # forest: dense pines and cypresses, leaving the path and a few clearings open
    clearings = [(92, 14), (100, 30), (95, 45), (104, 58), (90, 62)]
    for ty in range(2, VH - 2):
        for tx in range(84, VW - 2):
            if any(math.hypot(tx - cx, ty - cy) < 4 for cx, cy in clearings):
                continue
            if 92 <= tx <= 108 and 5 <= ty <= 18:       # keep the citadel's grounds clear
                continue
            if r.random() < 0.34:
                tree("pine" if r.random() < 0.75 else "cypress", tx, ty, int(r.integers(0, 2)))
    for tx in range(0, VW, 2):
        tree("cypress" if r.random() < 0.3 else "olive", tx, 1, int(r.integers(0, 2)))
        tree("olive" if r.random() < 0.7 else "cypress", tx + 1, VH - 1, int(r.integers(0, 2)))
    for ty in range(3, VH - 1, 2):
        tree("olive" if r.random() < 0.6 else "cypress", 0, ty, int(r.integers(0, 2)))
        tree("pine", VW - 1, ty, int(r.integers(0, 2)))
    for ty in (0, VH - 1):
        for tx in range(VW):
            if grid[ty][tx] == ".":
                grid[ty][tx] = "O"
    for ty in range(VH):
        for tx in (0, 1, VW - 2, VW - 1):
            if grid[ty][tx] in ".,":
                grid[ty][tx] = "O" if tx < 2 else "}"
    for i, (tx, ty) in enumerate(((5, 22), (22, 33), (44, 44), (60, 44), (27, 21), (76, 33), (13, 41))):
        if free(tx, ty):
            place(va.bush("lavender" if i % 2 else "rose", i), tx * T + 8, ty * T + 14, (tx, ty, 1, 1), "u", "lavender" if i % 2 else "rosebush")

    # creatures: wolves in the forest clearings, wild dogs in the olive grove, cats and dogs in town
    for cx, cy in clearings:
        spawns.append({"kind": "wolf", "x": cx, "y": cy})
    for x, y in ((16, 58), (40, 64), (26, 50)):
        spawns.append({"kind": "wilddog", "x": x, "y": y})
    for kind, x, y in (("pet_cat", 42, 24), ("pet_dog", 52, 29), ("pet_cat", 21, 24), ("pet_dog", 63, 30), ("pet_cat", 48, 40)):
        spawns.append({"kind": kind, "x": x, "y": y})

    # people
    people = dict(PEOPLE)
    people["2"] = dock_start
    for ch, (tx, ty) in people.items():
        if grid[ty][tx] not in ".,:;=":
            # nudge to the nearest free tile
            for rr in range(1, 4):
                hit = [(tx + a, ty + b) for a in range(-rr, rr + 1) for b in range(-rr, rr + 1) if grid[ty + b][tx + a] in ".,:;"]
                if hit:
                    tx, ty = hit[0]
                    break
        grid[ty][tx] = ch

    # ---- rooms
    for key, es, en, shop, keeper, (ox, oy) in ROOMS:
        rimg = room_ground(key)
        save_png(rimg, "World", "Rooms", f"room_{key}.png")
        grounds.append({"sprite": "Rooms/room_" + key, "x": ox * T, "y": oy * T, "w": rimg.width, "h": rimg.height})
        for ty in range(RH):
            for tx in range(RW):
                inner = 1 <= tx <= RW - 2 and 3 <= ty <= RH - 2
                grid[oy + ty][ox + tx] = "_" if inner else "#"
        grid[oy + RH - 1][ox + 6] = grid[oy + RH - 1][ox + 7] = "X"
        areas.append({"key": key, "es": es, "en": en, "x": ox, "y": oy, "w": RW, "h": RH, "interior": True, "shop": shop, "keeper": keeper})

        def rplace(maker, tx, ty, letter, name, foot=None):
            place(maker, (ox + tx) * T + 8, (oy + ty) * T + 14, foot and (ox + foot[0], oy + foot[1], foot[2], foot[3]), letter, name)

        if shop:
            img, anc = counter()
            place((img, anc), (ox + 7) * T, (oy + 6) * T, (ox + 3, oy + 5, 8, 1), "K", "counter")
            img, anc = menu_board(key)
            place((img, anc), (ox + 10) * T + 8, (oy + 3) * T - 4, (ox + 10, oy + 2, 1, 1), "Q", "menu_board")
            img, anc = shelf(key)
            place((img, anc), (ox + 4) * T, (oy + 3) * T - 2, (ox + 2, oy + 2, 4, 1), "L", f"shelf_{key}")
            grid[oy + 4][ox + 7] = keeper
        if key == "bakery":
            rplace(oven(), 1, 3, "E", "oven", (1, 3, 1, 1))
        if key == "smithy":
            rplace(forge(), 1, 3, "E", "forge", (1, 3, 1, 1))
            rplace(anvil(), 3, 7, "U", "anvil", (3, 7, 1, 1))
        if key in ("store", "weapons", "candy", "bakery", "fishing"):
            rplace(va.barrels(), 11, 7, "b", "barrels", (11, 7, 1, 1))
            rplace(crates(), 2, 7, "T", "crates", (2, 7, 1, 1))
        if key == "pets":
            rplace(pen(), 3, 7, "N", "pen", (2, 7, 4, 1))
            for i, kind in enumerate(("pet_cat", "pet_rabbit", "pet_dog", "pet_hamster")):
                spawns.append({"kind": kind, "x": ox + 3 + i * 2, "y": oy + 6 + (i % 2), "room": True})
        if key == "tower":
            img = Image.open(os.path.join(ART, "World", "Objects", "altar.png")).convert("RGBA")
            place((img, (24, 36)), (ox + 7) * T, (oy + 7) * T, (ox + 6, oy + 6, 3, 1), "ø", "altar")
            grid[oy + 4][ox + 7] = keeper
            img, anc = shelf("tower")
            place((img, anc), (ox + 4) * T, (oy + 3) * T - 2, (ox + 2, oy + 2, 4, 1), "L", "shelf_tower")
        if key == "music_school":
            img = Image.open(os.path.join(ART, "World", "Objects", "piano.png")).convert("RGBA")
            place((img, (24, 35)), (ox + 4) * T + 8, (oy + 4) * T, (ox + 3, oy + 3, 3, 1), "ñ", "piano")
            grid[oy + 5][ox + 8] = keeper
            img, anc = shelf("music_store")
            place((img, anc), (ox + 11) * T, (oy + 3) * T - 2, (ox + 9, oy + 2, 4, 1), "L", "shelf_music_store")
        if key == "home":
            img, anc = bed()
            place((img, anc), (ox + 2) * T + 16, (oy + 5) * T, (ox + 2, oy + 3, 2, 2), "J", "bed")
            img, anc = fireplace()
            place((img, anc), (ox + 7) * T, (oy + 3) * T - 2, (ox + 6, oy + 2, 2, 1), "G", "fireplace")
            rplace(table(), 9, 6, "t", "table", (9, 6, 2, 1))
            img, anc = shelf("store")
            place((img, anc), (ox + 11) * T + 8, (oy + 3) * T - 2, (ox + 10, oy + 2, 3, 1), "L", "shelf_store")
        rplace(va.flower_pot(), 12, 3, "p", "pot", (12, 3, 1, 1))

        # doors in and out
        outside = key
        dx, dy = doors[outside]
        portals.append({"x": dx, "y": dy, "tx": ox + 6, "ty": oy + RH - 2, "face": 2})
        for ex_x in (6, 7):
            portals.append({"x": ox + ex_x, "y": oy + RH - 1, "tx": dx, "ty": dy + 1, "face": 0})

    gd = ImageDraw.Draw(ground)
    for ty in range(21, 24):
        for tx in range(12, 18):
            X, Y = tx * T, ty * T
            gd.rectangle([X + 1, Y + 1, X + 14, Y + 14], fill=(104, 74, 50), outline=(82, 58, 40))
            for k in (4, 8, 12):
                gd.line([X + 2, Y + k, X + 13, Y + k], fill=(88, 62, 42))
            grid[ty][tx] = "+"
    save_png(ground, "World", "village_ground.png")
    wx, wy, ww, wh = va.make_water_frames(water, None)
    overlays = [{"frames": ["water_0", "water_1", "water_2"], "fps": 2, "x": wx, "y": wy, "w": ww, "h": wh}]

    maps = os.path.join(ROOT, "Assets", "Resources", "Maps")
    with open(os.path.join(maps, "village.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join("".join(row) for row in grid))
    data = {"objects": objects, "overlays": overlays, "grounds": grounds, "areas": areas, "portals": portals,
            "spawns": spawns, "walkable": WALKABLE,
            "words": [{"c": c, "es": es, "en": en} for c, (es, en) in WORDS.items()]}
    with open(os.path.join(maps, "village_objects.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False)
    make_minimap(ground, grid)
    return grid, objects


def make_minimap(ground, grid):
    mm = ground.resize((VW * 4, VH * 4), Image.BOX)
    d = ImageDraw.Draw(mm)
    for ty in range(VH):
        for tx in range(VW):
            c = grid[ty][tx]
            X, Y = tx * 4, ty * 4
            if c in "kqswjmhdarv[(`BCSWMHD^<>])'":
                d.rectangle([X, Y - 4, X + 3, Y + 3], fill=(176, 82, 54))
            elif c in "ðÐþÞ":
                d.rectangle([X, Y - 4, X + 3, Y + 3], fill=(150, 144, 130))
            elif False:
                d.rectangle([X, Y - 4, X + 3, Y + 3], fill=(176, 82, 54))
            elif c in "iIy":
                d.rectangle([X, Y - 4, X + 3, Y + 3], fill=(206, 190, 150))
            elif c in "ZOY}":
                d.ellipse([X, Y, X + 3, Y + 3], fill=(40, 72, 42))
            elif c == "N":
                d.rectangle([X + 1, Y + 1, X + 2, Y + 2], fill=(120, 86, 56))
    save_png(mm, "UI", "minimap.png")


if __name__ == "__main__":
    g, objs = make_world()
    print(len(objs), "objects")
    print("\n".join("".join(r[:112]) for r in g[::2]))
