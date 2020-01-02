using System;
using UniRx;

public enum AimMode
{
    Free, // Hip fire
    Shoulder, // Shoulder Shot or Third Person Aiming Mode (3rd person)
    AimDownSight, // Open mirror (ADS aim)
}

[Serializable]
public class AimModeReactiveProperty : ReactiveProperty<AimMode>
{
    public AimModeReactiveProperty() { }
    public AimModeReactiveProperty(AimMode initialValue) : base(initialValue) { }
}
