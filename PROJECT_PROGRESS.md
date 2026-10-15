# IdleGo project context and progress

> Last code scan: 2026-10-15
> Purpose: this is the starting point for future development. Read this file before inspecting feature code. Update it after every project task according to the root `AGENTS.md`.

## 1. Project snapshot

IdleGo is a Unity 2D idle combat game. The current playable foundation includes automatic combat, multi-character teams, Addressable level data, pooled enemies, inventory and per-character equipment, enchant materials, queued chests, item information UI, and character/global progression.

Scene status:

- `Assets/_GAME/Scenes/SampleScene.unity` is the only enabled scene in `EditorBuildSettings`.
- `Assets/m.unity` currently contains the populated gameplay managers but is not in `EditorBuildSettings`; treat it as the active development scene until build-scene setup is finalized.

Main bootstrap:

- `Assets/_GAME/Scripts/Manager/GameManager.cs`
- Startup order in `Start()`: `PlayerManager.OnInit()` -> `ChestManager.OnInit()` -> `LootManager.OnInit()` -> `MapManager.OnInit()` -> open Combat, Inventory/Hero and Navigation canvases -> `LevelManager.OnInit()`.

Important packages:

- Addressables `4.0.1` for loading `LevelDataSO`.
- UGUI and TextMeshPro for UI.
- DOTween is used by chest carousel UI through the project integration. Prefer tweens over per-frame UI animation.

Current working-tree note at the 2026-10-14 scan:

- `Assets/_GAME/Resources/UI/Canvas-Inventory_Hero.prefab` already had local user changes. Do not overwrite or regenerate it without inspecting those changes.

## 2. Permanent project conventions

- Use `OnInit()`/`OnDespawn()` for new gameplay lifecycle code. Do not introduce new `Awake()` methods. `UIManager` and `UICanvas` still contain legacy `Awake()` code; leave it alone unless a task explicitly migrates it.
- Avoid `GetComponent` and all `Find*ByType`/scene-search APIs. Prefer `[SerializeField]`, explicit owner injection, `DataManager`, `PlayerManager`, and `EnemyManager`.
- Do not use C# events unless explicitly requested.
- One C# file must not exceed 250 lines. One method must not exceed 45 lines.
- All pooled objects inherit `GameUnit`; use `SimplePool.Spawn`, `Despawn`, `PreLoad`, and `Release`.
- Use `TextMeshProUGUI` for all text shown in the game.
- All visible game copy is English.
- Important game-state transitions and non-obvious formulas need a concise comment.
- Do not modify unrelated classes or user-edited spawn/scene/prefab logic.
- Prefer serialized references over implicit lookups.

## 3. Ownership and source of truth

| Concern | Owner/source of truth | Access point |
|---|---|---|
| Runtime service references | `DataManager` | `DataManager.Ins` |
| All characters and unlock state | `PlayerRoster` inside `PlayerManager` | `DataManager.Ins.PlayerData` methods |
| Active team, order, unlocked team slots | `PlayerTeam` inside `PlayerManager` | `DataManager`/`PlayerManager` |
| Inventory slots and capacity | `InventoryStorage` inside `Inventory` | `DataManager.Ins.InventoryData` |
| Per-character equipped items | each `Player.CharacterEquipment` | mutations through `DataManager` -> `PlayerManager` -> `Inventory` |
| Per-character base/current stats | `CharacterStat` | queries through `DataManager` for UI |
| Per-character stat/skill upgrades | `CharacterStatProgress` | mutations through `DataManager` |
| Global level, EXP and buffs | `PlayerProgression.GlobalStats` | `DataManager.Progression` |
| Equipped combat skill runtime states | `CharacterCombat.combatSkillStates` | `CharacterCombat` |
| Selected difficulty/map, map clear progress and map instance | `MapManager` + `MapProgressState` | system via `MapManager`; UI via `DataManager` |
| Loaded level and current wave | `LevelInfo` inside `LevelManager` | `LevelManager` |
| Active enemies and enemy-to-data mapping | `EnemyManager` | `EnemyManager.Ins` or `DataManager` target queries |
| Loot rules for current level | `LootManager` + `LevelLootController` | `LootManager.Ins` |
| Chest queues/settings/open state | `ChestManager` + `ChestState` | `DataManager.Ins.ChestData` |
| Current UI item/hero selection | `DataManager` | `DataManager.Ins` |
| Rarity colors/backgrounds, buff visuals, stat icons | serialized SO references in `DataManager` | `DataManager` getter methods |
| Difficulty border/name backgrounds | `DifficultyMapVisualSO` | `DataManager.GetDifficultyBorder/GetDifficultyNameBackground` |

`DataManager` is a gateway and coordinator. It does not replace the domain owners above. UI code should ask it for data and send commands through it.

## 4. Required data and UI flow

Use this order for every mutable feature:

```text
UI input
  -> DataManager command
  -> owning manager/data class validates and mutates runtime data
  -> owning class serializes JSON to PlayerPrefs
  -> owning class asks registered view(s) to refresh
  -> view reads the new state again through DataManager
```

Rules:

- A UI refresh method takes no domain-data parameter. Example: `RefreshInventory()`, not `RefreshInventory(Inventory source)`.
- UI must not update its own final state immediately after a mutation. The data owner completes save/refresh.
- Views register with their data owner through interfaces such as `IInventoryView` and `IChestView`.
- A failed mutation does not trigger a fake UI state change.
- Inventory currently refreshes registered views even when saving fails; `LastSaveSucceeded` records the result.

## 5. Save architecture

### Player, inventory, equipment, skills and progression

- PlayerPrefs key: `PLAYER_INVENTORY` by default.
- Serializer: Unity `JsonUtility` in `InventorySaveSystem`.
- Save DTO root: `InventorySaveData`.
- Saved together in the same JSON:
  - inventory capacity and unequipped inventory slots;
  - global stats;
  - unlocked character IDs and team order;
  - every character's current base/progress stats;
  - every character's equipped equipment instances;
  - equipped skills and their runtime levels;
  - available stat points, stat upgrade counts and learned skill levels;
  - equipment instance ID, quality roll and enhancement slot contents.
- Equipped equipment is not stored in `InventoryStorage` and does not consume inventory capacity. It is persisted separately under each character inside the same save document.
- Empty enhancement slots save as `isEmpty = true` with null stat/source rarity. They must not become `MAX_HEALTH = 0` or inherit the equipment rarity.
- `ItemSO.ItemId` and `CombatSkill.SkillId` must remain stable because saves restore references through IDs and `ItemDatabaseSO`.
- `ToolSave` exposes `Delete Inventory And Equipment Data` through a context menu and refuses to delete during Play Mode.

