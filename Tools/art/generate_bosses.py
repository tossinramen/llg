"""The ten bosses of La Ciudadela del Sol, 48 x 48, drawn with more care: faces, armour, shading, props.
They stand in the citadel's halls and are shown big in battle.

Run:  python Tools/art/generate_bosses.py
"""
import math

from PIL import Image, ImageDraw

from generate_art import save
from generate_items import finish

S = 48


def new():
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def mirror(img):
    """Copies the left half onto the right, so symmetric monsters only need drawing once."""
    left = img.crop((0, 0, S // 2, S))
    img.paste(left.transpose(Image.FLIP_LEFT_RIGHT), (S // 2, 0))
    return ImageDraw.Draw(img)


def shade(c, f):
    return tuple(max(0, min(255, int(v * f))) for v in c)


def done(name, img, outline=(26, 20, 28)):
    save(finish(img, outline=outline, light=0.22, dark=0.72), "Bosses", name)


def guardian():
    """El Guardián de Piedra: a mossy stone golem with a glowing rune."""
    img, d = new()
    stone, glow = (138, 136, 146), (110, 230, 255)
    d.rectangle([14, 4, 23, 15], fill=shade(stone, 1.1))                 # head
    d.rectangle([12, 6, 13, 12], fill=stone)
    d.rectangle([9, 16, 23, 37], fill=stone)                              # torso
    d.rectangle([2, 15, 9, 22], fill=shade(stone, 1.05))                  # shoulder
    d.rectangle([1, 22, 7, 36], fill=shade(stone, 0.92))                  # arm
    d.rectangle([0, 34, 8, 41], fill=shade(stone, 1.0))                   # fist
    d.rectangle([11, 37, 20, 46], fill=shade(stone, 0.9))                 # leg
    d.rectangle([9, 44, 21, 47], fill=shade(stone, 0.8))                  # foot
    d.rectangle([16, 8, 19, 10], fill=glow)                               # eye
    d.line([(15, 13), (23, 13)], fill=shade(stone, 0.6))                  # mouth slit
    for x, y, x2, y2 in ((11, 20, 14, 24), (18, 30, 22, 33), (3, 26, 5, 30), (13, 40, 16, 43)):
        d.line([(x, y), (x2, y2)], fill=shade(stone, 0.6))                # cracks
    for x, y in ((3, 15), (5, 14), (10, 16), (15, 4), (18, 3), (2, 16)):
        d.rectangle([x, y, x + 2, y + 1], fill=(92, 150, 78))             # moss
    d = mirror(img)
    d.line([(24, 20), (24, 33)], fill=glow, width=2)                      # the rune on its chest
    d.line([(24, 21), (29, 25), (24, 28)], fill=glow)
    d.point((21, 24), fill=glow); d.point((27, 31), fill=glow)
    done("guardian", img)


def spider():
    """La Reina Araña: a crowned spider with eight eyes and jointed legs."""
    img, d = new()
    body, dark = (88, 52, 110), (52, 30, 70)
    for i, (y0, bend) in enumerate(((18, 8), (22, 12), (27, 16), (31, 18))):      # four legs each side
        knee = (9 - i, y0 - 6 + i * 3)
        d.line([(18, y0 + 2), knee], fill=dark, width=2)
        d.line([knee, (2 + i, y0 + bend)], fill=dark, width=2)
    d.ellipse([12, 20, 35, 44], fill=body)                                 # abdomen (drawn across, then mirrored)
    d.ellipse([15, 9, 32, 25], fill=shade(body, 1.15))                     # head
    for x, y in ((18, 14), (21, 17), (17, 19)):
        d.rectangle([x, y, x + 1, y + 1], fill=(240, 60, 60))               # eyes
    d.rectangle([20, 12, 22, 14], fill=(255, 120, 100))
    d.polygon([(20, 23), (22, 28), (23, 23)], fill=(236, 230, 220))         # fang
    d.polygon([(17, 9), (17, 4), (20, 7), (23, 3), (23, 9)], fill=(240, 196, 70))   # crown
    d.point((23, 5), fill=(230, 60, 80))
    d = mirror(img)
    d.polygon([(24, 28), (20, 33), (24, 40), (28, 33)], fill=(210, 60, 70))  # red hourglass mark
    d.polygon([(24, 31), (22, 33), (24, 36), (26, 33)], fill=(250, 150, 90))
    done("spider", img)


def troll():
    """El Rey Trol: a big green troll king with tusks, a crown and a club."""
    img, d = new()
    skin, belly = (104, 150, 84), (150, 186, 120)
    d.ellipse([8, 16, 30, 44], fill=skin)                                   # body
    d.ellipse([14, 6, 30, 22], fill=shade(skin, 1.1))                       # head
    d.ellipse([8, 10, 14, 17], fill=shade(skin, 1.0))                       # ear
    d.ellipse([3, 20, 12, 34], fill=shade(skin, 0.95))                      # arm
    d.ellipse([2, 31, 10, 38], fill=shade(skin, 1.05))                      # hand
    d.rectangle([12, 40, 21, 46], fill=shade(skin, 0.9))                    # leg
    d.rectangle([10, 44, 22, 47], fill=shade(skin, 0.8))
    d.ellipse([15, 24, 32, 42], fill=belly)
    d.rectangle([17, 12, 19, 14], fill=(250, 240, 200)); d.point((18, 13), fill=(40, 30, 30))   # eye
    d.line([(16, 10), (20, 11)], fill=shade(skin, 0.6))                     # brow
    d.ellipse([21, 13, 26, 19], fill=shade(skin, 0.85))                     # big nose
    d.polygon([(19, 21), (20, 16), (22, 21)], fill=(250, 244, 226))         # tusk
    d.line([(18, 21), (23, 21)], fill=(60, 40, 40))
    d.polygon([(15, 7), (15, 2), (18, 5), (21, 1), (23, 5), (23, 7)], fill=(240, 196, 70))
    d.polygon([(13, 38), (23, 38), (23, 44), (18, 41), (13, 44)], fill=(130, 90, 60))           # loincloth
    d = mirror(img)
    d.line([(41, 34), (45, 12)], fill=(120, 84, 54), width=3)              # club in its right hand
    d.ellipse([41, 5, 47, 15], fill=(140, 100, 64))
    for x, y in ((42, 7), (46, 10), (43, 12)):
        d.point((x, y), fill=(220, 220, 226))
    d.point((24, 4), fill=(230, 60, 80))
    done("troll", img)


def sorcerer():
    """El Brujo de la Niebla: a hooded figure in swirling fog, with a glowing staff."""
    img, d = new()
    robe = (86, 70, 150)
    for x, y, r in ((8, 42, 7), (18, 44, 6), (4, 38, 4)):
        d.ellipse([x - r, y - r // 2, x + r, y + r // 2], fill=(196, 206, 220, 150))   # fog
    d.polygon([(24, 4), (15, 12), (11, 30), (8, 44), (24, 44)], fill=robe)              # robe
    d.polygon([(24, 4), (16, 12), (16, 20), (24, 22)], fill=shade(robe, 0.8))           # hood shadow
    d.polygon([(24, 9), (18, 13), (18, 19), (24, 21)], fill=(20, 16, 34))               # the dark inside the hood
    d.rectangle([20, 14, 21, 15], fill=(160, 255, 200))                                  # eye
    d.line([(13, 26), (17, 44)], fill=shade(robe, 0.75)); d.line([(20, 24), (21, 44)], fill=shade(robe, 0.75))
    d.polygon([(11, 24), (5, 30), (8, 33), (13, 29)], fill=shade(robe, 1.1))            # sleeve
    d.rectangle([4, 30, 7, 33], fill=(206, 190, 170))                                   # hand
    d = mirror(img)
    d.line([(42, 8), (42, 44)], fill=(110, 80, 56), width=2)                            # staff
    d.ellipse([38, 1, 46, 9], fill=(150, 110, 240))
    d.ellipse([40, 3, 43, 6], fill=(230, 210, 255))
    d.rectangle([20, 24, 27, 26], fill=(240, 196, 70))                                  # clasp
    done("sorcerer", img)


def iceking():
    """El Rey de Hielo: a tall king of ice with a crystal crown, a white beard and a frost sword."""
    img, d = new()
    ice, deep = (150, 210, 240), (90, 150, 210)
    d.polygon([(24, 16), (12, 22), (9, 44), (24, 44)], fill=deep)                       # cape/robe
    d.polygon([(24, 18), (15, 22), (14, 40), (24, 42)], fill=ice)                       # armour
    d.ellipse([16, 7, 31, 21], fill=(214, 236, 250))                                    # face
    d.polygon([(17, 16), (24, 17), (24, 30), (19, 26)], fill=(244, 250, 255))           # beard
    d.rectangle([19, 12, 21, 13], fill=(40, 90, 170))                                   # eye
    d.polygon([(16, 9), (14, 1), (18, 6), (20, 0), (22, 6), (24, 1), (24, 9)], fill=(190, 236, 255))   # ice crown
    d.polygon([(12, 22), (5, 26), (8, 31), (14, 27)], fill=shade(ice, 0.9))             # shoulder / arm
    d.rectangle([5, 29, 9, 33], fill=(214, 236, 250))
    for y in (26, 32, 38):
        d.line([(16, y), (23, y + 1)], fill=shade(ice, 0.75))                          # armour plates
    d.rectangle([12, 43, 23, 46], fill=shade(deep, 0.8))
    d = mirror(img)
    d.polygon([(40, 4), (42, 4), (43, 30), (39, 30)], fill=(220, 244, 255))             # frost sword
    d.rectangle([37, 30, 45, 31], fill=(240, 196, 70)); d.rectangle([40, 32, 42, 36], fill=(90, 70, 110))
    d.point((24, 3), fill=(110, 230, 255))
    done("iceking", img, outline=(30, 44, 80))


def serpent():
    """La Serpiente Emplumada: a coiled green serpent with a crest of bright feathers and wings."""
    img, d = new()
    g, belly = (60, 160, 90), (220, 210, 110)
    pts = [(10, 42), (30, 44), (40, 38), (38, 30), (22, 28), (12, 24), (14, 15), (26, 12)]
    for i in range(len(pts) - 1):
        d.line([pts[i], pts[i + 1]], fill=g, width=8)
    for p in pts:
        d.ellipse([p[0] - 4, p[1] - 4, p[0] + 4, p[1] + 4], fill=g)
    for i in range(len(pts) - 1):                                                       # belly stripe and scales
        (x0, y0), (x1, y1) = pts[i], pts[i + 1]
        d.line([(x0, y0 + 2), (x1, y1 + 2)], fill=belly, width=2)
        d.point(((x0 + x1) // 2, (y0 + y1) // 2 - 2), fill=shade(g, 0.6))
    d.ellipse([22, 5, 38, 18], fill=shade(g, 1.1))                                      # head
    d.polygon([(36, 10), (44, 12), (36, 16)], fill=shade(g, 1.05))                      # snout
    d.rectangle([31, 9, 33, 11], fill=(255, 230, 90)); d.point((32, 10), fill=(30, 20, 20))
    d.line([(43, 13), (47, 11)], fill=(220, 50, 60)); d.line([(43, 13), (47, 15)], fill=(220, 50, 60))   # tongue
    for i, c in enumerate(((220, 60, 60), (250, 190, 60), (60, 130, 230), (220, 60, 60), (250, 190, 60))):
        a = math.radians(200 + i * 22)
        d.polygon([(27, 8), (27 + math.cos(a) * 12, 8 + math.sin(a) * 12), (29, 6)], fill=c)             # feather crest
    for i, c in enumerate(((60, 130, 230), (250, 190, 60), (220, 60, 60))):
        d.polygon([(14, 22), (2 + i * 2, 8 + i * 5), (10, 26)], fill=c)                 # wing feathers
    done("serpent", img)


def lava():
    """El Gigante de Lava: a hulking giant of black rock with glowing cracks and a flaming head."""
    img, d = new()
    rock, hot, bright = (64, 50, 48), (240, 110, 30), (255, 220, 90)
    d.polygon([(24, 10), (12, 12), (5, 20), (6, 34), (10, 46), (24, 46)], fill=rock)    # body
    d.rectangle([1, 20, 7, 38], fill=shade(rock, 1.15))                                 # arm
    d.rectangle([0, 36, 8, 43], fill=shade(rock, 1.3))                                  # fist
    for (x0, y0), (x1, y1) in (((14, 16), (10, 26)), ((10, 26), (15, 34)), ((15, 34), (12, 44)), ((18, 22), (23, 30)), ((3, 24), (5, 34))):
        d.line([(x0, y0), (x1, y1)], fill=hot, width=2)                                 # lava cracks
        d.line([(x0, y0), (x1, y1)], fill=bright)
    d.rectangle([17, 15, 20, 17], fill=bright)                                          # eye
    d.polygon([(18, 22), (24, 21), (24, 25), (20, 25)], fill=hot)                       # mouth glow
    for x, h in ((14, 6), (18, 10), (22, 8)):
        d.polygon([(x - 2, 11), (x, 11 - h), (x + 2, 11)], fill=hot)                    # flames on top
        d.polygon([(x - 1, 11), (x, 13 - h), (x + 1, 11)], fill=bright)
    d = mirror(img)
    done("lava", img, outline=(30, 16, 14))


def witch():
    """La Bruja del Espejo: a witch in a pointed hat holding a silver mirror."""
    img, d = new()
    dress, hat = (120, 70, 150), (60, 44, 86)
    d.polygon([(24, 20), (14, 24), (8, 45), (24, 45)], fill=dress)                      # dress
    d.polygon([(24, 22), (18, 30), (20, 45), (24, 45)], fill=shade(dress, 1.2))
    d.ellipse([16, 12, 31, 26], fill=(246, 220, 200))                                   # face
    d.polygon([(15, 14), (12, 26), (16, 28), (18, 18)], fill=(230, 230, 240))           # silver hair
    d.rectangle([19, 17, 21, 18], fill=(90, 50, 130))                                   # eye
    d.line([(21, 22), (24, 23)], fill=(170, 60, 80))                                    # smile
    d.polygon([(8, 13), (24, 11), (24, 14), (10, 15)], fill=hat)                        # brim
    d.polygon([(15, 12), (22, 0), (24, 1), (24, 12)], fill=hat)                         # cone
    d.rectangle([16, 9, 23, 11], fill=(240, 196, 70))                                   # hat band
    d.polygon([(14, 24), (6, 30), (9, 34), (15, 29)], fill=shade(dress, 0.85))          # sleeve
    d.rectangle([5, 31, 8, 34], fill=(246, 220, 200))
    d = mirror(img)
    d.ellipse([36, 22, 47, 38], fill=(240, 196, 70))                                    # the mirror, in her left hand
    d.ellipse([38, 24, 45, 36], fill=(190, 226, 250))
    d.line([(40, 27), (43, 31)], fill=(255, 255, 255)); d.rectangle([40, 38, 42, 43], fill=(200, 160, 60))
    done("witch", img)


def knight():
    """El Caballero Sin Nombre: black armour, a red glow behind the visor, a plume, sword and shield."""
    img, d = new()
    steel, trim = (70, 72, 86), (150, 152, 170)
    d.rectangle([16, 4, 23, 16], fill=shade(steel, 1.15))                               # helm
    d.polygon([(16, 4), (24, 2), (24, 5), (17, 6)], fill=trim)
    d.rectangle([18, 9, 23, 10], fill=(240, 60, 50))                                    # visor glow
    d.line([(19, 12), (19, 15)], fill=shade(steel, 0.6)); d.line([(22, 12), (22, 15)], fill=shade(steel, 0.6))
    d.polygon([(24, 16), (12, 18), (10, 24), (13, 36), (24, 38)], fill=steel)           # breastplate
    d.ellipse([7, 16, 16, 24], fill=trim)                                               # pauldron
    d.rectangle([8, 23, 12, 34], fill=shade(steel, 0.9))                                # arm
    d.rectangle([7, 33, 12, 37], fill=trim)                                             # gauntlet
    d.line([(14, 26), (24, 27)], fill=shade(steel, 0.6)); d.line([(15, 31), (24, 32)], fill=shade(steel, 0.6))
    d.rectangle([14, 37, 21, 45], fill=shade(steel, 0.85))                              # leg
    d.rectangle([12, 44, 22, 47], fill=trim)
    d.polygon([(24, 2), (20, -1), (24, -3)], fill=(190, 40, 50))
    d = mirror(img)
    d.polygon([(22, 3), (30, 0), (32, 5), (26, 5)], fill=(190, 40, 50))                 # plume
    d.rectangle([40, 6, 42, 34], fill=(214, 220, 232))                                  # sword (right hand)
    d.polygon([(40, 6), (41, 2), (42, 6)], fill=(240, 244, 250))
    d.rectangle([37, 34, 45, 35], fill=(240, 196, 70)); d.rectangle([40, 36, 42, 40], fill=(90, 60, 50))
    d.polygon([(1, 22), (13, 22), (13, 34), (7, 41), (1, 34)], fill=(60, 60, 78))       # shield (left hand)
    d.polygon([(3, 24), (11, 24), (11, 33), (7, 38), (3, 33)], fill=(120, 30, 40))
    d.line([(7, 25), (7, 36)], fill=trim); d.line([(4, 29), (10, 29)], fill=trim)
    done("knight", img)


def demon():
    """El Rey Demonio: horns, bat wings, a burning crown and a wicked grin."""
    img, d = new()
    red, dark = (190, 50, 50), (110, 24, 40)
    d.polygon([(14, 18), (0, 8), (3, 20), (0, 26), (5, 28), (3, 36), (13, 30)], fill=dark)      # wing
    d.line([(14, 18), (2, 12)], fill=shade(dark, 1.5)); d.line([(13, 24), (2, 25)], fill=shade(dark, 1.5))
    d.polygon([(24, 18), (13, 20), (10, 32), (13, 45), (24, 45)], fill=red)                    # body
    d.ellipse([15, 7, 32, 23], fill=shade(red, 1.1))                                           # head
    d.polygon([(16, 10), (11, 1), (14, 2), (20, 8)], fill=(240, 230, 210))                     # horn
    d.polygon([(17, 14), (22, 15), (21, 17), (18, 16)], fill=(255, 230, 90))                   # angry eye
    d.point((20, 16), fill=(40, 10, 10))
    d.polygon([(19, 19), (24, 21), (24, 22), (19, 21)], fill=(40, 10, 16))                     # grin
    d.point((21, 21), fill=(250, 250, 240)); d.point((23, 22), fill=(250, 250, 240))
    d.polygon([(13, 22), (6, 30), (9, 34), (14, 29)], fill=shade(red, 0.9))                    # arm
    d.polygon([(5, 30), (3, 36), (6, 34), (7, 38), (9, 34)], fill=(40, 20, 30))                # claws
    d.polygon([(16, 30), (24, 28), (24, 40), (18, 38)], fill=shade(red, 1.25))                 # chest
    d.rectangle([14, 44, 22, 47], fill=dark)
    d.polygon([(19, 7), (19, 3), (21, 5), (23, 1), (24, 4), (24, 7)], fill=(240, 196, 70))     # crown
    d = mirror(img)
    for x, h in ((20, 5), (24, 7), (28, 5)):
        d.polygon([(x - 1, 2), (x, 2 - h), (x + 1, 2)], fill=(255, 150, 40))                   # crown fire
    d.point((24, 3), fill=(110, 230, 255))
    done("demon", img, outline=(40, 10, 20))


if __name__ == "__main__":
    for f in (guardian, spider, troll, sorcerer, iceking, serpent, lava, witch, knight, demon):
        f()
    print("bosses written")
