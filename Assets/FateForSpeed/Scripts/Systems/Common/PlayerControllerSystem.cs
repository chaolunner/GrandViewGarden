using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using System;
using UniRx;

public class PlayerControllerSystem : LockstepSystemBehaviour
{
    [MinMaxRange(0, 180)]
    public RangedFloat InvertRange = new RangedFloat(35, 145);

    private IGroup PlayerControllerComponents;
    private Camera mainCamera;
    private Type[] InputTypes = new Type[] { typeof(AxisInput), typeof(KeyInput), typeof(MouseInput) };

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        PlayerControllerComponents = this.Create(typeof(PlayerControllerComponent), typeof(CharacterController), typeof(ViewComponent));
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
            var viewComponent = entity.GetComponent<ViewComponent>();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (characterController.isGrounded)
                {
                    var angle = Vector3.Angle(viewComponent.Transforms[0].forward, mainCamera.transform.forward);
                    if (angle < InvertRange.Min || angle > InvertRange.Max)
                    {
                        playerControllerComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        playerControllerComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    playerControllerComponent.Motion = viewComponent.Transforms[0].TransformDirection(playerControllerComponent.Motion);
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
        LerpTimeline(entity, InputTypes);
    }

    public override void ApplyUserInput(IEntity entity, UserInputData[] userInputData, float deltaTime)
    {
        var playerControllerComponent = entity.GetComponent<PlayerControllerComponent>();
        var characterController = entity.GetComponent<CharacterController>();
        var viewComponent = entity.GetComponent<ViewComponent>();
        var axisInput = userInputData[0].Input as AxisInput;
        var keyInput = userInputData[1].Input as KeyInput;
        var mouseInput = userInputData[2].Input as MouseInput;

        if (mouseInput != null)
        {
            var rotLeftRight = (float)mouseInput.Delta.x * playerControllerComponent.MouseSensivity.x * deltaTime;
            var rotUpDown = (float)mouseInput.Delta.y * playerControllerComponent.MouseSensivity.y * deltaTime;
            var yAngle = ClampAngle(playerControllerComponent.Viewpoint.localEulerAngles.x, playerControllerComponent.YAngleLimit.Min, playerControllerComponent.YAngleLimit.Max);
            yAngle = ClampAngle(yAngle - rotUpDown, playerControllerComponent.YAngleLimit.Min, playerControllerComponent.YAngleLimit.Max);
            viewComponent.Transforms[0].Rotate(0, rotLeftRight, 0);
            playerControllerComponent.Viewpoint.localEulerAngles = new Vector3(yAngle, 0, 0);
        }

        if (characterController.isGrounded)
        {
            if (axisInput != null)
            {
                playerControllerComponent.Motion = new Vector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                playerControllerComponent.Motion = viewComponent.Transforms[0].TransformDirection(playerControllerComponent.Motion);
                playerControllerComponent.Motion *= playerControllerComponent.Speed;
            }

            if (keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.Space))
            {
                playerControllerComponent.Motion.y = playerControllerComponent.JumpSpeed;
            }
        }

        playerControllerComponent.Motion.y -= playerControllerComponent.Gravity * deltaTime;
        characterController.Move(playerControllerComponent.Motion * deltaTime);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360) { angle += 360; }
        while (angle > 360) { angle -= 360; }
        if (angle > max && angle - 360 >= min) { return angle - 360; }
        if (angle < min && angle + 360 <= max) { return angle + 360; }
        return Mathf.Clamp(angle, -max, -min);
    }
}
