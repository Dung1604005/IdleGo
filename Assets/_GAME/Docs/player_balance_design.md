# THIẾT KẾ CÂN BẰNG SỨC MẠNH PLAYER CHO IDLEGO

Tài liệu này quy định cách cân bằng nhiều player khác nhau, cách tăng sức mạnh theo
level và cách mỗi player tận dụng vũ khí. Nó sử dụng ItemPower từ
equipment_balance_design.md nhưng không thay đổi sức mạnh chuẩn của item.

Những mục ghi **hướng triển khai** là thiết kế cho code sau này. Runtime hiện tại chưa
tự động áp toàn bộ các hệ số đó.

## 1. Mục tiêu

- Nhiều player có lối chơi khác nhau nhưng không có một lựa chọn luôn tốt nhất.
- MELEE, RANGER và MAGE có ưu nhược điểm rõ.
- Player đánh nhanh có damage mỗi hit thấp hơn; player đánh chậm có damage mỗi hit cao.
- Vũ khí phù hợp làm player mạnh đúng hướng mà không nhân sức mạnh mất kiểm soát.
- Hai player cùng CharacterType vẫn có thể scale khác nhau với cùng một bộ stat.
- Player giữ nguyên level, stat, equipment và skill khi ra khỏi team.
- Team tối đa ba người được cân bằng bằng hiệu quả toàn đội, không chỉ cộng Power.

## 2. Hệ thống hiện tại

### 2.1 CharacterType

~~~text
MELEE  = 0
RANGER = 1
MAGE   = 2
~~~

### 2.2 Eligibility của equipment và skill

CharacterRequirementType là điều kiện cứng:

~~~text
ALL    -> mọi CharacterType
MELEE  -> chỉ MELEE
RANGER -> chỉ RANGER
MAGE   -> chỉ MAGE
~~~

Nếu không thỏa điều kiện, player không được equip. Weapon scaling không được dùng để
lách qua điều kiện này.

### 2.3 Stat hiện tại

CharacterStatSO chứa base stat. CharacterStat giữ progression stat của từng player và
áp modifier theo công thức:

~~~text
FinalStat =
    (Base + tổng Flat)
    × (1 + tổng PercentAdd)
    × tích từng (1 + PercentMultiply)
~~~

Level và Experience không nhận modifier từ equipment.

### 2.4 Giới hạn hiện tại cần nhớ

Code hiện tăng LEVEL khi đủ EXP nhưng chưa tự tăng DAMAGE, MAX_HEALTH hoặc stat khác
theo level. Nếu muốn player tự scale khi level up, cần thêm growth data và gọi cập nhật
base progression. Không được viết tài liệu như thể logic này đã tồn tại.

Equipment hiện chỉ phân biệt MAIN_WEAPON, OFF_HAND_WEAPON và
CharacterRequirementType. Dự án chưa có enum riêng cho kiếm, cung, trượng hoặc các biến
thể vũ khí.

## 3. Hai lớp cân bằng

Sức mạnh player được chia thành hai lớp.

### Lớp 1: sức mạnh chuẩn

- Base stat theo level.
- ItemPower chuẩn.
- Skill multiplier, cooldown, range và attack type.

### Lớp 2: mức độ phù hợp

- Player tận dụng stat nào tốt hơn.
- Player scale với vũ khí chính và phụ ra sao.
- Animation và nhịp đánh thực tế.
- Khả năng đánh đơn, AOE, hồi máu hoặc sống sót.

Không tăng đồng thời cả ItemPower lẫn player affinity cho cùng một lý do. ItemPower là
giá trị chung; affinity chỉ thay đổi hiệu quả của item đối với player cụ thể.

## 4. Chỉ số mục tiêu để cân bằng

Không dùng một con số Combat Power duy nhất làm nguồn quyết định. Mỗi player phải được
đo ít nhất bằng:

- Single Target DPS trong 30 và 60 giây.
- Three Target DPS trong 30 giây.
- Reference EHP trước một enemy chuẩn.
- Thời gian sống khi không hút máu.
- Sustain mỗi giây từ heal và Life Steal.
- Thời gian di chuyển để vào range.
- Tỉ lệ thời gian thực sự gây damage.
- Thời gian tiêu diệt boss chuẩn.

Đối với AutoCombat, simulation phải dùng đúng luật ưu tiên skill vừa hồi xong gần nhất
và đúng Animation Event. Công thức DPS đơn giản chỉ dùng để dự báo ban đầu.

## 5. Baseline level 1

