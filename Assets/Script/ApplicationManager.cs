using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FreeDraw;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] public GameObject[] viewObjects;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private ScrollManager _scrollManager;

    private readonly char _delimiter = '|';
    private readonly char _delimiter2 = '%';

    private readonly char _clientCommand = '#';
    private readonly char _serverCommand = '@';
    private readonly string _color = "CC->";
    private readonly string _background = "BG->";

    private readonly int resolutionX = 1080;
    private readonly int resolutionY = 1920;
    private IView[] views;

    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(3)
    {
        {"connection", 0},
        {"menu", 1},
        {"draw", 2}
//        {"error",3}
    };

    private void Start()
    {
        views = new IView[viewObjects.Length];
        for (int i = 0; i < viewObjects.Length; i++)
            views[i] = viewObjects[i].GetComponent<IView>();
        ChangeView("connection", "connection");
    }


    private void Awake()
    {
        Screen.SetResolution(540, 900, false);
    }

    public void ChangeView(string view, string state)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].ShowView(i == target);

        _networkManager.ChangeState(state);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public void SendCoordinateData(Vector2 data)
    {
        //var msg = (Math.Round((data.x / resolutionX), 4) + "," + Math.Round((data.y / resolutionY), 4));
        var msg = (data.x + "," + data.y);
        _networkManager.Send(msg);
    }

    public void SendCommendSignal(string command)
    {
        command = _clientCommand + command;
        _networkManager.Send(command);
    }

    public void SetMarkerColor(string color)
    {
        SendCommendSignal(_color + color);
    }

    public void ClearCanvas()
    {
        _drawable.ResetCanvas();
        SendCommendSignal(_background + "CLEAR");
    }

    public char GetDelimiter()
    {
        return _delimiter;
    }

    public char GetClientCommand()
    {
        return _clientCommand;
    }

    public char GetServerCommand()
    {
        return _serverCommand;
    }

    public void ReceiveDrawingData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;
        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(_delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;

            if (token.Contains(char.ToString(_serverCommand)))
            {
                if (token == _serverCommand + "ROOMCLOSED")
                    _networkManager.ReconnectToNameServer();
            }

//            else if (token.Contains(char.ToString(_clientCommand))) ;
//            else ;
        }
    }

    public void ReceiveRoomData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;
        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(_delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;
            if (token.Contains(char.ToString(_serverCommand)))
            {
                if (token.Contains(_serverCommand + "ROOM-LIST"))
                {
                    var roomList = token.Split(_delimiter2).ToList();
                    roomList.Remove(_serverCommand + "ROOM-LIST");
                    roomList.Remove("");
                    _scrollManager.AddItemsFromList(roomList);
                }
                else if (token == _serverCommand + "ENTER-ROOM")
                {
                }
            }
        }
    }

    public void GetRoomList()
    {
        _networkManager.Send(_serverCommand + "FIND-ROOM");
    }

    public void EnterRoom(string room, string pw)
    {
        Debug.Log(room + ":" + pw);
    }
}