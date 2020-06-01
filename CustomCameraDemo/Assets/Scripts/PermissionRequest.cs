using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermissionRequest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (HasMicrophone())
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("Microphone permission granted");
            }

            yield return 0;
        }

        // if (HasCamera())
        // {
        //     yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        //     if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        //     {
        //         Debug.Log("Camera permission granted");
        //     }
        //
        //     yield return 0;
        // }
    }

    private static bool HasMicrophone()
    {
        return Microphone.devices.Length > 0;
    }

    private static bool HasCamera()
    {
        return WebCamTexture.devices.Length > 0;
    }
}
