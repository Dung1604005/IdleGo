# Báo cáo thay đổi hệ Stat và Equipment

## Tổng quan

Hệ stat đã chuyển từ `equipmentStatBonuses` riêng biệt sang hệ `StatModifier` tổng quát. Equipment, pet, passive và buff đều có thể đóng vai trò là một `Source` cung cấp modifier cho nhân vật.

Luồng khởi tạo Player hiện tại:

```text
Reset base stat
→ Apply equipment modifier
→ Hồi đầy máu theo Max Health mới
→ Khởi tạo combat, movement và health bar
```

## StatModifier

Mỗi modifier gồm:

- `Source`: object tạo ra modifier, ví dụ equipment, pet hoặc buff.
- `StatType`: chỉ số bị ảnh hưởng.
- `Value`: giá trị thay đổi.
- `Operation`: cách áp giá trị.

Ba phép toán hiện có:

- `FLAT`: cộng thẳng vào base stat.
- `PERCENT_ADD`: các phần trăm được cộng chung; `0.1` tương đương 10%.
- `PERCENT_MULTIPLY`: từng phần trăm được nhân riêng.

Công thức tính:

```text
(Base + tổng Flat)
× (1 + tổng Percent Add)
× tích từng (1 + Percent Multiply)
```

`LEVEL` và `EXPERIENCE` không nhận modifier để tránh bonus bị ghi ngược vào dữ liệu progression.

API chính trong `CharacterStat`:

- `AddModifier(modifier)`
- `RemoveModifiersFromSource(source)`
- `ReplaceModifiersFromSources(oldSources, replacements)`
- `ClearAllModifiers()`

Kết quả stat được cache và chỉ tính lại khi base stat hoặc modifier thay đổi.

## Equipment runtime

`EquipmentDataSO` tiếp tục giữ definition bất biến. `Equipment` là instance runtime, giữ:

- `InstanceId` riêng.
- Data gốc.
- Trạng thái socket, enchantment và decoration.
- CharacterEquipment đang sở hữu nó.

Mỗi `StatValue` có thêm `Operation`. Dữ liệu cũ không có field này sẽ mặc định là `FLAT`.

Khi số slot trong SO giảm, dữ liệu ở slot dư được giữ lại nhưng không được áp stat. Getter không còn tự xóa dữ liệu runtime.

## CharacterEquipment

`listEquipment` là danh sách gọn các món đang mặc, không dùng vị trí list làm `EquipmentType`. Vì vậy thứ tự trong Inspector không ảnh hưởng kết quả và không còn lỗi list có capacity 8 nhưng count bằng 0.

Các hành vi chính:

- Mỗi `EquipmentType` chỉ có một món hoạt động.
- Một Equipment instance không thể được hai CharacterEquipment mặc cùng lúc.
- `TryEquip()` trả về món bị thay thế.
- `Unequip()` trả về món vừa tháo.
- `EquipmentChanged` thông báo cho UI hoặc inventory.
- Khi khảm/phù phép thay đổi, modifier từ equipment được dựng lại tự động.
- Chỉ modifier có source là equipment cũ bị thay; modifier từ pet, passive và buff không bị ảnh hưởng.

## Max Health

- Khi Player khởi tạo hoặc spawn: equipment được áp trước, sau đó máu được hồi đầy theo Max Health mới.
- Khi thay đồ trong gameplay: tăng Max Health không tự hồi máu; giảm Max Health sẽ clamp Current Health về giới hạn mới.

## Mở rộng pet, passive và buff

Mỗi hệ thống nên dùng một object có vòng đời ổn định làm `Source`, thêm modifier khi kích hoạt và xóa toàn bộ modifier bằng chính source đó khi kết thúc.

Modifier là dữ liệu runtime và không được save trực tiếp. Khi load game, pet/passive/buff/equipment phải tự đăng ký lại modifier từ trạng thái đã lưu.

## File liên quan

- [`StatModifier.cs`](../SO/Character/Stat/StatModifier.cs)
- [`StatType.cs`](../SO/Character/Stat/StatType.cs)
- [`StatValue.cs`](../SO/Character/Stat/StatValue.cs)
- [`CharacterStat.cs`](../Scripts/Character/CharacterStat.cs)
- [`Equipment.cs`](../Scripts/Character/Equipment.cs)
- [`CharacterEquipment.cs`](../Scripts/Character/CharacterEquipment.cs)
- [`Character.cs`](../Scripts/Character/Character.cs)
- [`Player.cs`](../Scripts/Character/Player.cs)
