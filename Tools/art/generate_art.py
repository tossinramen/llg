"""Generates all placeholder pixel art for the game.

Run:  python Tools/art/generate_art.py
Output goes to Assets/Resources/Art/. Unity re-imports it automatically.

Character layers are drawn in greyscale so Unity can tint them
(skin / hair / shirt / pants colours are chosen in the character creator).
"""
import math
import os
import random

from PIL import Image

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
ART = os.path.join(ROOT, "Assets", "Resources", "Art")
W, H = 16, 24                     # character sprite size
INK = (40, 30, 50, 255)           # dark outline colour


# ---------------------------------------------------------------- helpers
def grid():
    return [[0] * W for _ in range(H)]


def rect(g, x0, y0, x1, y1, v=255):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if 0 <= x < W and 0 <= y < H:
                g[y][x] = v


def clear(g, *pts):
    for x, y in pts:
        g[y][x] = 0


def shade(g, v=190):
    """Darken pixels whose right or lower neighbour is empty (light from top-left)."""
    src = [r[:] for r in g]
    for y in range(H):
        for x in range(W):
            if not src[y][x]:
                continue
            right = src[y][x + 1] if x + 1 < W else 0
            below = src[y + 1][x] if y + 1 < H else 0
            if not right or not below:
                g[y][x] = v


def dilate(mask):
    out = [r[:] for r in mask]
    for y in range(H):
        for x in range(W):
            if mask[y][x]:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < W and 0 <= ny < H:
                        out[ny][nx] = 1
    return out


def save_gray(g, name):
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    for y in range(H):
        for x in range(W):
            v = g[y][x]
            if v:
                img.putpixel((x, y), (v, v, v, 255))
    save(img, "Character", name)


def save(img, folder, name):
    d = os.path.join(ART, folder)
    os.makedirs(d, exist_ok=True)
    img.save(os.path.join(d, name + ".png"))


# ---------------------------------------------------------------- character
def head(g):
    rect(g, 3, 2, 12, 11)
    clear(g, (3, 2), (12, 2), (3, 11), (12, 11))


def body_layers(direction, frame=0):
    skin, shirt, pants = grid(), grid(), grid()
    detail = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    shoe, shoe_dark = (96, 64, 48, 255), (64, 40, 32, 255)

    def shoes(x0, x1, lift=0):
        for x in range(x0, x1 + 1):
            detail.putpixel((x, 22 - lift), shoe)
            detail.putpixel((x, 23 - lift), shoe_dark)

    head(skin)
    if direction in ("down", "up"):
        rect(shirt, 5, 12, 10, 17)
        rect(shirt, 3, 12, 4, 14)
        rect(shirt, 11, 12, 12, 14)
        rect(pants, 5, 18, 10, 18)
        la, lb = (1 if frame == 1 else 0), (1 if frame == 2 else 0)   # which foot is lifted
        rect(pants, 5, 19, 6, 21 - la)
        rect(pants, 9, 19, 10, 21 - lb)
        rect(skin, 3, 15 - lb, 4, 16 - lb)
        rect(skin, 11, 15 - la, 12, 16 - la)
        shoes(4, 6, la)
        shoes(9, 11, lb)
    else:  # facing right
        rect(shirt, 6, 12, 10, 17)
        rect(pants, 6, 18, 10, 18)
        if frame == 1:   # stride
            rect(pants, 5, 19, 6, 21)
            rect(pants, 9, 19, 10, 21)
            rect(skin, 9, 15, 10, 16)
            shoes(5, 7)
            shoes(9, 11)
        else:
            rect(pants, 6, 19, 9, 21)
            rect(skin, 7, 15, 8, 16)
            shoes(6, 10)

    for g in (skin, shirt, pants):
        shade(g)

    if direction in ("down", "up"):
        for y in (12, 13, 14):       # seam between arms and torso
            shirt[y][4] = 190
            shirt[y][11] = 190
    else:
        for y in (12, 13, 14):       # arm hint on the side view
            shirt[y][6] = 190
            shirt[y][9] = 190

    if direction == "down":
        for x in (6, 9):
            detail.putpixel((x, 7), INK)
            detail.putpixel((x, 8), INK)
        skin[10][7] = skin[10][8] = 150   # mouth
    elif direction == "right":
        detail.putpixel((10, 7), INK)
        detail.putpixel((10, 8), INK)

    # dark silhouette drawn behind everything
    mask = [[1 if (skin[y][x] or shirt[y][x] or pants[y][x] or detail.getpixel((x, y))[3]) else 0
             for x in range(W)] for y in range(H)]
    mask = dilate(mask)
    outline = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    for y in range(H):
        for x in range(W):
            if mask[y][x]:
                outline.putpixel((x, y), INK)
    return skin, shirt, pants, detail, outline


