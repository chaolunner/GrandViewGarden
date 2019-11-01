using System.Collections.Generic;
using UniEasy.ECS;
using TMPro;

public class RoomItemComponent : ComponentBehaviour
{
    public TextMeshProUGUI UsernameText;
    public TextMeshProUGUI TotalCountText;
    public TextMeshProUGUI WinCountText;
    public List<IEntity> UserEntities = new List<IEntity>();
}
