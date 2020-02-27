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

    private readonly int Speed_f = Animator.StringToHash("Speed_f");
    private readonly int Grounded = Animator.StringToHash("Grounded");
    private readonly int Jump_b = Animator.StringToHash("Jump_b");
    private readonly int Crouch_b = Animator.StringToHash("Crouch_b");
    private readonly int Head_Vertical_f = Animator.StringToHash("Head_Vertical_f");
    private readonly int Head_Horizontal_f = Animator.StringToHash("Head_Horizontal_f");
    private readonly int Body_Vertical_f = Animator.StringToHash("Body_Vertical_f");
    private readonly int Body_Horizontal_f = Animator.StringToHash("Body_Horizontal_f");
    private static readonly Fix64 SmoothTime = 1;
    private static readonly Fix64 ShoulderAimTime = 0.2f;

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
                        playerControlComponent.motion = new FixVector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
                    }
                    else
                    {
                        playerControlComponent.motion = new FixVector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
                    }
                    playerControlComponent.motion = (FixVector3)viewComponent.Transforms[0].TransformDirection((Vector3)playerControlComponent.motion);
                    playerControlComponent.motion *= playerControlComponent.Run;

                    if (Input.GetButton(InputParameters.Jump))
                    {
                        playerControlComponent.motion.y = playerControlComponent.Jump;
                    }
                }

                playerControlComponent.motion.y -= playerControlComponent.Gravity * Time.deltaTime;
                characterController.Move((Vector3)playerControlComponent.motion * Time.deltaTime);
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
            var axisInput = userInputData[0].GetInput<AxisInput>();
            var keyInput = userInputData[1].GetInput<KeyInput>();
            var mouseInput = userInputData[2].GetInput<MouseInput>();
            var playerControlResult = new PlayerControlResult();
            var smoothTime = playerControlComponent.smoothTime;
            var yAngle = Fix64.Zero;

            playerControlResult.Rotation = viewComponent.Transforms[0].rotation;
            playerControlResult.Follow = playerControlComponent.Follow.rotation;
            playerControlResult.Position = viewComponent.Transforms[0].position;

            if (mouseInput != null && keyInput != null && axisInput != null)
            {
                var rotLeftRight = mouseInput.Delta.x * playerControlComponent.MouseSensivity.x * deltaTime;
                var rotUpDown = mouseInput.Delta.y * playerControlComponent.MouseSensivity.y * deltaTime;

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

                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt))
                {
                    playerControlComponent.Follow.rotation = (Quaternion)((FixQuaternion)playerControlComponent.Follow.rotation * FixQuaternion.Euler(-rotUpDown, rotLeftRight, 0));
                    animator.SetFloat(Head_Vertical_f, 0);
                    animator.SetFloat(Body_Vertical_f, 0);
                    smoothTime = SmoothTime;
                }
                else
                {
                    if (smoothTime > 0)
                    {
                        playerControlComponent.Follow.localRotation = (Quaternion)FixQuaternion.Lerp(FixQuaternion.identity, (FixQuaternion)playerControlComponent.Follow.localRotation, FixMath.Clamp01(smoothTime / SmoothTime));
                        animator.SetFloat(Head_Vertical_f, 0);
                        animator.SetFloat(Body_Vertical_f, 0);
                        smoothTime -= deltaTime;
                    }
                    else
                    {
                        yAngle = ClampAngle(playerControlComponent.Follow.localEulerAngles.x, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);
                        yAngle = ClampAngle(yAngle - rotUpDown, playerControlComponent.YAngleLimit.Min, playerControlComponent.YAngleLimit.Max);

                        var vertical = 2 * (0.5f - FixMath.Abs(yAngle - playerControlComponent.YAngleLimit.Min) / FixMath.Abs(playerControlComponent.YAngleLimit.Max - playerControlComponent.YAngleLimit.Min));
                        animator.SetFloat(Head_Vertical_f, 0.5f * (float)vertical);
                        animator.SetFloat(Body_Vertical_f, (float)vertical);
                        viewComponent.Transforms[0].rotation = (Quaternion)((FixQuaternion)viewComponent.Transforms[0].rotation * FixQuaternion.Euler(0, rotLeftRight, 0));
                        playerControlComponent.Follow.localEulerAngles = (Vector3)new FixVector3(yAngle, 0, 0);
                    }
                }

                if (characterController.isGrounded)
                {
                    playerControlComponent.motion = new FixVector3((float)axisInput.Horizontal, 0, (float)axisInput.Vertical);
                    playerControlComponent.motion = (FixVector3)viewComponent.Transforms[0].TransformDirection((Vector3)playerControlComponent.motion);

                    Fix64 t = 0.75f;
                    Fix64 speed = playerControlComponent.Walk;
                    Fix64 h = FixMath.Abs(axisInput.Horizontal);
                    Fix64 sum = h + FixMath.Abs(axisInput.Vertical);
                    Fix64 horizontal = sum == 0 ? 0 : axisInput.Horizontal * (h / sum);
                    if (playerControlComponent.Aim.Value == AimMode.Shoulder)
                    {
                        t = 0.5f;
                    }
                    else if (playerControlComponent.Aim.Value == AimMode.AimDownSight)
                    {
                        t = 0.25f;
                    }
                    else if (keyInput != null && keyInput.KeyCodes.Contains((int)KeyCode.LeftShift))
                    {
                        t = 1f;
                        speed = playerControlComponent.Run;
                    }

                    playerControlComponent.Crouched = keyInput.KeyCodes.Contains((int)KeyCode.LeftControl);
                    animator.SetBool(Crouch_b, playerControlComponent.Crouched);
                    if (playerControlComponent.Crouched)
                    {
                        speed = 0;
                    }

                    playerControlComponent.motion *= t * speed;
                    animator.SetFloat(Speed_f, (axisInput.Horizontal != Fix64.Zero || axisInput.Vertical != Fix64.Zero) ? (float)FixMath.Clamp(t, 0.3f, 1f) : 0f);
                    animator.SetFloat(Head_Horizontal_f, 0.25f * (float)horizontal);
                    animator.SetFloat(Body_Horizontal_f, 0.35f * (float)horizontal);
                }

                if (keyInput.KeyCodes.Contains((int)KeyCode.Space) && !playerControlComponent.Crouched)
                {
                    playerControlComponent.motion.y = playerControlComponent.Jump;
                }
            }

            playerControlComponent.motion.y -= playerControlComponent.Gravity * deltaTime;
            playerControlComponent.smoothTime = smoothTime;
            characterController.Move((Vector3)(playerControlComponent.motion * deltaTime));

            animator.SetBool(Jump_b, playerControlComponent.motion.y > 0);
            animator.SetBool(Grounded, characterController.isGrounded && playerControlComponent.motion.y <= 0);

            return new IUserInputResult[] { playerControlResult };
        }).AddTo(this.Disposer);
    }

    private Fix64 ClampAngle(Fix64 angle, Fix64 min, Fix64 max)
    {
        while (angle < -360) { angle += 360; }
        while (angle > 360) { angle -= 360; }
        if (angle > max && angle - 360 >= min) { return angle - 360; }
        if (angle < min && angle + 360 <= max) { return angle + 360; }
        return FixMath.Clamp(angle, -max, -min);
    }
}
