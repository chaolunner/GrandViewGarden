using UniEasy.ECS;
using UniRx;

public class UserComponent : ComponentBehaviour
{
    public bool IsLocalPlayer;
    public BoolReactiveProperty IsRoomOwner;
    public int UserId;
    public StringReactiveProperty Username;
    public IntReactiveProperty TotalCount;
    public IntReactiveProperty WinCount;
}
