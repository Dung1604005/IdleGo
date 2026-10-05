using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffStatHeaderUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    public void OnInit(BuffStatType buffStatType)
    {
        BuffStatVisualSO visual = DataManager.Ins.BuffStatVisualSO;
        Color color = visual != null
            ? visual.GetColorBuffStat(buffStatType)
            : Color.white;

        SetIcon(visual?.GetIconBuffStat(buffStatType), color);
        SetName(EquipmentInformationText.GetBuffStatName(buffStatType), color);
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    private void SetIcon(Sprite icon, Color color)
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    private void SetName(string value, Color color)
    {
        if (nameText == null)
        {
            return;
        }

        nameText.text = value;
        nameText.color = color;
    }
}
