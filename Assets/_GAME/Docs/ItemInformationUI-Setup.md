# Hướng dẫn setup Item Information UI

Tài liệu này hướng dẫn setup `PanelItemInformation` để hiển thị thông tin của
`Equipment`, gồm ba vùng theo thứ tự:

1. Main stat.
2. Substat.
3. Enhancement slot: Socket, Enchantment và Decoration.

Các dòng stat được lấy từ `StatLine.prefab` thông qua `SimplePool`. Hai đường
phân cách và ba header BuffStat là các object có sẵn trong hierarchy. Code sẽ
tự đổi thứ tự sibling, bật/tắt object và rebuild layout khi nội dung thay đổi.

## 1. Hierarchy đề xuất

```text
Panel-ItemInformation
├── Image-Background
└── PanelLayoutContent                         [VerticalLayoutGroup]
    ├── FixedHeader                            [LayoutElement]
    │   ├── Image-ItemIcon
    │   ├── Text-ItemName                      [TextMeshProUGUI]
    │   ├── Text-Rarity                        [TextMeshProUGUI]
    │   └── Text-MainStats                     [TextMeshProUGUI]
    ├── DynamicContent                         [VerticalLayoutGroup]
    │   ├── Image-Line-MainToSub               [Image + LayoutElement]
    │   ├── Image-Line-SubToEnhancement        [Image + LayoutElement]
    │   ├── Header-Socket                      [BuffStatHeaderUI]
    │   │   ├── Image-Icon
    │   │   └── Text-Name                      [TextMeshProUGUI]
    │   ├── Header-Enchantment                 [BuffStatHeaderUI]
    │   │   ├── Image-Icon
    │   │   └── Text-Name                      [TextMeshProUGUI]
    │   └── Header-Decoration                  [BuffStatHeaderUI]
    │       ├── Image-Icon
    │       └── Text-Name                      [TextMeshProUGUI]
    ├── Text-LevelRequirement                  [TextMeshProUGUI]
    └── Text-CharacterRequirement              [TextMeshProUGUI]
```

`DynamicContent` chỉ được chứa:

- Hai separator.
- Ba `BuffStatHeaderUI`.
- Các `StatLineUI` được pool tạo ra khi chạy.

Không đặt main stat, level requirement hoặc object trang trí khác vào
`DynamicContent`, vì code dùng `SetSiblingIndex()` để sắp lại toàn bộ nội dung
động bên trong object này.

## 2. Component trên Panel-ItemInformation

Gắn hai component sau lên mỗi panel:

| Component | Chức năng |
|---|---|
| `PanelItemInformation` | Lấy Equipment cần hiển thị từ `DataManager` và cập nhật thông tin chung. |
| `EquipmentStatsContentUI` | Tạo substat/enhancement bằng pool, điều khiển separator và resize panel. |

### PanelItemInformation

| Field | Object cần gán | Ghi chú |
|---|---|---|
| `Panel Type` | `SELECTED_ITEM` hoặc `EQUIPPED_COMPARISON` | Panel bên trái dùng selected item; panel so sánh dùng equipped item. |
| `Item Icon` | `Image-ItemIcon` | Icon Equipment. |
| `Item Name Text` | `Text-ItemName` | Bắt buộc dùng `TextMeshProUGUI`. |
| `Rarity Text` | `Text-Rarity` | Bắt buộc dùng `TextMeshProUGUI`. |
| `Main Stats Text` | `Text-MainStats` | Giữ nguyên cách hiển thị main stat hiện tại. |
| `Level Requirement Text` | `Text-LevelRequirement` | Bắt buộc dùng `TextMeshProUGUI`. |
| `Character Requirement Text` | `Text-CharacterRequirement` | Tự ẩn nếu Equipment dùng được cho mọi class. |
| `Stats Content` | Component `EquipmentStatsContentUI` cùng panel | Không gán `DynamicContent` trực tiếp vào field này. |

