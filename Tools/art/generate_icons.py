"""Pets, equipment, empty equipment-slot silhouettes and HUD icons.

Run:  python Tools/art/generate_icons.py
"""
import math
import os

from PIL import Image, ImageDraw

from generate_art import ART, icon, save

O = (52, 34, 34)


def make_pets():
    dog = {"o": O, "b": (156, 104, 62), "l": (200, 150, 96), "k": (30, 20, 20), "t": (220, 90, 100)}
    rows_dog = [
        "................",
        "..........ooo...",
        ".........obbboo.",
        ".........obkbbbo",
        "..o......obbbbko",
        ".obo....obbbbbo.",
        ".obbooooobbbtoo.",
        "..obbbbbbbbboo..",
        "..oblbbbbbbbo...",
        "..obllbbbbbbo...",
        "..obbbbbbbbbo...",
        "..obbooobbobbo..",
        "..obo..obo.obo..",
        "..oo...oo..oo...",
    ]
    cat = {"o": O, "b": (150, 150, 160), "s": (100, 100, 112), "k": (40, 90, 40), "p": (230, 140, 150)}
    rows_cat = [
        "..........o..o..",
        ".........obooboo",
        ".........obbbbbo",
        ".........obkbkbo",
        "..o......obbpbbo",
        ".obo.....oobbbo.",
        "..obo..ooobbbo..",
        "...obbbbbbbbbo..",
        "...obsbsbsbbbo..",
        "...obbbbbbbbbo..",
        "...obbbbbbbbbo..",
        "...obooobooobo..",
        "...oo..oo..oo...",
    ]
    rabbit = {"o": O, "w": (246, 242, 234), "s": (200, 196, 190), "k": (40, 30, 30), "p": (236, 150, 160)}
    rows_rabbit = [
        "..........oo.oo.",
        ".........owoowo.",
        ".........opoopo.",
        ".........owoowo.",
        "........owwwwwo.",
        "........owkwwwo.",
        "........owwwwpo.",
        "....ooooowwwwo..",
        "...owwwwwwwwwo..",
        "..owwwwwwwwwwo..",
        ".oswwwwwwwwwwo..",
        ".owwwwwwwwwwwo..",
        "..oowwwooowwo...",
        "....ooo...ooo...",
    ]
    parrot = {"o": O, "g": (70, 170, 80), "w": (246, 246, 240), "k": (20, 20, 20), "y": (240, 196, 60),
              "r": (220, 60, 50), "b": (60, 110, 210)}
    rows_parrot = [
        ".........ooo....",
        "........ogggo...",
        ".......ogwkggo..",
        ".......ogwwgyo..",
        ".......oggggyyo.",
        "......ogggggoo..",
        ".....ogggrrgo...",
        "....oggggrrgo...",
        "....ogbggggo....",
        "...ogbbgggo.....",
        "...obbggo.......",
        "..obbbo.oyo.....",
        "..obbo..oyo.....",
        "...oo...........",
    ]
    for name, rows, pal in (("pet_dog", rows_dog, dog), ("pet_cat", rows_cat, cat),
                            ("pet_rabbit", rows_rabbit, rabbit), ("pet_parrot", rows_parrot, parrot)):
        icon(name, rows, pal)
        # second frame for the world: shifted up a pixel (a little hop when walking)
        img = Image.open(os.path.join(ART, "Icons", name + ".png"))
        hop = Image.new("RGBA", img.size, (0, 0, 0, 0))
        hop.paste(img, (0, -1))
        save(hop, "Icons", name + "_hop")


