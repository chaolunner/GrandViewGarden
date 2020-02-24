using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using UniEasy;
using UniRx;

public class MultiBlockSetupSystem : SystemBehaviour
{
    [Inject]
    public IPoolFactory PoolFactory;

    protected IGroup blockSetupers;
    private Dictionary<GameObject, Dictionary<GameObject, List<IEntity>>> blockDict = new Dictionary<GameObject, Dictionary<GameObject, List<IEntity>>>();

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        blockSetupers = this.Create(typeof(BlockSetuper), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        blockSetupers.OnAdd().Subscribe(entity =>
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var blockSetuper = entity.GetComponent<BlockSetuper>();
            var counter = new List<Dictionary<GameObject, int>>();
            var prefabs = new List<GameObject>();
            var blocks = new List<GameObject>();
            var maximum = new Dictionary<GameObject, int>();
            var disposer = new CompositeDisposable();

            foreach (var block in blockSetuper.Setup.Blocks)
            {
                var subCounter = new Dictionary<GameObject, int>();
                foreach (var kvp in block.ToDictionary())
                {
                    if (!subCounter.ContainsKey(kvp.Value.GameObject))
                    {
                        subCounter.Add(kvp.Value.GameObject, 1);
                    }
                    else
                    {
                        subCounter[kvp.Value.GameObject]++;
                    }
                    if (!prefabs.Contains(kvp.Value.GameObject))
                    {
                        prefabs.Add(kvp.Value.GameObject);
                    }
                }
                counter.Add(subCounter);
            }

            foreach (var prefab in prefabs)
            {
                var max = 0;
                foreach (var c in counter)
                {
                    if (c.ContainsKey(prefab) && c[prefab] > max)
                    {
                        max = c[prefab];
                    }
                }

                maximum.Add(prefab, max);
            }

            viewComponent.Transforms[0].OnActiveAsObservable().Subscribe(_ =>
            {
                foreach (var prefab in prefabs)
                {
                    for (int i = 0; i < blockSetuper.MaxVisibleCount * maximum[prefab]; i++)
                    {
                        PoolFactory.Despawn(PoolFactory.Spawn(prefab, transform));
                    }
                }

                int index = 0;
                blocks.Add(CreateBlock(blockSetuper.Setup.Blocks[index], viewComponent.Transforms[0]));

                EventSystem.OnEvent<RequestBlockProcessingEvent>().Subscribe(evt =>
                {
                    index++;
                    if (index < blockSetuper.Setup.Blocks.Count)
                    {
                        var block = CreateBlock(blockSetuper.Setup.Blocks[index], viewComponent.Transforms[0]);

                        block.transform.position = new Vector3(0, 0, 50 * index);
                        blocks.Add(block);
                    }
                    while (blocks.Count > blockSetuper.MaxVisibleCount)
                    {
                        RemoveBlock(blocks[0]);
                        blocks.RemoveAt(0);
                    }
                }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer).AddTo(disposer);
            }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer);

            viewComponent.Transforms[0].OnInactiveAsObservable().Subscribe(_ =>
            {
                disposer.Clear();
                foreach (var block in blocks)
                {
                    RemoveBlock(block);
                }
            }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer);
        }).AddTo(this.Disposer);
    }

    protected GameObject CreateBlock(EasyBlock easyBlock, Transform parent)
    {
        var block = new GameObject(easyBlock.name);

        block.transform.SetParent(parent);
        blockDict.Add(block, new Dictionary<GameObject, List<IEntity>>());
        foreach (var kvp in easyBlock.ToDictionary())
        {
            var entity = PoolFactory.Spawn(kvp.Value.GameObject);
            var viewComponent = entity.GetComponent<ViewComponent>();

            viewComponent.Transforms[0].SetParent(block.transform, false);
            viewComponent.Transforms[0].localPosition = kvp.Value.LocalPosition;
            viewComponent.Transforms[0].localRotation = kvp.Value.LocalRotation;
            viewComponent.Transforms[0].localScale = kvp.Value.LocalScale;

            if (!blockDict[block].ContainsKey(kvp.Value.GameObject))
            {
                blockDict[block].Add(kvp.Value.GameObject, new List<IEntity>());
            }
            blockDict[block][kvp.Value.GameObject].Add(entity);
        }

        return block;
    }

    protected void RemoveBlock(GameObject block)
    {
        if (blockDict.ContainsKey(block))
        {
            foreach (var kvp in blockDict[block])
            {
                foreach (var entity in kvp.Value)
                {
                    PoolFactory.Despawn(entity);
                }
            }

            blockDict.Remove(block);
            Destroy(block);
        }
    }
}