### Chest data

- PlayerPrefs key: `PLAYER_CHESTS` by default.
- Saved separately with `ChestSaveSystem`/`ChestSaveMapper`.
- Saves each queue, storage limit, auto-open unlock/enabled flags, interval, remaining timer and rolled equipment reward data.

### Map, difficulty and level progress

- PlayerPrefs key: `PLAYER_MAP_PROGRESS` by default.
- Serializer: Unity `JsonUtility` in `MapProgressSaveSystem`.
- Saved separately from inventory and chests:
  - current `DifficultType`;
  - current `MapType`;
  - current zero-based level index;
  - highest cleared level index for each difficulty/map pair.
- Sequential progress is derived from highest-cleared indices: level N requires N-1, a map requires the previous map to be complete, and a difficulty requires every configured map in the previous difficulty to be complete.

## 6. Core runtime flows

### 6.1 Automatic combat and attack animation

Source files:

- `Assets/_GAME/Scripts/Combat/AutoCombat.cs`
- `Assets/_GAME/Scripts/Combat/CharacterCombat*.cs`
- `Assets/_GAME/Scripts/Combat/CombatSkillState.cs`
- `Assets/_GAME/Scripts/Character/CharacterMovement.cs`
- `Assets/_GAME/Scripts/Character/CharacterAnimationEvent.cs`

Flow:

```text
Character.Update
  -> AutoCombat.UpdateAutoCombat(CharacterCombat)
  -> tick cooldown states
  -> get nearest opponent through DataManager/manager
  -> select ready skill with greatest LastReadyAt
  -> MoveToward target until the selected skill/basic attack range
  -> stop movement
  -> CharacterCombat.StartAttack (stores target + skill and starts animation)
  -> Animation Event: CharacterAnimationEvent.ExecuteAttack
  -> CharacterCombat.ExecuteAttack -> skill/AttackType deals damage
  -> Animation Event: CharacterAnimationEvent.EndAttack
  -> CharacterCombat.EndAttack -> start selected skill cooldown and unlock combat
```

Key behavior:

- `AutoCombat` contains execution logic and no character reference; the caller passes `CharacterCombat`.
- `CharacterCombat` owns up to two `CombatSkillState` objects.
- Skill priority is the ready skill that became ready most recently (`LastReadyAt` highest).
- `nextActionAt` is the shared attack cadence gate derived from attack speed and combat delay. It is separate from skill cooldown.
- Damage must not happen when the animation starts. It happens only at the `ExecuteAttack` Animation Event.
- Skill cooldown starts only at the `EndAttack` Animation Event.
- `hasExecutedAttack` prevents one animation from applying damage twice.
- Movement uses `Vector3.MoveTowards`; attacking immediately stops movement.
- Players target nearest active enemy through `EnemyManager`; enemies target nearest active team player through `PlayerManager` via `DataManager`.
- Player target selection raises the selected enemy's configured SpriteRenderer sorting orders, including its health bar, and restores them when target changes.
- Single-target and AOE behavior are separate `AttackType` classes. AOE uses overlap-circle logic.
- On a player killing blow, `CharacterCombat` calls `DataManager.RegisterEnemyKill` before the enemy death animation despawns it.

### 6.2 Damage, health and progression reward

- `CharacterStat` stores the indexed current stat list and current health separately.
- Equipment and progress bonuses use `StatModifier`; base/progress stat lists do not permanently absorb equipment bonuses.
- Damage includes character damage, skill multiplier, damage amplification, critical multiplier and player global damage multiplier.
- Armor reduction is centralized in `CombatFormula` and is percentage-based.
- Dodge is resolved in `CharacterHealthController`.
- Life steal heals from actual health lost.
- `CharacterHealthBar` changes SpriteRenderer local scale and smooths the visual without a C# event.
- Floating damage and stat feedback use `TextPopUp : GameUnit` and the shared `SimplePool` prefab. `TextPopUpPool` reparents reused instances and only clears popups owned by its view.
- Enemy EXP comes from `EnemyDataSO.Experience`.
- The player that lands the killing blow receives EXP. Global progression also receives reward EXP after the global EXP buff is applied.
- Each character level gained grants one stat point.

### 6.3 Level, map and wave

Source files:

- `Assets/_GAME/Scripts/Manager/MapManager.cs`
- `Assets/_GAME/Scripts/Manager/MapUnlockRules.cs`
- `Assets/_GAME/Scripts/Manager/MapCatalogValidator.cs`
- `Assets/_GAME/Scripts/Manager/MapProgressState.cs`
- `Assets/_GAME/Scripts/Manager/MapProgressSave*.cs`
- `Assets/_GAME/Scripts/Manager/LevelManager*.cs`
- `Assets/_GAME/Scripts/Manager/LevelInfo.cs`
- `Assets/_GAME/Scripts/Manager/EnemyManager*.cs`
- `Assets/_GAME/Scripts/UI/CanvasLevelSelect.cs`
- `Assets/_GAME/Scripts/UI/LevelSelect/*.cs`
- `Assets/_GAME/SO/LevelMap/LevelDataSO.cs`
- `Assets/_GAME/SO/LevelMap/MapDataSO.cs`
- `Assets/_GAME/SO/LevelMap/DifficultyMapVisualSO.cs`

Flow:

