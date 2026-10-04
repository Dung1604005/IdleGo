The full pack: Buff FX has 102: loot beams in all 6 rarity colours, auras, stuns, stat icons,
loot drops and rarity frames, and every effect in each colour. It adds debuffs, level ups, glows
and shines too, with the same sheets and player.
https://heyheythere.itch.io/buff-fx

sheets/256/    every effect at 256 px (level ups, beams and drops 128x256, shines and stuns 256x128,
               stat icons 128x128)
sheets/128/    the same at half size
sheets/pixel/  pixel-art versions, drawn at 64 px (scale them up with nearest filtering)
previews/      an animated GIF of each effect
sheets/effects.json  for each sheet: frame size per set, frame count, columns, fps, whether it
               loops, and the anchor (0..1 across and down the frame: a character's feet for
               buffs, debuffs, auras, heals and level ups, the foot of a beam or drop, the centre
               of the rest)

Each sheet is a grid of frames, left to right then top to bottom, 8 to a row, on a transparent
background (straight alpha). Buffs, debuffs, auras, shields, stuns, stat icons, beams, power ups,
glows, sparkles and frames loop seamlessly; heals, level ups, drops, pickups and shines play once.
The six colours double as loot rarities: white common, green uncommon, blue rare, purple epic,
gold legendary, red mythic. A frame's border runs 0.74 of its cell across.

Godot 4.3+
    Copy addons/buff_fx/ into your project and sheets/ into addons/buff_fx/, then
        var aura := BuffFX.spawn(hero, "gold_buff", hero.global_position)
        aura.finish()   # fades it out when the buff ends
        BuffFX.spawn(self, "green_heal", hero.global_position)
    One-shots free themselves when done. Spawned as a child of a character or a Control, an
    effect follows it. BuffFX.default_set = "pixel" (or "128") switches every spawn to that
    set; set BuffFX.sheets_dir if sheets/ is elsewhere. Open addons/buff_fx/demo/demo.tscn to try them all.

Unity
    Import a sheet, set Texture Type to Sprite (2D and UI), Sprite Mode to Multiple, then in the
    Sprite Editor: Slice > Grid By Cell Size with the frame size from effects.json. Select the
    sprites in order and drag them into the scene to make the animation; set its sample rate to
    the fps, and Loop Time to match "loop". For the pixel set: Filter Mode Point, no compression.

GameMaker
    Create Sprite > Import > the sheet, then Convert to Frames with the frame size, 8 frames per
    row and the frame count from effects.json. Set the sprite's speed to the fps and its origin
    to the anchor.

Anything else
    Any engine or framework that cuts a grid sheet: use the frame size and count from
    effects.json.