Stat mặc định trong dự án là MAX_HEALTH 100, DAMAGE 10, RUN_SPEED 5,
ATTACK_SPEED 1, Crit Chance 5% và Crit Damage x1.5.

Bảng sau là điểm bắt đầu cho ba archetype, chưa bao gồm equipment:

| Stat | MELEE | RANGER | MAGE |
|---|---:|---:|---:|
| MAX_HEALTH | 125 | 90 | 85 |
| DAMAGE | 10 | 9 | 12 |
| ARMOR | 8 | 3 | 2 |
| RUN_SPEED | 4.8 | 5.5 | 5.0 |
| ATTACK_SPEED | 1.00 | 1.10 | 0.82 |
| CRITICAL_CHANCE | 5% | 7% | 5% |
| CRITICAL_DAMAGE | x1.50 | x1.50 | x1.50 |
| COOLDOWN_REDUCTION | 0% | 0% | 5% |
| LIFE_STEAL | 0% | 0% | 0% |
| DODGE_CHANCE | 0% | 3% | 0% |
| DAMAGE_AMPLIFICATION | 0% | 0% | 0% |

Expected basic DPS trước Armor:

~~~text
MELEE  ≈ 10 × 1.00 × 1.025 = 10.25
RANGER ≈  9 × 1.10 × 1.035 = 10.25
MAGE   ≈ 12 × 0.82 × 1.025 = 10.09
~~~

Các baseline gần nhau về đánh thường. Khác biệt chính đến từ skill, range, AOE và khả
năng sống sót.

Đây không phải template bắt buộc cho mọi player. Mỗi player có thể lệch khỏi baseline
nếu phần còn lại của kit trả lại đúng Power Budget.

## 6. Growth theo level

### 6.1 Flat stat

Khuyến nghị ban đầu:

~~~text
BaseDamage(L) =
    DamageAtLevel1 × 1.05^(L - 1)

BaseHealth(L) =
    HealthAtLevel1 × 1.055^(L - 1)
~~~

Health tăng nhanh hơn Damage một chút để thời gian sống không giảm liên tục ở late game.

RUN_SPEED, ATTACK_SPEED, Crit, Dodge, Life Steal, Cooldown Reduction và Damage
Amplification không tăng exponential theo level. Chúng tăng qua equipment, skill,
passive hoặc mốc nâng cấp có kiểm soát.

### 6.2 Armor

Không scale Armor exponential một cách độc lập vì Armor dùng:

~~~text
DamageReduction = Armor / (Armor + 100)
~~~

Muốn đặt một mốc giảm damage mục tiêu:

~~~text
RequiredArmor =
    100 × TargetReduction / (1 - TargetReduction)
~~~

Ví dụ:

| Target Reduction | Required Armor |
|---:|---:|
| 20% | 25 |
| 35% | 53.85 |
| 50% | 100 |
| 60% | 150 |

Base Armor của player nên tăng chậm hoặc theo mốc. Phần lớn progression phòng thủ đến
từ HP, equipment và build thay vì cho Armor tăng vô hạn theo level.

### 6.3 EXP

Code hiện dùng:

~~~text
ExpToNextLevel =
    100 × Level^1.5
~~~

Đường EXP quyết định tốc độ đạt level, không quyết định trực tiếp stat growth. Hai đường
này phải được tune riêng.

### 6.4 Hướng triển khai growth

Mỗi player cần một growth profile gồm:

- Giá trị level 1.
- Growth rate của DAMAGE và MAX_HEALTH.
- Các mốc tăng Armor.
- Stat thưởng tại những level đặc biệt.

Khi level thay đổi:

~~~text
cập nhật progression data
    -> tính base stat mới
    -> save
    -> CharacterStat rebuild
    -> UI đọc lại
~~~

Không ghi growth bonus thành equipment modifier vì player phải giữ progression khi tháo
toàn bộ đồ.

## 7. Phân chia sức mạnh base và equipment

Tại một checkpoint level với bộ đồ chuẩn:

~~~text
40% đến 50% hiệu quả = base player + skill
50% đến 60% hiệu quả = equipment
~~~

Mục tiêu này giúp:

- Player không trở nên vô dụng khi thiếu một slot.
- Loot vẫn tạo khác biệt rõ.
- Đổi player không làm toàn bộ progression mất giá trị.

Trong phần offensive power của full gear:

~~~text
MAIN_WEAPON        = khoảng 35% đến 45%
OFF_HAND_WEAPON    = khoảng 10% đến 20%
6 slot còn lại     = phần còn lại
~~~