```text
GameManager
  -> MapManager.OnInit
  -> load PLAYER_MAP_PROGRESS
  -> normalize saved difficulty/map/level against current unlock rules

Difficulty selection
  -> DataManager.SelectDifficulty
  -> MapManager validates difficulty unlock and saves selection
  -> MapManager refreshes registered map-progress views

Map selection
  -> DataManager.SelectMap
  -> MapManager validates map unlock and saves selection
  -> MapManager refreshes CanvasLevelSelect from saved runtime state
  -> current gameplay map prefab remains until a level is selected successfully

Level selection
  -> DataManager.SelectLevel(zero-based levelIndex)
  -> LevelManager validates the level through MapManager
  -> build Addressable key: "Level_" + mapNumber + "-" + levelNumber
  -> load only that LevelDataSO
  -> require global level >= LevelDataSO.LevelRequired
  -> MapManager keeps the current MapController when MapType is unchanged
  -> MapManager destroys/spawns the map prefab when MapType changed
  -> MapManager commits and saves the selected level
  -> LevelInfo stores only the loaded level and wave
  -> LootManager receives current LevelDataSO
  -> LevelManager.OnLevelStart
  -> clear the old result overlay
  -> EnemyManager.ResetForLevelStart stops pending spawn and collects old enemies
  -> move every active team member to LevelDataSO.TeamSpawnPosition
  -> restore every active team member for a clean attempt
  -> StartNextWave
  -> EnemyManager.SpawnWave using SimplePool and a small spawn interval
  -> LevelManager polls alive/spawning state
  -> wait waveTransitionDelay and start the next wave

All active team members dead
  -> LevelManager.OnLevelFail(displayDuration)
  -> CanvasCombat shows black overlay + "You Failed"
  -> LevelManager.Invoke(OnLevelStart, displayDuration)

No next wave remains
  -> LevelManager.OnLevelComplete(displayDuration)
  -> MapManager records the cleared difficulty/map/level and saves
  -> CanvasCombat shows black overlay + "Level Completed"
  -> LevelManager.Invoke(OnNextLevel, displayDuration)
```

Level attempt reset:

- `OnLevelStart` stops an unfinished enemy spawn coroutine, collects all active level enemies, clears enemy counters/loot tracking, resets the wave index and starts wave zero again. The current level pools remain allocated for reuse.
- Active team members move to the world-space `LevelDataSO.TeamSpawnPosition`, recover to maximum health, stop movement/attacks, return to idle and reset equipped skill cooldowns.
- Temporary stat modifier sources implement `ITemporaryStatModifierSource`; only those modifiers are removed. Equipment and permanent progression modifiers remain.
- `CanvasCombat` owns only result visuals. It never starts/restarts a level or mutates game data.
- The result text scales from zero with DOTween `Ease.OutBack`; the result overlay is moved to the top UI sibling while visible.
- Result durations are serialized on `LevelManager`; result text scale duration is serialized on `CanvasCombat`.
- `MapManager` owns the ordered `MapDataSO` catalog and the active map instance. `LevelManager` owns Addressable level loading, waves and level results.
- `CanvasLevelSelect` registers as `IMapProgressView`; UI commands go through `DataManager`, while `MapManager` saves before refreshing the canvas.
- The level-select canvas has two horizontal map-selector rows. Map buttons are distributed between the rows in catalog order, and the selected map's pooled `MapSelectUI` comes from `MapDataSO.MiniMapPrefab`.
- Difficulty uses a custom popup with five ordered options. The selected difficulty changes `MapSelectUI` border/name background through `DifficultyMapVisualSO`.
- Each `LevelSelectButtonUI` stores a one-based level number. It swaps normal/selected/locked sprites directly and starts `DataManager.SelectLevel`; Unity Button's Selected Sprite is not used.
- Selecting another map or difficulty does not replace the combat map immediately. The prefab changes only after the requested level loads and passes validation.
- `LevelManager` snapshots the difficulty/map of the loaded battle so a late selection change cannot credit a clear to the wrong progress entry.

Addressable convention:

- Default prefix is `Level_`.
- Map enum index and zero-based level index are converted to one-based display numbers.
- Example: `MapType.FOREST = 0`, level index `0` -> address `Level_1-1`.
- The Addressable address must match this generated key. The asset filename itself is not used by loading once the address is set.
- Difficulty currently shares the same `LevelDataSO` address; it controls selection, saving and unlock progression. Difficulty-specific enemy scaling remains a separate future step.
- Only `LevelDataSO` needs to be Addressable for this flow. Referenced wave/enemy/equipment-source assets are dependencies.

Enemy spawning:

- `WaveData` contains groups of `EnemyDataSO + amount` and one spawn position.
- The first enemy spawns immediately; later enemies wait `spawnInterval`.
- Enemy instances use `SimplePool`; the manager owns spawn and despawn.
- The manager releases level prefab pools before the Addressable level handle is released.
- `totalAliveEnemy` increases only after a successful spawn.

### 6.4 Enemy death, loot roll and chest queue

```text
Player killing blow
  -> award character/global EXP
Enemy death animation event
  -> Enemy.Despawn
  -> EnemyManager.DespawnEnemy
  -> LootManager.OnEnemyDefeated(enemyData)
  -> map EnemyType to matching ChestType
  -> roll chest drop rate
  -> roll EquipmentDataSO from current LevelDataSO source
  -> create Equipment instance
  -> roll and store QualityRoll
  -> ChestManager.TryEnqueue
  -> if queue is full, discard the already-rolled reward
  -> otherwise save chest JSON and refresh chest UI
```

- Normal enemies only roll normal chests, elite enemies elite chests, and bosses boss chests.
- `LevelDataSO` has Normal, Elite and Boss `EquipmentSourceSO` references.
- `EquipmentSourceSO` entries use relative weights and enforce their common source level.
- Main/sub stats remain authored in `EquipmentDataSO`; only equipment choice and quality roll are randomized.

### 6.5 Opening a chest

```text
ChestSlotUI click or auto timer
  -> DataManager.ChestData.TryOpenChest(type)
  -> validate ChestState.CanOpen and inventory capacity
  -> set IsOpening to block repeated opens in code
  -> Inventory.AddItem(reward)
  -> inventory save + inventory UI refresh
  -> remove queued chest, chest save + chest UI refresh
  -> ChestSlotUI plays opening animation
  -> optional OnRevealReward Animation Event shows reward icon
  -> OnOpenAnimationFinished Animation Event
  -> ChestManager.CompleteOpenAnimation unlocks this chest type
```

- `ChestState.CanOpen = Count > 0 && !IsInventoryFull && !IsOpening`.
- There is no opening blocker GameObject. Button interaction and manager validation enforce the lock.
- `fullIcon` remains visible when the front reward cannot fit in inventory.
- Chest UI lives under `CanvasNavigation`.
- There are three `ChestSlotUI` objects ordered by numeric `ChestType`. Prev/Next uses a DOTween carousel: current moves/scales to one side while the adjacent chest moves/scales into the center.
- Prev/Next notification icons indicate unseen rewards in neighboring chest types.
- The custom particle loot reveal effect was removed/deferred. `ChestSlotUI` currently has a simple optional Image-based reward/loot effect path; do not assume a ParticleSystem is configured.
- `ChestRuntimeTestTool` can create a selected chest reward from the current level's matching source while in Play Mode.

