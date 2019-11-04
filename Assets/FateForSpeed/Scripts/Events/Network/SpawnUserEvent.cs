public class SpawnUserEvent
{
    public bool IsLocalPlayer;
    public int UserId;
    public string Username;
    public int TotalCount;
    public int WinCount;

    public SpawnUserEvent(int userId, string username, int totalCount, int winCount, bool isLocalPlayer)
    {
        UserId = userId;
        Username = username;
        TotalCount = totalCount;
        WinCount = winCount;
        IsLocalPlayer = isLocalPlayer;
    }
}
