public class SpawnUserEvent
{
    public bool IsLocalPlayer;
    public string Username;
    public int TotalCount;
    public int WinCount;

    public SpawnUserEvent(string username, int totalCount, int winCount, bool isLocalPlayer)
    {
        Username = username;
        TotalCount = totalCount;
        WinCount = winCount;
        IsLocalPlayer = isLocalPlayer;
    }
}