### 6.6 Inventory, equipment and enchant material

- Inventory has a fixed capacity that can be increased.
- A slot stores an `Item` plus amount. Equipment instances carry a stable instance ID and normally do not stack.
- Inventory UI only renders unequipped inventory slots. Equipped items are owned by each character and do not count toward capacity/full checks.
- Sorting orders inventory items by rarity descending.
- Equipment has eight types: main weapon, off-hand weapon, body armor, helmet, coat, boot, ring and necklace.
- Character restrictions are `ALL`, `MELEE`, `RANGER`, or `MAGE`; current class enum is intentionally spelled `CharacaterClassType` in code.
- Equip validation checks selected character, level requirement, class requirement and matching equipment slot.
- Every unlocked character retains stats, equipment and skills when removed from the active team.
- Maximum active team size is three. Team slot two and three unlock levels are serialized in `PlayerTeam`.
- `EquipmentDataSO.GetMainStats()` returns the main stat and attacks/second only for `MAIN_WEAPON`. `OFF_HAND_WEAPON` does not display attacks/second.
- `GetSubStats()` returns authored stat entries after index zero.
- Runtime enhancement groups are indexed by `BuffStatType`: `SOCKET`, `ENCHANTMENT`, `DECORATION`.
- `EnchantMaterialDataSO` defines rarity, matching `BuffStatType`, and a serialized pool of possible stats. Applying one requires an empty matching slot, rolls one valid stat, stores source rarity, consumes one material, then saves and refreshes.

### 6.7 Item information UI

Source files:

- `Assets/_GAME/Scripts/UI/ItemInformation/CanvasItemInfomationUI.cs`
- `Assets/_GAME/Scripts/UI/ItemInformation/PanelItemInformation*.cs`
- `Assets/_GAME/Scripts/UI/ItemInformation/EquipmentStatsContentUI*.cs`
- setup guide: `Assets/_GAME/Docs/ItemInformationUI-Setup.md`

Behavior:

- Clicking an inventory equipment slot opens the selected item panel and, if the selected hero wears the same equipment type, an equipped comparison panel.
- Clicking an equipped slot only shows the selected item panel.
- One panel is centered at X = 0. Two panels use equal horizontal offsets.
- Clicking the background closes the canvas.
- Equip and Unequip buttons exist only on the selected-item panel and close the information canvas after a successful action.
- Item name and rarity use rarity colors. Unmet level/class requirements render red.
- Main stat is fixed at the top. Substat and enhancement sections spawn pooled `StatLineUI` rows and only show separators/headers when the corresponding section exists.
- Enhancement headers use `BuffStatVisualSO`. Filled rows show source-rarity tier (`T1`...`T8`); empty rows show `Empty` with default rank visuals.
- Equipment main, sub and enhancement stat labels use the shared `StatTypeUtility.GetDisplayName` mapping; cooldown reduction displays as `CD` and damage amplification as `DA`.
- Level and character requirements are children of the panel and stay pinned to the bottom while content height changes.
- Enchant material display uses its buff type as main information, shows the matching icon/color, and lists all possible rolled stats with the material rarity tier.

### 6.8 Hero status and skill progression UI

Source files:

- `Assets/_GAME/Scripts/UI/HeroStatus/CanvasHeroStatus.cs`
- `Assets/_GAME/Scripts/UI/HeroStatus/StatProgressLineUI.cs`
- `Assets/_GAME/Scripts/UI/HeroStatus/StatValueFeedback.cs`
- `Assets/_GAME/Scripts/UI/HeroStatus/CanvasHeroStatus.Stats.cs`
- `Assets/_GAME/Scripts/UI/TextPopUp.cs` and `TextPopUpPool.cs`
- `Assets/_GAME/Scripts/UI/HeroStatus/HeroStatusSkillSlotUI.cs`
- `Assets/_GAME/Scripts/UI/HeroStatus/HeroSkillInformationPanel.cs`
- `Assets/_GAME/Editor/HeroStatusPrototypeBuilder.cs`

Current implementation:

- `CanvasHeroStatus` shows selected hero portrait/name, level, current/required EXP, EXP fill, all upgradable stats, available stat points, reset button, horizontal skill list and a skill detail panel.
- Stat lines and skill slots are pooled `GameUnit` prefabs.
- Every refresh explicitly restores UI sibling order: stat lines follow numeric `StatType`; skill slots follow the serialized `CharacterStatProgressSO.CombatSkills` list.
- Stat icons come from `StatIconSO` through `DataManager.GetStatIcon`.
- Hero Status stat names use the same shared display-name mapping as equipment, including `CD` and `DA`.
- Stat rows persist across ordinary refreshes. A confirmed increase in upgrade count and actual current value triggers DOTween value pop (1.28x), color return (0.2s), number ticker (0.15s), and a pooled `+delta` popup (0.6s).
- Both the value flash and popup read the stat's feedback color through `DataManager.GetStatFeedbackColor`; the colors are authored alongside icons in `StatIconSO`.
- Runtime-only popup helpers are recreated after Unity serialization/domain reload, and value tweens are linked to their text GameObject so disabling a row cannot leave an orphan tween running.
- Initial display, switching heroes, rejected upgrades and unrelated refreshes do not trigger upgrade feedback. Rapid upgrades resume from the displayed number; popup delta uses the actual previous/new data values. Reset/close/switch clears owned feedback and restores text scale/color.
- A stat upgrade uses its `CharacterStatUpgradeRule`: value per upgrade and point cost.
- A stat without a matching `CharacterStatUpgradeRule` remains visible for inspection but its `+` button is hidden. A valid rule with insufficient points keeps the button visible but non-interactable.
- Reset first ends the current attack when necessary and unequips every skill, then refunds points spent on both stat upgrades and every purchased skill level. Character level and EXP remain unchanged.
- Skill refund is cumulative by purchased level: a level 2 skill refunds the configured costs for level 1 plus level 2, then returns the learned skill level to 0.
- Skill level costs are authored per target level in `CombatSkill`.
- The skill upgrade label shows `Max Level` only at the skill cap. Before the hero reaches `CombatSkill.LevelRequire`, it instead shows `Level Require: <level>` in red and keeps the upgrade button disabled; the authored text color returns after the requirement is met.
- A learned eligible skill can be equipped; each character can equip at most two.
- Skill cooldown UI in the regular hero panel reads progress from `CharacterCombat`; UI does not run its own cooldown clock.
- Prototype generator menu: `IdleGo/UI/Build Hero Status Prototype`.
- Generated assets:
  - `Assets/_GAME/Resources/UI/Canvas-HeroStatus.prefab`
  - `Assets/_GAME/Prefabs/UI/StatProgressLine.prefab`
  - `Assets/_GAME/Prefabs/UI/HeroStatusSkillSlot.prefab`
  - `Assets/_GAME/Resources/Data/StatIconData.asset`

