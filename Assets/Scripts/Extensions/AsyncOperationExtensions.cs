using System.Collections;
using UnityEngine;
using System;
using UniRx;

public static class AsyncOperationExtensions
{
	public static IObservable<int> ToObservable (this AsyncOperation asyncOperation)
	{
		if (asyncOperation == null) {
			throw new ArgumentNullException ("asyncOperation");
		}
		return Observable.FromCoroutine<int> ((observer, cancellationToken) => RunAsyncOperation (asyncOperation, observer, cancellationToken));
	}

	static IEnumerator RunAsyncOperation (AsyncOperation asyncOperation, IObserver<int> observer, CancellationToken cancellationToken)
	{
		var totalProgress = 0;
		var currentProgress = 0;
		asyncOperation.allowSceneActivation = false;
		while (!asyncOperation.isDone && !cancellationToken.IsCancellationRequested) {
			observer.OnNext (currentProgress);
			if (asyncOperation.progress < 0.9f) {
				totalProgress = (int)(asyncOperation.progress * 100);
			} else {
				asyncOperation.allowSceneActivation = true;
				totalProgress = 100;
			}
			if (currentProgress < totalProgress) {
				currentProgress++;
			}
			if (currentProgress == 100) {
				break;
			}
			yield return null;
		}
		observer.OnNext (currentProgress);
		observer.OnCompleted ();
	}
}
