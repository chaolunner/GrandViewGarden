using UniEasy.ECS;
using UniEasy.Net;
using UniEasy.DI;

public class NetworkSystemBehaviour : SystemBehaviour
{
    [Inject]
    protected INetworkSystem NetworkSystem;
    [Inject]
    protected NetworkPrefabFactory NetworkPrefabFactory;
    [Inject]
    protected LockstepFactory LockstepFactory;

    protected const char Separator = ',';
    protected const char VerticalBar = '|';
    protected const string EmptyStr = "";
}
