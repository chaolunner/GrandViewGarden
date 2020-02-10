using UniEasy.ECS;

public enum LoadingScreenLayer
{
    Default,
    Overlay,
}

public class LoadingScreen : ComponentBehaviour
{
    public FadeStateReactiveProperty State;
    public LoadingScreenLayer Layer;
}
