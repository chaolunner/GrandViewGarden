using UniRx.Triggers;
using UnityEngine;
using System;
using UniECS;
using UniRx;

public class RenderParticleSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var RenderParticleEntities = GroupFactory.Create (new Type[] {
			typeof(ParticleSystemRenderer),
			typeof(RenderParticle),
		});

		RenderParticleEntities.OnAdd ().Subscribe (entity => {
			var renderParticle = entity.GetComponent<RenderParticle> ();
			var particleSystemRenderer = entity.GetComponent<ParticleSystemRenderer> ();

			renderParticle.Target.DistinctUntilChanged ().Subscribe (target => {
				#if UNITY_EDITOR
				Observable.EveryUpdate ().TakeWhile (_ => target == renderParticle.Target.Value).Subscribe (_ => {
					particleSystemRenderer.enabled = target.IsVisible ();
				}).AddTo (this.Disposer).AddTo (renderParticle.Disposer);
				#else
				target.OnBecameVisibleAsObservable ().TakeWhile (_ => target == renderParticle.Target.Value).Subscribe (_ => {
					particleSystemRenderer.enabled = true;
				}).AddTo (this.Disposer).AddTo (renderParticle.Disposer);

				target.OnBecameInvisibleAsObservable ().TakeWhile (_ => target == renderParticle.Target.Value).Subscribe (_ => {
					particleSystemRenderer.enabled = false;
				}).AddTo (this.Disposer).AddTo (renderParticle.Disposer);
				#endif
			}).AddTo (this.Disposer).AddTo (renderParticle.Disposer);
		}).AddTo (this.Disposer);
	}
}
