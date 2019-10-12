using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class PlayerMoveSystem : SystemBehaviour
{
    protected IGroup playerControllers;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        playerControllers = this.Create(typeof(PlayerController), typeof(CharacterController));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        playerControllers.OnAdd().Subscribe(entity =>
        {
            var characterController = entity.GetComponent<CharacterController>();
            var playerController = entity.GetComponent<PlayerController>();
            var moveDirection = Vector3.zero;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (characterController.isGrounded)
                {
                    moveDirection = new Vector3(Input.GetAxis(InputParameters.Vertical), 0, Input.GetAxis(InputParameters.Horizontal));
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
}
