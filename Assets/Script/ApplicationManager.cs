using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 어플리케이션의 모든 기능을 총괄하는 부분.
/// UI를 비롯해서 대부분의 기능을 실행하기 위한 컨트롤러 역할을 한다.
/// </summary>
public class ApplicationManager : MonoBehaviour
{
    [SerializeField] public GameObject[] viewObjects;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private ScrollManager _scrollManager;
    [SerializeField] private WarningOverlayManager _warningOverlayManager;

    private IView[] views;
    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(3)
    {
        {"connection", 0},
        {"menu", 1},
        {"draw", 2}
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
        Screen.SetResolution(330, 550, false);
    }

    /// <summary>
    /// 어플리케이션에서 뷰의 변경을 총괄한다.
    /// 모든 뷰는 해당 매소드를 통해서 변경된다.
    /// </summary>
    /// <param name="view"></param>
    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].ShowView(i == target);

        _networkManager.ChangeState(view);
    }

    /// <summary>
    /// 사용자에게 메시지를 통지하는 모달을 띄운다.
    /// 모달의 내용은 WarningOverlayManager에 정의되어 있다.
    /// </summary>
    /// <param name="type"></param>
    public void ShowWaringModal(string type)
    {
        _warningOverlayManager.ShowOverlay(type);
    }
    /// <summary>
    /// 생성된 그림 좌표를 전송한다.
    /// </summary>
    /// <param name="data"></param>
    public void SendCoordinateData(Vector2 data)
    {
        _networkManager.Send(data.x + "," + data.y);
    }
    /// <summary>
    /// 앱에서 발생한 명령문을 명령 헤더를 추가하여 전송한다.
    /// </summary>
    /// <param name="command"></param>
    public void SendCommendSignal(string command)
    {
        command = AppData.ClientCommand + command;
        _networkManager.Send(command);
    }
    /// <summary>
    /// 마커의 색을 변경하도록 다른 프로그램에 명령을 전달한다.
    /// </summary>
    /// <param name="color"></param>
    public void SetMarkerColor(string color)
    {
        SendCommendSignal(AppData.ColorCommand + color);
    }
    /// <summary>
    /// 캔버스를 새로 그리는 명령을 통지한다.
    /// 자신의 캔버스도 동일하게 초기화한다.
    /// </summary>
    public void ClearCanvas()
    {
        _drawable.ResetCanvas();
        SendCommendSignal(AppData.BackgroundClearCommand);
    }

     /// <summary>
     /// Drawing Room에 존재할 때 처리해야 하는 모든 동작을 포함한다.
     /// 현재는 방이 닫히는 것을 통지받는 역할을 한다.
     /// </summary>
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
                    continue;
                }
            }
        }
    }
    /// <summary>
    /// 프로그램이 Name Server에서 처리해야 하는 모든 동작과 명령을 포함한다.
    /// </summary>
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
                    continue;
                }
                else if (token == CommendBook.START_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer();
                    continue;
                }
                else if (token == CommendBook.ERROR_MESSAGE)
                {
                    _warningOverlayManager.ShowOverlay("");
                    continue;
                }
                else if (token == CommendBook.COMMEND_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Invalid-Commend");
                    continue;
                }
                else if (token == CommendBook.PASSWORD_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Wrong-Pw");
                    continue;
                }
                else if (token == CommendBook.NO_ROOM_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("No-Room");
                    continue;
                }
            }
        }
    }
    /// <summary>
    /// 방의 리스트를 요청한다.
    /// </summary>
    public void GetRoomList()
    {
        _networkManager.Send(CommendBook.FIND_ROOM);
    }
    /// <summary>
    /// 방에 접속을 시도한다.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="pw"></param>
    public void EnterRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.ENTER_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }
}