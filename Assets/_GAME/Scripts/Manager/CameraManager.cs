using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private List<Camera> cameraList = new List<Camera>();


    public Camera GetCamera(CameraType cameraType)
    {
        return cameraList[(int)cameraType];
    }
}


public enum CameraType
{
    MAIN = 0,
    UI = 1
}
