# THIẾT KẾ CÂN BẰNG TRANG BỊ CHO IDLEGO

Tài liệu này là nguồn quy ước cân bằng trang bị của dự án hiện tại. Tên enum, stat và
công thức phải trùng với code. Phần được ghi là **hướng mở rộng** chưa tồn tại trong
runtime và không được xem là hành vi đã triển khai.

## 1. Mục tiêu

- Level yêu cầu cao hơn tạo item mạnh hơn theo đường tăng trưởng ổn định.
- Rarity tạo khác biệt rõ nhưng không làm level mất ý nghĩa.
- Hai item cùng level, rarity và slot được phép khác nhau nhẹ.
- Mọi stat khác đơn vị đều quy đổi qua cùng một ngân sách sức mạnh.
- Item dùng được cho MELEE, RANGER, MAGE hoặc ALL.
- Socket, enchantment và decoration không tạo sức mạnh miễn phí.
- Có thể cân bằng bằng config và simulation thay vì sửa nhiều công thức.

## 2. Kiểu dữ liệu chính xác của dự án

### 2.1 EquipmentType

| Giá trị | EquipmentType | Ý nghĩa |
|---:|---|---|
| 0 | MAIN_WEAPON | Vũ khí chính |
| 1 | OFF_HAND_WEAPON | Vũ khí phụ |
| 2 | BODY_ARMOR | Giáp thân |
| 3 | HEAD_ARMOR | Giáp đầu |
| 4 | LEG_ARMOR | Giáp chân |
| 5 | SHOES | Giày |
| 6 | RING | Nhẫn |
| 7 | NECKLACE | Vòng |

Không dùng các key WEAPON, ARMOR, HELMET, PANTS hoặc BOOTS trong config vì chúng không
trùng enum hiện tại.

### 2.2 RarityType

| Giá trị | RarityType |
|---:|---|
| 0 | COMMON |
| 1 | UNCOMMON |
| 2 | RARE |
| 3 | LEGEND |
| 4 | DEMON |
| 5 | ARCANA |
| 6 | BEYOND |
| 7 | CELESTIAL |

Không dùng EPIC hoặc LEGENDARY. Thứ tự enum tăng dần cũng là cơ sở để inventory sắp
xếp rarity giảm dần.

### 2.3 StatType được phép xuất hiện trên item

- MAX_HEALTH
- RUN_SPEED
- DAMAGE
- ATTACK_SPEED
- CRITICAL_CHANCE
- CRITICAL_DAMAGE
- COOLDOWN_REDUCTION
- ARMOR
- LIFE_STEAL
- DODGE_CHANCE
- DAMAGE_AMPLIFICATION

Không sinh modifier cho LEVEL và EXPERIENCE. StatTypeUtility.CanHaveModifiers() đã
chặn hai stat progression này.

### 2.4 StatModifierOperation

Runtime hiện tính:

~~~text
FinalStat =
    (BaseStat + tổng FLAT)
    × (1 + tổng PERCENT_ADD)
    × tích từng (1 + PERCENT_MULTIPLY)
~~~

| Nhóm stat | Operation mặc định | Lý do |
|---|---|---|
| DAMAGE, MAX_HEALTH, ARMOR | FLAT | Chỉ số thô tăng theo level |
| RUN_SPEED, ATTACK_SPEED | PERCENT_ADD | Nên tăng tương đối theo nhân vật |
| CRITICAL_CHANCE, COOLDOWN_REDUCTION, LIFE_STEAL, DODGE_CHANCE | FLAT | Giá trị bản thân đã là tỉ lệ 0..1 |
| CRITICAL_DAMAGE | FLAT | +0.10 nghĩa là x1.50 thành x1.60 |
| DAMAGE_AMPLIFICATION | FLAT | Base thường bằng 0 nên PERCENT_ADD không có tác dụng |
| Hiệu ứng đặc biệt hiếm | PERCENT_MULTIPLY | Chỉ dùng có kiểm soát vì các nguồn nhân riêng |

Ví dụ Crit Chance +5% phải lưu Value = 0.05 với Operation = FLAT.

## 3. Data hiện tại và hướng mở rộng

EquipmentDataSO hiện đã có:

