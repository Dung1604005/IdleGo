using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Video;

public class PanelHeroView : PanelView
{
    [SerializeField] private String characterId;
    [SerializeField] private Transform tf;

    [SerializeField] private List<SkillSlotUI> skillSlotUIs = new List<SkillSlotUI>();

    [SerializeField] private List<EquipmentSlotUI> equipmentSlotUIs = new List<EquipmentSlotUI>();

    public override void OnInit()
    {
        foreach(EquipmentSlotUI equipmentSlotUI in equipmentSlotUIs)
        {
            equipmentSlotUI.OnSpawn();
        }
    }
    public void SetActive(bool active)
    {
        tf.gameObject.SetActive(active);
    }

    public void RefreshEquipmentSlotUI()
    {
        for(int i = 0; i < equipmentSlotUIs.Count; i++)
        {
            equipmentSlotUIs[i].OnSpawn();
            EquipmentDataSO equipmentDataSO = PlayerManager.Ins.GetCharacter(characterId).Equipment.Data.Equipments[i].Data;
            equipmentSlotUIs[i].SetData(equipmentDataSO);
        }
        
    }

    public void OnButtonNextPlayer()
    {
        characterId = PlayerManager.Ins.GetNextPlayerId(characterId);
        RefreshEquipmentSlotUI();
    }

    public void OnButtonPrevPlayer()
    {
        characterId = PlayerManager.Ins.GetPrevPlayerId(characterId);
        RefreshEquipmentSlotUI();
    }




    
    
}
