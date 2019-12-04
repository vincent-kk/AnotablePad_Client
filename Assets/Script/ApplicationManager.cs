using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] public GameObject[] viewObjects;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private ScrollManager _scrollManager;
    [SerializeField] private WarningOverlayManager _warningOverlayManager;

//    private readonly char _delimiter = '|';
//    private readonly char _delimiterUI = '%';
//
//    private readonly char _clientCommand = '#';
//    private readonly char _serverCommand = '@';
//    private readonly string _color = "CC->";
//    private readonly string _background = "BG->";

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
        ChangeView("connection");
    }


    private void Awake()
    {
        Screen.SetResolution(540, 900, false);
    }

    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].ShowView(i == target);

        _networkManager.ChangeState(view);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public void ShowWaringModal(string type)
    {
        _warningOverlayManager.ShowOverlay(type);
    }

    public void SendCoordinateData(Vector2 data)
    {
        //var msg = (Math.Round((data.x / resolutionX), 4) + "," + Math.Round((data.y / resolutionY), 4));
        var msg = (data.x + "," + data.y);
        _networkManager.Send(msg);
    }

    public void SendCommendSignal(string command)
    {
        command = AppData.ClientCommand + command;
        _networkManager.Send(command);
    }

    public void SetMarkerColor(string color)
    {
        SendCommendSignal(AppData.ColorCommand + color);
    }

    public void ClearCanvas()
    {
        _drawable.ResetCanvas();
        SendCommendSignal(AppData.BackgroundClearCommand);
    }


    public void ReceiveDrawingData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;
        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(AppData.Delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;
            if (token.Contains(char.ToString(AppData.ServerCommand)))
            {
                if (token == CommendBook.ROOM_CLOSED)
                {
                    _warningOverlayManager.ShowOverlay("RoomServer-Closed");
                    _networkManager.ReconnectToNameServer();
                }

            }
        }
    }

    public void ReceiveRoomData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;
        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(AppData.Delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;
            if (token.Contains(char.ToString(AppData.ServerCommand)))
            {
                if (token.Contains(CommendBook.HEADER_ROOMLIST))
                {
                    var roomList = token.Split(AppData.DelimiterUI).ToList();
                    roomList.Remove(CommendBook.HEADER_ROOMLIST);
                    roomList.Remove("");
                    _scrollManager.AddItemsFromList(roomList);
                }
                else if (token == CommendBook.START_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer();
                }
                else if (token == CommendBook.ERROR_MESSAGE)
                {
                    _warningOverlayManager.ShowOverlay("");
                }
                else if (token == CommendBook.COMMEND_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Invalid-Commend");
                }
                else if (token == CommendBook.PASSWORD_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Wrong-Pw");
                }
                else if (token == CommendBook.NO_ROOM_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("No-Room");
                }
            }
        }
    }

    public void GetRoomList()
    {
        _networkManager.Send(CommendBook.FIND_ROOM);
    }

    public void EnterRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.ENTER_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }
}