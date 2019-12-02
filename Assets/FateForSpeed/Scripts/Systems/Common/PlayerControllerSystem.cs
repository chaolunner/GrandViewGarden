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
            var moveDirection = Vector3.zero;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (characterController.isGrounded)
                {
                    var angle = Vector3.Angle(characterController.transform.forward, mainCamera.transform.forward);
                    if (angle < InvertRange.Min || angle > InvertRange.Max)
                    {
                        moveDirection = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        moveDirection = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    moveDirection = playerControllerComponent.transform.TransformDirection(moveDirection);
                    moveDirection *= playerControllerComponent.Speed;

                    if (Input.GetButton(InputParameters.Jump))
                    {
                        moveDirection.y = playerControllerComponent.JumpSpeed;
                    }
                }

                moveDirection.y -= playerControllerComponent.Gravity * Time.deltaTime;
                characterController.Move(moveDirection * Time.deltaTime);
            }).AddTo(this.Disposer).AddTo(playerControllerComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    public override void UpdateTimeline(IEntity entity)
    {
        PushUntilLastStep(entity, typeof(AxisInput));
    }

    public override void ApplyUserInput(IEntity entity, UserInputData userInputData)
    {
        if (userInputData.Input is AxisInput)
        {
            var playerControllerComponent = entity.GetComponent<PlayerControllerComponent>();
            var characterController = entity.GetComponent<CharacterController>();
            var moveDirection = Vector3.zero;
            var axisInput = userInputData.Input as AxisInput;

            if (characterController.isGrounded)
            {
                var angle = Vector3.Angle(characterController.transform.forward, mainCamera.transform.forward);
                if (angle < InvertRange.Min || angle > InvertRange.Max)
                {
                    moveDirection = new Vector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                }
                else
                {
                    moveDirection = new Vector3((float)axisInput.Vertical, 0, (float)axisInput.Horizontal);
                }
                moveDirection = playerControllerComponent.transform.TransformDirection(moveDirection);
                moveDirection *= playerControllerComponent.Speed;
            }

            moveDirection.y -= playerControllerComponent.Gravity * (float)userInputData.DeltaTime;
            characterController.Move(moveDirection * (float)userInputData.DeltaTime);
        }
    }
}
