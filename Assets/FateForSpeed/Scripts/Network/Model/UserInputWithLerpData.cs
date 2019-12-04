using System.Collections.Generic;
using Common;

public class UserInputWithLerpData
{
    public int TickId;
    public float From;
    public float To;
    public Fix64 TotalTime;
    public Fix64 DeltaTime;
    public List<UserInputData[]> UserInputData;

    public void Loop()
    {
        if (To >= 1)
        {
            From = 0;
            To = 0;
            TotalTime = Fix64.Zero;
            DeltaTime = Fix64.Zero;
            UserInputData = null;
        }
    }
}
