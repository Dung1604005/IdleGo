using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PanelHeroView : MonoBehaviour
{
    [SerializeField] private Transform tf;
    public void SetActive(bool active)
    {
        tf.gameObject.SetActive(active);
    }

    
}