GEAR = {
    "gear_hat": ([
        "................",
        "................",
        "....oooooooo....",
        "....okkkkkko....",
        "....okkkkkko....",
        "....okkkkkko....",
        "....orrrrrro....",
        ".oooooooooooooo.",
        "okkkkkkkkkkkkkko",
        ".oooooooooooooo.",
    ], {"o": O, "k": (50, 46, 52), "r": (150, 40, 44)}),
    "gear_necklace": ([
        "..oo........oo..",
        "..oyo......oyo..",
        "...oyo....oyo...",
        "....oyo..oyo....",
        ".....oyyyyo.....",
        "......oyyo......",
        ".....orrrro.....",
        ".....orwrro.....",
        ".....orrrro.....",
        "......oooo......",
    ], {"o": O, "y": (232, 190, 80), "r": (200, 40, 60), "w": (255, 200, 210)}),
    "gear_cloak": ([
        ".....oooooo.....",
        "....oyrrrryo....",
        "...orrrrrrrro...",
        "...orrrrrrrro...",
        "..orrrdrrdrrro..",
        "..orrrdrrdrrro..",
        ".orrrrdrrdrrrro.",
        ".orrrrdrrdrrrro.",
        "orrrrrdrrdrrrrro",
        "oddddddddddddddo",
        ".oooooooooooooo.",
    ], {"o": O, "r": (176, 44, 52), "d": (128, 30, 40), "y": (232, 190, 80)}),
    "gear_vest": ([
        "...ooo....ooo...",
        "..obbbo..obbbo..",
        "..obbbboobbbbo..",
        ".obbbbbobbbbbbo.",
        ".obbbbbobbbbbbo.",
        ".obbbbyobybbbbo.",
        ".obbbbbobbbbbbo.",
        ".obbbbyobybbbbo.",
        ".obbbbbobbbbbbo.",
        ".odddddodddddddo",
        "..oooooooooooo..",
    ], {"o": O, "b": (150, 98, 58), "d": (110, 70, 42), "y": (232, 190, 80)}),
    "gear_pants": ([
        "..oooooooooooo..",
        "..obbbbbbbbbbo..",
        "..odddddddddo...",
        "..obbbbobbbbbo..",
        "..obbbboobbbbo..",
        "..obbbbo.obbbo..",
        "..obbbbo.obbbo..",
        "..obbbbo.obbbo..",
        "..obbbbo.obbbo..",
        "..oddddo.odddo..",
        "..oooooo.ooooo..",
    ], {"o": O, "b": (96, 76, 60), "d": (70, 54, 44)}),
    "gear_bracelet": ([
        "................",
        ".....oooooo.....",
        "...ooyyyyyyoo...",
        "..oyyoooooyyyo..",
        "..oyo......oyo..",
        "..oyo......oyo..",
        "..oyyoooooyyyo..",
        "...ooyybyyyoo...",
        ".....oooooo.....",
    ], {"o": O, "y": (232, 190, 80), "b": (70, 130, 220)}),
    "gear_shield": ([
        ".....oooooo.....",
        "...oowwwwwwoo...",
        "..owwbbbbbbwwo..",
        ".owbbbbbbbbbbwo.",
        ".owbbbbssbbbbwo.",
        "owbbbbsyysbbbbwo",
        "owbbbbsyysbbbbwo",
        ".owbbbbssbbbbwo.",
        ".owbbbbbbbbbbwo.",
        "..owwbbbbbbwwo..",
        "...oowwwwwwoo...",
        ".....oooooo.....",
    ], {"o": O, "w": (150, 156, 170), "b": (140, 96, 58), "s": (190, 196, 208), "y": (232, 190, 80)}),
}


def make_orange():
    icon("food_orange", [
        "........gg......",
        ".......gg.......",
        ".....oooooo.....",
        "...ooyyyyyyoo...",
        "..oyywyyyyyyyo..",
        "..oywyyyyyyyyo..",
        ".oyyyyyyyyyyyyo.",
        ".oyyyyyyyyyyyyo.",
        ".oyyyyyyyyyyydo.",
        "..oyyyyyyyyydo..",
        "..oyyyyyyyyddo..",
        "...ooyyyyddoo...",
        ".....oooooo.....",
    ], {"o": O, "y": (240, 140, 40), "d": (200, 100, 30), "w": (255, 210, 140), "g": (80, 140, 60)})


