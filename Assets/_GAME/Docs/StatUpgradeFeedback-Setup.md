# Stat upgrade feedback and shared TextPopUp

## Ready-to-use assets

- `Prefabs/UI/StatProgressLine.prefab` already references `TextPopUp.prefab` under **Value Feedback**.
- `Resources/UI/Canvas-Combat.prefab` uses that same popup prefab for damage.
- `Resources/Data/StatIconData.asset` contains **Feedback Color** for each `StatType`.
- The old damage popup script and prefab GUIDs are preserved by the rename.

## Tune the stat upgrade effect

Select `StatProgressLine.prefab`, then expand **Value Feedback** on `StatProgressLineUI`.

| Setting | Default | Purpose |
|---|---:|---|
| Peak Scale | 1.28 | Maximum scale relative to the value text's original scale |
| Pop Duration | 0.2 seconds | Total grow/shrink and color return time |
| Ticker Duration | 0.15 seconds | Count from the displayed number to the new data value |
| Floating Duration | 0.6 seconds | Lifetime of the `+value` popup |
| Floating Distance | 40 | Upward travel in local UI units |
| Floating Offset | 22 | Starting distance above the number |
| Floating Font Scale | 0.65 | Popup font size relative to the current value text |

The value returns to its original Inspector color and scale. The configured prefab uses white.

## Stat color mapping

Open `StatIconData.asset` and adjust each entry's **Feedback Color**.
Both the value flash and the floating text use `DataManager.GetStatFeedbackColor(statType)`.

Default groups:

- Max Health / Life Steal: bright green.
- Damage / Critical Chance / Critical Damage / DA: gold.
- Run Speed / Attack Speed / Level / Experience: mint green.
- Armor / Dodge Chance: light blue.
- CD: purple.

## Refresh flow

`+` button -> DataManager -> character progress -> inventory save -> registered view refresh.
The stat line compares the new upgrade count and current value with its previous snapshot.
It only animates a confirmed increase. Opening the canvas, changing heroes, rejected upgrades
and ordinary refreshes do not replay the effect. Reset snaps to the new value and clears feedback.

Rows remain allocated while the canvas is open so refresh cannot interrupt their animations.
Closing the canvas or switching heroes kills tweens and returns owned popups to the pool.

## Reuse floating text elsewhere

Use `TextPopUpPool.Spawn(prefab, parent)` to obtain a pooled popup and reparent it correctly.
Call `Play(text, color, anchoredPosition)` for the burst animation, `PlayDamage(...)` for
normal/critical damage, or `PlayFloating(...)` for upward movement and fade with a chosen font.
Call the pool wrapper's `OnDespawn(parent)` when its owning view closes.
Do not collect the entire shared prefab pool to close one view.

## Verification still needed in Unity

- Open Hero Status and upgrade flat, percentage and critical damage stats.
- Click rapidly, switch heroes, reset points, and close/reopen during animation.
- Confirm damage and stat popups can run together with the correct colors, fonts and size.
