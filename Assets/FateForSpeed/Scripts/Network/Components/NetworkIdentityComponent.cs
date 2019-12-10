using UniEasy.ECS;

public class NetworkIdentityComponent : ComponentBehaviour
{
    public bool IsLocalPlayer;
    public NetworkId Identity;
}
