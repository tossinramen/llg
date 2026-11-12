"""Draws the characters (player and villagers) as shaded, layered pixel art.

Run:  python Tools/art/generate_characters.py
Output: Assets/Resources/Art/Character/*.png  (24 x 32 pixels each)

Every body part is drawn as a simple shape, then a lighting pass shades it like a rounded
object lit from the top-left. That gives the "fake 3D" look instead of flat colour.
Layers are greyscale so the game can tint them (skin, hair, shirt, pants, accessories).

Layers per direction (down / right / up) and walk frame ("", _w1, _w2):
  outline  pants  shirt  skin  detail      body (detail = eyes, shoes, belt: already coloured)
  hair0..5                                 hair styles
  acc_hat acc_cloak acc_apron acc_beard acc_skirt acc_shawl   accessories (villagers, equipment)
"""
import os

import numpy as np
from PIL import Image, ImageDraw
from scipy.ndimage import binary_dilation, distance_transform_edt

from generate_art import ART

W, H = 24, 32
OUTLINE = (52, 34, 40)
LIGHT = np.array([-0.55, -0.83])      # light comes from the top-left (x, y with y pointing down)
FRAMES = ("", "_w1", "_w2")
DIRS = ("down", "right", "up")


# ---------------------------------------------------------------- drawing helpers
class Mask:
    """A shape on the 24x32 canvas, built from rectangles, ellipses and polygons."""

    def __init__(self):
        self.img = Image.new("L", (W, H), 0)
        self.d = ImageDraw.Draw(self.img)

    def rect(self, x0, y0, x1, y1):
        self.d.rectangle([x0, y0, x1, y1], fill=255)
        return self

    def ellipse(self, x0, y0, x1, y1):
        self.d.ellipse([x0, y0, x1, y1], fill=255)
        return self

    def poly(self, *pts):
        self.d.polygon(list(pts), fill=255)
        return self

    def cut(self, other):
        a = self.array & ~other.array
        self.img = Image.fromarray((a * 255).astype(np.uint8))
        self.d = ImageDraw.Draw(self.img)
        return self

    def keep(self, fn):
        """Keep only pixels where fn(x, y) is true."""
        a = self.array.copy()
        for y in range(H):
            for x in range(W):
                if a[y, x] and not fn(x, y):
                    a[y, x] = False
        self.img = Image.fromarray((a * 255).astype(np.uint8))
        self.d = ImageDraw.Draw(self.img)
        return self

    @property
    def array(self):
        return np.array(self.img) > 0


def union(*masks):
    out = np.zeros((H, W), bool)
    for m in masks:
        out |= m.array if isinstance(m, Mask) else m
    return out


def lit(mask, base=0.84, depth=0.32, soft=2.2, fade=0.10, levels=(0.56, 0.70, 0.84, 0.96)):
    """Shades a shape as if it were rounded and lit from the top-left, in 4 pixel-art tones."""
    if isinstance(mask, Mask):
        mask = mask.array
    if not mask.any():
        return np.zeros((H, W))
    dist = distance_transform_edt(np.pad(mask, 1))[1:-1, 1:-1]
    gy, gx = np.gradient(dist)
    nx, ny = -gx, -gy                                    # outward-facing direction
    norm = np.hypot(nx, ny) + 1e-6
    facing = (nx * LIGHT[0] + ny * LIGHT[1]) / norm      # +1 = faces the light
    weight = np.exp(-(dist - 1) / soft)
    ys = np.nonzero(mask)[0]
    top, bottom = ys.min(), ys.max()
    rows = np.arange(H)[:, None]
    ground = (rows - top) / max(1, bottom - top)         # a little darker lower down
    value = base + depth * facing * weight - fade * ground
    out = np.zeros((H, W))
    lv = np.array(levels)
    idx = np.abs(value[..., None] - lv).argmin(-1)
    out[mask] = lv[idx][mask]
    return out


def to_gray_image(values, extra=None):
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    px = img.load()
    for y in range(H):
        for x in range(W):
            v = values[y, x]
            if v > 0:
                g = int(round(v * 255))
                px[x, y] = (g, g, g, 255)
    if extra:
        for (x, y), c in extra.items():
            if 0 <= x < W and 0 <= y < H:
                px[x, y] = c
    return img


