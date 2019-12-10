using UniEasy.DI;

public class NetworkInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<NetworkPrefabFactory>().To<NetworkPrefabFactory>().AsSingle();
        Container.Bind<LockstepFactory>().To<LockstepFactory>().AsSingle();
    }
}
