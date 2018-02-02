using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ManifestLoader : ILoader, IEnumerator
{
	#region progress
	public string TargetName { get; set; }
	public float Progress { get; set; }
	public bool IsFailed { get { return wwwManifest.isError; } }
	public bool IsDone { get { return wwwOperation.isDone; } }
	#endregion

	#region ILoader
	void ILoader.StartDownload() { }
	void ILoader.DownloadFinish() { }
	#endregion

	#region IDisposable
	void IDisposable.Dispose() { }
	#endregion

	#region IEnumerator
	public object Current { get; set; }

	public bool MoveNext() { return IsDone; }

	public void Reset() { }
	#endregion

	public UnityWebRequest ThisWWW { get { return wwwManifest; } }

	UnityWebRequest wwwManifest = null;
	AsyncOperation wwwOperation = null;

	public ManifestLoader(string target,string url)
	{
		TargetName = target;

		wwwManifest = UnityWebRequest.Get(url);
		wwwOperation = wwwManifest.Send();
	}
}