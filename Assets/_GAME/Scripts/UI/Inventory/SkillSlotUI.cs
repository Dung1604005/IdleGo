using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : GameUnit
{
    [SerializeField] private Image skillImage;

    public override void OnSpawn()
    {
        Clear();
    }

    public override void OnDespawn()
    {
        Clear();
    }

    public void SetSkill(CombatSkill skill)
    {
        if (skillImage == null)
        {
            return;
        }

        skillImage.sprite = skill != null ? skill.IconSkill : null;
        skillImage.enabled = skillImage.sprite != null;
    }

    public void SetCooldownProgress(float progress)
    {
        if (skillImage != null)
        {
            // Slot chi hien thi gia tri CharacterCombat cung cap, khong tu chay cooldown.
            skillImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public void Clear()
    {
        if (skillImage == null)
        {
            return;
        }

        skillImage.sprite = null;
        skillImage.fillAmount = 0f;
        skillImage.enabled = false;
    }
}
