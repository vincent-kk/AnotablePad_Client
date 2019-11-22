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
        {"findServer", 0},
        {"selectHost", 1},
        {"notepad", 2}
    };

    private void Start()
    {
        ChangeView("findServer");
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
        _networkManager.Send(data.ToString());
    }
}