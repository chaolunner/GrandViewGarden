using ch.sycoforge.Decal;
using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class FootstepsSystem : SystemBehaviour
{
    public GameObject FootprintPrefab;

    private IGroup FootstepsComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        FootstepsComponents = this.Create(typeof(FootstepsComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        FootstepsComponents.OnAdd().Subscribe(entity =>
        {
            var footstepsComponent = entity.GetComponent<FootstepsComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            float rotation;
            RaycastHit hit;
            EasyDecal decal;

            footstepsComponent.Footsteps.OnAdd().Subscribe(index =>
            {
                if (Physics.Linecast(footstepsComponent.Foots[index].position, footstepsComponent.Foots[index].position + 0.1f * Vector3.down, out hit))
                {
                    rotation = viewComponent.Transforms[0].eulerAngles.y;
                    decal = EasyDecal.ProjectAt(FootprintPrefab, null, hit.point, hit.normal, rotation, 0.5f * Vector3.one);
                    decal.AtlasRegionIndex = index;
                }
                footstepsComponent.Footsteps.Remove(index);
            }).AddTo(this.Disposer).AddTo(footstepsComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