def outline_of(mask):
    ring = binary_dilation(mask, structure=np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])) & ~mask
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    px = img.load()
    for y, x in zip(*np.nonzero(ring | mask)):
        px[x, y] = OUTLINE + (255,)
    return img


def with_rim(values, mask, rim=0.42):
    """Adds a dark tinted rim around a separate piece (hair, hats) so it reads against the body."""
    ring = binary_dilation(mask, structure=np.array([[0, 1, 0], [1, 1, 1], [0, 1, 0]])) & ~mask
    out = values.copy()
    out[ring] = rim
    return out


def save_layer(img, name):
    d = os.path.join(ART, "Character")
    os.makedirs(d, exist_ok=True)
    img.save(os.path.join(d, name + ".png"))


# ---------------------------------------------------------------- body shapes
def body_parts(direction, frame):
    """Returns masks for skin, shirt, pants and positions for detail pixels."""
    skin, shirt, pants = Mask(), Mask(), Mask()
    legs, shoes = [], []
    step = {"": 0, "_w1": 1, "_w2": 2}[frame]

    if direction in ("down", "up"):
        skin.ellipse(7, 3, 16, 13)                         # head
        skin.rect(10, 13, 13, 14)                          # neck
        if direction == "down":
            skin.rect(6, 7, 6, 9).rect(17, 7, 17, 9)       # ears
        # arms swing opposite to the legs
        l_off, r_off = {0: (0, 0), 1: (0, -1), 2: (-1, 0)}[step]
        shirt.poly((7, 14), (16, 14), (17, 16), (16, 22), (7, 22), (6, 16))
        shirt.rect(4, 15 + l_off, 6, 18 + l_off).rect(17, 15 + r_off, 19, 18 + r_off)
        skin.rect(4, 19 + l_off, 6, 21 + l_off).rect(17, 19 + r_off, 19, 21 + r_off)
        pants.rect(7, 22, 16, 24)
        lift_l, lift_r = (2, 0) if step == 1 else (0, 2) if step == 2 else (0, 0)
        legs = [Mask().rect(7, 25, 11, 28 - lift_l), Mask().rect(12, 25, 16, 28 - lift_r)]
        shoes = [(6, 11, 29 - lift_l), (12, 17, 29 - lift_r)]
    else:  # facing right
        skin.ellipse(8, 3, 17, 13)
        skin.rect(18, 8, 18, 10)                           # nose
        skin.rect(11, 13, 14, 14)
        shirt.poly((9, 14), (15, 14), (16, 16), (15, 22), (9, 22))
        if step == 1:                                      # stride
            legs = [Mask().poly((12, 25), (15, 25), (17, 28), (14, 28)),
                    Mask().poly((9, 25), (12, 25), (10, 28), (7, 28))]
            shoes = [(14, 18, 29), (6, 10, 29)]
            arm = (10, 15, 12, 18, 19)
        else:
            legs = [Mask().rect(10, 25, 14, 28 - (1 if step == 2 else 0))]
            shoes = [(10, 16, 29 - (1 if step == 2 else 0))]
            arm = (11, 15, 13, 18, 19)
        x0, y0, x1, y1, hand = arm
        shirt.rect(x0, y0, x1, y1)
        skin.rect(x0, hand, x1, hand + 2)
        pants.rect(9, 22, 15, 24)
    for leg in legs:
        pants.img.paste(255, mask=leg.img)
    return skin, shirt, pants, legs, shoes


