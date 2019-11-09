using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIP = null;
    [SerializeField] private InputField serverPort = null;
    [SerializeField] private TextMeshProUGUI console = null;

    [SerializeField] private InputField message;

    public int bufferSize = 1024;

    private byte[] _receiveBuffer;

    private TcpManager _tcpManager = null;
    private Encoding _encode;

    private static ChatState _state = ChatState.HOST_TYPE_SELECT;

    enum ChatState
    {
        HOST_TYPE_SELECT = 0, // 방 선택.
        CONNECTION, // 연결.
        LEAVE, // 나가기.
        ERROR, // 오류.
    };


    // Start is called before the first frame update
    void Start()
    {
        _tcpManager = gameObject.AddComponent<TcpManager>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case ChatState.HOST_TYPE_SELECT:
                break;
            case ChatState.CONNECTION:
                Receive();
                break;
            case ChatState.LEAVE:
                break;
        }
    }

    public void ConnectToServer()
    {
        var ip = serverIP.text;
        var port = Convert.ToInt32(serverPort.text);

        Debug.Log("server : " + ip + " : " + port);

        serverIP.text = "";
        serverPort.text = "";

        ConsoleLogger("Try to Connect : " + ip + " : " + port);

        TcpConnection(ip, port);
    }

    private void TcpConnection(string serverIp, int port)
    {
        var ret = _tcpManager.Connect(serverIp, port);
        _state = ret ? ChatState.CONNECTION : ChatState.ERROR;
    }

    public void Send()
    {
        var dataToSend = "client [" + DateTime.Now.ToString("HH:mm:ss") + "] : " + message.text;
        
        message.text = "";

        var buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);

        _tcpManager.Send(buffer, buffer.Length);

        ConsoleLogger(dataToSend);
    }

    private void Receive()
    {
        var returnData = new byte[bufferSize];

        var recvSize = _tcpManager.Receive(ref returnData, returnData.Length);

        if (recvSize > 0)
        {
            var msg = System.Text.Encoding.UTF8.GetString(returnData);
            msg = "server " + msg;
            ConsoleLogger(msg);
        }
    }

    private void ConsoleLogger(string log)
    {
        if (console.text == "")
            console.text = log;
        else
        {
            string temp = console.text;
            temp += "\n" + log;
            console.text = temp;
        }
    }

    private void OnApplicationQuit()
    {
        if (_tcpManager != null)
        {
            _tcpManager.StopServer();
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