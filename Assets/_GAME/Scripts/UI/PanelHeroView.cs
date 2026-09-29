using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelHeroView : PanelView
{
    [SerializeField] private string characterId;
    [SerializeField] private TextMeshProUGUI nameHeroTxt;
    [SerializeField] private TextMeshProUGUI levelHeroTxt;
    [SerializeField] private Image expProgress;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Transform tf;
    [SerializeField] private List<SkillSlotUI> skillSlotUIs = new List<SkillSlotUI>();
    [SerializeField] private List<EquipmentSlotUI> equipmentSlotUIs = new List<EquipmentSlotUI>();

    public override void OnInit()
    {
        EnsureSelectedPlayer();
        RefreshHeroView();
    }

    public override void OnUpdate()
    {
        // Moi frame chi doc tien do da duoc CharacterCombat tinh san.
        RefreshSkillCooldowns(GetSelectedPlayer());
    }

    public override void OnDespawn()
    {
        for (int i = 0; i < skillSlotUIs.Count; i++)
        {
            skillSlotUIs[i]?.OnDespawn();
        }

        for (int i = 0; i < equipmentSlotUIs.Count; i++)
        {
            equipmentSlotUIs[i]?.OnDespawn();
        }
    }

    public void SetActive(bool active)
    {
        if (tf != null)
        {
            tf.gameObject.SetActive(active);
        }
    }

    public void RefreshEquipmentSlotUI()
    {
        RefreshEquipmentSlotUI(GetSelectedPlayer());
    }

    public void RefreshHeroView()
    {
        Player player = GetSelectedPlayer();
        RefreshHeroInformation(player);
        RefreshSkillSlotUI(player);
        RefreshEquipmentSlotUI(player);
    }

    public void OnButtonNextPlayer()
    {
        ChangeSelectedPlayer(PlayerManager.Ins?.GetNextPlayerId(characterId));
    }

    public void OnButtonPrevPlayer()
    {
        ChangeSelectedPlayer(PlayerManager.Ins?.GetPrevPlayerId(characterId));
    }

    private void RefreshHeroInformation(Player player)
    {
        bool hasPlayer = player != null;
        if (nameHeroTxt != null)
        {
            nameHeroTxt.text = hasPlayer ? player.CharacterName : string.Empty;
        }

        if (levelHeroTxt != null)
        {
            levelHeroTxt.text = hasPlayer ? player.Stats.CurrentLevel.ToString() : string.Empty;
        }

        RefreshExperience(player);
        if (portraitImage != null)
        {
            portraitImage.sprite = hasPlayer ? player.Portrait : null;
            portraitImage.enabled = portraitImage.sprite != null;
        }
    }

    private void RefreshExperience(Player player)
    {
        if (expProgress == null)
        {
            return;
        }

        int requiredExperience = player != null
            ? player.Stats.GetExpToNextLevel(player.Stats.CurrentLevel)
            : 0;
        expProgress.fillAmount = requiredExperience > 0
            ? Mathf.Clamp01(player.Stats.CurrentExperience / (float)requiredExperience)
            : 0f;
    }

    private void RefreshSkillSlotUI(Player player)
    {
        CharacterCombat combat = player != null ? player.Combat : null;
        for (int i = 0; i < skillSlotUIs.Count; i++)
        {
            SkillSlotUI slot = skillSlotUIs[i];
            slot?.OnSpawn();
            slot?.SetSkill(combat != null ? combat.GetEquippedSkill(i) : null);
        }

        RefreshSkillCooldowns(player);
    }

    private void RefreshSkillCooldowns(Player player)
    {
        CharacterCombat combat = player != null ? player.Combat : null;
        for (int i = 0; i < skillSlotUIs.Count; i++)
        {
            float progress = combat != null
                ? combat.GetSkillCooldownProgress(i)
                : 0f;
            skillSlotUIs[i]?.SetCooldownProgress(progress);
        }
    }

    private void RefreshEquipmentSlotUI(Player player)
    {
        IReadOnlyList<Equipment> equipments = player?.Equipment?.EquippedItems;
        for (int i = 0; i < equipmentSlotUIs.Count; i++)
        {
            EquipmentSlotUI slot = equipmentSlotUIs[i];
            Equipment equipment = equipments != null && i < equipments.Count
                ? equipments[i]
                : null;
            slot?.OnSpawn();
            slot?.SetData(equipment?.Data);
        }
    }

    private void EnsureSelectedPlayer()
    {
        if (GetSelectedPlayer() != null)
        {
            return;
        }

        Player firstPlayer = PlayerManager.Ins != null
            ? PlayerManager.Ins.GetPlayer(0)
            : null;
        characterId = firstPlayer != null ? firstPlayer.CharacterId : string.Empty;
    }

    private Player GetSelectedPlayer()
    {
        return PlayerManager.Ins != null
            ? PlayerManager.Ins.GetCharacter(characterId)
            : null;
    }

    private void ChangeSelectedPlayer(string nextCharacterId)
    {
        if (string.IsNullOrWhiteSpace(nextCharacterId))
        {
            return;
        }

        characterId = nextCharacterId;
        RefreshHeroView();
    }
}
