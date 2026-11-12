# Art scripts

Run from this folder (`Tools/art`) with Python 3 (needs Pillow, numpy, scipy):

| Script | Draws |
|---|---|
| `generate_characters.py` | Player and villager layers (body, hair, hats, cloaks, beards...) |
| `generate_village.py` | The village: ground, buildings, trees, props, water, minimap, and the map files |
| `generate_icons.py` | Pets, equipment, empty-slot silhouettes, HUD icons |
| `generate_world.py` | Items, food, panels, buttons, cursor, speaker icons |
| `generate_art.py` | Title-screen background, clouds, food, HP/mana/coin, gender symbols |

Run `generate_art.py` and `generate_world.py` before the others (they make icons the others reuse).
