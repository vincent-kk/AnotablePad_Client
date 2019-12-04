public class AppData
{
    private static readonly char delimiter = '|';
    private static readonly char delimiterUI = '%';
    private static readonly char clientCommand = '#';
    private static readonly char serverCommand = '@';
    private static readonly string color = "CC->";
    private static readonly string backgroundClear = "BG->CLEAR";
    private static string serverIp;
    private static int serverPort;
    public static char Delimiter => delimiter;
    public static char DelimiterUI => delimiterUI;
    public static char ClientCommand => clientCommand;
    public static char ServerCommand => serverCommand;
    public static string ColorCommand => color;
    public static string BackgroundClearCommand => backgroundClear;

    public static string ServerIp
    {
        get => serverIp;
        set => serverIp = value;
    }

    public static int ServerPort
    {
        get => serverPort;
        set => serverPort = value;
    }
}

public static class CommendBook
{
    private static readonly string findRoom = AppData.ServerCommand + "FIND-ROOM";
    private static readonly string createRoom = AppData.ServerCommand + "CREATE-ROOM";
    private static readonly string enterRoom = AppData.ServerCommand + "ENTER-ROOM";
    private static readonly string startDrawing = AppData.ServerCommand + "START-DRAWING";
    private static readonly string guestDrawing = AppData.ServerCommand + "GUEST-DRAWING";
    private static readonly string errorMessage = AppData.ServerCommand + "ERROR";
    private static readonly string roomListHeader = AppData.ServerCommand + "ROOM-LIST";
    private static readonly string roomClosed = AppData.ServerCommand + "ROOMCLOSED";

    public static string FIND_ROOM => findRoom;
    public static string CREATE_ROOM => createRoom;
    public static string ENTER_ROOM => enterRoom;
    public static string HEADER_ROOMLIST => roomListHeader;
    public static string ROOM_CLOSED => roomClosed;
    public static string ERROR_MESSAGE => errorMessage;
    public static string START_DRAWING => startDrawing;
    public static string GUEST_DRAWING => guestDrawing;
}