using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ManifestLoader : ILoader<ManifestRequest>
{
	#region progress
	public string TargetName { get; set; }
	public float Progress { get; set; }
	public bool IsFailed { get { return wwwManifest.isError; } }
	public bool IsDone { get { return wwwManifest.isDone && wwwOperation.isDone; } }
	#endregion


	public event Action<string, DownloadHandler> OnComplete;
	#region ILoader
	public IEnumerator StartDownload(string target, string url)
	{
		TargetName = target;

		wwwManifest = UnityWebRequest.Get(url);
		wwwOperation = wwwManifest.Send();

		while (!IsDone)
		{
			yield return null;

		}

		if (null != OnComplete)
		{
			OnComplete(TargetName, wwwManifest.downloadHandler);
		}
	}

	void ILoader<ManifestRequest>.DownloadFinish() { }
	#endregion

	#region IDisposable
	void IDisposable.Dispose() { }
	#endregion

	#region IEnumerator
	public object Current { get; set; }

	public bool MoveNext() { return IsDone; }

	public void Reset() { }

	public IEnumerator DownloadProcess(ManifestRequest loadRequest, Func<ManifestRequest> callBack)
	{
		UnityWebRequest wwwManifest = UnityWebRequest.Get(loadRequest.LoadPath);
		loadRequest.LoadRequest = wwwManifest;
		AsyncOperation wwwOperation = loadRequest.LoadRequest.Send();

		while (!wwwOperation.isDone)
			yield return null;

		callBack();
	}

	#endregion

	public UnityWebRequest ThisWWW { get { return wwwManifest; } }
	public AsyncOperation ThisOperation { get { return wwwOperation; } }

	UnityWebRequest wwwManifest = null;
	AsyncOperation wwwOperation = null;

	//public ManifestLoader(MonoBehaviour mo)
	//{

	//	mo.StartCoroutine(DownloadProcess());

		

		
	//	//do
	//	//{
	//	//	Debug.Log(wwwOperation.progress);
	//	//}
	//	//while (!wwwOperation.isDone);

		
	//}

	//public IEnumerator vvv()
	//{
	//	while (!wwwOperation.isDone)
	//	{
	//		yield return null;
	//	}

	//	Debug.Log("cccccccc");

	//	if (null != OnComplete)
	//	{
	//		OnComplete(TargetName, wwwManifest.downloadHandler);
	//	}
	//}
}

