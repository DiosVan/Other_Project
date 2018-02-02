using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetLoader : ILoader, IEnumerator
{
	#region progress
	public string TargetName { get; set; }
	public float Progress { get { return mRequestOperation.progress; } }
	public bool IsFailed { get { return (null != mWebRequest) ? mWebRequest.isError : false; } }
	public bool IsDone { get { return mRequestOperation.isDone; } }
	#endregion

	#region ILoader
	public void StartDownload()
	{
		if (null != mWebRequest)
		{
			mRequestOperation = mWebRequest.Send();

			while (!mRequestOperation.isDone)
			{
				if (null != OnDownloading)
					OnDownloading(TargetName, mRequestOperation.progress * 100f);
			}

			DownloadFinish();
		}
	}

	public void DownloadFinish()
	{
		if (null != OnDownloadComplete)
			OnDownloadComplete();
	}
	#endregion

	#region IDisposable
	void IDisposable.Dispose() { }
	#endregion

	#region IEnumerator
	public object Current { get; set; }

	public bool MoveNext() { return IsDone; }

	public void Reset() { }
	#endregion

	public event Action<string, float> OnDownloading;
	public event Action OnDownloadComplete;

	UnityWebRequest mWebRequest = null;
	AsyncOperation mRequestOperation = null;

	private static string BASE_URL = string.Format("file://{0}/AssetBundles/", Application.dataPath);

	public AssetLoader(string target, UnityWebRequest w)
	{
#if UNITY_ANDROID
#else
		string url = string.Format("{0}/{1}/{2}", BASE_URL, "StandaloneWindows", target);
		string mainfestURL = string.Format("{0}.manifest", url);
#endif
		TargetName = target;

		mWebRequest = w;
	}

	public AssetBundle GetContent()
	{
		if (IsDone)
		{
			var assetBundle = DownloadHandlerAssetBundle.GetContent(mWebRequest);
			return (AssetBundle)assetBundle;
		}
		else
			return null;
	}
}