using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetLoader : ILoader<AssetRequest>
{
	#region progress
	public string TargetName { get; set; }
	public float Progress { get { return mRequestOperation.progress; } }
	public bool IsFailed { get { return (null != mWebRequest) ? mWebRequest.isError : false; } }
	public bool IsDone { get { return mRequestOperation.isDone; } }
	#endregion

	#region ILoader

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

	public IEnumerator DownloadProcess(AssetRequest loadRequest, Func<AssetRequest> callBack)
	{
		TargetName = loadRequest.LoadName;

		bool isHaveCache = Caching.IsVersionCached(loadRequest.LoadPath, loadRequest.H128);

		if (isHaveCache)
			Debug.Log("Bundle with this hash is already cached!");
		else
			Debug.Log("No cached version founded for this hash..");

		UnityWebRequest wwwAssetRequest = UnityWebRequest.GetAssetBundle(loadRequest.LoadPath, loadRequest.H128, loadRequest.CRC);
		loadRequest.LoadRequest = wwwAssetRequest;
		AsyncOperation wwwOperation = loadRequest.LoadRequest.Send();

		while (!wwwOperation.isDone)
		{
			loadRequest.DownloadSize = (isHaveCache) ? loadRequest.BundleSize : (long)(wwwAssetRequest.downloadedBytes / 1024);

			if (null != OnDownloading)
				OnDownloading(loadRequest.LoadName, 100f * wwwOperation.progress);

			yield return null;
		}

		callBack();

		wwwAssetRequest.Dispose();
	}
}