def body_layers(direction, frame):
    skin, shirt, pants, legs, shoes = body_parts(direction, frame)

    head = Mask().ellipse(7, 3, 16, 13) if direction != "right" else Mask().ellipse(8, 3, 17, 13)
    skin_v = lit(skin, base=0.9)
    skin_v = np.where(head.array & skin.array, lit(head, base=0.9), skin_v)   # the head is shaded as a ball
    shirt_v = lit(shirt, base=0.86)
    # each leg is shaded on its own so a crease shows between them
    pants_v = np.zeros((H, W))
    hips = Mask().rect(7, 22, 16, 24) if direction != "right" else Mask().rect(9, 22, 15, 24)
    pants_v = np.maximum(pants_v, lit(hips, base=0.86))
    for leg in legs:
        pants_v = np.where(leg.array, lit(leg, base=0.82), pants_v)

    detail = {}
    shoe_top, shoe_low, shoe_hi = (110, 72, 50, 255), (78, 50, 38, 255), (150, 104, 72, 255)
    for x0, x1, y in shoes:
        for x in range(x0, x1 + 1):
            detail[(x, y)] = shoe_top
            detail[(x, y + 1)] = shoe_low
        detail[(x0 + 1, y)] = shoe_hi
    belt, buckle = (86, 58, 40, 255), (232, 190, 96, 255)
    bx0, bx1 = (7, 16) if direction != "right" else (9, 15)
    for x in range(bx0, bx1 + 1):
        detail[(x, 22)] = belt
    if direction == "down":
        detail[(11, 22)] = detail[(12, 22)] = buckle
        white, iris, brow = (250, 250, 246, 255), (54, 38, 44, 255), (70, 46, 40, 220)
        for ex in (9, 13):
            detail[(ex, 7)] = white
            detail[(ex + 1, 7)] = iris
            detail[(ex, 8)] = iris
            detail[(ex + 1, 8)] = iris
            detail[(ex, 6)] = detail[(ex + 1, 6)] = brow
        detail[(11, 11)] = detail[(12, 11)] = (156, 76, 72, 230)       # mouth
        detail[(8, 10)] = detail[(15, 10)] = (236, 120, 120, 120)      # blush
        skin_v[10, 12] = 0.7 if skin_v[10, 12] else 0                   # nose shadow
    elif direction == "right":
        detail[(15, 7)] = (250, 250, 246, 255)
        detail[(16, 7)] = detail[(15, 8)] = detail[(16, 8)] = (54, 38, 44, 255)
        detail[(15, 6)] = detail[(16, 6)] = (70, 46, 40, 220)
        detail[(16, 11)] = (156, 76, 72, 230)
        detail[(14, 10)] = (236, 120, 120, 110)
        skin_v[8, 12] = skin_v[9, 12] = 0.7                             # ear

    everything = skin.array | shirt.array | pants.array
    for (x, y), c in detail.items():
        if 0 <= x < W and 0 <= y < H and c[3] == 255:
            everything[y, x] = True
    return skin_v, shirt_v, pants_v, detail, everything


# ---------------------------------------------------------------- hair
def hair_mask(style, direction):
    m = Mask()
    if direction == "down":
        m.ellipse(6, 1, 17, 11).keep(lambda x, y: y <= 4 or x <= 7 or x >= 16)
        if style != 4:
            for x in (8, 9, 10, 13, 14, 15):
                m.rect(x, 5, x, 5)
            m.rect(8, 5, 8, 6).rect(15, 5, 15, 6)
        if style == 1:
            m.rect(5, 6, 7, 18).rect(16, 6, 18, 18)
        if style == 2:
            m.poly((7, 3), (8, -1), (10, 2)).poly((10, 2), (12, -2), (14, 2)).poly((13, 2), (16, -1), (16, 3))
        if style == 3:
            m.ellipse(2, 7, 6, 12).ellipse(17, 7, 21, 12)
        if style == 4:
            m = Mask().ellipse(7, 2, 16, 9).keep(lambda x, y: y <= 4 or x <= 7 or x >= 16)
        if style == 5:
            m.ellipse(9, -1, 14, 4)
    elif direction == "up":
        m.ellipse(6, 1, 17, 12).keep(lambda x, y: y <= 11)
        if style == 1:
            m.rect(6, 6, 17, 17)
        if style == 2:
            m.poly((7, 3), (8, -1), (10, 2)).poly((10, 2), (12, -2), (14, 2)).poly((13, 2), (16, -1), (16, 3))
        if style == 3:
            m.ellipse(2, 7, 6, 12).ellipse(17, 7, 21, 12)
        if style == 4:
            m = Mask().ellipse(7, 2, 16, 12).keep(lambda x, y: y <= 11)
        if style == 5:
            m.ellipse(9, 0, 14, 5)
    else:  # right
        m.ellipse(7, 1, 18, 11).keep(lambda x, y: y <= 4 or x <= 12)
        if style != 4:
            m.rect(14, 5, 17, 5)
        if style == 1:
            m.rect(6, 6, 11, 18)
        if style == 2:
            m.poly((8, 3), (9, -1), (11, 2)).poly((11, 2), (13, -2), (15, 2)).poly((14, 2), (17, 0), (17, 4))
        if style == 3:
            m.ellipse(4, 7, 8, 12)
        if style == 4:
            m = Mask().ellipse(8, 2, 17, 10).keep(lambda x, y: y <= 4 or x <= 12)
        if style == 5:
            m.ellipse(5, 2, 10, 7)
    return m.array


