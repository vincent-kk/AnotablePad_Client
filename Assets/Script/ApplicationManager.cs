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

    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(3)
    {
        {"connect", 0},
        {"select", 1},
        {"draw", 2}
    };

    private void Start()
    {
        ChangeView("connect");
    }

    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].SetActive(i == target);

        StartDrawing(target == 2);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public void ChangeColor(string color)
    {
    }

    public void SendCoordinateData(Vector2 data)
    {
        var msg = data.x + "," + data.y;
        _networkManager.Send(msg);
    }

    public void SendReleasedSignal()
    {
        var msg = "#EOF";
        _networkManager.Send(msg);
    }
}