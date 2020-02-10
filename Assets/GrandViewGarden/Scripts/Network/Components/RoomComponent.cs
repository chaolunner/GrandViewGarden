using UnityEngine.UI;
using UniEasy.ECS;
using TMPro;

public class RoomComponent : ComponentBehaviour
{
    public TextMeshProUGUI User1NameText;
    public TextMeshProUGUI User1TotalCountText;
    public TextMeshProUGUI User1WinCountText;
    public TextMeshProUGUI User2NameText;
    public TextMeshProUGUI User2TotalCountText;
    public TextMeshProUGUI User2WinCountText;
    public Button StartButton;
    public Button ExitButton;
}
