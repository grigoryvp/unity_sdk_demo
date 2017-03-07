using UnityEngine;
using Voximplant;

public class Conference : MonoBehaviour
{
    VoximplantSDK vox;
    string ACC = "eyeofhell";

    void Start()
    {
        vox = gameObject.AddVoximplantSDK(); // extension method helper
        vox.init(granted => {
            if (granted) vox.connect(); // check audio and video permissions
        });
        vox.LogMethod += Debug.Log;
        vox.ConnectionSuccessful += () => {
            vox.login(new LoginClassParam("unity-demo-user@unity-demo-app." + ACC + ".voximplant.com", "unitydemopass"));
        };
        vox.LoginSuccessful += (name) => {
            vox.call(new CallClassParam("*", false, false, ""));
        };
        vox.CallConnected += (callid, headers) => {
            vox.beginUpdatingTextureWithVideoStream(VoximplantSDK.VideoStream.Remote, texture => {
                // Assign texture to same object each frame, ex
                // something.GetComponent<MeshRenderer>().material.mainTexture = texture;
            });
        };
        vox.ConnectionFailedWithError += (reason) => { };
        vox.LoginFailed += (reason) => { };
        vox.CallConnected += (callid, headers) => { };
        vox.CallDisconnected += (callid, headers) => { };
        vox.CallFailed += (callid, code, reason, heasers) => { };
    }
}
