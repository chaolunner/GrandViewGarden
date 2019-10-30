using UniEasy.ECS;
using UniRx;

public class UserComponent : ComponentBehaviour
{
    public bool IsLocalPlayer;
    public StringReactiveProperty Username;
    public IntReactiveProperty TotalCount;
    public IntReactiveProperty WinCount;
}
