using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Voximplant.Unity;
using Voximplant.Unity.Call;
using Voximplant.Unity.Call.EventArgs;
using Voximplant.Unity.Client;
using Voximplant.Unity.Client.EventArgs;

public class CallButtonTouchScript : MonoBehaviour
{
    private int _screenWidth = 0;

    private IClient _client;
    private ICall _call;

    private Renderer _remote;
    private IVideoStream _remoteVideoStream;

    private Renderer _local;
    private IVideoStream _localVideoStream;

    private Texture2D _defaultTexture;

    private static readonly Color BackgroundColor = new Color32(57, 43, 91, 255);
    private static readonly Color ButtonColor = new Color32(102, 46, 255, 255);
    private static readonly Color ButtonHangupColor = new Color32(245, 75, 94, 255);

    private Renderer _callButton;
    private TextMesh _callButtonText;

    private static readonly CallSettings CallSettings = new CallSettings
    {
        CustomData = "Unity Video Call Demo",
        VideoFlags = new VideoFlags
        {
            SendVideo = true,
            ReceiveVideo = true
        }
    };

    #error Enter valid credentials
    private const string Login = "";
    private const string Password = "";
    private const string Callee = "";

    private bool _inCall;

    private void OnMouseDown()
    {
        if (_call == null)
        {
            _call = _client.Call(Callee, CallSettings);
            if (_call == null) return;
            BindCallEventHandlers();
            _call.Start();

            _callButtonText.text = "Hangup";
            _callButton.material.color = ButtonHangupColor;
        }
        else
        {
            _call.Hangup();
        }
    }

    private void BindCallEventHandlers()
    {
        _call.LocalVideoStreamAdded += CallVideoStreamAdded;
        _call.LocalVideoStreamRemoved += CallVideoStreamRemoved;
        _call.EndpointAdded += (o, args) =>
        {
            args.Endpoint.RemoteVideoStreamAdded += EndpointVideoStreamAdded;
            args.Endpoint.RemoteVideoStreamRemoved += EndpointVideoStreamRemoved;
        };
        foreach (var endpoint in _call.Endpoints)
        {
            endpoint.RemoteVideoStreamAdded += EndpointVideoStreamAdded;
            endpoint.RemoteVideoStreamRemoved += EndpointVideoStreamRemoved;
        }

        _call.Connected += (sender, args) =>
        {
            _callButtonText.text = "Hangup";
            _callButton.material.color = ButtonHangupColor;
        };
        _call.Disconnected += (sender, args) =>
        {
            _callButton.material.color = ButtonColor;
            _callButtonText.text = $"Call: {Callee}";
            _call = null;

            _localVideoStream = null;
            _remoteVideoStream = null;

            _screenWidth = 0;
        };
        _call.Failed += (sender, args) =>
        {
            _callButton.material.color = ButtonColor;
            _callButtonText.text = $"Call: {Callee}";
            Debug.LogError(args.Error.Message);
            _call = null;

            _localVideoStream = null;
            _remoteVideoStream = null;

            _screenWidth = 0;
        };
    }