Tỉ lệ thực tế vẫn phải lấy từ ItemPower và slot multiplier, không hard-code lần nữa khi
áp stat.

## 8. Hệ thống weapon scaling

Weapon scaling gồm ba bước độc lập.

### 8.1 Bước 1: Eligibility

CharacterRequirementType quyết định có được equip hay không. Đây là hard gate.

### 8.2 Bước 2: Item Power chuẩn

Equipment dùng công thức trong equipment_balance_design.md. Hai player nhìn cùng một
item sẽ thấy cùng ItemPower và raw StatValue.

### 8.3 Bước 3: Player Weapon Affinity

Mỗi player có profile riêng để chuyển stat của MAIN_WEAPON và OFF_HAND_WEAPON thành
modifier thực tế.

~~~text
EffectiveWeaponStat =
    RawWeaponStat × PlayerAffinity(EquipmentType, StatType)
~~~

Nếu không khai báo affinity:

~~~text
Affinity = 1.00
~~~

Khoảng an toàn ban đầu:

~~~text
0.85 <= Affinity <= 1.15
~~~

Không cho một coefficient quá cao rồi bù bằng coefficient không liên quan. Tổng hiệu
quả của cả profile phải được simulation.

### 8.4 Stat nào được affinity

Affinity chỉ áp lên modifier đến từ MAIN_WEAPON và OFF_HAND_WEAPON.

Nên áp:

- DAMAGE.
- ATTACK_SPEED.
- CRITICAL_CHANCE và CRITICAL_DAMAGE.
- DAMAGE_AMPLIFICATION.
- COOLDOWN_REDUCTION.
- Stat phòng thủ trên OFF_HAND_WEAPON nếu đó là hướng thiết kế của player.

Không áp weapon affinity lên BODY_ARMOR, HEAD_ARMOR, LEG_ARMOR, SHOES, RING hoặc
NECKLACE. Các slot đó giữ hiệu quả chuẩn để build dễ đọc.

### 8.5 Profile khởi đầu theo CharacterType

| Nguồn stat | MELEE | RANGER | MAGE |
|---|---:|---:|---:|
| Main weapon DAMAGE | 1.05 | 1.00 | 1.00 |
| Main weapon ATTACK_SPEED | 0.95 | 1.10 | 0.85 |
| Main weapon CRIT | 1.00 | 1.05 | 0.95 |
| Main weapon DAMAGE_AMPLIFICATION | 0.95 | 0.95 | 1.10 |
| Main weapon COOLDOWN_REDUCTION | 0.95 | 1.00 | 1.10 |
| Off-hand offensive stat | 0.90 | 1.00 | 1.05 |
| Off-hand HP/Armor | 1.10 | 0.90 | 0.90 |

Các hệ số này mô tả xu hướng archetype. Profile cuối cùng thuộc từng player và có thể
khác, miễn tổng hiệu quả vẫn trong dải cân bằng.

Ví dụ hai MELEE:

- Knight có Off-hand HP/Armor 1.15 nhưng Main weapon Attack Speed 0.90.
- Duelist có Main weapon Attack Speed 1.12 nhưng Off-hand HP/Armor 0.85.

Hai nhân vật vẫn dùng được item MELEE nhưng scale theo hướng khác nhau.

## 9. Vũ khí ALL và vũ khí theo class

Item có requirement ALL phải dùng được cho cả ba class. Để item ALL không luôn là lựa
chọn tốt nhất:

- Item ALL dùng pool stat trung tính.
- Item class-specific dùng pool tập trung hơn.
- Không tự giảm raw stat của item ALL nếu ItemPower đã bằng nhau.
- Sự khác biệt đến từ độ phù hợp affix và affinity của player.

Một item ALL có roll tốt vẫn có thể thắng item class-specific roll xấu. Trung bình,
class-specific item phải tạo build rõ hơn.

## 10. Khi cần nhiều loại vũ khí hơn

Hiện tại CharacterRequirementType chỉ phân biệt nhóm MELEE, RANGER và MAGE. Nếu sau này
cần phân biệt Sword, Great Sword, Dual Blade, Bow, Crossbow, Staff, Wand, Shield hoặc
Focus, thêm WeaponType riêng vào EquipmentDataSO.

Không dùng tên asset hoặc itemId để suy ra loại vũ khí.

Luồng khi có WeaponType:

~~~text
CharacterRequirementType
    -> kiểm tra class có được equip

WeaponType
    -> lấy affinity cụ thể của player

EquipmentType
    -> xác định main-hand hoặc off-hand
~~~

Player profile nên có fallback:

