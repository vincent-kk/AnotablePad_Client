﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIp = null;
    [SerializeField] private InputField serverPort = null;

    [SerializeField] private ApplicationManager _applicationManager;

    private byte[] _receiveBuffer;

    private TcpManager _tcpManager = null;
    private Encoding _encode;

    private static State _state = State.CONNECTION;

    enum State
    {
        CONNECTION, // 연결 준비
        MENU, //메뉴 화면
        DRAW, //실제로 그리는중
        PAUSE, // 패치 일시중지
        ERROR, // 오류.
    };

    // Start is called before the first frame update
    void Start()
    {
        _tcpManager = gameObject.AddComponent<TcpManager>();
    }

    private void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case State.CONNECTION:
                break;
            case State.MENU:
                _applicationManager.ReceiveRoomData();
                break;
            case State.DRAW:
                _applicationManager.ReceiveDrawingData();
                break;
            case State.ERROR:
                break;
        }
    }

    public void ReconnectToNameServer()
    {
        TcpDisconnect();
        Thread.Sleep(100);

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "Tablet");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(AppData.ServerCommand + "CONNECTION"))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }

        TcpDisconnect();
        _applicationManager.ChangeView("connection");
        ConsoleLogger("Fail To Connect Server");
    }

    public void SwitchRoomServer()
    {
        var returnData = new byte[64];
        var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            TcpDisconnect();
            var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
            var port = Convert.ToInt32(msg);
            ConsoleLogger(AppData.ServerIp + ":" + port + " Reconnect");
            if (TcpConnection(AppData.ServerIp, port))
            {
                Send(AppData.ServerCommand + "Tablet");
                recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
                if (recvSize > 0)
                {
                    msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                    if (msg.Equals(AppData.ServerCommand + "CONNECTION"))
                    {
                        _applicationManager.ChangeView("draw");
                    }
                }
                else
                {
                    TcpDisconnect();
                    _applicationManager.ChangeView("connection");
                    ConsoleLogger("Fail To Connect Server");
                }
            }
        }
    }

    public void ConnectToServer()
    {
        AppData.ServerIp = serverIp.text;
        AppData.ServerPort = Convert.ToInt32(serverPort.text);

        Debug.Log("server : " + AppData.ServerIp + " : " + AppData.ServerPort);

        serverIp.text = "";
        serverPort.text = "";
//        if (TcpConnection(_serverIp, _serverPort))
//        {
//            Send(_serverCommand + "Tablet");
//            var returnData = new byte[64];
//            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
//            if (recvSize > 0)
//            {
//                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
//                if (msg.Equals(_serverCommand + "CONNECTION"))
//                {
//                    _applicationManager.ChangeView("draw");
//                    return;
//                }
//            }
//        }

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "Tablet");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(AppData.ServerCommand + "CONNECTION"))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }

        TcpDisconnect();
        _applicationManager.ChangeView("connection");
        ConsoleLogger("Fail To Connect Server");
    }

    public void PauseNetworkThread()
    {
        _tcpManager.Pause();
    }

    public void ResumeNetworkThread()
    {
        _tcpManager.Resume();
    }

    private bool TcpConnection(string serverIp, int port)
    {
        return _tcpManager.Connect(serverIp, port);
    }

    private void TcpDisconnect()
    {
        _tcpManager.Disconnect();
    }

    public void ChangeState(string state)
    {
        switch (state)
        {
            case "connection":
                _state = State.CONNECTION;
                break;
            case "menu":
                _state = State.MENU;
                break;
            case "draw":
                _state = State.DRAW;
                break;
            case "pause":
                _state = State.PAUSE;
                break;
            case "error":
            default:
                _state = State.ERROR;
                break;
        }
    }

    public void Send(string msg)
    {
        msg += AppData.Delimiter;
        var buffer = System.Text.Encoding.UTF8.GetBytes(msg);
        _tcpManager.Send(buffer, buffer.Length);
        ConsoleLogger(msg);
    }


    public string Receive()
    {
        var returnData = new byte[AppData.BufferSize];
        var recvSize = _tcpManager.Receive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            var msg = System.Text.Encoding.UTF8.GetString(returnData);
            ConsoleLogger(msg);
            return msg;
        }

        return null;
    }

    private void ConsoleLogger(string log)
    {
        Debug.Log(log);
    }

    private void OnApplicationQuit()
    {
        if (_tcpManager != null)
        {
            _tcpManager.Disconnect();
        }
    }

    public void OnEventHandling(NetEventState state)
    {
        switch (state.type)
        {
            case NetEventType.Connect:
                if (_tcpManager.IsServer())
                {
                }
                else
                {
                }

                break;

            case NetEventType.Disconnect:
                if (_tcpManager.IsServer())
                {
                }
                else
                {
                }

                break;
        }
    }
}