    private void RefreshRenderers()
    {
        var height = 2.0f * Mathf.Tan(0.5f * Camera.main.fieldOfView * Mathf.Deg2Rad) * 2.0f;
        var width = height * Screen.width / Screen.height;
        Debug.Log($"RefreshRenderers: screen {Screen.width}x{Screen.height}");
        Debug.Log($"RefreshRenderers: {width}x{height}");

        var localWidth = width / 4;
        var localHeight = height / 4;
        if (_localVideoStream != null)
        {
            Debug.Log($"RefreshRenderers: local {_localVideoStream.StreamId}");
            if (_localVideoStream.Width >= _localVideoStream.Height)
            {
                localHeight = localWidth * _localVideoStream.Height / _localVideoStream.Width;
                Debug.Log($"RefreshRenderers: local,horizontal {localWidth}x{localHeight}");
            }
            else
            {
                localWidth = localHeight * _localVideoStream.Width / _localVideoStream.Height;
                Debug.Log($"RefreshRenderers: local,vertical {localWidth}x{localHeight}");
            }
        }
        else
        {
            Debug.Log($"RefreshRenderers: local {localWidth}x{localHeight}");
        }

        var local = _local.gameObject;
        local.transform.localScale = new Vector3(localWidth, 1, localHeight);
        var point = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 19.9f));
        Debug.Log($"RefreshRenderers: hit ({point.x},{point.y},{point.z})");
        point.x -= _local.bounds.size.x / 2;
        point.y -= _local.bounds.size.y / 2;
        Debug.Log($"RefreshRenderers: hit ({point.x},{point.y},{point.z})");
        local.transform.position = point;

        var remoteWidth = width;
        var remoteHeight = height;
        if (_remoteVideoStream != null && _remoteVideoStream.Width > 0 && _remoteVideoStream.Height > 0)
        {
            Debug.Log($"RefreshRenderers: remote {_remoteVideoStream.StreamId} {_remoteVideoStream.Rotation}");
            if (_remoteVideoStream.Width >= _remoteVideoStream.Height)
            {
                remoteHeight = remoteWidth * _remoteVideoStream.Height / _remoteVideoStream.Width;
                if (remoteHeight > height)
                {
                    remoteHeight = height;
                    remoteWidth = remoteHeight * _remoteVideoStream.Width / _remoteVideoStream.Height;
                }

                Debug.Log($"RefreshRenderers: remote,horizontal {remoteWidth}x{remoteHeight}");
            }
            else
            {
                remoteWidth = remoteHeight * _remoteVideoStream.Width / _remoteVideoStream.Height;
                if (remoteWidth > width)
                {
                    remoteWidth = width;
                    remoteHeight = remoteWidth * _remoteVideoStream.Height / _remoteVideoStream.Width;
                }

                Debug.Log($"RefreshRenderers: remote,vertical {remoteWidth}x{remoteHeight}");
            }
        }

        _remote.gameObject.transform.localScale = new Vector3(remoteWidth, 1, remoteHeight);

        var buttonPoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, 19.8f));
        Debug.Log($"RefreshRenderers: hit ({point.x},{point.y},{point.z})");
        buttonPoint.y += _callButton.bounds.size.y / 2 + 0.1f;
        Debug.Log($"RefreshRenderers: hit ({point.x},{point.y},{point.z})");
        _callButton.gameObject.transform.position = buttonPoint;
    }

    private static bool HasMicrophone()
    {
        return Microphone.devices.Length > 0;
    }

    private static bool HasCamera()
    {
        return WebCamTexture.devices.Length > 0;
    }

    private IEnumerator Start()
    {
        _screenWidth = Screen.width;

        const int size = 64;
        _defaultTexture = new Texture2D(size, size);
        var pixels = Enumerable.Repeat(BackgroundColor, size * size).ToArray();
        _defaultTexture.SetPixels(pixels);
        _defaultTexture.Apply();

        _remote = GameObject.Find("Remote Video").GetComponent<Renderer>();
        _local = GameObject.Find("Local Video").GetComponent<Renderer>();
        _callButton = GameObject.Find("Call Button").GetComponent<Renderer>();
        _callButtonText = GameObject.Find("Call Button Text").GetComponent<TextMesh>();

        Camera.main.backgroundColor = BackgroundColor;
        _local.material.mainTexture = _defaultTexture;
        _remote.material.mainTexture = _defaultTexture;

        _callButton.material.color = ButtonColor;
        _callButtonText.text = $"Call: {Callee}";

        if (HasMicrophone())
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("Microphone permission granted");
            }

            yield return 0;
        }

        if (HasCamera())
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("Camera permission granted");
            }

            yield return 0;
        }

        VoximplantSdk.Initialize();
        _client = VoximplantSdk.GetClient();

        _client.Connected += ClientOnConnected;
        _client.ConnectionFailed += ClientOnConnectionFailed;
        _client.Disconnected += ClientOnDisconnected;

        _client.LoginSuccess += ClientOnLoginSuccess;
        _client.LoginFailed += ClientOnLoginFailed;

        _client.IncomingCall += ClientOnIncomingCall;

        _client.Connect();

        RefreshRenderers();
    }

    private void Update()
    {
        if (_screenWidth == Screen.width) return;

        if (_call == null)
        {
            _local.material.mainTexture = _defaultTexture;
            _remote.material.mainTexture = _defaultTexture;
        }

        _screenWidth = Screen.width;
        RefreshRenderers();
    }

    private void ClientOnConnected(IClient sender)
    {
        _client.Login(Login, Password);
    }

    private void ClientOnConnectionFailed(IClient sender, ConnectionFailedEventArgs e)
    {
        Debug.LogError(e.Error);
        StartCoroutine(Reconnect());
    }

    private void ClientOnDisconnected(IClient sender)
    {
        StartCoroutine(Reconnect());
    }

    private IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(10);
        _client.Connect();
    }

    private void ClientOnLoginSuccess(IClient sender, LoginSuccessEventArgs e)
    {
    }

    private void ClientOnLoginFailed(IClient sender, LoginFailedEventArgs e)
    {
        Debug.LogError(e.Error);
        _client.Disconnect();
    }

    private void ClientOnIncomingCall(IClient sender, IncomingCallEventArgs e)
    {
        if (_call != null)
        {
            e.Call.Reject(RejectMode.Busy, null);
        }

        _call = e.Call;
        BindCallEventHandlers();
        _call.Answer(CallSettings);
    }

    private void CallVideoStreamAdded(ICall sender, CallLocalVideoStreamAddedEventArgs e)
    {
        _localVideoStream = e.VideoStream;
        Debug.Log($"Local added: {_localVideoStream.StreamId}");
        _localVideoStream.VideoStreamChanged += OnVideoStreamChanged;
        _localVideoStream.AddRenderer(_local.material);
    }

    private void CallVideoStreamRemoved(ICall sender, CallLocalVideoStreamRemovedEventArgs e)
    {
        e.VideoStream.RemoveRenderer(_local.material);
        _localVideoStream = null;
    }

    private void EndpointVideoStreamAdded(IEndpoint sender, EndpointRemoteVideoStreamAddedEventArgs e)
    {
        _remoteVideoStream = e.VideoStream;
        Debug.Log($"Remote added: {_remoteVideoStream.StreamId}");
        _remoteVideoStream.VideoStreamChanged += OnVideoStreamChanged;
        _remoteVideoStream.AddRenderer(_remote.material);
    }

    private void EndpointVideoStreamRemoved(IEndpoint sender, EndpointRemoteVideoStreamRemovedEventArgs e)
    {
        _remoteVideoStream = null;
        e.VideoStream.RemoveRenderer(_remote.material);
    }

    private void OnVideoStreamChanged(IVideoStream sender, VideoStreamChangedEventArgs e)
    {
        Debug.Log($"VideoChanged: {sender.StreamId} to {e.Width}x{e.Height}");
        RefreshRenderers();
    }
}