def hair_layer(style, direction):
    mask = hair_mask(style, direction)
    v = lit(mask, base=0.82, depth=0.36, levels=(0.5, 0.66, 0.82, 0.97))
    # a few darker strands for texture
    for y in range(H):
        for x in range(W):
            if mask[y, x] and (x * 3 + y * 5) % 7 == 0 and v[y, x] > 0.6:
                v[y, x] -= 0.14
    return to_gray_image(with_rim(v, mask))


# ---------------------------------------------------------------- accessories
def accessory_mask(kind, direction):
    m = Mask()
    if kind == "hat":            # sombrero cordobés: flat wide brim, flat-topped crown
        if direction == "right":
            m.ellipse(4, 4, 21, 7).rect(9, 0, 16, 5)
        else:
            m.ellipse(3, 4, 20, 7).rect(7, 0, 16, 5)
    elif kind == "cloak":
        if direction == "down":
            m.poly((6, 13), (17, 13), (20, 27), (3, 27))
        elif direction == "up":
            m.poly((6, 13), (17, 13), (19, 27), (4, 27))
        else:
            m.poly((8, 13), (13, 13), (11, 27), (5, 27))
    elif kind == "apron":
        if direction == "down":
            m.rect(8, 16, 15, 27).rect(9, 14, 9, 15).rect(14, 14, 14, 15)
        elif direction == "right":
            m.rect(14, 16, 16, 27)
        else:
            m.rect(7, 21, 16, 21)
    elif kind == "beard":
        if direction == "down":
            m.poly((8, 10), (15, 10), (15, 13), (12, 15), (11, 15), (8, 13)).cut(Mask().rect(10, 11, 13, 11))
        elif direction == "right":
            m.poly((13, 10), (17, 10), (16, 13), (14, 14))
    elif kind == "skirt":
        if direction == "right":
            m.poly((9, 21), (15, 21), (17, 28), (8, 28))
        else:
            m.poly((7, 21), (16, 21), (18, 28), (5, 28))
    elif kind == "shawl":
        if direction == "down":
            m.poly((6, 14), (17, 14), (18, 17), (12, 22), (5, 17))
        elif direction == "up":
            m.poly((6, 14), (17, 14), (18, 18), (12, 24), (5, 18))
        else:
            m.poly((9, 14), (16, 14), (16, 18), (9, 19))
    return m.array


def accessory_layer(kind, direction):
    mask = accessory_mask(kind, direction)
    v = lit(mask, base=0.84)
    if kind == "hat":
        v[5:8][mask[5:8]] = np.clip(v[5:8][mask[5:8]] - 0.1, 0.5, 1)   # band under the crown
    return to_gray_image(with_rim(v, mask, 0.38) if kind in ("hat", "beard", "cloak") else v)


# ---------------------------------------------------------------- output
def make_characters():
    for d in DIRS:
        for frame in FRAMES:
            skin_v, shirt_v, pants_v, detail, everything = body_layers(d, frame)
            save_layer(to_gray_image(skin_v), f"skin_{d}{frame}")
            save_layer(to_gray_image(pants_v), f"pants_{d}{frame}")
            save_layer(to_gray_image(shirt_v), f"shirt_{d}{frame}")
            img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
            for (x, y), c in detail.items():
                if 0 <= x < W and 0 <= y < H:
                    img.putpixel((x, y), c)
            save_layer(img, f"detail_{d}{frame}")
            save_layer(outline_of(everything), f"outline_{d}{frame}")
        for style in range(6):
            save_layer(hair_layer(style, d), f"hair{style}_{d}")
        for kind in ("hat", "cloak", "apron", "beard", "skirt", "shawl"):
            save_layer(accessory_layer(kind, d), f"acc_{kind}_{d}")


if __name__ == "__main__":
    make_characters()
    print("Characters written to", os.path.join(ART, "Character"))
