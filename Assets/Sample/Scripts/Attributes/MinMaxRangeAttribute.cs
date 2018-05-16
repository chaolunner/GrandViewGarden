using UnityEngine;

public class MinMaxRangeAttribute : PropertyAttribute
{
	public float minLimit = 0;
	public float maxLimit = 0;

	public MinMaxRangeAttribute (float min, float max)
	{
		this.minLimit = min;
		this.maxLimit = max;
	}
}