Các field cũ `Sub Stats Text` và `Enhancement Slots Text` không còn được dùng.
Mỗi stat hiện được hiển thị bằng một `StatLineUI` riêng.

## 3. Setup PanelLayoutContent

Gắn `VerticalLayoutGroup` lên `PanelLayoutContent`.

| Thuộc tính | Giá trị đề xuất |
|---|---|
| `Child Alignment` | `Upper Center` |
| `Control Child Size > Width` | Bật |
| `Control Child Size > Height` | Bật |
| `Use Child Scale` | Tắt cả hai trục |
| `Child Force Expand > Width` | Bật |
| `Child Force Expand > Height` | Tắt |
| `Spacing` | Tùy giao diện, ví dụ `4`–`8` |
| `Padding` | Tùy khung nền của panel |

Có thể gắn `ContentSizeFitter` lên `PanelLayoutContent`:

| Thuộc tính | Giá trị |
|---|---|
| `Horizontal Fit` | `Unconstrained` |
| `Vertical Fit` | `Preferred Size` |

Nếu một container con không tự cung cấp preferred height, thêm `LayoutElement`
và đặt `Preferred Height`. Các text trực tiếp dùng `TextMeshProUGUI` có thể tự
cung cấp preferred height theo nội dung.

## 4. Setup DynamicContent

Gắn `VerticalLayoutGroup` lên `DynamicContent`.

| Thuộc tính | Giá trị đề xuất |
|---|---|
| `Child Alignment` | `Upper Center` |
| `Control Child Size > Width` | Bật |
| `Control Child Size > Height` | Bật |
| `Use Child Scale` | Tắt cả hai trục |
| `Child Force Expand > Width` | Bật |
| `Child Force Expand > Height` | Tắt |
| `Spacing` | Khoảng cách giữa các stat, ví dụ `2`–`5` |

Gắn thêm `ContentSizeFitter`:

| Thuộc tính | Giá trị |
|---|---|
| `Horizontal Fit` | `Unconstrained` |
| `Vertical Fit` | `Preferred Size` |

Các object động phải là con trực tiếp của `DynamicContent`. Không tạo thêm một
container ở giữa `DynamicContent` và separator/header/StatLine.

## 5. Setup EquipmentStatsContentUI

| Field | Object cần gán | Yêu cầu |
|---|---|---|
| `Dynamic Content` | `DynamicContent` | Có `VerticalLayoutGroup`. |
| `Stat Line Prefab` | `Assets/_GAME/Prefabs/UI/StatLine.prefab` | Prefab kế thừa `GameUnit` và được spawn qua pool. |
| `Main To Sub Stat Line` | `Image-Line-MainToSub` | Separator giữa main stat và substat. |
| `Sub Stat To Enhancement Line` | `Image-Line-SubToEnhancement` | Separator giữa substat và enhancement. |
| `Buff Stat Headers[0]` | `Header-Socket` | Index `0` tương ứng `BuffStatType.SOCKET`. |
| `Buff Stat Headers[1]` | `Header-Enchantment` | Index `1` tương ứng `BuffStatType.ENCHANTMENT`. |
| `Buff Stat Headers[2]` | `Header-Decoration` | Index `2` tương ứng `BuffStatType.DECORATION`. |
| `Panel Rect` | RectTransform của `Panel-ItemInformation` | RectTransform sẽ đổi chiều cao. |
| `Panel Layout Content` | `PanelLayoutContent` | Code lấy preferred height từ object này. |

Thứ tự object trong hierarchy lúc edit không quan trọng. Khi refresh, code sẽ
gọi `SetSiblingIndex()` để sắp xếp theo dữ liệu thực tế.

## 6. Setup hai separator

Tạo hai GameObject UI có component `Image`. Có thể duplicate separator mà bạn
đã thiết kế để chúng có cùng sprite, material và màu.