## 7. Indexed enum contracts

Do not reorder or insert existing enum values without a save migration. Lists use the numeric enum value as their index.

### Character `StatType`

| Index | Value |
|---:|---|
| 0 | `MAX_HEALTH` |
| 1 | `RUN_SPEED` |
| 2 | `DAMAGE` |
| 3 | `LEVEL` |
| 4 | `EXPERIENCE` |
| 5 | `ATTACK_SPEED` |
| 6 | `CRITICAL_CHANCE` |
| 7 | `CRITICAL_DAMAGE` |
| 8 | `COOLDOWN_REDUCTION` |
| 9 | `ARMOR` |
| 10 | `LIFE_STEAL` |
| 11 | `DODGE_CHANCE` |
| 12 | `DAMAGE_AMPLIFICATION` |

### `GlobalStatType`

| Index | Value |
|---:|---|
| 0 | `LEVEL` |
| 1 | `EXPERIENCE` |
| 2 | `GOLD_BUFF` |
| 3 | `EXP_BUFF` |
| 4 | `DAMAGE_BUFF` |
| 5 | `SPEED_BUFF` |

### Other ordered enums

- `EquipmentType`: `MAIN_WEAPON(0)`, `OFF_HAND_WEAPON(1)`, `BODY_ARMOR(2)`, `HELMET(3)`, `COAT(4)`, `BOOT(5)`, `RING(6)`, `NECKLACE(7)`.
- `RarityType`: `COMMON(0)`, `UNCOMMON(1)`, `RARE(2)`, `LEGEND(3)`, `DEMON(4)`, `ARCANA(5)`, `BEYOND(6)`, `CELESTIAL(7)`.
- `BuffStatType`: `SOCKET(0)`, `ENCHANTMENT(1)`, `DECORATION(2)`.
- `ChestType`: `NORMAL(0)`, `ELITE(1)`, `BOSS(2)`. Numeric order drives the chest carousel.
- `EnemyType`: `NORMAL(0)`, `ELITE(1)`, `BOSS(2)`.
- `DifficultType`: `EASY(0)`, `NORMAL(1)`, `HARD(2)`, `NIGHTMARE(3)`, `LEGEND(4)`. Numeric order drives sequential difficulty unlocks and is persisted.
- `CharacaterClassType`: `MELEE(0)`, `RANGER(1)`, `MAGE(2)`.

## 8. Important ScriptableObject setup

- `CharacterDataSO`: stable ID, display data, class, `CharacterStatSO`, `CharacterCombatSO`, and `CharacterStatProgressSO`.
- `CharacterStatSO`: base character stat list indexed by `StatType`.
- `CharacterStatProgressSO`: allowed stat upgrade rules and all progress skills for one character.
- `CharacterCombatSO`: basic range/type/delay and available starting combat skills.
- `CombatSkill`: stable `SkillId`, cooldown, level count/costs, requirement, animation name and subclass-specific level data.
- `EquipmentDataSO`: stable item ID, rarity/type, requirements, enhancement slot counts, main stat at index zero, substats afterward, and attacks/second for main weapons.
- `EnchantMaterialDataSO`: rarity inherited from `ItemSO`, buff type, and possible stat roll list.
- `ItemDatabaseSO`: must include every item ID that can exist in inventory, equipped data or chest saves.
- `EquipmentSourceSO`: one common required level and weighted equipment entries of that level.
- `EnemyDataSO`: prefab, enemy type and EXP reward.
- `LevelDataSO`: zero-based level index, global level requirement, world-space team spawn position, waves and three equipment sources.
- `MapDataSO`: map enum, pooled `MapSelectUI` mini-map prefab, gameplay map prefab, spawn position and level count. Every map must be assigned once to `MapManager.maps` in progression order.
- `DifficultyMapVisualSO`: one border and name-background sprite per `DifficultType`; loaded through `DataManager` with Resources fallback `Data/DifficultyMapVisualData`.
- `RarityColorSO`, `RarityBGSO`, `BuffStatVisualSO`, and `StatIconSO`: referenced by `DataManager`; `StatIconSO` stores per-stat icons and feedback colors and falls back to Resources path `Data/StatIconData`.

## 9. Editor and design tools

- Equipment generator: `Assets/_GAME/Editor/EquipmentBalanceGeneratorWindow*.cs` and calculator/rules files.
- Enemy generator: `Assets/_GAME/Editor/EnemyBalanceGeneratorWindow.cs` and calculator/rules files. Inputs include level, enemy type and a manual 0.5-2 balance multiplier.
- Item Information setup tools: `Assets/_GAME/Editor/ItemInformationUISetup*.cs`.
- Hero Status prototype builder: `Assets/_GAME/Editor/HeroStatus*Builder.cs`.
- Runtime chest test: `Assets/_GAME/Scripts/UI/Chest/ChestRuntimeTestTool.cs`.
- Save reset context menu: `Assets/_GAME/Scripts/ToolSave.cs`.

Detailed balance references:

- `Assets/_GAME/Docs/equipment_balance_design.md`
- `Assets/_GAME/Docs/player_balance_design.md`
- `Assets/_GAME/Docs/Stat-Equipment-Modifier-System.md`
- `Assets/_GAME/Docs/ItemInformationUI-Setup.md`
- `Assets/_GAME/Docs/StatUpgradeFeedback-Setup.md`

Read only the relevant detailed document after this file when working on those systems.

## 10. Feature status