~~~text
exact WeaponType affinity
    -> CharacterType weapon affinity
    -> 1.00
~~~

Chỉ thêm enum này khi content thực sự có nhiều vũ khí trong cùng một CharacterType.

## 11. Effective Item Power theo player

ItemPower chuẩn không đổi. Để UI so sánh item cho player đang chọn:

~~~text
EffectiveItemPower(player, item) =
    tổng Power của từng modifier
    × affinity tương ứng
~~~

Với slot không phải vũ khí, affinity bằng 1.

Hiển thị nên tách:

- Item Score: sức mạnh chuẩn của item.
- Effective Score: độ hiệu quả với player hiện tại.

Không sửa ItemScore gốc khi đổi hero vì item data không thuộc UI và không thuộc player.

## 12. Cân bằng tốc đánh và animation

DPS lý thuyết:

~~~text
ExpectedBasicDPS =
    DAMAGE
    × ATTACK_SPEED
    × ExpectedCritMultiplier
    × (1 + DAMAGE_AMPLIFICATION)
~~~

Runtime còn bị giới hạn bởi:

- DelayAttack.
- Độ dài animation.
- Frame ExecuteAttack.
- Frame EndAttack.
- Thời gian di chuyển tới range.

Khoảng đánh thực tế gần đúng:

~~~text
ActualAttackInterval =
    max(AnimationCycleDuration, DelayAttack / ATTACK_SPEED)
~~~

Nếu animation dài hơn interval, tăng Attack Speed sẽ không tạo DPS như dự kiến. Mọi
player phải có benchmark bằng animation thật.

## 13. Cân bằng skill

Mỗi skill cần quy đổi về hiệu quả trên một chu kỳ:

~~~text
EffectiveCooldown =
    BaseCooldown × (1 - COOLDOWN_REDUCTION)

SingleTargetSkillDPS =
    ExpectedDamagePerCast / EffectiveCooldown
~~~

Với AOE:

~~~text
AOESkillDPS =
    ExpectedDamagePerTarget
    × ReferenceTargetCount
    / EffectiveCooldown
~~~

Dùng ReferenceTargetCount = 3 cho benchmark thường. Không định giá AOE theo số enemy
tối đa có thể đứng trong OverlapCircle vì trường hợp đó không ổn định.

Heal skill quy đổi bằng:

~~~text
HealingPerSecond =
    ExpectedEffectiveHeal / EffectiveCooldown
~~~

Không tính phần heal vượt Max Health.

AutoCombat ưu tiên skill vừa hồi xong gần nhất. Khi nhiều skill tranh cùng animation và
nhịp hành động, tổng SkillDPS không bằng phép cộng đơn giản. Simulation rotation là kết
quả cuối cùng.

## 14. Archetype target

### MELEE

- EHP cao hơn baseline khoảng 20% đến 35%.
- Range ngắn nên cần bù thời gian tiếp cận.
- Single target DPS có thể thấp hơn RANGER/MAGE khoảng 0% đến 8% nếu sống sót tốt hơn.
- Off-hand phòng thủ có giá trị cao.

### RANGER

- Basic DPS và uptime ổn định.
- EHP thấp hơn MELEE.
- Tốc chạy và range giảm thời gian mất DPS.
- Crit và Attack Speed có giá trị cao nhưng phải tránh chạm cap quá sớm.

### MAGE

- Damage mỗi hit và AOE cao.
- Attack Speed thấp hơn.
- Scale tốt với Damage Amplification và Cooldown Reduction.
- EHP thấp; sức mạnh phải đến từ skill chứ không cho mọi stat tấn công đều vượt trội.

Đây là target theo nhóm. Một player cụ thể có thể là tank, burst, sustain hoặc support
khác với xu hướng chung.

## 15. Role budget

Nếu sau này thêm RoleType, dùng các trọng số benchmark sau:

| Role | Single target | AOE | EHP | Sustain/Utility |
|---|---:|---:|---:|---:|
| Damage Dealer | 50% | 25% | 15% | 10% |
| Tank | 25% | 15% | 45% | 15% |
| Support | 25% | 20% | 20% | 35% |

Các trọng số chỉ dùng cho báo cáo balance. Không nên biến chúng thành một Combat Power
duy nhất dùng để tự động quyết định mọi thứ.

## 16. Team balance

Team có tối đa ba player. Phải benchmark:

- Một player.
- Hai player ở mốc mở slot thứ hai.
- Ba player ở mốc mở slot thứ ba.
- Team thuần damage.
- Team có tank.
- Team có sustain hoặc support.

