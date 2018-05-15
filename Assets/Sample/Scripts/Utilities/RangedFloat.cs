using System;

[Serializable]
public struct RangedFloat
{
	public float min;
	public float max;

	public RangedFloat (float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
