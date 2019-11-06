public class SpawnUserEvent
{
    public bool IsLocalPlayer;
    public bool IsRoomOwner;
    public int UserId;
    public string Username;
    public int TotalCount;
    public int WinCount;

    public SpawnUserEvent(int userId, string username, int totalCount, int winCount, bool isLocalPlayer, bool isRoomOwner)
    {
        UserId = userId;
        Username = username;
        TotalCount = totalCount;
        WinCount = winCount;
        IsLocalPlayer = isLocalPlayer;
        IsRoomOwner = isRoomOwner;
    }
}
