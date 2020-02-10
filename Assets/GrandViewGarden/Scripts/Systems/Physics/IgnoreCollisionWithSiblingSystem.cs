using System.Collections.Generic;
using UniEasy.ECS;
using UnityEngine;
using UniEasy;
using UniRx;

[ContextMenuAttribute("Physics/IgnoreCollisionWithSiblingSystem")]
public class IgnoreCollisionWithSiblingSystem : RuntimeSystem
{
    private IGroup ignoreCollisionWithSiblings;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        ignoreCollisionWithSiblings = this.Create(typeof(ViewComponent), typeof(IgnoreCollisionWithSibling));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        ignoreCollisionWithSiblings.OnAdd().Subscribe(entity =>
        {
            var ignoreCollisionWithSibling = entity.GetComponent<IgnoreCollisionWithSibling>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var colliderIndexs = new Dictionary<Transform, List<Collider>>();

            UpdateColliders(viewComponent.Transforms[0], colliderIndexs);

            IgnoreCollisionWithSibling(viewComponent.Transforms[0], colliderIndexs);

            EventSystem.OnEvent<ChildsChangedEvent>().Where(evt => evt.Parent == viewComponent.Transforms[0]).Subscribe(_ =>
            {
                UpdateColliders(viewComponent.Transforms[0], colliderIndexs);

                IgnoreCollisionWithSibling(viewComponent.Transforms[0], colliderIndexs);
            }).AddTo(this.Disposer).AddTo(ignoreCollisionWithSibling.Disposer);
        }).AddTo(this.Disposer);
    }

    private void UpdateColliders(Transform parent, Dictionary<Transform, List<Collider>> indexs)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (!indexs.ContainsKey(child))
            {
                indexs.Add(child, new List<Collider>());
            }
            var list = indexs[child];

            list.Clear();
            list.AddRange(child.GetComponentsInChildren<Collider>());
            indexs[child] = list;
        }
    }

    private void IgnoreCollisionWithSibling(Transform parent, Dictionary<Transform, List<Collider>> indexs)
    {
        var colliders1 = new List<Collider>();
        var colliders2 = new List<Collider>();

        for (int i = 0; i < parent.childCount; i++)
        {
            for (int j = 0; j < parent.childCount; j++)
            {
                if (indexs.TryGetValue(parent.GetChild(i), out colliders1))
                {
                    if (indexs.TryGetValue(parent.GetChild(j), out colliders2))
                    {
                        foreach (var collider1 in colliders1)
                        {
                            foreach (var collider2 in colliders2)
                            {
                                Physics.IgnoreCollision(collider1, collider2, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
