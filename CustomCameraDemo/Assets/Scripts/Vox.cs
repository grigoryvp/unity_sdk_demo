using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Voximplant.Unity;
using Voximplant.Unity.Call;
using Voximplant.Unity.Call.EventArgs;
using Voximplant.Unity.Client;
using Voximplant.Unity.Client.EventArgs;

public class Vox : MonoBehaviour
{
    private bool _started;
    private IClient _client;
    private ICall _call;

    void Start()
    {
        VoximplantSdk.Initialize();
        _client = VoximplantSdk.GetClient();
        _client.Connected += ClientOnConnected;
        _client.LoginSuccess += ClientOnLoginSuccess;
        _client.LoginFailed += ClientOnLoginFailed;
        _client.Disconnected += ClientOnDisconnected;

        _started = true;
        _client.Connect();
    }

    private void ClientOnLoginFailed(IClient sender, LoginFailedEventArgs eventargs)
    {
        _started = false;
        _client.Disconnect();
    }

    private void ClientOnDisconnected(IClient sender)
    {
        if (_started)
        {
            _client.Connect();
        }
    }

    private void OnDestroy()
    {
        _started = false;
        if (_call != null)
        {
            _call.Connected -= CallOnConnected;
            _call.Disconnected -= CallOnDisconnected;
            _call.Hangup();
            _call = null;
        }
        _client.Connected -= ClientOnConnected;
        _client.LoginSuccess -= ClientOnLoginSuccess;
        _client.LoginFailed -= ClientOnLoginFailed;
        _client.Disconnected -= ClientOnDisconnected;
        _client.Disconnect();
    }

    void Call()
    {
        if (_call != null)
        {
            _call.Connected -= CallOnConnected;
            _call.Disconnected -= CallOnDisconnected;
            _call.Hangup();
            _call = null;
        }
        _call = _client.Call("stream", new CallSettings
        {
            VideoCodec = VideoCodec.H264,
            VideoFlags = new VideoFlags
            {
                ReceiveVideo = false,
                SendVideo = true,
            }
        });
        if (_call == null) return;
        _call.SetVideoSource(Camera.current, 640, 640 * Screen.height / Screen.width);
        _call.Connected += CallOnConnected;
        _call.Disconnected += CallOnDisconnected;
        _call.Start();
    }

    private void CallOnConnected(ICall sender, CallConnectedEventArgs eventargs)
    {
        _call.SendAudio(false);
    }

    private void CallOnDisconnected(ICall sender, CallDisconnectedEventArgs eventargs)
    {
        Call();
    }

    private void ClientOnLoginSuccess(IClient sender, LoginSuccessEventArgs eventargs)
    {
        if (_started)
        {
            Call();
        }
    }

    private void ClientOnConnected(IClient sender)
    {
        #error Enter valid credentials
        _client.Login("user@app.account.voximplant.com", "p@ssw0rd");
    }
}