| Object | Component | Giá trị đề xuất |
|---|---|---|
| `Image-Line-MainToSub` | `Image` | Sprite/màu theo giao diện. Tắt `Raycast Target`. |
| `Image-Line-MainToSub` | `LayoutElement` | `Preferred Height = 3`–`5`. |
| `Image-Line-SubToEnhancement` | `Image` | Dùng cùng visual với separator đầu. |
| `Image-Line-SubToEnhancement` | `LayoutElement` | `Preferred Height = 3`–`5`. |

Không chỉnh vị trí Y bằng tay. `VerticalLayoutGroup` sẽ đặt vị trí dựa trên số
dòng phía trên. Hai field trong code chỉ giữ reference đến GameObject, không giữ
tọa độ cố định.

### Quy tắc hiển thị separator hiện tại

| Có substat | Có enhancement slot | Separator Main → Sub | Separator Sub → Enhancement |
|---:|---:|---:|---:|
| Không | Không | Ẩn | Ẩn |
| Có | Không | Hiện | Ẩn |
| Không | Có | Ẩn | Ẩn |
| Có | Có | Hiện | Hiện |

Nếu Equipment không có substat nhưng có enhancement slot, enhancement được đặt
ngay sau main stat và không có separator, đúng theo quy tắc hiện tại.

## 7. Setup ba BuffStat header

Mỗi header là một GameObject con trực tiếp của `DynamicContent`.

Ví dụ:

```text
Header-Socket                         [BuffStatHeaderUI + LayoutElement]
├── Image-Icon                       [Image]
└── Text-Name                        [TextMeshProUGUI]
```

### BuffStatHeaderUI

| Field | Object cần gán |
|---|---|
| `Icon Image` | Child `Image-Icon`. |
| `Name Text` | Child `Text-Name` dùng `TextMeshProUGUI`. |

Thêm `LayoutElement` lên root của header và đặt `Preferred Height` phù hợp với
giao diện. Header sẽ tự lấy:

- Sprite từ `DataManager.BuffStatVisualSO`.
- Màu từ `DataManager.BuffStatVisualSO`.
- Tên Socket, Enchantment hoặc Decoration theo `BuffStatType`.

Header chỉ hiện khi Equipment có số slot của loại tương ứng lớn hơn `0`.

## 8. Setup StatLine.prefab

Prefab hiện tại đã được gán sẵn các reference trong `StatLineUI`:

| Field | Child trong prefab |
|---|---|
| `Tf` | RectTransform root `StatLine`. |
| `Stat Text` | Text hiển thị tên và giá trị stat. |
| `Image Rank Stat` | `Image-RankType`. |
| `Text Rank Stat` | `Text (TMP)-RankType`. |

Thêm `LayoutElement` lên root `StatLine` nếu chưa có:

| Thuộc tính | Giá trị đề xuất |
|---|---|
| `Preferred Height` | `27` hoặc bằng chiều cao thiết kế hiện tại. |
| `Flexible Height` | `0` |

Không đặt prefab `StatLine` trực tiếp và cố định trong hierarchy của panel.
`EquipmentStatsContentUI` sẽ preload và spawn nó bằng `SimplePool`.

### Cách StatLine hiển thị

| Loại dòng | Stat text | Image rank | Text rank |
|---|---|---|---|
| Substat | Tên stat và giá trị | Visual mặc định của prefab | Text mặc định của prefab |
| Enhancement trống | `Empty` | Visual mặc định của prefab | Text mặc định của prefab |
| Enhancement có stat | Tên stat và giá trị | Màu theo rarity Equipment | `T1` đến `T8` |

Tier được tính theo thứ tự `RarityType`:

| Rarity | Tier |
|---|---:|
| Common | T1 |
| Uncommon | T2 |
| Rare | T3 |
| Legend | T4 |
| Demon | T5 |
| Arcana | T6 |
| Beyond | T7 |
| Celestial | T8 |

## 9. Setup DataManager

Kiểm tra `DataManager` đã được gán hai ScriptableObject sau:

| Field | Dữ liệu cần có |
|---|---|
| `Rarity Color Data` | Màu của đủ 8 `RarityType`. |
| `Buff Stat Visual SO` | Đủ 3 icon và 3 màu theo đúng index BuffStatType. |

