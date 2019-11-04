using UnityEngine.UI;
using UniEasy.ECS;
using TMPro;

public class RoomItemComponent : ComponentBehaviour
{
    public int UserId;
    public TextMeshProUGUI UsernameText;
    public TextMeshProUGUI TotalCountText;
    public TextMeshProUGUI WinCountText;
    public Button JoinButton;
}
