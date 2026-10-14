using System.Collections.Generic;
using UnityEngine;

public sealed class TextPopUpPool
{
    private readonly List<TextPopUp> popups = new List<TextPopUp>();

    public TextPopUp Spawn(TextPopUp prefab, Transform parent)
    {
        if (prefab == null || parent == null) return null;
        popups.RemoveAll(p => p == null || !p.gameObject.activeSelf || p.TF.parent != parent);
        TextPopUp popup = SimplePool.Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
        if (popup == null) return null;
        // SimplePool giu parent cua lan preload dau; moi noi su dung phai gan lai parent.
        popup.TF.SetParent(parent, false);
        popup.TF.SetAsLastSibling();
        if (!popups.Contains(popup)) popups.Add(popup);
        return popup;
    }

    public void OnDespawn(Transform owner)
    {
        foreach (TextPopUp popup in popups)
        {
            // Chi thu hoi popup cua view nay, tranh tat damage popup dang dung chung pool.
            if (popup != null && popup.gameObject.activeSelf && popup.TF.parent == owner)
            {
                SimplePool.Despawn(popup);
            }
        }
        popups.Clear();
    }
}
