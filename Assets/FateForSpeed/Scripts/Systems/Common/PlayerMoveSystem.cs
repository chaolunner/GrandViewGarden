using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class PlayerMoveSystem : SystemBehaviour
{
    private IGroup PlayerControllerComponents;
    private Camera mainCamera;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        PlayerControllerComponents = this.Create(typeof(PlayerController), typeof(CharacterController));
        mainCamera = Camera.main;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        PlayerControllerComponents.OnAdd().Subscribe(entity =>
        {
            var characterController = entity.GetComponent<CharacterController>();
            var playerController = entity.GetComponent<PlayerController>();
            var moveDirection = Vector3.zero;

            Observable.EveryUpdate().Subscribe(_ =>
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
            }).AddTo(this.Disposer).AddTo(playerController.Disposer);
        }).AddTo(this.Disposer);
    }

    public Vector3 GetBestInputDirection(Vector3 from, Vector3 to)
    {
        if (Vector3.Angle(from, to) < 35)
        {
            return new Vector3(Input.GetAxis(InputParameters.Horizontal), 0, Input.GetAxis(InputParameters.Vertical));
        }
        else
        {
            return new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
        }
    }
}