- RarityType, EquipmentType và LevelRequired.
- CharacterRequirementType.
- Danh sách StatValue.
- Số ô socket, enchantment và decoration.

Equipment runtime hiện lưu:

- Instance ID và SO gốc.
- Stat trong từng socket.
- Stat phù phép và stat decoration.
- CharacterEquipment đang sở hữu item.

Hiện chưa có ItemLevel riêng, UpgradeLevel, ItemPower được lưu, affix ngẫu nhiên trên
từng instance, special effect hoặc set item.

Trong giai đoạn hiện tại, dùng LevelRequired làm **BalanceLevel**. Nếu level rơi đồ và
level yêu cầu cần tách riêng, thêm ItemLevel rồi dùng nó thay BalanceLevel mà không đổi
các công thức còn lại.

## 4. Item Power

ItemPower là ngân sách để tạo hoặc kiểm tra stat, không phải tổng raw stat.

~~~text
EquipmentTier(L) =
    floor(L / LEVELS_PER_EQUIPMENT_TIER)

LevelPower(L) =
    BASE_ITEM_POWER × EQUIPMENT_TIER_GROWTH^EquipmentTier(L)

ItemPower =
    LevelPower(LevelRequired)
    × RarityMultiplier
    × SlotMultiplier
    × RollMultiplier
~~~

Giá trị khởi đầu:

~~~text
BASE_ITEM_POWER = 10
LEVELS_PER_EQUIPMENT_TIER = 5
EQUIPMENT_TIER_GROWTH = 1.84
ROLL_MIN = 0.90
ROLL_MAX = 1.10
~~~

Trang bị mới xuất hiện mỗi 5 level. Level 1 thuộc tier 0, level 5 thuộc tier 1,
level 10 thuộc tier 2. Các level nằm giữa hai mốc không tạo thêm một bậc sức mạnh item.

~~~text
Lv1 LevelPower  = 10
Lv5 LevelPower  = 18.4
Lv10 LevelPower = 33.856
~~~

Với cùng slot và xét cả Quality Roll:

~~~text
Lv1 Uncommon cao nhất = 10 × 1.50 × 1.10 = 16.50
Lv5 Common thấp nhất  = 18.4 × 1.00 × 0.90 = 16.56
Lv5 Common cao nhất   = 18.4 × 1.00 × 1.10 = 20.24
Lv1 Rare thấp nhất    = 10 × 2.25 × 0.90 = 20.25
~~~

Do đó Lv5 Common luôn mạnh hơn Lv1 Uncommon nhưng luôn yếu hơn Lv1 Rare.

## 5. Rarity multiplier

| RarityType | Multiplier tích lũy | So với bậc trước |
|---|---:|---:|
| COMMON | 1.000 | - |
| UNCOMMON | 1.500 | x1.50 |
| RARE | 2.250 | x1.50 |
| LEGEND | 3.375 | x1.50 |
| DEMON | 5.906 | x1.75 |
| ARCANA | 11.813 | x2.00 |
| BEYOND | 26.578 | x2.25 |
| CELESTIAL | 66.445 | x2.50 |

Khoảng cách rarity tăng dần. Các rarity đầu chênh tối thiểu x1.50; khoảng cách cuối
cùng đạt tối đa x2.50. Đây là multiplier tích lũy trên ItemPower, không phải cộng dồn
thêm lần nữa khi áp stat.

## 6. Slot multiplier

Tổng multiplier của đủ 8 slot được chuẩn hóa bằng 8.00.

| EquipmentType | Multiplier | Vai trò chính |
|---|---:|---|
| MAIN_WEAPON | 1.30 | Nguồn tấn công chính |
| OFF_HAND_WEAPON | 0.95 | Tấn công, phòng thủ hoặc utility |
| BODY_ARMOR | 1.20 | Máu và giáp |
| HEAD_ARMOR | 1.00 | Giáp, máu và utility |
| LEG_ARMOR | 1.00 | Máu, giáp và né |
| SHOES | 0.85 | Tốc chạy, né và tốc đánh |
| RING | 0.80 | Crit, hút máu và damage |
| NECKLACE | 0.90 | Khuếch đại damage và hồi chiêu |

Slot multiplier chỉ xác định budget khi sinh hoặc đánh giá item. Không nhân lại khi áp
StatValue lên CharacterStat.

