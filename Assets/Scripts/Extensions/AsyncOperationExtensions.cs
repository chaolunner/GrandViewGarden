using System.Collections;
using UnityEngine;
using System;
using UniRx;

public static class AsyncOperationExtensions
{
	public static IObservable<float> ToObservable (this AsyncOperation asyncOperation)
	{
		if (asyncOperation == null) {
			throw new ArgumentNullException ("asyncOperation");
		}
		return Observable.FromCoroutine<float> ((observer, cancellationToken) => RunAsyncOperation (asyncOperation, observer, cancellationToken));
	}

	static IEnumerator RunAsyncOperation (AsyncOperation asyncOperation, IObserver<float> observer, CancellationToken cancellationToken)
	{
		asyncOperation.allowSceneActivation = false;
		while (!asyncOperation.isDone && !cancellationToken.IsCancellationRequested) {
			observer.OnNext (asyncOperation.progress);
			if (asyncOperation.progress >= 0.9f) {
				asyncOperation.allowSceneActivation = true;
				observer.OnCompleted ();
			}
			yield return null;
		}
		if (!cancellationToken.IsCancellationRequested) {
			observer.OnNext (asyncOperation.progress);
			observer.OnCompleted ();
		}
	}
}