def make_gear():
    for name, (rows, pal) in GEAR.items():
        icon(name, rows, pal)
    # faint silhouettes for empty equipment slots
    for slot, src in (("hat", "gear_hat"), ("necklace", "gear_necklace"), ("cloak", "gear_cloak"),
                      ("chest", "gear_vest"), ("legs", "gear_pants"), ("wrists", "gear_bracelet"),
                      ("primary", "item_sword"), ("secondary", "gear_shield")):
        img = Image.open(os.path.join(ART, "Icons", src + ".png")).convert("RGBA")
        sil = Image.new("RGBA", img.size, (0, 0, 0, 0))
        for y in range(img.height):
            for x in range(img.width):
                if img.getpixel((x, y))[3] > 0:
                    sil.putpixel((x, y), (255, 255, 255, 70))
        save(sil, "UI", "slot_" + slot)


def make_hud_icons():
    # translate: a little globe
    g = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(g)
    d.ellipse([1, 1, 14, 14], fill=(80, 150, 220), outline=O)
    d.line([7, 2, 7, 13], fill=(200, 232, 255))
    d.arc([4, 2, 11, 13], 0, 360, fill=(200, 232, 255))
    d.line([2, 7, 13, 7], fill=(200, 232, 255))
    d.polygon([(4, 4), (7, 3), (6, 6), (3, 7)], fill=(96, 176, 90))
    d.polygon([(9, 8), (12, 9), (10, 12)], fill=(96, 176, 90))
    save(g, "UI", "translate")

    # controls: a parchment page with lines
    p = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(p)
    d.rectangle([3, 1, 13, 14], fill=(246, 232, 196), outline=O)
    d.rectangle([1, 1, 4, 3], fill=(214, 190, 140), outline=O)
    d.rectangle([12, 12, 14, 14], fill=(214, 190, 140), outline=O)
    for y in (4, 6, 8, 10):
        d.line([5, y, 11, y], fill=(120, 96, 70))
    save(p, "UI", "page")

    # minimap: gold ring frame (64x64) and a white disc used as the round mask
    ring = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(ring)
    d.ellipse([0, 0, 63, 63], outline=O, width=1)
    d.ellipse([1, 1, 62, 62], outline=(214, 170, 70), width=3)
    d.ellipse([4, 4, 59, 59], outline=(120, 84, 40), width=1)
    d.arc([1, 1, 62, 62], 200, 300, fill=(250, 220, 140), width=1)
    for ang in (0, 90, 180, 270):
        x = 32 + math.cos(math.radians(ang)) * 30
        y = 32 + math.sin(math.radians(ang)) * 30
        d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(232, 190, 80), outline=O)
    save(ring, "UI", "minimap_ring")
    disc = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    ImageDraw.Draw(disc).ellipse([3, 3, 60, 60], fill=(255, 255, 255, 255))
    save(disc, "UI", "minimap_mask")

    arrow = Image.new("RGBA", (9, 9), (0, 0, 0, 0))
    ImageDraw.Draw(arrow).polygon([(4, 0), (8, 8), (4, 6), (0, 8)], fill=(255, 236, 120), outline=O)
    save(arrow, "UI", "map_arrow")

    dot = Image.new("RGBA", (5, 5), (0, 0, 0, 0))
    ImageDraw.Draw(dot).ellipse([0, 0, 4, 4], fill=(255, 255, 255), outline=O)
    save(dot, "UI", "map_dot")

    slot = Image.new("RGBA", (20, 20), (0, 0, 0, 0))
    d = ImageDraw.Draw(slot)
    d.rectangle([0, 0, 19, 19], fill=(60, 48, 44), outline=O)
    d.rectangle([1, 1, 18, 18], outline=(120, 96, 70))
    d.line([2, 2, 17, 2], fill=(40, 30, 28))
    d.line([2, 2, 2, 17], fill=(40, 30, 28))
    save(slot, "UI", "slot")


if __name__ == "__main__":
    make_pets()
    make_orange()
    make_gear()
    make_hud_icons()
    print("Icons written to", ART)