| System | Status | Notes |
|---|---|---|
| Character base/stats | Implemented | Indexed stats, modifiers, health, dodge, armor, life steal, amplification |
| Auto combat | Implemented | Nearest target, skill priority, MoveTowards, animation-timed damage/cooldown |
| Attack types | Implemented | Single target and AOE separated for extension |
| Team/roster | Implemented | Many characters, max three active, unlockable slots, persistent per-character state |
| Map/difficulty progress | UI prototype implemented | Five difficulties, ordered maps, sequential unlocks, save, pooled mini-map view, two map rows and difficulty popup |
| Level/wave | Core flow implemented | Addressable load, pooled waves, team defeat/restart, completion/next-level result flow |
| Inventory | Implemented | Capacity, add/remove/sort, pooled UI, save/load |
| Equipment | Implemented | Requirements, per-character equip data, quality, modifiers, comparison UI |
| Enchant materials | Implemented | Matching slot validation, one random allowed stat, persistence/display |
| Loot/chests | Implemented | Type-specific drop roll, queues, auto-open, save, carousel and runtime test tool |
| Custom loot reveal particle | Deferred | Removed by user; simple optional Image reveal remains |
| Global progression | Implemented foundation | Level/EXP plus gold, EXP, damage and speed buffs |
| Character progression | Implemented foundation | EXP, level stat points, stat/skill upgrade and reset persistence |
| Hero Status UI | Prototype implemented | Code/prefabs generated; visual tuning and Play Mode setup verification remain |
| Item Information UI | Implemented foundation | Dynamic equipment/material layout and equip/unequip flow |
| Balance tooling/docs | Implemented foundation | Equipment/enemy tools and design documents exist; values remain tunable |

## 11. Open TODO

These are known unfinished or unverified items, not automatic permission to change unrelated code.

- Visually tune `Canvas-HeroStatus.prefab`, `StatProgressLine.prefab`, and `HeroStatusSkillSlot.prefab` in Unity.
- Visually tune `Canvas-LevelSelect.prefab`, `Button-MapSelect.prefab`, and `MapSelect-Forest.prefab` in Unity after checking the portrait layout.
- Add every future `MapDataSO` to `MapManager.maps` in progression order and set a correct `TotalLevel`; only Forest is currently configured in `Assets/m.unity`.
- Define and apply gameplay stat scaling/reward changes for `DifficultType`; current difficulty logic covers selection, save/load and unlock progression only.
- Verify map progress in Play Mode: save reload, sequential level/map/difficulty unlocks, keeping the same map prefab and replacing it when MapType changes.
- Verify the level-select UI in Play Mode: Navigation open/close, both horizontal rows, difficulty popup locks, map switching, level lock/selected sprites and Addressable level start.
- Finalize the startup scene: either add the populated `Assets/m.unity` scene to `EditorBuildSettings` or move its gameplay managers into the enabled `SampleScene`.
- Assign real sprites in `Assets/_GAME/Resources/Data/StatIconData.asset`; the generated asset only guarantees entries exist.
- Verify Hero Status buttons, pooling, horizontal skill scrolling, stat upgrades, reset, skill upgrade/equip/unequip, locked/eligible/max skill upgrade labels, and save reload in Unity Play Mode.
- Verify stat pop/color/ticker and floating `+delta` visually in Play Mode, including rapid clicks, percentage stats, reset, hero switching, close/reopen and concurrent damage popup reuse.
- Correct the current user-edited stat popup canvas-parent positioning: convert TMP local bounds into the popup parent's centered-anchor coordinates before PlayFloating. Also pass the actual popup parent to cleanup; current cleanup still checks valueText.rectTransform. Diagnosis only; runtime code has not been changed.
- Decide and implement the final chest loot reveal particle effect later. The prior particle implementation was intentionally removed.
- Validate all `LevelDataSO` Addressable addresses follow `Level_<map>-<level>` and that each level has the required equipment sources.
- Verify level fail/restart and level complete/next-level timing, overlays, team spawn position, enemy cleanup, health, temporary modifiers and skill cooldown reset in Unity Play Mode.
- Perform end-to-end save migration tests with older PlayerPrefs data, especially legacy equipped-item and legacy buff-slot fields.
- Review legacy `Awake()`/`GetComponent` usage in `UIManager`, `UICanvas`, and `Singleton` only if a dedicated migration task is requested.

## 12. Verification record

### 2026-10-15 Map, difficulty and level selection UI

- Unity Editor compiled the new runtime/editor setup code and generated the final UI assets without logged C# errors; the temporary one-shot asset builder was removed afterward.
- `dotnet build Assembly-CSharp.csproj --no-restore -p:UseSharedCompilation=false -v:q -clp:ErrorsOnly` and the matching `Assembly-CSharp-Editor.csproj` build both passed with 0 errors.
- Confirmed all affected runtime C# files remain at or below 250 lines; `MapManager.cs` is the longest at 239 lines.
- Confirmed `Canvas-LevelSelect.prefab` has two horizontal ScrollRects, five difficulty options and assigned runtime references; `MapSelect-Forest.prefab` has exactly ten indexed level-button components; Map1 points to that prefab with `TotalLevel = 10`; Navigation points to the authored `Button-Map`.
- Unity Play Mode interaction and portrait-layout verification were not performed.

### 2026-10-15 Map selection and progression logic

- `dotnet build Assembly-CSharp.csproj --no-restore -p:UseSharedCompilation=false -v:q -clp:ErrorsOnly` passed with 0 errors and the existing project warning. New source entries were added temporarily to Unity's generated project because the open Editor had not regenerated it, then those generated-file edits were removed.
- Confirmed all affected C# files are at or below 250 lines and methods are at or below 45 lines; `MapManager.cs` is the longest new file at 225 lines and `LoadLevelRoutine` is the longest affected method at 36 lines.
- Validated seven new 32-character meta GUIDs, unique YAML anchors in `Assets/m.unity`, one MapManager component, and its existing Forest map-instance reference.
- `git diff --check` passed for the affected runtime code and scene after cleanup. Unity Play Mode save/unlock/map-switch verification was not performed.

### 2026-10-15 Hero skill level requirement feedback

- `dotnet build Assembly-CSharp.csproj --no-restore -p:UseSharedCompilation=false -v:minimal` passed with 0 warnings and 0 errors.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` could not complete its project-reference step in the sandbox: the diagnostic run reported compiler-server `UnauthorizedAccessException`, while the command summary reported 0 warnings and 0 errors.
- `git diff --check` passed for `HeroSkillInformationPanel.cs`; the file is 196 lines and the added methods remain below 45 lines.
- Unity Play Mode visual verification was not performed and remains covered by the existing Hero Status TODO.

### 2026-10-14 Stat feedback review and lifecycle hardening

- Re-read the shared popup, stat feedback, Hero Status refresh, DataManager mutation/save flow, pool lifecycle and all serialized prefab references.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` passed with 0 warnings and 0 errors.
- Verified the preserved 32-character TextPopUp script/prefab GUIDs and confirmed both Canvas-Combat and StatProgressLine reference the same popup prefab/component fileID.
- Confirmed the affected `StatValueFeedback.cs` remains below 250 lines and its methods remain below 45 lines; Unity Play Mode visual verification was not performed.

