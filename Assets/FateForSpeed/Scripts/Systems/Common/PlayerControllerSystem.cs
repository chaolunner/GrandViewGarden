using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using Common;
using UniRx;

public class PlayerControllerSystem : SystemBehaviour
{
    [MinMaxRange(0, 180)]
    public RangedFloat InvertRange = new RangedFloat(35, 145);
    [Range(1, 50)]
    public float Smooth = 10;

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

        PlayerControllerComponents.OnAdd().Subscribe(entity =>
        {
            var characterController = entity.GetComponent<CharacterController>();
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            var moveDirection = Vector3.zero;
            var offline = true;
            var isLocalPlayer = false;

            TransformInput currentTransform = new TransformInput();
            currentTransform.Position = (FixVector3)characterController.transform.position;
            currentTransform.Rotation = (FixQuaternion)characterController.transform.rotation;
            currentTransform.LocalScale = (FixVector3)characterController.transform.localScale;

            if (entity.HasComponent<NetworkPlayerComponent>())
            {
                var networkPlayerComponent = entity.GetComponent<NetworkPlayerComponent>();

                offline = false;
                isLocalPlayer = networkPlayerComponent.IsLocalPlayer;

                EventSystem.OnEvent<UserInputEvent>().Where(evt => evt.UserId == networkPlayerComponent.UserId && evt.Input is TransformInput).Subscribe(evt =>
                {
                    currentTransform = evt.Input as TransformInput;
                }).AddTo(this.Disposer).AddTo(playerController.Disposer);
            }

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (offline || isLocalPlayer)
                {
                    if (characterController.isGrounded)
                    {
                        moveDirection = GetBestInputDirection(transform.forward, mainCamera.transform.forward);
                        moveDirection = transform.TransformDirection(moveDirection);
                        moveDirection *= playerController.Speed;

                        if (Input.GetButton(InputParameters.Jump))
                        {
                            moveDirection.y = playerController.JumpSpeed;
                        }
                    }

                    moveDirection.y -= playerController.Gravity * Time.deltaTime;
                    characterController.Move(moveDirection * Time.deltaTime);
                }

                if (isLocalPlayer)
                {
                    TransformInput transformInput = new TransformInput();
                    transformInput.Position = (FixVector3)characterController.transform.position;
                    LockstepUtility.AddInput(transformInput);
                }
                else if (!offline)
                {
                    characterController.transform.position = Vector3.Lerp(characterController.transform.position, (Vector3)currentTransform.Position, Smooth * Time.deltaTime);
                }
            }).AddTo(this.Disposer).AddTo(playerController.Disposer);
        }).AddTo(this.Disposer);
    }

    public Vector3 GetBestInputDirection(Vector3 from, Vector3 to)
    {
        var direction = Vector3.zero;
        var angle = Vector3.Angle(from, to);
        if (angle < InvertRange.Min || angle > InvertRange.Max)
        {
            direction = new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
        }
        else
        {
            direction = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
        }
        return direction.normalized;
    }
}
