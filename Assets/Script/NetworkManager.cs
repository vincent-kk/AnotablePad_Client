﻿using System;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIp = null;
    [SerializeField] private InputField serverPort = null;
    [SerializeField] private TcpManager _tcpManager;

    [SerializeField] private ApplicationManager _applicationManager;

    private byte[] _receiveBuffer;

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
    private void Awake()
    {
        _tcpManager.RegisterEventHandler(OnServerDisconnectedEvent);
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
                if (!_tcpManager.Sock.Connected) TcpDisconnect(false);
                _applicationManager.ReceiveDrawingData();
                break;
            case State.ERROR:
                break;
        }
    }

    public void ReconnectToNameServer()
    {
        TcpDisconnect(true);
        Thread.Sleep(100);

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "Tablet");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(CommendBook.CONNECTION))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }

        TcpDisconnect(false);
    }

    public void SwitchRoomServer()
    {
        var returnData = new byte[64];
        var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            TcpDisconnect(true);
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
                    if (msg.Contains(CommendBook.CONNECTION))
                    {
                        _applicationManager.ChangeView("draw");
                        return;
                    }
                }
            }
        }

        TcpDisconnect(false);
    }

    public void ConnectToServer()
    {
        if (!AppData.IpRegex.IsMatch(serverIp.text))
        {
            _applicationManager.ShowWaringModal("Invalid-Ip");
            return;
        }

        AppData.ServerIp = serverIp.text;
        AppData.ServerPort = Convert.ToInt32(serverPort.text);

        Debug.Log("server : " + AppData.ServerIp + " : " + AppData.ServerPort);

        serverIp.text = "";
        serverPort.text = "";

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "Tablet");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(CommendBook.CONNECTION))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }
        _applicationManager.ShowWaringModal("Server-Not-Found");
        TcpDisconnect(false);
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

    private void TcpDisconnect(bool switchServer)
    {
        _tcpManager.Disconnect(switchServer);
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
    }


    public string Receive()
    {
        var returnData = new byte[AppData.BufferSize];
        var recvSize = _tcpManager.Receive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            var msg = System.Text.Encoding.UTF8.GetString(returnData);
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
            _tcpManager.Disconnect(true);
        }
    }

    private void OnServerDisconnectedEvent()
    {
        _applicationManager.ShowWaringModal("Network-Disconnection");
        _applicationManager.ChangeView("connection");
        ConsoleLogger("Fail To Connect Server");
    }
}