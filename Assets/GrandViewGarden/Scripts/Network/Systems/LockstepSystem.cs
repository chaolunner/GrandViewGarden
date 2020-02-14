using UniEasy.ECS;
using Common;
using UniRx;

public class LockstepSystem : NetworkSystemBehaviour
{
    public override void OnEnable()
    {
        base.OnEnable();

        NetworkSystem.Receive(RequestCode.Input).Subscribe(data =>
        {
            if (data.Mode == SessionMode.Offline)
            {
                UserInputs userInputs = MessagePackUtility.Deserialize<UserInputs>(data.Value);
                userInputs.UserId = 0;
                LockstepInputs lockstepInputs = LockstepUtility.CreateLockstepInputs(userInputs);
                NetworkSystem.Publish(RequestCode.Lockstep, MessagePackUtility.Serialize(lockstepInputs));
            }
        }).AddTo(this.Disposer);

        NetworkSystem.Receive(RequestCode.Lockstep).Subscribe(data =>
        {
            LockstepInputs lockstepInputs = MessagePackUtility.Deserialize<LockstepInputs>(data.Value);
            LockstepUtility.AddToTimeline(lockstepInputs);
        }).AddTo(this.Disposer);
    }

    private void Update()
    {
        LockstepFactory.Update();
        UserInputs userInputs = LockstepUtility.GetUserInputs();
        byte[] dataBytes = MessagePackUtility.Serialize(userInputs);
        NetworkSystem.Publish(RequestCode.Input, dataBytes);
    }

    private void FixedUpdate()
    {
        LockstepFactory.FixedUpdate();
    }
}
