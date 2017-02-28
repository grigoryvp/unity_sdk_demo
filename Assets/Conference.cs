using UnityEngine;
using System.Collections.Generic;
using Voximplant;

public class Conference : MonoBehaviour
{
    InvSDK vox;
    string ACC = "eyeofhell";

    void Start()
    {
        vox = gameObject.AddComponent<InvSDK>();
        vox.init(gameObject.name, granted => {
            if (granted) vox.connect();
        });
        vox.LogMethod += Debug.Log;
        vox.onConnectionSuccessful += onConnectionSuccessfull;
        vox.onConnectionFailedWithError += onConnectionFailed;
        vox.onLoginSuccessful += onLoginSuccessful;
        vox.onLoginFailed += onLoginFailed;
        vox.onCallConnected += OnCallConnected;
        vox.onCallDisconnected += OnCallDisconnected;
        vox.onCallFailed += OnCallFailed;
    }

    private void onConnectionSuccessfull()
    {
        vox.login(new LoginClassParam("unity-demo-user@unity-demo-app." + ACC + ".voximplant.com", "unitydemopass"));
    }

    private void onLoginSuccessful(string name)
    {
        vox.call(new CallClassParam("*", false, false, ""));
    }

    private void onConnectionFailed(string reason) { }
    private void onLoginFailed(LoginFailureReason reason) { }
    private void OnCallConnected(string callid, Dictionary<string, string> headers) { }
    private void OnCallDisconnected(string callid, Dictionary<string, string> headers) { }
    private void OnCallFailed(string callId, int code, string reason, Dictionary<string, string> headers) { }
}
