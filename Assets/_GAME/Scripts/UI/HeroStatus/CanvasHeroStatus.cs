using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasHeroStatus : UICanvas, IInventoryView
{
    [Header("Hero")]
    [SerializeField] private Image heroAvatar;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Image experienceFill;
    [SerializeField] private TextMeshProUGUI statPointText;

    [Header("Stat Progress")]
    [SerializeField] private RectTransform statContent;
    [SerializeField] private StatProgressLineUI statLinePrefab;

    [Header("Skills")]
    [SerializeField] private RectTransform skillContent;
    [SerializeField] private HeroStatusSkillSlotUI skillSlotPrefab;
    [SerializeField] private HeroSkillInformationPanel skillInformationPanel;

    private readonly List<StatProgressLineUI> activeStatLines =
        new List<StatProgressLineUI>();
    private readonly List<HeroStatusSkillSlotUI> activeSkillSlots =
        new List<HeroStatusSkillSlotUI>();
    private CombatSkill selectedSkill;

    public override void SetUp()
    {
        OnInit();
    }

    public void OnInit()
    {
        OnDespawn();
        EnsureSelectedCharacter();
        SimplePool.PreLoad(statLinePrefab, StatTypeUtility.StatCount, statContent);
        SimplePool.PreLoad(skillSlotPrefab, 6, skillContent);
        DataManager.Ins.InventoryData?.RegisterView(this);
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        // View chi doc du lieu qua DataManager sau khi data da save xong.
        string characterId = DataManager.Ins.SelectedCharacterId;
        RefreshHeader(characterId);
        RefreshStatLines(characterId);
        RefreshSkillSlots(characterId);
        skillInformationPanel?.Refresh(this, characterId, selectedSkill);
    }

    public void OnButtonResetStatPoints()
    {
        DataManager.Ins.ResetCharacterStatPoints(
            DataManager.Ins.SelectedCharacterId);
    }

    public void OnButtonClose()
    {
        CloseDirectly();
    }

    public void SelectSkill(CombatSkill skill)
    {
        selectedSkill = skill;
        RefreshInventory();
    }

    public void CloseSkillInformation()
    {
        selectedSkill = null;
        skillInformationPanel?.Hide();
        RefreshSkillSelection();
    }

    public override void CloseDirectly()
    {
        OnDespawn();
        base.CloseDirectly();
    }

    public void OnDespawn()
    {
        DataManager.Ins.InventoryData?.UnregisterView(this);
        ClearStatLines();
        ClearSkillSlots();
        selectedSkill = null;
        skillInformationPanel?.Hide();
    }

    private void RefreshHeader(string characterId)
    {
        int level = Mathf.RoundToInt(DataManager.Ins.GetCharacterStat(
            characterId, StatType.LEVEL));
        int experience = Mathf.RoundToInt(DataManager.Ins.GetCharacterStat(
            characterId, StatType.EXPERIENCE));
        int required = DataManager.Ins.GetCharacterExperienceRequired(characterId);
        SetText(heroNameText, DataManager.Ins.GetCharacterName(characterId));
        SetText(levelText, $"Level {level}");
        SetText(experienceText, $"EXP {experience} / {required}");
        SetText(statPointText,
            $"Stat Points: {DataManager.Ins.GetCharacterAvailableStatPoints(characterId)}");
        SetImage(heroAvatar, DataManager.Ins.GetCharacterPortrait(characterId));
        if (experienceFill != null)
        {
            experienceFill.fillAmount = required > 0
                ? Mathf.Clamp01(experience / (float)required)
                : 0f;
        }
    }

    private void RefreshStatLines(string characterId)
    {
        // Level va EXP da nam o header; cac stat con lai dung chung pooled line.
        ClearStatLines();
        for (int i = 0; i < StatTypeUtility.StatCount; i++)
        {
            StatType statType = (StatType)i;
            if (!StatTypeUtility.CanHaveModifiers(statType))
            {
                continue;
            }

            StatProgressLineUI line = SpawnStatLine();
            line?.SetData(characterId, statType);
        }
    }

    private void RefreshSkillSlots(string characterId)
    {
        ClearSkillSlots();
        IReadOnlyList<CombatSkill> skills =
            DataManager.Ins.GetCharacterProgressSkills(characterId);
        if (!ContainsSkill(skills, selectedSkill))
        {
            selectedSkill = null;
        }

        for (int i = 0; skills != null && i < skills.Count; i++)
        {
            HeroStatusSkillSlotUI slot = SpawnSkillSlot();
            slot?.SetData(this, characterId, skills[i], skills[i] == selectedSkill);
        }
    }

    private StatProgressLineUI SpawnStatLine()
    {
        StatProgressLineUI line = SimplePool.Spawn(
            statLinePrefab, Vector3.zero, Quaternion.identity, statContent);
        if (line != null)
        {
            line.TF.SetParent(statContent, false);
            activeStatLines.Add(line);
        }
        return line;
    }

    private HeroStatusSkillSlotUI SpawnSkillSlot()
    {
        HeroStatusSkillSlotUI slot = SimplePool.Spawn(
            skillSlotPrefab, Vector3.zero, Quaternion.identity, skillContent);
        if (slot != null)
        {
            slot.TF.SetParent(skillContent, false);
            activeSkillSlots.Add(slot);
        }
        return slot;
    }

    private void ClearStatLines()
    {
        for (int i = activeStatLines.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeStatLines[i]);
        }
        activeStatLines.Clear();
    }

    private void ClearSkillSlots()
    {
        for (int i = activeSkillSlots.Count - 1; i >= 0; i--)
        {
            SimplePool.Despawn(activeSkillSlots[i]);
        }
        activeSkillSlots.Clear();
    }

    private void RefreshSkillSelection()
    {
        for (int i = 0; i < activeSkillSlots.Count; i++)
        {
            activeSkillSlots[i]?.SetSelected(false);
        }
    }

    private void EnsureSelectedCharacter()
    {
        if (!string.IsNullOrWhiteSpace(DataManager.Ins.SelectedCharacterId))
        {
            return;
        }

        DataManager.Ins.SetSelectedCharacter(
            DataManager.Ins.GetTeamCharacterId(0));
    }

    private static bool ContainsSkill(
        IReadOnlyList<CombatSkill> skills,
        CombatSkill target)
    {
        if (target == null)
        {
            return true;
        }
        for (int i = 0; skills != null && i < skills.Count; i++)
        {
            if (ReferenceEquals(skills[i], target))
            {
                return true;
            }
        }
        return false;
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }
}