def hair(style, direction):
    g = grid()

    def cap():
        rect(g, 3, 1, 12, 4)
        clear(g, (3, 1), (12, 1))

    def spikes():
        for x in (4, 7, 8, 11):
            g[0][x] = 255

    if style == 4:  # buzz cut
        rect(g, 4, 1, 11, 1)
        if direction == "down":
            rect(g, 3, 2, 12, 3)
        elif direction == "up":
            rect(g, 3, 2, 12, 5)
        else:
            rect(g, 3, 2, 12, 3)
            rect(g, 3, 4, 5, 6)
    elif direction == "down":
        cap()
        rect(g, 3, 5, 5, 5)
        rect(g, 10, 5, 12, 5)
        g[6][3] = g[6][12] = 255
        if style == 1:
            rect(g, 2, 3, 3, 14)
            rect(g, 12, 3, 13, 14)
        if style == 2:
            spikes()
        if style == 3:
            rect(g, 1, 4, 2, 7)
            rect(g, 13, 4, 14, 7)
    elif direction == "up":
        cap()
        rect(g, 3, 5, 12, 8)
        if style == 1:
            rect(g, 2, 3, 13, 14)
        if style == 2:
            spikes()
        if style == 3:
            rect(g, 1, 4, 2, 7)
            rect(g, 13, 4, 14, 7)
    else:  # right
        cap()
        rect(g, 3, 5, 6, 8)
        rect(g, 11, 5, 12, 5)
        if style == 1:
            rect(g, 2, 3, 5, 14)
        if style == 2:
            spikes()
        if style == 3:
            rect(g, 1, 4, 2, 8)

    shade(g, 200)
    solid = [[1 if g[y][x] else 0 for x in range(W)] for y in range(H)]
    ring = dilate(solid)
    for y in range(H):
        for x in range(W):
            if ring[y][x] and not solid[y][x]:
                g[y][x] = 70          # baked dark rim, tints to a dark hair colour
    return g


def make_character():
    for d in ("down", "right", "up"):
        skin, shirt, pants, detail, outline = body_layers(d)
        save_gray(skin, f"skin_{d}")
        save_gray(shirt, f"shirt_{d}")
        save_gray(pants, f"pants_{d}")
        save(detail, "Character", f"detail_{d}")
        save(outline, "Character", f"outline_{d}")
        for s in range(5):
            save_gray(hair(s, d), f"hair{s}_{d}")
        for frame in (1, 2):   # walking frames: only legs, hands, shoes and silhouette change
            skin, shirt, pants, detail, outline = body_layers(d, frame)
            save_gray(skin, f"skin_{d}_w{frame}")
            save_gray(pants, f"pants_{d}_w{frame}")
            save(detail, "Character", f"detail_{d}_w{frame}")
            save(outline, "Character", f"outline_{d}_w{frame}")


# ---------------------------------------------------------------- background
def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))


