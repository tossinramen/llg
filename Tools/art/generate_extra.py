"""Armour tiers, musical instruments, seeds, produce, growing-crop sprites and the piano.

Run:  python Tools/art/generate_extra.py      (after generate_items.py)
"""
from PIL import Image

from generate_art import save
from generate_items import canvas, icon, finish

# ---------------------------------------------------------------- armour: 6 tiers x 5 pieces
TIERS = {
    "cloth":   ((196, 180, 150), (150, 132, 104), (236, 226, 200)),
    "leather": ((150, 100, 60), (110, 70, 40), (196, 146, 96)),
    "mail":    ((160, 166, 180), (110, 116, 130), (214, 220, 232)),
    "iron":    ((120, 126, 140), (80, 84, 98), (190, 196, 210)),
    "toledo":  ((206, 210, 224), (150, 120, 60), (250, 230, 150)),
    "rune":    ((70, 90, 130), (40, 50, 80), (120, 230, 255)),
}


def armour_piece(slot, base, dark, shine):
    img, d = canvas()
    if slot == "head":
        d.chord([2, 2, 13, 15], 180, 360, fill=base)
        d.rectangle([2, 8, 13, 10], fill=dark)
        d.point((5, 5), fill=shine)
    elif slot == "chest":
        d.polygon([(3, 2), (12, 2), (14, 6), (12, 14), (3, 14), (1, 6)], fill=base)
        d.line([(8, 3), (8, 13)], fill=dark)
        d.arc([3, 4, 12, 12], 200, 340, fill=shine)
    elif slot == "legs":
        for x in (3, 9):
            d.rounded_rectangle([x, 2, x + 4, 14], radius=2, fill=base)
            d.line([(x + 1, 4), (x + 1, 12)], fill=shine)
        d.rectangle([3, 2, 13, 4], fill=dark)
    elif slot == "wrists":
        for y in (3, 9):
            d.rounded_rectangle([2, y, 13, y + 4], radius=2, fill=base)
            d.line([(3, y + 2), (12, y + 2)], fill=shine)
    else:  # shield
        d.polygon([(2, 2), (13, 2), (13, 8), (7, 14), (2, 8)], fill=base)
        d.polygon([(4, 4), (11, 4), (11, 8), (7, 12), (4, 8)], fill=dark)
        d.line([(7, 4), (7, 11)], fill=shine)
    return img


def armour():
    for tier, (base, dark, shine) in TIERS.items():
        for slot in ("head", "chest", "legs", "wrists", "shield"):
            icon(f"armor_{tier}_{slot}", armour_piece(slot, base, dark, shine))


# ---------------------------------------------------------------- instruments
def instruments():
    img, d = canvas()                                                       # guitar
    d.ellipse([1, 7, 9, 15], fill=(196, 130, 70))
    d.ellipse([4, 4, 10, 10], fill=(196, 130, 70))
    d.ellipse([4, 9, 7, 12], fill=(60, 40, 30))
    d.line([(7, 8), (14, 1)], fill=(110, 70, 40), width=2)
    icon("item_guitar", img)
    img, d = canvas()                                                       # flute
    d.line([(1, 14), (14, 1)], fill=(214, 200, 150), width=2)
    for k in (4, 7, 10):
        d.point((k, 15 - k), fill=(60, 50, 40))
    icon("item_flute", img)
    img, d = canvas()                                                       # clarinet
    d.line([(4, 1), (10, 13)], fill=(40, 36, 40), width=3)
    d.polygon([(8, 12), (13, 15), (6, 15)], fill=(40, 36, 40))
    for y in (4, 7, 10):
        d.point((6 + y // 3, y), fill=(200, 200, 210))
    icon("item_clarinet", img)
    img, d = canvas()                                                       # tambourine
    d.ellipse([1, 2, 14, 14], fill=(220, 196, 140))
    d.ellipse([4, 5, 11, 11], fill=(246, 236, 210))
    for x, y in ((2, 7), (13, 7), (7, 2), (7, 13)):
        d.ellipse([x - 1, y - 1, x + 1, y + 1], fill=(232, 190, 80))
    icon("item_tambourine", img)
    img, d = canvas()                                                       # trumpet
    d.rectangle([2, 7, 11, 9], fill=(232, 190, 80))
    d.polygon([(10, 4), (15, 2), (15, 14), (10, 12)], fill=(232, 190, 80))
    for x in (4, 6, 8):
        d.rectangle([x, 4, x, 7], fill=(200, 150, 50))
    icon("item_trumpet", img)
    img, d = canvas()                                                       # violin
    d.ellipse([3, 6, 10, 15], fill=(150, 70, 40))
    d.ellipse([4, 3, 9, 9], fill=(150, 70, 40))
    d.line([(6, 3), (6, 0)], fill=(60, 40, 30), width=2)
    d.line([(10, 2), (2, 14)], fill=(220, 210, 180))
    icon("item_violin", img)

    # the piano in the music school (a world object, not an icon)
    img = Image.new("RGBA", (48, 36), (0, 0, 0, 0))
    from PIL import ImageDraw
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 47, 26], fill=(50, 34, 30, 255), outline=(30, 20, 18, 255))
    d.rectangle([2, 14, 45, 22], fill=(250, 250, 245, 255))
    for x in range(4, 45, 4):
        d.line([x, 14, x, 22], fill=(160, 160, 160, 255))
        if x % 8 == 4:
            d.rectangle([x + 1, 14, x + 2, 18], fill=(20, 20, 20, 255))
    d.rectangle([4, 26, 7, 35], fill=(50, 34, 30, 255))
    d.rectangle([40, 26, 43, 35], fill=(50, 34, 30, 255))
    d.rectangle([14, 3, 33, 10], fill=(240, 230, 200, 255))        # sheet music
    for y in (5, 7, 9):
        d.line([16, y, 31, y], fill=(120, 110, 100, 255))
    save(img, "World/Objects", "piano")