## 7. Roll multiplier

Hai item cùng BalanceLevel, rarity và slot được phép lệch nhẹ.

| Rarity | Min roll | Max roll |
|---|---:|---:|
| COMMON | 0.90 | 1.10 |
| UNCOMMON | 0.91 | 1.10 |
| RARE | 0.92 | 1.10 |
| LEGEND | 0.93 | 1.10 |
| DEMON | 0.94 | 1.10 |
| ARCANA | 0.95 | 1.10 |
| BEYOND | 0.96 | 1.10 |
| CELESTIAL | 0.97 | 1.10 |

High-roll của bậc dưới đôi lúc được vượt low-roll của bậc ngay trên. Average power của
rarity cao vẫn luôn phải lớn hơn.

## 8. Phân bổ ngân sách

ItemPower đại diện cho sức mạnh khi item đã dùng hết tiềm năng:

~~~text
ItemPower =
    BaseStatBudget
    + SocketPotentialBudget
    + EnchantmentPotentialBudget
    + DecorationPotentialBudget
    + SpecialEffectBudget
~~~

| Rarity | Base stat | Socket | Enchant | Decoration/Special |
|---|---:|---:|---:|---:|
| COMMON | 100% | 0% | 0% | 0% |
| UNCOMMON | 100% | 0% | 0% | 0% |
| RARE | 90% | 10% | 0% | 0% |
| LEGEND | 85% | 10% | 5% | 0% |
| DEMON | 80% | 12% | 8% | 0% |
| ARCANA | 75% | 14% | 8% | 3% |
| BEYOND | 70% | 16% | 9% | 5% |
| CELESTIAL | 65% | 18% | 10% | 7% |

Item có nhiều ô mở rộng phải dành một phần budget cho các ô đó. Không giữ nguyên toàn
bộ base stat rồi cộng slot miễn phí.

Code hiện áp cả decorationStats vào nhân vật. Vì vậy decoration có stat phải được tính
vào budget. Nếu decoration chuyển thành ngoại hình thuần túy, đặt budget của nó bằng 0.

## 9. Số affix và số ô khuyến nghị

Đây là giới hạn validation cho content, chưa phải logic tự sinh trong runtime.

| Rarity | Main | Sub | Socket | Enchant | Decoration |
|---|---:|---:|---:|---:|---:|
| COMMON | 1 | 0 | 0 | 0 | 0 |
| UNCOMMON | 1 | 1 | 0 | 0 | 0 |
| RARE | 1 | 2 | 1 | 0 | 0 |
| LEGEND | 1 | 2 | 1 | 1 | 0 |
| DEMON | 1 | 3 | 2 | 1 | 0 |
| ARCANA | 1 | 3 | 2 | 2 | 1 |
| BEYOND | 1 | 4 | 3 | 2 | 1 |
| CELESTIAL | 1 | 4 | 3 | 3 | 2 |

Không cho cùng một cặp StatType + Operation xuất hiện nhiều lần trong base stat list.
Nếu cần hai nguồn cùng loại, merge chúng trước khi lưu.

Trong BaseStatBudget, chia cho main và sub stat như sau. Phần còn lại sau main được chia
đều cho đúng số sub stat của rarity.

| Rarity | Main share | Tổng sub share |
|---|---:|---:|
| COMMON | 100% | 0% |
| UNCOMMON | 80% | 20% |
| RARE | 70% | 30% |
| LEGEND | 68% | 32% |
| DEMON | 60% | 40% |
| ARCANA | 58% | 42% |
| BEYOND | 55% | 45% |
| CELESTIAL | 50% | 50% |

## 10. Chuyển Power thành stat

### 10.1 Flat stat

~~~text
BaseStatValue = AllocatedPower / PowerCost

DamageValue =
    BaseDamageValue × CharacterRequirementDamageScale
~~~

| StatType | Power cost | Kết quả với 20 Power |
|---|---:|---:|
| DAMAGE | 1 Power / 1 Damage trước class scale | xem bảng bên dưới |
| MAX_HEALTH | 1 Power / 10 HP | +200 HP |
| ARMOR | 2 Power / 1 Armor | +10 Armor |