Team DPS không luôn bằng tổng DPS từng người vì:

- Cùng chọn một enemy.
- AOE có thể trùng mục tiêu.
- Player khác nhau có thời gian vào range khác nhau.
- Enemy có thể chết trước Animation Event.
- Heal và Life Steal phụ thuộc damage thực tế.

Mỗi player được phép mạnh trong một team composition, nhưng không được bắt buộc trong
mọi team.

## 17. Mốc cân bằng

Tại mỗi checkpoint level, định nghĩa:

- Reference gear rarity.
- Reference ItemPower cho đủ 8 slot.
- Reference enemy HP, damage, Armor và số lượng.
- Target boss kill time.
- Target wave clear time.
- Target survival time.

Khuyến nghị dải so sánh với cùng level và reference gear:

| Chỉ số | Sai lệch cho phép |
|---|---:|
| Basic DPS trước skill | ±5% |
| Single target DPS của cùng role | ±8% |
| Three target DPS của cùng role | ±10% |
| EHP của cùng archetype | ±10% |
| Tổng hiệu quả trong role | ±10% |

Khác role có thể vượt các dải riêng lẻ nếu phần còn lại bù lại đúng budget.

## 18. Quy trình tạo player mới

1. Chọn CharacterType.
2. Chọn vai trò chiến đấu.
3. Điền baseline level 1.
4. Chọn Damage và Attack Speed sao cho Basic DPS đạt target.
5. Chọn HP và Armor theo EHP target.
6. Thiết kế tối đa hai skill được equip.
7. Tạo weapon affinity profile.
8. Test với gear ALL chuẩn.
9. Test với gear class-specific chuẩn.
10. Test rotation AutoCombat và Animation Event.
11. Test trong team một, hai và ba người.
12. Chỉnh skill hoặc affinity; không tăng tất cả base stat cùng lúc.

## 19. Hướng data đề xuất

Khi triển khai, nên có một SO profile riêng cho balance của player, tham chiếu từ
CharacterDataSO hoặc CharacterCombatSO.

Profile cần chứa:

- Growth rate theo StatType.
- Các mốc stat theo level.
- Main weapon affinity theo StatType.
- Off-hand affinity theo StatType.
- WeaponType affinity nếu dự án thêm WeaponType.
- Role target hoặc tag phục vụ simulation.

Profile là definition. Level, EXP, equipment và skill đang equip vẫn nằm trong save data
của từng player.

Không lưu Dictionary trực tiếp nếu muốn Inspector và JSON save ổn định. Dùng list entry
có enum + float giống cách StatValue hiện tại.

## 20. Kiểm thử bắt buộc

Mỗi player phải chạy simulation ở ít nhất:

- Level đầu game, giữa game và cuối game.
- Không có gear.
- Full gear chuẩn.
- Full gear high-roll.
- Gear ALL và gear đúng CharacterType.
- Một target và ba target.
- Enemy đứng sẵn trong range và enemy cần di chuyển tới.
- Không crit, average crit và high crit sample.
- Có và không có Life Steal.
- Có và không có Cooldown Reduction.

Phải cảnh báo khi:

- Affinity ngoài 0.85 đến 1.15 mà không có lý do được ghi rõ.
- Một player dẫn đầu cả DPS, AOE và EHP.
- Attack Speed tăng nhưng ActualAttackInterval không đổi do animation.
- CDR làm một skill chiếm gần toàn bộ rotation.
- Dodge, Life Steal hoặc CDR vượt hard cap nội dung.
- Item ALL luôn tốt hơn item class-specific hoặc ngược lại.
- Đổi vũ khí tạo chênh lệch Effective Power vượt budget dự kiến.

## 21. Acceptance Criteria

- Mỗi player có baseline, growth profile và weapon affinity rõ ràng.
- Eligibility vẫn do CharacterRequirementType quyết định.
- ItemPower chuẩn không đổi theo player.
- Effective Power có thể khác theo affinity.
- Hệ số weapon affinity mặc định là 1.00 và nằm trong dải kiểm soát.
- Player cùng CharacterType vẫn tạo được lối build khác nhau.
- Damage, animation, skill rotation và range đều được tính trong benchmark.
- Không giả định stat tự tăng theo level trước khi growth logic được triển khai.
- Save vẫn giữ riêng stat, equipment và skill của từng player khi ra khỏi team.
- Có simulation cho đội một, hai và ba thành viên.

Tài liệu equipment định nghĩa một item mạnh bao nhiêu. Tài liệu này định nghĩa player
khai thác sức mạnh đó như thế nào.
