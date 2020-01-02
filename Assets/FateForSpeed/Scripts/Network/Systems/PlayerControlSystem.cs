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

    private const float SmoothTime = 1f;
    private const float ShoulderAimTime = 0.2f;

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
                        playerControlComponent.motion = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        playerControlComponent.motion = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    playerControlComponent.motion = viewComponent.Transforms[0].TransformDirection(playerControlComponent.motion);
                    playerControlComponent.motion *= playerControlComponent.Speed;

                    if (Input.GetButton(InputParameters.Jump))
                    {
                        playerControlComponent.motion.y = playerControlComponent.JumpSpeed;
                    }
                }

                playerControlComponent.motion.y -= playerControlComponent.Gravity * Time.deltaTime;
                characterController.Move(playerControlComponent.motion * Time.deltaTime);
            }).AddTo(this.Disposer).AddTo(playerControlComponent.Disposer);
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnReverse((entity, result) =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var playerControlResult = (PlayerControlResult)result[0];

            viewComponent.Transforms[0].rotation = playerControlResult.Rotation;
            playerControlComponent.Follow.rotation = playerControlResult.Follow;
            viewComponent.Transforms[0].position = playerControlResult.Position;
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime) =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var characterController = entity.GetComponent<CharacterController>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var animator = entity.GetComponent<Animator>();
            var axisInput = userInputData[0].Input as AxisInput;
            var keyInput = userInputData[1].Input as KeyInput;
            var mouseInput = userInputData[2].Input as MouseInput;
            var playerControlResult = new PlayerControlResult();
            var smoothTime = playerControlComponent.smoothTime;
            var yAngle = 0f;

            playerControlResult.Rotation = viewComponent.Transforms[0].rotation;
            playerControlResult.Follow = playerControlComponent.Follow.rotation;
            playerControlResult.Position = viewComponent.Transforms[0].position;

            if (mouseInput != null)
            {
                var rotLeftRight = (float)mouseInput.Delta.x * playerControlComponent.MouseSensivity.x * deltaTime;
                var rotUpDown = (float)mouseInput.Delta.y * playerControlComponent.MouseSensivity.y * deltaTime;

                if (mouseInput.MouseButtons.Contains(1))
                {
                    playerControlComponent.aimTime += deltaTime;
                    if (playerControlComponent.Aim.Value == AimMode.Free && playerControlComponent.aimTime > ShoulderAimTime)
                    {
                        playerControlComponent.Aim.Value = AimMode.Shoulder;
                    }
                }
                else
                {
                    if (playerControlComponent.Aim.Value == AimMode.Free)
                    {
                        if (playerControlComponent.aimTime > 0)
                        {
                            playerControlComponent.Aim.Value = AimMode.AimDownSight;
                        }
                    }
                    else if (playerControlComponent.aimTime > 0)
                    {
                        playerControlComponent.Aim.Value = AimMode.Free;
                    }
                    playerControlComponent.aimTime = 0;
                }

                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt))
                {
                    playerControlComponent.Follow.Rotate(-rotUpDown, rotLeftRight, 0);
                    smoothTime = SmoothTime;
                }
                else
                {
                    if (smoothTime > 0)
                    {
                        playerControlComponent.Follow.localRotation = Quaternion.Lerp(Quaternion.identity, playerControlComponent.Follow.localRotation, Mathf.Clamp01(smoothTime / SmoothTime));
                        smoothTime -= deltaTime;
                    }
                    else
                    {
                        yAngle = ClampAngle(playerControlComponent.Follow.localEulerAngles.x, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);
                        yAngle = ClampAngle(yAngle - rotUpDown, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);

                        viewComponent.Transforms[0].Rotate(0, rotLeftRight, 0);
                        playerControlComponent.Follow.localEulerAngles = new Vector3(yAngle, 0, 0);
                    }
                }
            }

            if (characterController.isGrounded)
            {
                if (axisInput != null)
                {
                    playerControlComponent.motion = new Vector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                    playerControlComponent.motion = viewComponent.Transforms[0].TransformDirection(playerControlComponent.motion);
                    playerControlComponent.motion *= playerControlComponent.Speed;

                    var speed = 1f;
                    if (playerControlComponent.Aim.Value == AimMode.Shoulder)
                    {
                        speed = 0.25f;
                    }
                    else if (playerControlComponent.Aim.Value == AimMode.AimDownSight)
                    {
                        speed = 0.1f;
                    }
                    playerControlComponent.motion *= speed;
                    animator.SetFloat("Speed_f", (axisInput.Horizontal != Fix64.Zero || axisInput.Vertical != Fix64.Zero) ? Mathf.Clamp(speed, 0.3f, 1) : 0);
                }

                if (keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.Space))
                {
                    playerControlComponent.motion.y = playerControlComponent.JumpSpeed;
                }
            }

            animator.SetBool("Grounded", characterController.isGrounded || Physics.Raycast(viewComponent.Transforms[0].position, Vector3.down, 0.2f) || characterController.velocity.y >= 0);
            animator.SetBool("Jump_b", playerControlComponent.motion.y > 0);

            playerControlComponent.motion.y -= playerControlComponent.Gravity * deltaTime;
            playerControlComponent.smoothTime = smoothTime;
            characterController.Move(playerControlComponent.motion * deltaTime);

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