Thứ tự list trong `BuffStatVisualSO`:

| Index | BuffStatType |
|---:|---|
| 0 | Socket |
| 1 | Enchantment |
| 2 | Decoration |

## 10. Setup CanvasItemInfomationUI

| Field | Object cần gán |
|---|---|
| `Selected Item Panel` | Panel có `Panel Type = SELECTED_ITEM`. |
| `Equipped Comparison Panel` | Panel có `Panel Type = EQUIPPED_COMPARISON`. |

Hai panel có thể dùng cùng một `StatLine.prefab`. Khi lấy object từ pool, code
sẽ tự gán lại parent để StatLine về đúng `DynamicContent` của panel đang dùng.

## 11. Cách panel thay đổi chiều cao

Sau khi tạo nội dung, code thực hiện theo thứ tự:

1. Bật/tắt separator và BuffStat header.
2. Spawn các `StatLineUI` cần thiết.
3. Đặt lại sibling index theo thứ tự hiển thị.
4. Rebuild `DynamicContent`.
5. Lấy preferred height của `PanelLayoutContent`.
6. Gán chiều cao đó cho `Panel Rect`.

Để panel nở xuống dưới và giữ nguyên cạnh trên, đặt RectTransform của panel:

| Thuộc tính | Giá trị đề xuất |
|---|---|
| `Anchor` | Neo phía trên vùng chứa panel. |
| `Pivot Y` | `1` |

Nếu `Pivot Y = 0.5`, panel sẽ nở đồng thời lên trên và xuống dưới.

## 12. Checklist kiểm tra trong Play Mode

- [ ] Bấm Equipment có substat: separator đầu và từng StatLine xuất hiện.
- [ ] Bấm Equipment không có substat: separator đầu biến mất.
- [ ] Equipment không có slot của một BuffStatType: header loại đó biến mất.
- [ ] Slot enhancement trống hiển thị `Empty` và rank mặc định.
- [ ] Slot enhancement có stat hiển thị tier và màu theo rarity.
- [ ] Main weapon hiển thị attacks per second.
- [ ] Off-hand weapon không hiển thị attacks per second.
- [ ] Panel dài thêm khi số dòng tăng và ngắn lại khi số dòng giảm.
- [ ] Panel so sánh không xuất hiện nếu hero chưa trang bị Equipment cùng loại.
- [ ] Mở/đóng UI nhiều lần không tạo thêm StatLine ngoài pool.

## 13. Lỗi setup thường gặp

| Hiện tượng | Nguyên nhân thường gặp | Cách sửa |
|---|---|---|
| Các dòng chồng lên nhau | `DynamicContent` thiếu `VerticalLayoutGroup`. | Thêm và setup theo mục 4. |
| Separator đứng yên | Separator không phải con trực tiếp của `DynamicContent`, hoặc đang đặt tọa độ thủ công ngoài layout. | Chuyển separator vào đúng hierarchy. |
| StatLine có chiều cao bằng 0 | Root prefab thiếu `LayoutElement` khi layout đang control child height. | Đặt `Preferred Height` cho root StatLine. |
| Panel không đổi chiều cao | `Panel Rect` hoặc `Panel Layout Content` chưa được gán. | Gán đủ hai field trong Inspector. |
| Header không có icon/màu | `BuffStatVisualSO` chưa gán trong `DataManager` hoặc list thiếu phần tử. | Gán đủ ba icon/màu theo đúng index. |
| Substat bị đổi màu rank | Prefab đang lưu visual mặc định không đúng hoặc reference rank gán sai. | Kiểm tra ba reference trong `StatLineUI`. |
| Tier không hiện | Slot đang trống hoặc `Text Rank Stat` chưa được gán. | Gắn stat vào slot hoặc sửa reference prefab. |
| Nội dung panel nở về hai phía | Pivot Y của panel đang là `0.5`. | Đổi Pivot Y thành `1`. |
