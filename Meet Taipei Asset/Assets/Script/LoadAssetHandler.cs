#define USE_DROPBOX
//#define USE_LOCAL

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

public class LoadAssetHandler : MonoBehaviour
{
	public static LoadAssetHandler Instance = null;

	private ManifestManager mMainfestMgr;

	private Dictionary<string, AssetBundle> mBundleDict = new Dictionary<string, AssetBundle>();

#if USE_DROPBOX
	private Dictionary<string, string> mDropboxDict = new Dictionary<string, string>();
#endif

	private int mLoadCompleteNum = 0;

	/// <summary>total Asset Size</summary>
	private float mFullSize = 0f;

	/// <summary>loaded Size</summary>
	private float mLoadingSize = 0f;

	public static void Initial()
	{
		if (null == Instance)
		{
			var go = new GameObject("AssetHandler", typeof(LoadAssetHandler));
			DontDestroyOnLoad(go);
			Instance = go.GetComponent<LoadAssetHandler>();
		}
	}

	void Awake()
	{
#if USE_DROPBOX
		mDropboxDict["background"] = "https://www.dropbox.com/s/3elzw7ug0pgk4n3/";
		mDropboxDict["testasset_1"] = "https://www.dropbox.com/s/htk4y8z7jb1r2pp/";
		mDropboxDict["avatar_ch1"] = "https://www.dropbox.com/s/wk92on19ac3wqk3/";
		mDropboxDict["avatar_ch2"] = "https://www.dropbox.com/s/wd7rr1seub7axpk/";
#endif
	}

	public void GetManifest(bool forceRedownloadAsset = false)
	{
		if (null == Instance)
			return;

		if (null == mMainfestMgr)
			mMainfestMgr = new ManifestManager();

		string cachePath = Application.persistentDataPath + "/UnityCache/Shared/";

		if (forceRedownloadAsset && Caching.CleanCache())
			Debug.Log("Successfully cleaned the cache.");

		StartCoroutine(StartDownload());
	}

	IEnumerator StartDownload()
	{
		yield return StartCoroutine(mMainfestMgr.LoadManifest());

		yield return StartCoroutine(GetAssetBundles(mMainfestMgr.MainfestDict));
	}

	private IEnumerator GetAssetBundles(Dictionary<string, AssetRequest> downlist)
	{
		//
		while (!Caching.ready)
			yield return null;

		mLoadCompleteNum = 0;
		mLoadingSize = 0;
		mFullSize = 0;

		yield return StartCoroutine(PreProcessAssetRequest(downlist));

		foreach (KeyValuePair<string, AssetRequest> kv in downlist)
		{
#if false
			string assetURL = string.Format("{0}/{1}/{2}", basePath, platform, kv.Key);

			#region dropbox
			string key = kv.Key.ToString().ToLower();
			assetURL = String.Format("{0}{1}?dl=1", mDropboxDict[key], key);
			#endregion

			long fileSize = 0;
			using (var headRequest = UnityWebRequest.Head(assetURL))
			{
				yield return headRequest.Send();
				if (headRequest.responseCode != 200)
				{
					// TODO: Error response
				}
				else
				{
					var contentLength = headRequest.GetResponseHeader("CONTENT-LENGTH");
					long.TryParse(contentLength, out fileSize);
				}
			}
#endif
			string key = kv.Key.ToString().ToLower();

			if (string.IsNullOrEmpty(downlist[key].LoadPath))
				continue;

			AssetLoader asb = new AssetLoader();
			asb.OnDownloading += DownloadingListener;

			StartCoroutine(asb.DownloadProcess(downlist[key], () => { return DownloadCompleteListenr(downlist[key]); }));
		}

		while (mLoadCompleteNum < downlist.Count)
			yield return null;

		foreach (KeyValuePair<string, AssetBundle> kv in mBundleDict)
			Debug.Log(kv.Key + "/" + kv.Value);
	}

	IEnumerator PreProcessAssetRequest(Dictionary<string, AssetRequest> downlist)
	{
		foreach (KeyValuePair<string, AssetRequest> kv in downlist)
		{

#if USE_DROPBOX
			string key = kv.Key.ToString().ToLower();
			string assetURL = String.Format("{0}{1}?dl=1", mDropboxDict[key], key);
#endif

			long fileSize = 0;
			using (var headRequest = UnityWebRequest.Head(assetURL))
			{
				yield return headRequest.Send();

				if (headRequest.responseCode != 200)
				{
					// TODO: Error response
				}
				else
				{
					var contentLength = headRequest.GetResponseHeader("CONTENT-LENGTH");
					long.TryParse(contentLength, out fileSize);

					//byte to kb
					mFullSize += (float)(fileSize / 1024);
					downlist[key].LoadPath = assetURL;
					downlist[key].BundleSize = (fileSize / 1024);
				}
			}
		}
	}

	private void DownloadingListener(string down, float progress)
	{
		mLoadingSize = 0;
		foreach (KeyValuePair<string, AssetRequest> kv in mMainfestMgr.MainfestDict)
			mLoadingSize += mMainfestMgr.MainfestDict[kv.Key].DownloadSize;

		//Debug.Log(string.Format("[{0}]:{1}%", down, progress));
		Debug.Log(string.Format("----Full Get {0}kb, progress {1}%----", mLoadingSize, (mLoadingSize / mFullSize) * 100f));
		//
	}

	private AssetRequest DownloadCompleteListenr(AssetRequest req)
	{
		mLoadCompleteNum++;

		var assetBundle = DownloadHandlerAssetBundle.GetContent(req.LoadRequest);

		mBundleDict[req.LoadName] = assetBundle;

		return req;
	}
}

public class AssetRequest : WebRequestBase
{
	public uint CRC;
	public Hash128 H128;
	public long BundleSize;
	public long DownloadSize;
}