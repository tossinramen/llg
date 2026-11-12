"""Icons for the full weapon trees (melee, axes, ranged up to guns) and their projectiles.

Run:  python Tools/art/generate_weapons.py     (after generate_items.py)
"""
from PIL import Image

from generate_art import save
from generate_items import GOLD, GRIP, STEEL, WOOD, axe_icon, blade_icon, bow_icon, canvas, icon

DARK_STEEL = (90, 94, 108)


def gun_icon(name, length, stock=WOOD, barrel=DARK_STEEL, pistol=False, cylinder=False):
    """A gun lying flat, barrel pointing right."""
    img, d = canvas()
    if pistol:
        d.rectangle([5, 6, 5 + length, 8], fill=barrel)                      # barrel
        d.polygon([(3, 6), (7, 6), (7, 9), (5, 14), (2, 14), (3, 9)], fill=stock)   # grip
        if cylinder:
            d.rectangle([6, 5, 9, 9], fill=(130, 134, 150))
        d.line([(7, 9), (8, 11)], fill=barrel)                               # trigger
        d.point((4, 5), fill=barrel)
    else:
        d.polygon([(0, 9), (5, 8), (6, 11), (1, 13)], fill=stock)             # stock
        d.rectangle([5, 7, 5 + length, 9], fill=stock)
        d.rectangle([6, 6, 6 + length, 7], fill=barrel)
        d.line([(7, 10), (8, 12)], fill=barrel)
    icon(name, img)


def weapons():
    # melee
    img, d = canvas()
    d.line([(2, 14), (13, 2)], fill=(130, 90, 56), width=2)
    d.point((7, 8), fill=(90, 120, 60)); d.point((8, 6), fill=(90, 120, 60))
    icon("item_stick", img)
    blade_icon("item_kitchen_knife", 8, 4, 0, blade=(220, 226, 236), grip=(40, 36, 40))
    blade_icon("item_toledo", 12, 3, 6, blade=(236, 240, 250), hilt=(232, 190, 80))
    blade_icon("item_tizona", 13, 3, 7, blade=(250, 236, 200), hilt=(200, 40, 60), grip=(120, 30, 40))
    blade_icon("item_colada", 13, 3, 7, blade=(206, 230, 255), hilt=(60, 110, 220), grip=(40, 50, 110))
    axe_icon("item_doubleaxe", 5, double=True, head=(232, 190, 80))

    # ranged
    img, d = canvas()                                     # sling: two cords and a pouch with a stone
    d.line([(2, 2), (8, 11)], fill=(150, 110, 70))
    d.line([(14, 2), (9, 11)], fill=(150, 110, 70))
    d.ellipse([6, 9, 11, 14], fill=(120, 80, 50))
    d.ellipse([7, 9, 10, 12], fill=(160, 160, 170))
    icon("item_sling", img)
    bow_icon("item_heavycrossbow", 0, wood=(70, 56, 46), crossbow=True)
    gun_icon("item_arquebus", 9, stock=(130, 86, 50), barrel=(110, 100, 90))
    gun_icon("item_flintlock", 6, stock=(120, 80, 48), pistol=True)
    gun_icon("item_musket", 10, stock=(110, 72, 44))
    gun_icon("item_revolver", 6, stock=(70, 50, 40), pistol=True, cylinder=True)
    gun_icon("item_rifle", 10, stock=(60, 46, 40), barrel=(50, 52, 60))

    # projectiles: a stone for the sling, a lead ball for guns (6x6, drawn small in flight)
    for name, col in (("item_stone", (150, 150, 160)), ("item_bullet", (70, 72, 84))):
        img, d = canvas(6, 6)
        d.ellipse([1, 1, 4, 4], fill=col)
        d.point((2, 2), fill=(230, 230, 240))
        save(img, "Icons", name)


if __name__ == "__main__":
    weapons()
    print("weapon icons written")
