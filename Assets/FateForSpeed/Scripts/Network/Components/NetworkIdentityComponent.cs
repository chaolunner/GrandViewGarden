using UniEasy.ECS;

public class NetworkIdentityComponent : ComponentBehaviour
{
    public bool IsLocalPlayer;
    public int TickIdWhenCreated;
    public NetworkId Identity;
}
