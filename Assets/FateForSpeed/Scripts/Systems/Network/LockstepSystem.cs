using UnityEngine;
using UniEasy.ECS;
using Common;
using UniRx;

public class LockstepSystem : NetworkSystemBehaviour
{
    public override void OnEnable()
    {
        base.OnEnable();

        NetworkSystem.Receive<string>(RequestCode.Input).Subscribe(data =>
        {
        }).AddTo(this.Disposer);

        NetworkSystem.Receive<string>(RequestCode.Lockstep).Subscribe(data =>
        {
            LockstepInputs lockstepInputs = JsonUtility.FromJson<LockstepInputs>(data);
            LockstepUtility.AddToHistory(lockstepInputs);
            if (lockstepInputs.UserInputs != null)
            {
                foreach (var userInputs in lockstepInputs.UserInputs)
                {
                    if (userInputs.InputData != null)
                    {
                        foreach (var inputData in userInputs.InputData)
                        {
                            EventSystem.Send(new UserInputEvent(userInputs.UserId, lockstepInputs.TickId, InputUtility.FromInputData(inputData)));
                        }
                    }
                }
            }
        }).AddTo(this.Disposer);
    }

    private void FixedUpdate()
    {
        UserInputs userInputs = LockstepUtility.GetUserInputs();
        string data = JsonUtility.ToJson(userInputs);
        NetworkSystem.Publish(RequestCode.Input, data);
    }
}
