using UnityEngine;
using DG.Tweening;
using UniECS;

public abstract class FadeInSequence : ScriptableObject
{
	abstract public Sequence GetSequence (IEntity entity, float endValue);
}
