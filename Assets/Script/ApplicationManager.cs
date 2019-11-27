using System;
using System.Collections;
using System.Collections.Generic;
using FreeDraw;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private GameObject[] views;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;

    private readonly char _delimiter = '|';
    private readonly char _clientCommand = '#';
    private readonly char _serverCommand = '@';
    private readonly string _color = "CC->";
    private readonly string _background = "BG->";

    private readonly int resolutionX = 1080;
    private readonly int resolutionY = 1920;


    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(3)
    {
        {"connect", 0},
        {"select", 1},
        {"draw", 2}
    };

    private void Start()
    {
        ChangeView("connect", "connection");
    }

    public void ChangeView(string view, string state)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].SetActive(i == target);
        
        _networkManager.ChangeState(state);
        StartDrawing(target == 2);
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
}