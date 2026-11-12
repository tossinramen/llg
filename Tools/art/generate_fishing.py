"""Fishing: 16 kinds of fish (and an old boot), rods, the net, the fishing hat, baits and the bobber.

Run:  python Tools/art/generate_fishing.py      (after generate_items.py; before generate_village.py)
"""
import math

from PIL import Image, ImageDraw

from generate_art import save
from generate_items import canvas, icon


def fish(name, body, belly, fin, length=13, height=6, stripes=None, spots=None, eye=(20, 20, 30), y=8):
    """A side-view fish facing left: oval body, forked tail, a fin on top, lighter belly."""
    img, d = canvas()
    x0 = (16 - length - 3) // 2 + 1
    x1 = x0 + length - 1
    d.polygon([(x1 - 1, y), (x1 + 3, y - 3), (x1 + 2, y), (x1 + 3, y + 3)], fill=fin)          # tail
    d.ellipse([x0, y - height // 2, x1, y + height // 2], fill=body)
    d.ellipse([x0 + 2, y, x1 - 2, y + height // 2], fill=belly)                                 # belly
    d.polygon([(x0 + length // 3, y - height // 2), (x0 + length // 2 + 1, y - height // 2 - 2), (x0 + length * 2 // 3, y - height // 2)], fill=fin)
    for sx in stripes or []:
        d.line([(x0 + sx, y - height // 2 + 1), (x0 + sx, y + height // 2 - 1)], fill=fin)
    for sx, sy, col in spots or []:
        d.point((x0 + sx, y + sy), fill=col)
    d.point((x0 + 2, y - 1), fill=eye)
    d.point((x0 + 2, y - 2), fill=(255, 255, 255))
    icon(name, img)


def fishes():
    fish("fish_sardine", (150, 170, 190), (220, 230, 236), (110, 130, 150), length=11, height=4)
    fish("fish_anchovy", (120, 150, 170), (210, 220, 228), (90, 110, 130), length=10, height=3, stripes=[])
    fish("fish_carp", (176, 140, 70), (226, 200, 130), (140, 100, 50), length=12, height=7)
    fish("fish_trout", (130, 150, 100), (236, 200, 190), (100, 110, 80), length=12, height=5,
         spots=[(4, -1, (60, 50, 40)), (7, -2, (60, 50, 40)), (9, 0, (200, 80, 80)), (6, 1, (200, 80, 80))])
    fish("fish_perch", (150, 170, 80), (230, 210, 140), (210, 110, 60), length=11, height=6, stripes=[4, 6, 8])
    fish("fish_seabass", (140, 150, 160), (226, 228, 230), (90, 100, 110), length=13, height=6)
    fish("fish_salmon", (200, 120, 110), (246, 186, 160), (150, 90, 90), length=13, height=6)
    fish("fish_bream", (190, 196, 206), (236, 236, 240), (110, 116, 130), length=11, height=7,
         spots=[(4, -3, (230, 190, 60)), (5, -3, (230, 190, 60))])
    fish("fish_cod", (150, 130, 100), (220, 210, 190), (110, 96, 76), length=13, height=6,
         spots=[(5, -1, (100, 86, 66)), (8, -2, (100, 86, 66)), (10, 0, (100, 86, 66))])
    fish("fish_tuna", (50, 70, 130), (200, 206, 216), (220, 190, 60), length=14, height=7)
    fish("fish_koi", (246, 244, 238), (250, 250, 246), (240, 120, 50), length=12, height=6,
         spots=[(3, -2, (240, 100, 40)), (4, -2, (240, 100, 40)), (7, -1, (240, 100, 40)), (8, 0, (240, 100, 40)), (9, -2, (40, 30, 30))])
    fish("fish_rune", (240, 196, 70), (255, 232, 140), (110, 220, 255), length=13, height=7,
         stripes=[5, 8], spots=[(6, 0, (110, 220, 255)), (9, 1, (110, 220, 255))])

    img, d = canvas()                                                       # swordfish: a long sword nose
    d.polygon([(13, 8), (15, 5), (14, 8), (15, 11)], fill=(60, 80, 130))
    d.ellipse([3, 6, 13, 10], fill=(80, 100, 150))
    d.ellipse([5, 8, 12, 10], fill=(200, 210, 224))
    d.polygon([(6, 6), (8, 2), (10, 6)], fill=(60, 80, 130))
    d.line([(0, 8), (3, 8)], fill=(170, 180, 196))
    d.point((5, 7), fill=(20, 20, 30))
    icon("fish_swordfish", img)

    img, d = canvas()                                                       # eel: long and wavy
    for x in range(1, 15):
        yy = 8 + int(round(math.sin(x / 2.0) * 2))
        d.line([(x, yy - 1), (x, yy + 1)], fill=(90, 100, 60))
        d.point((x, yy + 1), fill=(170, 170, 110))
    d.point((2, 8 + int(round(math.sin(0.5) * 2)) - 1), fill=(240, 220, 120))
    icon("fish_eel", img)

    img, d = canvas()                                                       # octopus: round head and curly legs
    d.ellipse([4, 1, 12, 9], fill=(200, 90, 120))
    for k, x in enumerate((3, 5, 7, 9, 11, 13)):
        d.line([(x - 1 if x < 8 else x - 2, 8), (x - 2 + (k % 2), 13)], fill=(200, 90, 120))
        d.point((x - 2 + (k % 2) + (1 if k % 2 else -1), 14), fill=(200, 90, 120))
    d.point((6, 5), fill=(20, 20, 30)); d.point((10, 5), fill=(20, 20, 30))
    d.point((6, 4), fill=(255, 255, 255)); d.point((10, 4), fill=(255, 255, 255))
    icon("fish_octopus", img)

    img, d = canvas()                                                       # crab
    d.ellipse([3, 6, 12, 12], fill=(210, 80, 60))
    for x in (2, 13):
        d.ellipse([x - 2, 2, x + 1, 5], fill=(210, 80, 60))
        d.line([(x, 5), (x if x < 8 else x - 1, 7)], fill=(210, 80, 60))
    for x in (4, 6, 9, 11):
        d.line([(x, 12), (x - 1 if x < 8 else x + 1, 14)], fill=(170, 60, 50))
    d.point((6, 7), fill=(20, 20, 30)); d.point((9, 7), fill=(20, 20, 30))
    icon("fish_crab", img)

    img, d = canvas()                                                       # old boot (junk)
    d.polygon([(4, 2), (9, 2), (9, 10), (14, 11), (14, 14), (3, 14)], fill=(110, 80, 56))
    d.line([(3, 13), (14, 13)], fill=(60, 44, 32))
    d.line([(5, 4), (8, 4)], fill=(170, 150, 110)); d.line([(5, 6), (8, 6)], fill=(170, 150, 110))
    d.point((12, 9), fill=(90, 150, 80)); d.point((11, 8), fill=(90, 150, 80))     # weed
    icon("fish_boot", img)


def rods():
    for name, pole, reel in (("item_rod_bamboo", (196, 176, 96), (120, 100, 60)), ("item_rod_carbon", (50, 50, 60), (180, 40, 40)),
                             ("item_rod_gold", (240, 196, 70), (110, 220, 255))):
        img, d = canvas()
        d.line([(2, 14), (13, 1)], fill=pole, width=2)
        if name == "item_rod_bamboo":
            for k in (4, 8, 11):
                d.point((2 + k, 14 - k - 1), fill=(120, 100, 60))
        d.ellipse([3, 9, 7, 13], fill=reel)
        d.line([(13, 1), (14, 6)], fill=(230, 230, 230))
        d.point((14, 7), fill=(220, 60, 50))
        icon(name, img)


def gear():
    img, d = canvas()                                                       # net
    d.polygon([(2, 3), (14, 3), (12, 14), (4, 14)], fill=(170, 150, 110))
    for x in range(3, 14, 3):
        d.line([(x, 3), (x + (8 - x) // 3, 14)], fill=(110, 90, 60))
    for yy in range(5, 14, 3):
        d.line([(2 + (yy - 3) // 6, yy), (14 - (yy - 3) // 6, yy)], fill=(110, 90, 60))
    d.line([(1, 3), (15, 3)], fill=(110, 80, 50), width=2)
    icon("item_net", img)

    img, d = canvas()                                                       # fishing hat with a fly on the band
    d.ellipse([1, 8, 15, 13], fill=(130, 140, 90))
    d.rectangle([4, 4, 12, 10], fill=(150, 160, 106))
    d.ellipse([4, 2, 12, 6], fill=(150, 160, 106))
    d.line([(4, 9), (12, 9)], fill=(90, 100, 60))
    d.point((10, 8), fill=(220, 60, 60)); d.point((11, 7), fill=(240, 200, 60))
    icon("gear_fishing_hat", img)

    img, d = canvas()                                                       # bread crumbs
    for x, y in ((4, 9), (8, 7), (11, 10), (6, 12), (10, 13), (7, 10)):
        d.rectangle([x, y, x + 2, y + 1], fill=(220, 180, 110))
    icon("bait_bread", img)

    img, d = canvas()                                                       # worm
    pts = [(x, 9 + math.sin(x / 1.1) * 2.2) for x in range(2, 15)]
    d.line(pts, fill=(214, 120, 120), width=2)
    for x in (4, 8, 12):
        d.point((x, int(round(9 + math.sin(x / 1.1) * 2.2))), fill=(170, 80, 90))       # segments
    d.point((2, int(round(9 + math.sin(2 / 1.1) * 2.2)) - 1), fill=(240, 160, 160))
    icon("bait_worm", img)

    img, d = canvas()                                                       # shrimp
    d.arc([3, 3, 13, 13], 200, 20, fill=(240, 140, 100), width=3)
    d.line([(12, 6), (15, 3)], fill=(240, 140, 100))
    d.point((11, 6), fill=(20, 20, 30))
    icon("bait_shrimp", img)

    img, d = canvas()                                                       # squid
    d.polygon([(8, 1), (12, 6), (11, 10), (5, 10), (4, 6)], fill=(236, 210, 220))
    for x in (5, 7, 9, 11):
        d.line([(x, 10), (x + (1 if x > 8 else -1), 14)], fill=(220, 180, 196))
    d.point((7, 7), fill=(20, 20, 30)); d.point((9, 7), fill=(20, 20, 30))
    icon("bait_squid", img)

    img = Image.new("RGBA", (8, 12), (0, 0, 0, 0))                          # the bobber (corcho)
    d = ImageDraw.Draw(img)
    d.line([(4, 0), (4, 3)], fill=(60, 60, 60))
    d.ellipse([1, 3, 7, 11], fill=(240, 240, 236), outline=(40, 30, 30))
    d.chord([1, 3, 7, 11], 180, 360, fill=(220, 50, 50), outline=(40, 30, 30))
    save(img, "UI", "bobber")


if __name__ == "__main__":
    fishes()
    rods()
    gear()
    print("fishing art written")
