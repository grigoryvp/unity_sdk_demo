using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voximplant.Unity;
using CameraType = Voximplant.Unity.Hardware.CameraType;

public class CameraSwitchScript : MonoBehaviour
{
    private bool _frontCameraActive = true;
    private void OnMouseDown()
    {
        _frontCameraActive = !_frontCameraActive;
        VoximplantSdk.GetCameraManager().Camera = _frontCameraActive ? CameraType.Front : CameraType.Back;
    }
}
