using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeroStatusSkillSlotUI : GameUnit
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject equippedIcon;

    private CanvasHeroStatus owner;
    private CombatSkill skill;

    public override void OnSpawn()
    {
        Clear();
    }

    public override void OnDespawn()
    {
        Clear();
    }

    public void SetData(
        CanvasHeroStatus canvas,
        string characterId,
        CombatSkill targetSkill,
        bool isSelected)
    {
        owner = canvas;
        skill = targetSkill;
        int level = DataManager.Ins.GetCharacterSkillLevel(characterId, skill);
        if (skillIcon != null)
        {
            skillIcon.sprite = skill != null ? skill.IconSkill : null;
            skillIcon.enabled = skillIcon.sprite != null;
            skillIcon.color = level > 0
                ? Color.white
                : new Color(0.35f, 0.35f, 0.35f);
        }
        if (levelText != null)
        {
            levelText.text = level > 0 ? $"Lv.{level}" : "Locked";
        }
        SetSelected(isSelected);
        SetActive(equippedIcon,
            DataManager.Ins.IsCharacterSkillEquipped(characterId, skill));
    }

    public void SetSelected(bool value)
    {
        SetActive(selectedFrame, value);
    }

    public void OnButtonSelect()
    {
        if (skill != null)
        {
            owner?.SelectSkill(skill);
        }
    }

    private void Clear()
    {
        owner = null;
        skill = null;
        if (skillIcon != null)
        {
            skillIcon.sprite = null;
            skillIcon.enabled = false;
            skillIcon.color = Color.white;
        }
        if (levelText != null)
        {
            levelText.text = string.Empty;
        }
        SetActive(selectedFrame, false);
        SetActive(equippedIcon, false);
    }

    private static void SetActive(GameObject target, bool value)
    {
        if (target != null)
        {
            target.SetActive(value);
        }
    }
}