def make_background():
    w, h = 320, 180
    img = Image.new("RGBA", (w, h))
    px = img.load()
    stops = [(0.0, (70, 140, 244)), (0.55, (110, 204, 250)), (1.0, (196, 244, 236))]
    bands = 14
    for y in range(h):
        t = min(1.0, y / 112.0)
        t = math.floor(t * bands) / bands          # banded gradient = 16-bit look
        for i in range(len(stops) - 1):
            if stops[i][0] <= t <= stops[i + 1][0]:
                u = (t - stops[i][0]) / (stops[i + 1][0] - stops[i][0])
                c = lerp(stops[i][1], stops[i + 1][1], u)
        for x in range(w):
            px[x, y] = c + (255,)

    # rainbow
    colours = [(255, 96, 96), (255, 168, 72), (255, 236, 96), (112, 220, 112), (96, 176, 255), (150, 120, 240)]
    rcx, rcy = 118, 150
    for y in range(h):
        for x in range(w):
            d = math.hypot(x - rcx, (y - rcy) * 1.15)
            band = int((132 - d) // 3)
            if 0 <= band < len(colours):
                px[x, y] = lerp(px[x, y][:3], colours[band], 0.62) + (255,)

    # sun with rays
    sx, sy = 262, 40
    for y in range(sy - 30, sy + 31):
        for x in range(sx - 30, sx + 31):
            if not (0 <= x < w and 0 <= y < h):
                continue
            d = math.hypot(x - sx, y - sy)
            ang = math.atan2(y - sy, x - sx)
            if d <= 13:
                px[x, y] = (255, 238, 120, 255) if d > 10 else (255, 250, 190, 255)
            elif d <= 26 and int((ang + math.pi) / (math.pi / 8)) % 2 == 0 and d > 16:
                px[x, y] = lerp(px[x, y][:3], (255, 244, 170), 0.7) + (255,)

    def layer(fn, colour, light):
        for x in range(w):
            y0 = int(fn(x))
            for y in range(max(0, y0), h):
                px[x, y] = (light if y <= y0 + 1 else colour) + (255,)

    mountain = lambda x: 98 - 30 * abs(math.sin(x / 47.0 + 0.6)) - 6 * math.sin(x / 13.0)
    layer(mountain, (170, 150, 232), (204, 188, 248))
    for x in range(w):                               # snow caps
        y0 = int(mountain(x))
        if y0 < 76:
            for y in range(y0, min(y0 + (76 - y0) // 2 + 1, h)):
                px[x, y] = (250, 250, 255, 255)
    hill1 = lambda x: 114 + 9 * math.sin(x / 38.0 + 1.0)
    hill2 = lambda x: 136 + 8 * math.sin(x / 55.0 + 3.0)
    layer(hill1, (124, 214, 120), (176, 240, 140))
    layer(hill2, (84, 188, 104), (132, 220, 124))

    # winding river on the middle hills
    for y in range(120, 162):
        t = (y - 120) / 42.0
        cx = 190 + 34 * math.sin(t * 4.2) - 20 * t
        half = 2 + 9 * t
        for x in range(int(cx - half), int(cx + half) + 1):
            if 0 <= x < w and y >= hill1(x) + 2:
                edge = abs(x - cx) > half - 1.5
                px[x, y] = (150, 216, 250, 255) if edge else (84, 164, 240, 255)

    # little cottages
    for hx, roof in ((92, (236, 88, 88)), (118, (250, 176, 64)), (140, (120, 110, 230)), (232, (240, 110, 170))):
        hy = int(hill1(hx)) + 6
        for y in range(hy, hy + 7):
            for x in range(hx, hx + 11):
                px[x, y] = (250, 238, 210, 255)
        for i in range(6):
            for x in range(hx - 1 + i, hx + 12 - i):
                px[x, hy - 1 - i] = roof + (255,)
        for y in range(hy + 3, hy + 7):
            px[hx + 5, y] = px[hx + 4, y] = (120, 76, 48, 255)
        px[hx + 8, hy + 2] = px[hx + 8, hy + 3] = (110, 190, 240, 255)

    layer(lambda x: 158 + 3 * math.sin(x / 21.0), (60, 160, 84), (104, 200, 104))

    # blossom tree on the left
    for y in range(118, 152):
        for x in range(38, 45):
            px[x, y] = (128, 88, 60, 255) if x < 42 else (96, 64, 48, 255)
    rnd = random.Random(3)
    for cx, cy, r in ((41, 102, 18), (27, 113, 13), (56, 112, 13), (41, 89, 12)):
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if math.hypot(x - cx, y - cy) <= r:
                    lit = (x - cx) + (y - cy) < -r * 0.4
                    c = (255, 184, 214) if lit else (240, 130, 180)
                    if rnd.random() < 0.08:
                        c = (255, 240, 246)
                    px[x, y] = c + (255,)

    # flowers
    rnd = random.Random(7)
    petals = [(255, 232, 96), (255, 140, 170), (255, 255, 255), (190, 150, 255), (255, 120, 90), (120, 200, 255)]
    for _ in range(90):
        x, y = rnd.randrange(3, w - 3), rnd.randrange(160, h - 3)
        if rnd.random() < 0.6:
            c = rnd.choice(petals)
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                px[x + dx, y + dy] = c + (255,)
            px[x, y] = (255, 210, 60, 255)
            px[x, y + 2] = (36, 120, 64, 255)
        else:
            px[x, y] = px[x + 1, y - 1] = px[x - 1, y - 1] = (36, 120, 64, 255)
    save(img, "UI", "title_background")


def make_cloud():
    w, h = 48, 16
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for cx, cy, r in ((12, 10, 6), (22, 7, 7), (33, 9, 6), (40, 11, 4), (6, 12, 3)):
        for y in range(h):
            for x in range(w):
                if math.hypot(x - cx, y - cy) <= r and y <= 13:
                    img.putpixel((x, y), (228, 238, 252, 255) if y >= 11 else (255, 255, 255, 255))
    save(img, "UI", "cloud")


# ---------------------------------------------------------------- icons
def icon(name, rows, palette, folder="Icons"):
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pad = (16 - len(rows)) // 2
    for y, row in enumerate(rows):
        assert len(row) <= 16, name
        for x, ch in enumerate(row):
            if ch != ".":
                img.putpixel((x, y + pad), palette[ch] + (255,))
    save(img, folder, name)


O = INK[:3]


def make_icons():
    icon("food_apple", [
        "........b.......",
        ".......b.gg.....",
        ".......b.g......",
        "...ooo.booo.....",
        "..orrrorrrro....",
        ".orwrrrrrrrRo...",
        ".orwrrrrrrrRo...",
        ".orrrrrrrrrRo...",
        ".orrrrrrrrrRo...",
        ".orrrrrrrrRRo...",
        "..orrrrrrRRo....",
        "...orRRRRRo.....",
        "....oooooo......",
    ], {"o": O, "r": (224, 60, 60), "R": (168, 36, 52), "w": (255, 200, 190),
        "g": (88, 176, 80), "b": (110, 76, 52)})
    icon("food_bread", [
        "....oooooooo....",
        "..oobbbbbbbboo..",
        ".obbBbbBbbBbbbo.",
        ".obBbbBbbBbbbbo.",
        ".obbbbbbbbbbbbo.",
        ".obbbbbbbbbbddo.",
        ".obbbbbbbbbdddo.",
        ".oddddddddddddo.",
        "..oooooooooooo..",
    ], {"o": O, "b": (232, 180, 110), "B": (176, 112, 60), "d": (204, 148, 84)})
    icon("food_cheese", [
        "..........oo....",
        ".......oooyyo...",
        "....oooyyyyyo...",
        ".oooyyyyyyyyo...",
        ".oyyyyyyyyyyo...",
        ".oooooooooooo...",
        ".oyyyhyyyyyyo...",
        ".oyyyyyyhhyyo...",
        ".oyhyyyyhhyyo...",
        ".oyyyyyyyyyyo...",
        ".oooooooooooo...",
    ], {"o": O, "y": (250, 208, 80), "h": (214, 160, 48)})
    icon("food_fish", [
        ".....oooooo.....",
        "o..oobbbbbboo...",
        "oo.obbbbbbbbbo..",
        "obobbbbbbbbkbbo.",
        "obbbbbbbbbbbbbo.",
        "obobbbbbbbbbbbo.",
        "oo.obbwwwwbbbo..",
        "o..oowwwwwwoo...",
        ".....oooooo.....",
    ], {"o": O, "b": (96, 160, 224), "w": (230, 240, 250), "k": (20, 20, 30)})
    icon("food_egg", [
        "......oooo......",
        ".....owwwwo.....",
        "....owwwwwwo....",
        "...owwwwwwwwo...",
        "...owwwwwwwwo...",
        "..owwwwwwwwwwo..",
        "..owwwwwwwwwso..",
        "..owwwwwwwwsso..",
        "..owwwwwwwwsso..",
        "...owwwwwssso...",
        "...oswwsssso....",
        "....oossssoo....",
        "......oooo......",
    ], {"o": O, "w": (255, 248, 232), "s": (226, 208, 184)})
    icon("hp_heart", [
        "..ooo....ooo....",
        ".orrro..orrro...",
        "orwrrroorrrrro..",
        "orwrrrrrrrrrro..",
        "orrrrrrrrrrRro..",
        "orrrrrrrrrrRro..",
        ".orrrrrrrrRro...",
        "..orrrrrrRro....",
        "...orrrrRro.....",
        "....orrRro......",
        ".....orro.......",
        "......oo........",
    ], {"o": O, "r": (232, 64, 80), "R": (176, 40, 64), "w": (255, 190, 200)}, "UI")
    icon("mana_drop", [
        "......oo........",
        "......oo........",
        ".....obbo.......",
        ".....obbo.......",
        "....obbbbo......",
        "....obbbbo......",
        "...obbbbbbo.....",
        "..obwbbbbbBo....",
        "..obwbbbbbBo....",
        "..obwbbbbbBo....",
        "..obbwbbbBBo....",
        "...obbBBBBo.....",
        "....oooooo......",
    ], {"o": O, "b": (80, 150, 240), "B": (48, 100, 200), "w": (200, 230, 255)}, "UI")
    icon("coin", [
        "....oooooo......",
        "...oyyyyyyo.....",
        "..oyyhhhhyyo....",
        ".oyyhyyyyyydo...",
        ".oyhyyddyyydo...",
        ".oyhyydyyyydo...",
        ".oyhyyddyyydo...",
        ".oyhyyydyyydo...",
        ".oyhyyddyyydo...",
        ".oyyyyyyyyydo...",
        "..oyyyyyyddo....",
        "...oddddddo.....",
        "....oooooo......",
    ], {"o": O, "y": (250, 210, 72), "h": (255, 244, 170), "d": (200, 140, 40)}, "UI")


def make_gender_icons():
    def canvas():
        return Image.new("RGBA", (16, 16), (0, 0, 0, 0))

    def ring(img, cx, cy, r):
        for y in range(16):
            for x in range(16):
                if abs(math.hypot(x - cx, y - cy) - r) < 0.75:
                    img.putpixel((x, y), (255, 255, 255, 255))

    def dots(img, pts):
        for x, y in pts:
            img.putpixel((x, y), (255, 255, 255, 255))

    m = canvas()
    ring(m, 6, 10, 4)
    dots(m, [(9, 7), (10, 6), (11, 5), (12, 4), (13, 3)])
    dots(m, [(x, 3) for x in range(9, 14)] + [(13, y) for y in range(3, 8)])
    save(m, "UI", "gender_male")

    f = canvas()
    ring(f, 8, 5, 4)
    dots(f, [(8, y) for y in range(10, 16)] + [(x, 12) for x in range(5, 12)])
    save(f, "UI", "gender_female")

    n = canvas()
    ring(n, 8, 11, 3.5)
    dots(n, [(8, y) for y in range(1, 8)])
    dots(n, [(6, 2), (7, 3), (9, 5), (10, 6), (10, 2), (9, 3), (7, 5), (6, 6)])
    save(n, "UI", "gender_nonbinary")


if __name__ == "__main__":
    # characters are drawn by generate_characters.py, the village by generate_village.py
    make_background()
    make_cloud()
    make_icons()
    make_gender_icons()
    print("Art written to", ART)
