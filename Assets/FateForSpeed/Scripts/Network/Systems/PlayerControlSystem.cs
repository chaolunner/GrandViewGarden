using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class PlayerControlSystem : NetworkSystemBehaviour
{
    [MinMaxRange(0, 180)]
    public RangedFloat InvertRange = new RangedFloat(35, 145);

    private IGroup PlayerControlComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private Camera mainCamera;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        PlayerControlComponents = this.Create(typeof(PlayerControlComponent), typeof(CharacterController), typeof(ViewComponent));
        Network = LockstepFactory.Create(PlayerControlComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(AxisInput), typeof(KeyInput), typeof(MouseInput));
        mainCamera = Camera.main;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        PlayerControlComponents.OnAdd()
            .Where(entity => !entity.HasComponent<NetworkPlayerComponent>())
            .Subscribe(entity =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var characterController = entity.GetComponent<CharacterController>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (characterController.isGrounded)
                {
                    var angle = Vector3.Angle(viewComponent.Transforms[0].forward, mainCamera.transform.forward);
                    if (angle < InvertRange.Min || angle > InvertRange.Max)
                    {
                        playerControlComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        playerControlComponent.Motion = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    playerControlComponent.Motion = viewComponent.Transforms[0].TransformDirection(playerControlComponent.Motion);
                    playerControlComponent.Motion *= playerControlComponent.Speed;

                    if (Input.GetButton(InputParameters.Jump))
                    {
                        playerControlComponent.Motion.y = playerControlComponent.JumpSpeed;
                    }
                }

                playerControlComponent.Motion.y -= playerControlComponent.Gravity * Time.deltaTime;
                characterController.Move(playerControlComponent.Motion * Time.deltaTime);
            }).AddTo(this.Disposer).AddTo(playerControlComponent.Disposer);
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnReverse((entity, result) =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var playerControlResult = (PlayerControlResult)result[0];

            viewComponent.Transforms[0].rotation = playerControlResult.Rotation;
            playerControlComponent.Viewpoint.rotation = playerControlResult.Viewpoint;
            viewComponent.Transforms[0].position = playerControlResult.Position;
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime) =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var characterController = entity.GetComponent<CharacterController>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var axisInput = userInputData[0].Input as AxisInput;
            var keyInput = userInputData[1].Input as KeyInput;
            var mouseInput = userInputData[2].Input as MouseInput;
            var playerControlResult = new PlayerControlResult();

            playerControlResult.Rotation = viewComponent.Transforms[0].rotation;
            playerControlResult.Viewpoint = playerControlComponent.Viewpoint.rotation;
            playerControlResult.Position = viewComponent.Transforms[0].position;

            if (mouseInput != null)
            {
                var rotLeftRight = (float)mouseInput.Delta.x * playerControlComponent.MouseSensivity.x * deltaTime;
                var rotUpDown = (float)mouseInput.Delta.y * playerControlComponent.MouseSensivity.y * deltaTime;
                var yAngle = ClampAngle(playerControlComponent.Viewpoint.localEulerAngles.x, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);
                yAngle = ClampAngle(yAngle - rotUpDown, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);
                viewComponent.Transforms[0].Rotate(0, rotLeftRight, 0);
                playerControlComponent.Viewpoint.localEulerAngles = new Vector3(yAngle, 0, 0);
            }

            if (characterController.isGrounded)
            {
                if (axisInput != null)
                {
                    playerControlComponent.Motion = new Vector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                    playerControlComponent.Motion = viewComponent.Transforms[0].TransformDirection(playerControlComponent.Motion);
                    playerControlComponent.Motion *= playerControlComponent.Speed;
                }

                if (keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.Space))
                {
                    playerControlComponent.Motion.y = playerControlComponent.JumpSpeed;
                }
            }

            playerControlComponent.Motion.y -= playerControlComponent.Gravity * deltaTime;
            characterController.Move(playerControlComponent.Motion * deltaTime);

            return new IUserInputResult[] { playerControlResult };
        }).AddTo(this.Disposer);
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
