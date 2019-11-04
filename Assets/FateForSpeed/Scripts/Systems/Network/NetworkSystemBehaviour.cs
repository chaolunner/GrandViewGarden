using UniEasy.ECS;
using UniEasy.Net;
using UniEasy.DI;

public class NetworkSystemBehaviour : SystemBehaviour
{
    [Inject]
    protected INetworkSystem NetworkSystem;

    protected const char Separator = ',';
    protected const char VerticalBar = '|';
    protected const string EmptyStr = "";
}