Damage của vũ khí được phân cấp theo CharacterRequirementType:

| CharacterRequirementType | Damage scale | 20 Power tạo ra |
|---|---:|---:|
| RANGER | 0.80 | 16 Damage |
| MELEE | 1.00 | 20 Damage |
| MAGE | 1.25 | 25 Damage |
| ALL | 1.00 | 20 Damage |

Thứ tự luôn là RANGER < MELEE < MAGE với cùng level, rarity, slot, roll và phần Power
được cấp cho DAMAGE. Ranger bù lại bằng Attack Speed/range; Mage có damage mỗi hit cao
nhưng Attack Speed thấp hơn.

Armor phải được đánh giá bằng phần trăm giảm damage, không so raw Armor trực tiếp với HP.

### 10.2 Stat tỉ lệ

Nếu stat phần trăm tăng exponential theo level, nhân vật sẽ chạm cap quá sớm. Vì vậy:

~~~text
NormalizedBudget =
    AllocatedPower / LevelPower(BalanceLevel)
~~~

| Stat | Cost cho +1 điểm phần trăm | Kết quả với NormalizedBudget 0.20 |
|---|---:|---:|
| CRITICAL_CHANCE | 0.040 | +5.0% |
| CRITICAL_DAMAGE | 0.015 | +13.3 điểm % |
| ATTACK_SPEED | 0.025 | +8.0% |
| RUN_SPEED | 0.030 | +6.7% |
| COOLDOWN_REDUCTION | 0.050 | +4.0% |
| LIFE_STEAL | 0.080 | +2.5% |
| DODGE_CHANCE | 0.060 | +3.3% |
| DAMAGE_AMPLIFICATION | 0.040 | +5.0% |

Khi ghi StatValue, chia điểm phần trăm cho 100. Ví dụ +5% ghi thành 0.05.

Các cost này là baseline để simulation. Uptime skill, animation, số mục tiêu và damage
thực nhận có thể yêu cầu điều chỉnh chúng.

## 11. Affix pool theo slot

### MAIN_WEAPON

- Main: DAMAGE.
- Sub: ATTACK_SPEED, CRITICAL_CHANCE, CRITICAL_DAMAGE,
  DAMAGE_AMPLIFICATION, COOLDOWN_REDUCTION, LIFE_STEAL.

### OFF_HAND_WEAPON

Pool phụ thuộc CharacterRequirementType:

- MELEE: MAX_HEALTH, ARMOR, sau đó DAMAGE.
- RANGER: DAMAGE, ATTACK_SPEED, CRITICAL_CHANCE.
- MAGE: DAMAGE_AMPLIFICATION, COOLDOWN_REDUCTION, DAMAGE.
- ALL: DAMAGE, MAX_HEALTH, ARMOR, COOLDOWN_REDUCTION.

### BODY_ARMOR

- Main: MAX_HEALTH.
- Sub: ARMOR, DODGE_CHANCE, LIFE_STEAL, RUN_SPEED.

### HEAD_ARMOR

- Main: ARMOR hoặc MAX_HEALTH.
- Sub: COOLDOWN_REDUCTION, DAMAGE_AMPLIFICATION, CRITICAL_CHANCE.

### LEG_ARMOR

- Main: MAX_HEALTH hoặc ARMOR.
- Sub: DODGE_CHANCE, RUN_SPEED, LIFE_STEAL.

### SHOES

- Main: RUN_SPEED.
- Sub: DODGE_CHANCE, ATTACK_SPEED, MAX_HEALTH, ARMOR.

### RING

- Main: DAMAGE, CRITICAL_CHANCE hoặc CRITICAL_DAMAGE.
- Sub: LIFE_STEAL, ATTACK_SPEED, DAMAGE_AMPLIFICATION.

### NECKLACE

- Main: DAMAGE_AMPLIFICATION, COOLDOWN_REDUCTION hoặc DAMAGE.
- Sub: CRITICAL_DAMAGE, LIFE_STEAL, MAX_HEALTH.

Mỗi pool dùng weighted roll. Stat mạnh như Cooldown Reduction và Life Steal không nên
có cùng xác suất với mọi stat khác.

## 12. CharacterRequirementType

Eligibility là điều kiện cứng:

~~~text
ALL    -> mọi CharacterType
MELEE  -> chỉ MELEE
RANGER -> chỉ RANGER
MAGE   -> chỉ MAGE
~~~

Rarity và ItemPower không bỏ qua điều kiện equip. Item không phù hợp phải bị từ chối
trước khi modifier được áp.

ItemPower ở đây là sức mạnh chuẩn, chưa có affinity riêng của từng player. Sức mạnh
hiệu quả theo player nằm trong player_balance_design.md.

## 13. Công thức combat hiện tại

### 13.1 Damage

~~~text
RawDamage =
    DAMAGE
    × SkillOrAttackMultiplier
    × (1 + DAMAGE_AMPLIFICATION)
    × CriticalMultiplier

ExpectedCritMultiplier =
    1 + CRITICAL_CHANCE × (CRITICAL_DAMAGE - 1)
~~~

### 13.2 Armor

CombatFormula hiện dùng ARMOR_CONSTANT = 100:

~~~text
DamageReduction =
    ARMOR / (ARMOR + 100)

DamageAfterArmor =
    max(1, round(RawDamage × (1 - DamageReduction)))
~~~

| Armor | Giảm damage |
|---:|---:|
| 25 | 20% |
| 50 | 33.3% |
| 100 | 50% |
| 200 | 66.7% |
| 300 | 75% |

### 13.3 Dodge và EHP

~~~text
ExpectedIncomingDamage =
    DamageAfterArmor × (1 - DODGE_CHANCE)

ReferenceEHP =
    MAX_HEALTH
    × ReferenceEnemyHit
    / max(1, ExpectedIncomingDamage)
~~~

EHP phụ thuộc ReferenceEnemyHit vì damage cuối được làm tròn và có tối thiểu 1.

### 13.4 Life Steal

Runtime hồi theo lượng máu mục tiêu thực sự mất:

~~~text
Heal = ActualHealthLost × LIFE_STEAL
~~~

Overkill không tạo thêm hồi máu.

### 13.5 Cooldown Reduction

~~~text
EffectiveCooldown =
    BaseCooldown × (1 - COOLDOWN_REDUCTION)
~~~

Cooldown bắt đầu tại EndAttack() qua Animation Event.

### 13.6 Attack Speed

~~~text
RequestedAttackInterval =
    DelayAttack / ATTACK_SPEED
~~~

Animation có thể là bottleneck vì nhân vật chưa EndAttack() thì không bắt đầu đòn mới.
Balance tốc đánh phải đo animation thực, không chỉ dùng DPS lý thuyết.

## 14. Giới hạn stat cho content

Code hiện hard clamp một số stat ở 0..1. Soft cap dưới đây là quy tắc authoring; nếu muốn
diminishing return sau soft cap thì cần triển khai thêm.

| Stat | Soft cap | Hard cap nội dung |
|---|---:|---:|
| CRITICAL_CHANCE | 75% | 100% |
| ATTACK_SPEED | 2.50 | 4.00 |
| DODGE_CHANCE | 35% | 60% |
| LIFE_STEAL | 15% | 30% |
| COOLDOWN_REDUCTION | 35% | 60% |
| DAMAGE_AMPLIFICATION | 100% | 200% |

Armor không dùng cap raw; kiểm soát bằng Power Cost và mốc Damage Reduction.

## 15. Item Score và Gear Score

Trước khi lưu StatValue, làm tròn DAMAGE, MAX_HEALTH và stat cần hiển thị dạng số nguyên.
Các stat tỉ lệ giữ precision tối đa bốn chữ số thập phân. Điều này tránh item có power
khác nhau nhưng cho cùng DAMAGE vì Character.AttackDamage hiện chuyển giá trị sang int.

~~~text
ItemScore = round(ItemPower)

CurrentItemScore =
    BaseStatPower + Power của các ô đang có dữ liệu

MaxItemScore =
    ItemPower khi toàn bộ ô hợp lệ đã được sử dụng

GearScore =
    tổng CurrentItemScore của 8 slot đang equip
~~~

Không cộng raw HP, Damage, Armor và Crit để làm score vì chúng khác đơn vị.

GearScore chỉ đo ngân sách trang bị. Nó không thay thế DPS, EHP hoặc độ phù hợp với
player. UI có thể hiển thị thêm EffectiveGearScore theo affinity của player.

