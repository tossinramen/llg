"""La Torre Rúnica (the enchanting tower), its altar, and icons for spells and enchantments.

Run:  python Tools/art/generate_magic.py      (after generate_items.py; before generate_village.py)
"""
import math

from PIL import Image, ImageDraw

from generate_art import save
from generate_items import canvas, finish, icon

GLOW = (110, 220, 255)


def tower():
    """A round stone tower with a slate cone roof and glowing rune windows."""
    w, h = 80, 150
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([6, 138, 74, 149], fill=(20, 28, 16, 70))                      # shadow
    stone, stone_d, stone_l = (150, 146, 156), (110, 106, 118), (186, 182, 192)
    d.rectangle([14, 50, 66, 144], fill=stone)
    d.rectangle([50, 50, 66, 144], fill=stone_d)                              # shaded side (round feel)
    d.rectangle([14, 50, 22, 144], fill=stone_l)
    for y in range(54, 144, 8):                                                # stone courses
        off = (y // 8) % 2 * 6
        for x in range(14 + off, 66, 12):
            d.rectangle([x, y, min(x + 11, 65), y + 7], outline=(96, 92, 104))
    d.polygon([(8, 52), (72, 52), (40, 2)], fill=(70, 74, 104))                # slate cone roof
    d.polygon([(40, 2), (72, 52), (52, 52)], fill=(52, 56, 84))
    for y in range(12, 52, 6):
        half = (y - 2) * 32 / 50
        d.line([(40 - half, y), (40 + half, y)], fill=(60, 64, 94))
    d.ellipse([36, 0, 44, 6], fill=GLOW)                                       # glowing orb on top
    for wy in (66, 92):                                                        # rune windows
        d.ellipse([33, wy, 47, wy + 18], fill=(30, 34, 60))
        d.line([(40, wy + 3), (40, wy + 15)], fill=GLOW)
        d.line([(40, wy + 5), (44, wy + 8)], fill=GLOW)
        d.line([(44, wy + 8), (40, wy + 11)], fill=GLOW)
    d.rectangle([30, 118, 50, 144], fill=(40, 30, 50))                         # door
    d.ellipse([30, 110, 50, 128], fill=(40, 30, 50))
    d.line([(40, 112), (40, 144)], fill=(70, 56, 80))
    d.point((46, 132), fill=(232, 190, 80))
    d.rectangle([26, 104, 54, 109], fill=(246, 244, 236), outline=(40, 70, 140))   # little tile sign
    for x in range(30, 52, 3):
        d.point((x, 106), fill=(30, 56, 120))
    save(img, "World/Objects", "tower")


def altar():
    """The rune altar: a stone table with a glowing open book and floating runes."""
    img = Image.new("RGBA", (48, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([4, 32, 44, 39], fill=(20, 20, 30, 70))
    d.polygon([(6, 18), (42, 18), (38, 36), (10, 36)], fill=(70, 66, 90))
    d.rectangle([4, 14, 44, 19], fill=(110, 106, 130))
    d.line([(4, 14), (44, 14)], fill=(150, 146, 170))
    for x in (12, 22, 32):
        d.line([(x, 22), (x + 3, 26)], fill=GLOW)
        d.line([(x + 3, 26), (x, 30)], fill=GLOW)
    d.polygon([(14, 13), (24, 9), (34, 13), (24, 15)], fill=(236, 226, 196))    # open book
    d.line([(24, 9), (24, 15)], fill=(150, 120, 90))
    for x, y in ((10, 3), (24, 0), (38, 4), (16, 6), (32, 7)):                    # floating runes
        d.line([(x, y), (x, y + 3)], fill=GLOW)
        d.point((x + 1, y + 1), fill=GLOW)
    save(img, "World/Objects", "altar")


def spell_icons():
    img, d = canvas()                                     # chispa
    for k in range(8):
        a = k * math.pi / 4
        d.line([(8, 8), (8 + math.cos(a) * 7, 8 + math.sin(a) * 7)], fill=(255, 170, 60) if k % 2 else (255, 230, 120))
    d.ellipse([5, 5, 11, 11], fill=(255, 240, 160))
    icon("spell_spark", img)
    img, d = canvas()                                     # escudo de luz
    d.ellipse([1, 1, 14, 14], fill=(255, 236, 150))
    d.ellipse([4, 4, 11, 11], fill=(255, 250, 220))
    icon("spell_shield", img)
    img, d = canvas()                                     # rayo
    d.polygon([(9, 0), (3, 9), (7, 9), (5, 15), (13, 5), (8, 5), (11, 0)], fill=(255, 230, 80))
    icon("spell_lightning", img)
    img, d = canvas()                                     # hielo
    for a in range(3):
        ang = a * math.pi / 3
        d.line([(8 - math.cos(ang) * 7, 8 - math.sin(ang) * 7), (8 + math.cos(ang) * 7, 8 + math.sin(ang) * 7)], fill=(170, 230, 255), width=2)
    d.ellipse([6, 6, 10, 10], fill=(230, 250, 255))
    icon("spell_ice", img)
    img, d = canvas()                                     # curación
    d.rectangle([6, 2, 9, 13], fill=(90, 210, 110)); d.rectangle([2, 6, 13, 9], fill=(90, 210, 110))
    icon("spell_heal", img)
    img, d = canvas()                                     # terremoto
    d.rectangle([1, 9, 14, 14], fill=(140, 100, 60))
    d.line([(3, 9), (6, 12), (8, 10), (11, 14)], fill=(60, 40, 30))
    for x in (3, 8, 12):
        d.rectangle([x, 4 + x % 3, x + 1, 6 + x % 3], fill=(160, 120, 80))
    icon("spell_quake", img)
    img, d = canvas()                                     # meteoro
    d.line([(2, 2), (9, 9)], fill=(255, 170, 60), width=3)
    d.ellipse([7, 7, 14, 14], fill=(200, 60, 40))
    d.ellipse([8, 8, 11, 11], fill=(255, 150, 60))
    icon("spell_meteor", img)


def enchant_icons():
    img, d = canvas()                                     # afilado: a blade with a glint
    d.line([(2, 14), (13, 3)], fill=(214, 222, 236), width=3)
    d.line([(10, 1), (12, 3), (14, 1)], fill=(255, 255, 255))
    icon("ench_sharp", img)
    img, d = canvas()                                     # fuego
    d.polygon([(8, 1), (13, 9), (11, 14), (5, 14), (3, 9)], fill=(240, 110, 40))
    d.polygon([(8, 6), (10, 11), (8, 14), (6, 11)], fill=(255, 220, 90))
    icon("ench_fire", img)
    img, d = canvas()                                     # hielo
    for a in range(3):
        ang = a * math.pi / 3 + 0.5
        d.line([(8 - math.cos(ang) * 6, 8 - math.sin(ang) * 6), (8 + math.cos(ang) * 6, 8 + math.sin(ang) * 6)], fill=(170, 230, 255), width=2)
    icon("ench_ice", img)
    img, d = canvas()                                     # protección
    d.polygon([(2, 2), (13, 2), (13, 8), (7, 14), (2, 8)], fill=(90, 140, 220))
    d.line([(7, 4), (7, 11)], fill=(230, 240, 255))
    icon("ench_protect", img)
    img, d = canvas()                                     # vampiro: a red drop
    d.polygon([(8, 1), (12, 8), (4, 8)], fill=(200, 30, 50))
    d.ellipse([4, 5, 12, 14], fill=(200, 30, 50))
    d.point((6, 9), fill=(255, 140, 150))
    icon("ench_vampire", img)
    img, d = canvas()                                     # suerte: a four-leaf clover
    for x, y in ((5, 3), (10, 3), (5, 8), (10, 8)):
        d.ellipse([x - 3, y - 2, x + 2, y + 3], fill=(80, 180, 80))
    d.line([(8, 10), (10, 15)], fill=(60, 130, 60))
    icon("ench_luck", img)


if __name__ == "__main__":
    tower()
    altar()
    spell_icons()
    enchant_icons()
    print("magic art written")
