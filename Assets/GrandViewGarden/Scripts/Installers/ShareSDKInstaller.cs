using cn.sharesdk.unity3d;
using UnityEngine;
using UniEasy.DI;

public class ShareSDKInstaller : MonoInstaller
{
    [SerializeField]
    private ShareSDK shareSDK;

    public override void InstallBindings()
    {
        Container.Bind<ShareSDK>().FromInstance(shareSDK);
    }
}