### 2026-10-14 Stat upgrade feedback and shared TextPopUp

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` passed with 0 errors and the one existing legacy Singleton warning.
- Checked all ten affected/new C# files against the 250-line file and 45-line method limits; longest method is 37 lines.
- Validated local fileID references and unique object IDs for TextPopUp, StatProgressLine and Canvas-Combat prefabs; verified preserved popup script/prefab GUIDs and all 13 authored feedback colors.
- Unity Play Mode animation/interaction verification was not performed; concrete checks are recorded in `Open TODO`.

### 2026-10-14 Shared abbreviated stat names

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the one existing project warning from legacy `Singleton.FindObjectOfType` usage.
- Confirmed the three affected C# files remain at or below 250 lines; the longest is `EquipmentInformationText.cs` at 143 lines.
- Unity Editor/Play Mode visual verification was not run.

### 2026-10-14 Level restart cleanup and team spawn

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and the one existing project warning from legacy `Singleton.FindObjectOfType` usage.
- Confirmed affected C# files remain at or below 250 lines; `EnemyManager.cs` is the longest at 230 lines.
- Unity Editor/Play Mode restart verification was not run and remains in `Open TODO`.

### 2026-10-14 Complete level attempt flow

- `dotnet build Assembly-CSharp-Editor.csproj` passed with 0 errors and 41 existing warnings. The two new partial files were temporarily added to Unity's generated C# project for this command, then the generated project file was restored.
- Confirmed every affected C# file remains at or below 250 lines; the longest affected file is `PlayerManager.cs` at 249 lines.
- Confirmed the edited `Canvas-Combat.prefab` has unique YAML object anchors and serialized references to its result overlay and `TextMeshProUGUI` text.
- Unity Editor/Play Mode flow verification was not run and is recorded in `Open TODO`.

### 2026-10-14 Full stat and skill reset

- `dotnet build Assembly-CSharp-Editor.csproj` passed with 0 errors and 41 existing warnings after restoring Unity's temporary NuGet assets.
- Confirmed all four changed C# files remain at or below 250 lines; the longest is `CharacterStatProgress.cs` at 250 lines.
- The initial `--no-restore` attempt could not run because Unity's generated `Temp/obj/.../project.assets.json` was absent; the normal build restored it successfully.
- Unity Editor/Play Mode reset and save-reload verification was not run and remains in the existing Hero Status TODO.

### 2026-10-14 Hero Status list ordering

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and 41 existing warnings after setting `DOTNET_CLI_HOME` inside the project `Temp` directory.
- Confirmed `CanvasHeroStatus.cs` is 249 lines and `StatProgressLineUI.cs` is 96 lines.
- Unity Editor/Play Mode visual verification was not run.

### 2026-10-14 context initialization

- Scanned all script paths under `Assets/_GAME/Scripts`, SO/data paths, Editor tools, Resources UI/data, scene list and package manifest.
- Read the central manager, combat, level, inventory/save, loot/chest, item data and major UI flows listed above.
- Checked Git working-tree summary before documentation; one pre-existing modified prefab was present: `Canvas-Inventory_Hero.prefab`.
- `dotnet build Assembly-CSharp-Editor.csproj` passed with 0 errors and 41 warnings after restoring generated project assets. The only warning in project-owned runtime code is the known obsolete `FindObjectOfType` call in `Singleton.cs`; the other warnings come from Unity packages.
- The first `--no-restore` build attempt could not run because Unity's generated `Temp/obj/.../project.assets.json` was absent; the normal build restored it and completed successfully.
- No gameplay code or Unity asset was changed by this documentation task.
- Unity Editor/Play Mode was not run for this documentation task.

## 13. Recent changes

Use newest entry first. Keep entries focused on behavior, ownership, data/save changes and setup requirements.

### 2026-10-15 - Map, difficulty and level selection UI

- Changed: added `CanvasLevelSelect` with two horizontal pooled map-button rows, a five-option difficulty popup and a pooled selected-map view sourced from `MapDataSO.MiniMapPrefab`. The existing Forest map art and ten authored level-button positions were preserved in `MapSelect-Forest.prefab`.
- Flow: Navigation's authored `Button-Map` opens the canvas. Map/difficulty commands go through `DataManager`; `MapManager` saves and refreshes registered `IMapProgressView` views. Level buttons call `DataManager.SelectLevel` and close the canvas after loading begins.
- Visual state: each level button owns its one-based level number and directly swaps normal/selected/locked sprites; Button Selected Sprite is not used. Difficulty border and name background come from `DifficultyMapVisualSO` through `DataManager`.
- Data/save: no schema or PlayerPrefs-key change. `Map1.asset` now assigns `MapSelect-Forest.prefab` and declares ten levels.
- Setup: `DifficultyMapVisualData.asset` has five entries and uses the supplied Easy, Normal, Hard, Nightmare and Legend border/name-background sprites.
- Verification: Unity asset generation/import and both runtime/editor C# builds passed; Play Mode and final visual tuning remain in Open TODO.

### 2026-10-15 - Map selection, difficulty and clear progression

- Changed: added singleton `MapManager` as owner of the ordered map catalog, current map instance, five-difficulty selection and sequential map/difficulty unlock rules. `LevelManager` now selects only levels and reports clears using the loaded battle's map/difficulty snapshot.
- Map behavior: selecting map/difficulty updates saved selection; a successful level load asks MapManager to keep the current prefab for the same MapType or replace it for a different MapType.
- Data/save: added `PLAYER_MAP_PROGRESS` JSON containing current difficulty/map/level and the highest cleared level for every difficulty/map pair.
- Scene setup: moved the Forest `MapDataSO`, map container and existing Forest instance reference from the old owners into MapManager on `Assets/m.unity`. Future maps must be appended in progression order.
- Verification: runtime C# build, file limits, meta GUIDs, scene anchors/references and targeted diff checks passed; Play Mode verification remains in Open TODO.
- Remaining: selection UI and difficulty-specific combat/reward scaling are intentionally deferred.

### 2026-10-15 - Hero skill level requirement feedback

- Changed: the Hero Skill detail panel now distinguishes a maxed skill from an unmet hero-level requirement. A non-max skill below `CombatSkill.LevelRequire` shows `Level Require: <level>` in red; max and upgrade-cost states use the text's original authored color.
- Data/save: none.
- Setup: none.
- Verification: runtime C# build and diff/file-size checks passed; the Editor aggregate project remained blocked by sandbox compiler-server access, and Unity Play Mode was not run.
- Remaining: visually verify the locked, eligible and max-level label states in the existing Hero Status Play Mode pass.

### 2026-10-14 - Diagnosed canvas-parent stat popup offset

- Current user code spawns stat popups under CanvasHeroStatus and sets their world position, but PlayFloating/Prepare immediately overwrites it with anchoredPosition calculated from TMP-local textBounds.
- The popup position must be converted from value-text local space to the new parent's centered-anchor space. Cleanup also needs the new parent because TextPopUpPool filters by parent identity.
- Verification: read current StatValueFeedback, TextPopUp and TextPopUpPool; no runtime edits, build or Play Mode checks were performed for this diagnosis.
- Remaining: coordinate conversion and cleanup correction are recorded in Open TODO.

### 2026-10-14 - Reviewed stat feedback lifecycle

- Changed: hardened `StatValueFeedback` against Unity domain reload by lazily restoring its runtime-only popup pool and linked both value tweens to the value text GameObject lifecycle.
- Corrected context: recorded the difference between the enabled SampleScene build scene and the populated `Assets/m.unity` development scene.
- Data/save: none.
- Setup: none; the existing TextPopUp references and per-stat feedback colors remain assigned.
- Verification: Editor C# build and prefab GUID/reference checks passed; Play Mode visual checks remain in Open TODO.

### 2026-10-14 - Stat upgrade feedback and shared TextPopUp

- Renamed DamagePopup script/prefab to TextPopUp while preserving GUIDs and migration attributes for serialized fields. The component supports damage burst and generic floating strings; both consumers use the existing SimplePool.
- Added stat value scale/color/ticker and floating delta feedback after the data owner's save/refresh. Retained stat rows so refresh does not interrupt tweens.
- Added editable per-StatType feedback colors to StatIconData, accessed through DataManager; green health/life steal, gold offensive stats, mint speed, blue defenses, purple CD.
- Data/save: no PlayerPrefs schema changes.
- Setup: popup references and defaults are assigned in the existing prefabs. Tune StatProgressLine's Value Feedback and StatIconData's Feedback Color; details are in `Assets/_GAME/Docs/StatUpgradeFeedback-Setup.md`.
- Verification: Editor C# build and static asset/size checks passed; Play Mode visual checks remain in Open TODO.

### 2026-10-14 - Shared abbreviated stat names

- Centralized runtime stat labels in `StatTypeUtility.GetDisplayName` so equipment information and Hero Status cannot use different names.
- Cooldown reduction now displays as `CD`; damage amplification now displays as `DA` in equipment main/sub/enhancement rows and Hero Status stat lines.
- Data/save: none.
- Setup: none.
- Verification: Editor C# build passed; Play Mode visual verification was not run.

### 2026-10-14 - Level restart cleanup and team spawn

- Added a world-space `TeamSpawnPosition` to each `LevelDataSO`; `OnLevelStart` now moves every active team member there before starting wave zero.
- Added `EnemyManager.ResetForLevelStart` to stop unfinished wave spawning, collect tracked and remaining pooled enemies, and clear attempt counters/loot tracking while retaining the current level pools.
- Data/save: no PlayerPrefs change; existing LevelData assets need their team spawn position authored.
- Setup: `Level1_1` starts at `(-85.56917, 4.65, 0)` to preserve its current scene start; assign the field for every new level SO.
- Verification: Editor C# build passed; Play Mode verification remains open.

### 2026-10-14 - Complete level attempt flow

- Added `OnLevelStart`, `OnLevelFail`, `OnLevelComplete` and `OnNextLevel` orchestration in `LevelManager`; fail restarts the same level and completion loads the next level after the configured display duration.
- Added team defeat polling and clean attempt reset for health, movement/attack state, temporary stat modifiers and equipped skill cooldowns.
- Added the black result overlay and English result text to `Canvas-Combat.prefab`; `CanvasCombat` only runs the DOTween `Ease.OutBack` visual and hides it after the supplied duration.
- Data/save: none.
- Setup: adjust fail/complete display durations on `LevelManager` and result scale duration on `CanvasCombat` if desired; prefab references are already assigned.
- Verification: Editor C# build passed and prefab YAML references were checked; Play Mode verification remains open.

### 2026-10-14 - Reset stats and skills together

- Reset now unequips all equipped skills before changing learned skill progress.
- Refund includes every stat upgrade cost and the cumulative configured cost of every purchased skill level; learned skill levels return to 0.
- If reset is requested during an attack, combat ends that attack before clearing its skill runtime states.
- Save schema and Inspector setup are unchanged; the existing inventory JSON flow persists the reset loadout, skill levels and refunded points.
- Editor C# build passed; Play Mode verification remains covered by the existing Hero Status TODO.

### 2026-10-14 - Stable Hero Status list order

- Forced pooled stat and skill entries back into source order after every refresh.
- Hid the stat increase button when the selected character's progress SO has no upgrade rule for that stat.
- Data/save and Inspector setup are unchanged.
- Editor C# build passed; Play Mode verification remains covered by the existing Hero Status TODO.

### 2026-10-14 - Project context entry point

- Added this project progress/architecture document.
- Added root `AGENTS.md` requiring this file to be read before work and updated after work.
- Recorded current runtime flows, ownership, save boundaries, enum index contracts, setup dependencies and known TODOs.

### 2026-10-14 - Hero Status prototype

- Added global and per-character progression foundations, enemy kill EXP, stat points and skill level costs.
- Added the Hero Status canvas, pooled stat/skill lines, skill detail controls, stat icon data and editor prototype builders.
- Kept all Hero Status reads/mutations behind `DataManager` and the existing inventory save/refresh cycle.

## 14. Required update template for future tasks

After a material change, update the relevant sections above and add an entry like:

```md
### YYYY-MM-DD - Short feature name

- Changed: concrete final behavior and owner.
- Data/save: schema, key or migration impact, or "none".
- Setup: Inspector/Addressable/Animation Event steps, or "none".
- Verification: exact build, test or Play Mode checks that passed.
- Remaining: concrete incomplete work, also copied to `Open TODO`.
```

If code and this document disagree, inspect the relevant source files, correct this document, and record the correction in `Recent changes`.
