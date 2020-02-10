using UnityEngine.UI;
using UnityEngine;
using UniEasy.ECS;
using TMPro;

public class LobbyComponent : ComponentBehaviour
{
    public Transform RoomItemRoot;
    public TextMeshProUGUI UsernameText;
    public TextMeshProUGUI TotalCountText;
    public TextMeshProUGUI WinCountText;
    public Button CreateRoomButton;
    public Button RefreshButton;
}
