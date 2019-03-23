using System;
using UniRx;

public enum FadeState
{
    FadedIn,
    FadingOut,
    FadedOut,
    FadingIn
}

[Serializable]
public class FadeStateReactiveProperty : ReactiveProperty<FadeState>
{
    public FadeStateReactiveProperty() { }
    public FadeStateReactiveProperty(FadeState initialValue) : base(initialValue) { }
}
