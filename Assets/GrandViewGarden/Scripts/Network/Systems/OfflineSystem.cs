using UnityEngine;
using Common;

public class OfflineSystem : NetworkSystemBehaviour
{
    [SerializeField]
    private GameObject UserPrefab;

    private const string UserName = "Offline Player";

    public override void OnEnable()
    {
        base.OnEnable();

        PrefabFactory.Instantiate(UserPrefab, null, false, go =>
        {
            var userComponent = go.GetComponent<UserComponent>();

            userComponent.IsLocalPlayer = true;
            userComponent.IsRoomOwner.Value = true;
            userComponent.UserId = 0;
            userComponent.UserName.Value = UserName;

            NetworkSystem.Mode = SessionMode.Offline;
            LockstepUtility.AddInput(new EventInput(EventCode.GameStart, userComponent.IsRoomOwner.Value.ToString()));
        });
    }
}