# ---------------------------------------------------------------- crops
CROPS = {
    # id: produce drawing colours
    "wheat": (214, 180, 80), "lettuce": (120, 190, 90), "tomato": (220, 60, 50), "carrot": (240, 140, 40),
    "strawberry": (226, 50, 70), "grape": (120, 60, 140), "olive": (110, 130, 60),
}


def produce(name, col):
    img, d = canvas()
    if name == "wheat":
        for x in (5, 8, 11):
            d.line([(x, 14), (x, 4)], fill=(170, 140, 60))
            d.ellipse([x - 1, 2, x + 1, 7], fill=col)
    elif name == "lettuce":
        d.ellipse([2, 4, 13, 14], fill=col)
        d.ellipse([5, 6, 10, 11], fill=(170, 220, 130))
    elif name == "tomato":
        d.ellipse([2, 4, 13, 14], fill=col)
        d.polygon([(6, 4), (8, 2), (10, 4), (8, 5)], fill=(70, 130, 60))
    elif name == "carrot":
        d.polygon([(4, 4), (11, 4), (8, 15)], fill=col)
        d.line([(6, 4), (5, 1)], fill=(70, 140, 60), width=2)
        d.line([(9, 4), (10, 1)], fill=(70, 140, 60), width=2)
    elif name == "strawberry":
        d.polygon([(3, 5), (12, 5), (8, 14)], fill=col)
        d.rectangle([4, 3, 11, 5], fill=(70, 140, 60))
        for x, y in ((6, 7), (9, 7), (7, 10)):
            d.point((x, y), fill=(250, 230, 120))
    elif name == "grape":
        for x, y in ((5, 4), (9, 4), (7, 7), (4, 8), (10, 8), (7, 11)):
            d.ellipse([x - 2, y - 2, x + 2, y + 2], fill=col)
        d.line([(7, 1), (7, 3)], fill=(110, 80, 50))
    else:  # olive
        for x, y in ((5, 7), (10, 9)):
            d.ellipse([x - 3, y - 4, x + 3, y + 4], fill=col)
        d.line([(3, 2), (12, 4)], fill=(110, 80, 50))
    return img


def crops():
    for name, col in CROPS.items():
        icon(f"crop_{name}", produce(name, col))
        # seed packet: a paper bag with the crop's picture on it
        img, d = canvas()
        d.rectangle([3, 2, 12, 14], fill=(226, 206, 160))
        d.rectangle([3, 2, 12, 4], fill=(190, 160, 110))
        pic = produce(name, col).resize((8, 8), Image.NEAREST)
        img.alpha_composite(pic, (4, 6))
        icon(f"seed_{name}", img)
        # growing stages for the garden: 0 sprout, 1 young plant, 2 ripe (plant + produce)
        for stage in range(3):
            st = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
            from PIL import ImageDraw
            sd = ImageDraw.Draw(st)
            green, dark = (90, 160, 70, 255), (50, 110, 50, 255)
            if stage == 0:
                sd.line([(8, 13), (8, 10)], fill=green)
                sd.point((7, 10), fill=green); sd.point((9, 9), fill=green)
            else:
                h = 6 if stage == 1 else 9
                sd.line([(8, 14), (8, 14 - h)], fill=dark)
                for k in range(0, h, 2):
                    sd.line([(8, 13 - k), (5 + k % 3, 12 - k)], fill=green)
                    sd.line([(8, 13 - k), (11 - k % 3, 12 - k)], fill=green)
                if stage == 2:
                    st.alpha_composite(produce(name, col).resize((10, 10), Image.NEAREST), (3, 1))
            save(finish(st, outline=(40, 60, 30)), "World/Crops", f"{name}_{stage}")


if __name__ == "__main__":
    armour()
    instruments()
    crops()
    print("extra art written")
