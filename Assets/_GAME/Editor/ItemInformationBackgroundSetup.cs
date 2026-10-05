using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public static class ItemInformationBackgroundSetup
{
    public static bool NeedsSetup(Transform canvas)
    {
        Transform background = canvas != null
            ? ItemInformationUISetupFactory.FindChild(
                canvas, "Image-Background")
            : null;
        Button button = background != null
            ? background.GetComponent<Button>()
            : null;
        CanvasItemInfomationUI canvasUI = canvas != null
            ? canvas.GetComponent<CanvasItemInfomationUI>()
            : null;
        return button == null
            || button.onClick.GetPersistentEventCount() != 1
            || button.onClick.GetPersistentTarget(0) != canvasUI
            || button.onClick.GetPersistentMethodName(0) != "OnBackgroundClick";
    }

    public static void Setup(Transform canvas)
    {
        Transform background = ItemInformationUISetupFactory.FindChild(
            canvas, "Image-Background");
        CanvasItemInfomationUI canvasUI =
            canvas.GetComponent<CanvasItemInfomationUI>();
        if (background == null || canvasUI == null)
        {
            Debug.LogError("Item Information background or canvas is missing.");
            return;
        }

        Button button = ItemInformationUISetupFactory.GetOrAdd<Button>(
            background.gameObject);
        button.targetGraphic = background.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(
            button.onClick, canvasUI.OnBackgroundClick);
        EditorUtility.SetDirty(button);
    }
}
