using UnityEngine;
using UniEasy.ECS;
using Common;
using UniRx;

public class LockstepSystem : NetworkSystemBehaviour
{
    public override void OnEnable()
    {
        base.OnEnable();
        LockstepUtility.OnSyncTimeline += OnSyncTimeline;

        NetworkSystem.Receive<string>(RequestCode.Input).Subscribe(data =>
        {
        }).AddTo(this.Disposer);

        NetworkSystem.Receive<string>(RequestCode.Timeline).Subscribe(data =>
        {
        }).AddTo(this.Disposer);

        NetworkSystem.Receive<string>(RequestCode.Lockstep).Subscribe(data =>
        {
            LockstepInputs lockstepInputs = JsonUtility.FromJson<LockstepInputs>(data);
            LockstepUtility.AddToTimeline(lockstepInputs);
        }).AddTo(this.Disposer);
    }

    private void Update()
    {
        UserInputs userInputs = LockstepUtility.GetUserInputs();
        string data = JsonUtility.ToJson(userInputs);
        NetworkSystem.Publish(RequestCode.Input, data);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        LockstepUtility.OnSyncTimeline -= OnSyncTimeline;
    }

    private void OnSyncTimeline(int index)
    {
        NetworkSystem.Publish(RequestCode.Timeline, index.ToString());
    }
}
