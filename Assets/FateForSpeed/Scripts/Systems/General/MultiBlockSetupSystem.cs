using System.Collections.Generic;
using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using UniEasy;
using UniRx;

public class MultiBlockSetupSystem : SystemBehaviour
{
    private IPoolSystem poolSystem;

    protected IPoolSystem PoolSystem
    {
        get
        {
            if (poolSystem == null)
            {
                poolSystem = ProjectContext.ProjectContainer.Resolve<PoolSystem>();
            }
            return poolSystem;
        }
    }

    protected IGroup blockSetupers;
    private Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>> AllocedPrefabs = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();

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
                    PoolSystem.Alloc(prefab, transform, blockSetuper.MaxVisibleCount * maximum[prefab]);
                }

                int index = 0;
                blocks.Add(AllocBlock(blockSetuper.Setup.Blocks[index], viewComponent.Transforms[0]));

                EventSystem.OnEvent<RequestBlockProcessingEvent>().Subscribe(evt =>
                {
                    index++;
                    if (index < blockSetuper.Setup.Blocks.Count)
                    {
                        var block = AllocBlock(blockSetuper.Setup.Blocks[index], viewComponent.Transforms[0]);

                        block.transform.position = new Vector3(0, 0, 50 * index);
                        blocks.Add(block);
                    }
                    while (blocks.Count > blockSetuper.MaxVisibleCount)
                    {
                        RecycleBlock(blocks[0]);
                        blocks.RemoveAt(0);
                    }
                }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer).AddTo(disposer);
            }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer);

            viewComponent.Transforms[0].OnInactiveAsObservable().Subscribe(_ =>
            {
                disposer.Clear();
                foreach (var block in blocks)
                {
                    RecycleBlock(block);
                }
                foreach (var prefab in prefabs)
                {
                    PoolSystem.Alloc(prefab, transform, 0);
                }
            }).AddTo(this.Disposer).AddTo(blockSetuper.Disposer);
        }).AddTo(this.Disposer);
    }

    protected GameObject AllocBlock(EasyBlock easyBlock, Transform parent)
    {
        var block = new GameObject(easyBlock.name);

        block.transform.SetParent(parent);
        AllocedPrefabs.Add(block, new Dictionary<GameObject, List<GameObject>>());
        foreach (var kvp in easyBlock.ToDictionary())
        {
            var go = PoolSystem.Alloc(kvp.Value.GameObject, transform);

            go.transform.SetParent(block.transform, false);
            go.transform.localPosition = kvp.Value.LocalPosition;
            go.transform.localRotation = kvp.Value.LocalRotation;
            go.transform.localScale = kvp.Value.LocalScale;

            if (!AllocedPrefabs[block].ContainsKey(kvp.Value.GameObject))
            {
                AllocedPrefabs[block].Add(kvp.Value.GameObject, new List<GameObject>());
            }
            AllocedPrefabs[block][kvp.Value.GameObject].Add(go);
        }

        return block;
    }

    protected void RecycleBlock(GameObject block)
    {
        if (AllocedPrefabs.ContainsKey(block))
        {
            foreach (var kvp in AllocedPrefabs[block])
            {
                foreach (var go in kvp.Value)
                {
                    PoolSystem.Recycle(kvp.Key, go, transform);
                }
            }

            AllocedPrefabs.Remove(block);
            Destroy(block);
        }
    }
}
