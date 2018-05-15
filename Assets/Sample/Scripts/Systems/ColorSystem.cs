using UniRx.Triggers;
using UnityEngine;
using System.Linq;
using UniECS;
using System;
using UniRx;

public class ColorSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var ColorEntities = GroupFactory.Create (new Type[] {
			typeof(ColorComponent)
		});

		ColorEntities.OnAdd ()
			.Select (entity => entity.GetComponent<ColorComponent> ())
			.Subscribe (colorComponent => {

			colorComponent.ColorCollection.ObserveAdd ().Subscribe (cc => {
				cc.Value.OnDestroyAsObservable ().Subscribe (_ => {
					colorComponent.ColorCollection.Remove (cc.Value);
				}).AddTo (this.Disposer).AddTo (colorComponent.Disposer);
			}).AddTo (this.Disposer).AddTo (colorComponent.Disposer);

			colorComponent.ColorCollection.ObserveRemove ().Select (_ => true)
					.Merge (colorComponent.IncludeChild.DistinctUntilChanged ().Where (b => b == true))
					.Subscribe (_ => {
				var renderers = colorComponent.GetComponentsInChildren<Renderer> ();
				foreach (var renderer in renderers) {
					var colComponent = renderer.GetComponent<ColorComponent> () ?? renderer.gameObject.AddComponent<ColorComponent> ();
					if (colorComponent.ColorCollection.Contains (colComponent)) {
						continue;
					}
					var entityBehaviour = renderer.GetComponent<EntityBehaviour> () ?? renderer.gameObject.AddComponent<EntityBehaviour> ();
					colComponent.IncludeChild.Value = false;
					colComponent.Color.Value = colorComponent.Color.Value;
					if (!entityBehaviour.Entity.HasComponent<ColorComponent> ()) {
						entityBehaviour.Entity.AddComponent (colComponent);
					}
					colorComponent.ColorCollection.Add (colComponent);
				}
			}).AddTo (this.Disposer).AddTo (colorComponent.Disposer);
		}).AddTo (this.Disposer);

		ColorEntities.OnAdd ().Subscribe (entity => {
			var colorComponent = entity.GetComponent<ColorComponent> ();
			var renderer = colorComponent.GetComponent<Renderer> ();

			if (!colorComponent.ColorCollection.Contains (colorComponent)) {
				colorComponent.ColorCollection.Add (colorComponent);
			}

			colorComponent.Color.DistinctUntilChanged ().Subscribe (color => {
				foreach (var cc in colorComponent.ColorCollection) {
					cc.Color.Value = color;
				}

				if (renderer != null) {
					foreach (var mat in renderer.materials) {
						mat.SetColor ("_MainColor", color);
					}
				}
			}).AddTo (this.Disposer).AddTo (colorComponent.Disposer);
		}).AddTo (this.Disposer);
	}
}
