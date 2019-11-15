using UniEasy.ECS;

public class NetworkPlayerComponent : ComponentBehaviour
{
    public IEntity UserEntity;
    public UserComponent UserComponent { get { return UserEntity.GetComponent<UserComponent>(); } }
    public int UserId { get { return UserComponent.UserId; } }
    public bool IsLocalPlayer { get { return UserComponent.IsLocalPlayer; } }
}
