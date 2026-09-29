using System;
using UnityEngine;

[Serializable]
public class PlayerProgression
{
    [SerializeField, Min(1)] private int globalLevel = 1;

    public int GlobalLevel => Mathf.Max(1, globalLevel);

    public bool SetLevel(int value)
    {
        int normalizedValue = Mathf.Max(1, value);
        if (globalLevel == normalizedValue)
        {
            return false;
        }

        globalLevel = normalizedValue;
        return true;
    }
}
