using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class PlayerControllerSystem : LockstepSystemBehaviour
{
    [MinMaxRange(0, 180)]
    public RangedFloat InvertRange = new RangedFloat(35, 145);

    private IGroup PlayerControllerComponents;
    private Camera mainCamera;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        PlayerControllerComponents = this.Create(typeof(PlayerControllerComponent), typeof(CharacterController));
        mainCamera = Camera.main;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        PlayerControllerComponents.OnAdd()
            .Where(entity => !entity.HasComponent<NetworkPlayerComponent>())
            .Subscribe(entity =>
        {
            var playerControllerComponent = entity.GetComponent<PlayerControllerComponent>();
            var characterController = entity.GetComponent<CharacterController>();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (characterController.isGrounded)
                {
                    var angle = Vector3.Angle(characterController.transform.forward, mainCamera.transform.forward);
                    if (angle < InvertRange.Min || angle > InvertRange.Max)
                    {
                        playerControllerComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        playerControllerComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    playerControllerComponent.Motion = playerControllerComponent.transform.TransformDirection(playerControllerComponent.Motion);
                    playerControllerComponent.Motion *= playerControllerComponent.Speed;

                    if (Input.GetButton(InputParameters.Jump))
                    {
                        playerControllerComponent.Motion.y = playerControllerComponent.JumpSpeed;
                    }
                }

                playerControllerComponent.Motion.y -= playerControllerComponent.Gravity * Time.deltaTime;
                characterController.Move(playerControllerComponent.Motion * Time.deltaTime);
            }).AddTo(this.Disposer).AddTo(playerControllerComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    public override void UpdateTimeline(IEntity entity)
    {
        PushUntilLastStep(entity, typeof(AxisInput), typeof(KeyInput), typeof(MouseInput));
    }

    public override void ApplyUserInput(IEntity entity, UserInputData[] userInputData)
    {
        var playerControllerComponent = entity.GetComponent<PlayerControllerComponent>();
        var characterController = entity.GetComponent<CharacterController>();
        var axisInput = userInputData[0].Input as AxisInput;
        var keyInput = userInputData[1].Input as KeyInput;
        var mouseInput = userInputData[2].Input as MouseInput;

        if (characterController.isGrounded)
        {
            if (axisInput != null)
            {
                var angle = Vector3.Angle(characterController.transform.forward, mainCamera.transform.forward);
                if (angle < InvertRange.Min || angle > InvertRange.Max)
                {
                    playerControllerComponent.Motion = new Vector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                }
                else
                {
                    playerControllerComponent.Motion = new Vector3((float)axisInput.Vertical, 0, (float)axisInput.Horizontal);
                }
                playerControllerComponent.Motion = playerControllerComponent.transform.TransformDirection(playerControllerComponent.Motion);
                playerControllerComponent.Motion *= playerControllerComponent.Speed;
            }

            if (keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.Space))
            {
                playerControllerComponent.Motion.y = playerControllerComponent.JumpSpeed;
            }
        }

        playerControllerComponent.Motion.y -= playerControllerComponent.Gravity * (float)userInputData[0].DeltaTime;
        characterController.Move(playerControllerComponent.Motion * (float)userInputData[0].DeltaTime);
    }
}