## 16. Hướng mở rộng item generator

~~~text
LevelRequired + RarityType + EquipmentType
    -> tính ItemPower
    -> tách base và enhancement budget
    -> chọn main stat theo slot
    -> chọn sub stat không trùng
    -> đổi budget thành StatValue
    -> tạo Equipment instance
    -> lưu toàn bộ roll của instance
~~~

Không ghi stat random ngược vào EquipmentDataSO. SO là definition dùng chung; roll ngẫu
nhiên thuộc từng Equipment instance.

Special effect, set item và upgrade chỉ thêm sau khi có data runtime tương ứng. Mọi hiệu
ứng phải trừ vào cùng Power Budget.

## 17. BalanceConfig đề xuất

Khi bắt đầu code generator, gom các giá trị sau vào một BalanceConfig SO:

- BASE_ITEM_POWER, LEVELS_PER_EQUIPMENT_TIER và EQUIPMENT_TIER_GROWTH.
- RarityMultiplier cho 8 rarity.
- SlotMultiplier cho 8 equipment type.
- RollRange theo rarity.
- FlatStatPowerCost.
- NormalizedPercentStatCost.
- RarityBudgetDistribution.
- AffixPool và AffixWeight theo slot.
- ContentSoftCap.

Không duy trì thêm một JSON config song song nếu Unity SO là nguồn data chính.

## 18. Kiểm thử bắt buộc

Mỗi lần đổi config, generate tối thiểu 10.000 mẫu cho từng checkpoint gồm BalanceLevel,
RarityType, EquipmentType và CharacterRequirementType.

Phải kiểm tra:

- Average power tăng theo rarity và BalanceLevel.
- Mọi cặp rarity đầu có tỉ lệ ít nhất x1.50 và cặp cuối không vượt x2.50.
- Lv5 Common luôn nằm giữa Lv1 Uncommon và Lv1 Rare ở toàn bộ dải Quality Roll.
- DAMAGE cùng Power phải thỏa RANGER < MELEE < MAGE.
- High-roll bậc dưới chỉ thỉnh thoảng vượt low-roll bậc kế tiếp.
- Không có LEVEL hoặc EXPERIENCE trên item.
- Không duplicate StatType + Operation.
- Tổng budget không vượt ItemPower ngoài sai số rounding.
- Item thỏa đúng CharacterRequirementType.
- Full gear không làm stat tỉ lệ vượt hard cap nội dung.
- Armor được đánh giá bằng Damage Reduction.
- Socket, enchantment và decoration đều được tính vào score.

## 19. Editor Tool

Mở công cụ tại:

~~~text
Tools > IdleGo > Equipment Balance Generator
~~~

Tool đọc trực tiếp LevelRequired, RarityType, EquipmentType và CharacterRequirementType
từ EquipmentDataSO. Người dùng chọn main stat, chọn đủ sub stat theo rarity, xem Power
Preview rồi bấm Generate & Save Stats. Preset theo slot chỉ là lựa chọn nhanh và vẫn có
thể chỉnh lại trước khi generate.

Tool ghi data bằng SerializedObject, hỗ trợ Undo và save asset trước khi UI/runtime đọc
lại. Việc Generate sẽ thay toàn bộ danh sách base stats của EquipmentDataSO, không thay
socket, enchantment hoặc decoration runtime.

## 20. Acceptance Criteria

- Dùng đúng 8 EquipmentType và 8 RarityType của dự án.
- Dùng đúng StatType và StatModifierOperation hiện tại.
- LevelRequired là BalanceLevel cho tới khi có ItemLevel riêng.
- Rarity không lấn át hoàn toàn level.
- Mỗi stat được quy đổi từ một Power Budget.
- Stat tỉ lệ không tăng exponential theo level.
- Công thức Armor trùng với CombatFormula.
- Data cập nhật trước save và UI.
- Có thể đổi hệ số qua một config mà không sửa logic.
- Có simulation chứng minh các dải item không phá progression.

Tài liệu này phụ trách sức mạnh chuẩn của item. Cách từng player chuyển sức mạnh item
thành hiệu quả chiến đấu nằm trong player_balance_design.md.
