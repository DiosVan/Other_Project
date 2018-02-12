using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LoadAssetHandler : MonoBehaviour
{
	public delegate void OnDownLoadProgress(float progress, bool isComplete, string error);

	public static LoadAssetHandler Instance = null;

	private Dictionary<string, AssetBundle> mBundleDict = new Dictionary<string, AssetBundle>();

	private ManifestManager mMainfestMgr;

	private int loadCompleteNum = 0;

	private float mFullSize = 0f;

	private float mloadingSize = 0f;

	#region dropbox
	private Dictionary<string, string> mDropboxDict = new Dictionary<string, string>();
	#endregion

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
		#region dropbox
		mDropboxDict["background"] = "https://www.dropbox.com/s/muhm1gccgftqdrq/";
		mDropboxDict["finalani_1"] = "https://www.dropbox.com/s/ajcmzf0axb5qumx/";
		mDropboxDict["avatar_ch1"] = "https://www.dropbox.com/s/wk92on19ac3wqk3/";
		mDropboxDict["avatar_ch2"] = "https://www.dropbox.com/s/wd7rr1seub7axpk/";
		#endregion
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

		loadCompleteNum = 0;
		mloadingSize = 0;
		mFullSize = 0;

		string platform = string.Empty;

#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
#else
		string basePath = string.Format("file://{0}/AssetBundles/", Application.dataPath);
		platform = "StandaloneWindows";
#endif

		yield return StartCoroutine(CalculateFullSize(downlist));

		List<AssetLoader> loadQuene = new List<AssetLoader>();

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
#if false
			downlist[key].LoadPath = assetURL;
#endif
			LoadAssetHandler.Instance.StartCoroutine(asb.DownloadProcess(downlist[key], () => { return DownloadCompleteListenr(downlist[key]); }));
		}

		while (loadCompleteNum < downlist.Count)
			yield return null;

		foreach (KeyValuePair<string, AssetBundle> kv in mBundleDict)
			Debug.Log(kv.Key + "/" + kv.Value);
	}

	IEnumerator CalculateFullSize(Dictionary<string, AssetRequest> downlist)
	{
		string platform = string.Empty;

#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
#else
		string basePath = string.Format("file://{0}/AssetBundles/", Application.dataPath);
		platform = "StandaloneWindows";
#endif

		foreach (KeyValuePair<string, AssetRequest> kv in downlist)
		{
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

					Debug.Log(key + ": " + fileSize + "/" + (fileSize / 1024));

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
		mloadingSize = 0;
		foreach (KeyValuePair<string, AssetRequest> kv in mMainfestMgr.MainfestDict)
			mloadingSize += mMainfestMgr.MainfestDict[kv.Key].DownloadSize;

		//Debug.Log(string.Format("[{0}]:{1}%", down, progress));
		Debug.Log(string.Format("----Full Get {0}kb, progress {1}%----", mloadingSize, (mloadingSize / mFullSize) * 100f));
		//
	}

	private AssetRequest DownloadCompleteListenr(AssetRequest req)
	{
		loadCompleteNum++;

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