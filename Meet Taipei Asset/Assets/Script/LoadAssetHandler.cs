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

	private Dictionary<string, AssetBundle> mBundleDict;

	private ManifestManager mMainfestMgr;

	public static void Initial()
	{
		if (null == Instance)
		{
			var go = new GameObject("AssetHandler", typeof(LoadAssetHandler), typeof(ManifestManager));
			DontDestroyOnLoad(go);
			Instance = go.GetComponent<LoadAssetHandler>();
		}
	}

	public void GetManifest()
	{
		if (null == Instance)
			return;

		if (null == mMainfestMgr)
		{
			mMainfestMgr = new ManifestManager();
			mMainfestMgr.OnManifestDone += StartDownloadAsset;
		}

		//
		StartCoroutine(mMainfestMgr.LoadManifest());
	}

	public void StartDownloadAsset(Dictionary<string, uint> downDict)
	{
		if (null == mBundleDict)
			mBundleDict = new Dictionary<string, AssetBundle>();
		else
			mBundleDict.Clear();

		StartCoroutine(GetAssetBundles(downDict));
	}

#if false
	private IEnumerator LoadBundleList()
	{
		while (!Caching.ready)
			yield return null;

//#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
//#else
		string basePath = string.Format("file://{0}/AssetBundles/StandaloneWindows/", Application.dataPath);
		string mainbundleName = "StandaloneWindows";
//#endif

		string url = basePath + mainbundleName;
		string manifestURL = url + ".manifest";
		//
		url += ((url.Contains("?")) ? "&" : "?") + "t=" + DateTime.Now.ToString("yyyyMMddHHmmss");
		manifestURL += ((manifestURL.Contains("?")) ? "&" : "?") + "t=" + DateTime.Now.ToString("yyyyMMddHHmmss");

		UnityWebRequest wwwManifest = UnityWebRequest.Get(manifestURL);
		// 
		yield return wwwManifest.Send();

		// 
		uint latestCRC = 0;
		if (string.IsNullOrEmpty(wwwManifest.error))
		{
			// 
			//string[] lines = wwwManifest.downloadHandler.text.Split(new string[] { "CRC: " }, StringSplitOptions.None);
			//latestCRC = uint.Parse(lines[1].Split(new string[] { "\n" }, StringSplitOptions.None)[0]);
			string[] lines = wwwManifest.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

			List<string> assetList = new List<string>();
			foreach (var s in lines)
			{
				if (s.Contains("CRC: "))
				{
					int startCatchIndex = s.IndexOf("CRC: ") + 5;
					latestCRC = uint.Parse(s.Substring(startCatchIndex, s.Length - startCatchIndex));
				}

				if (s.Contains("Name: "))
				{
					int startCatchIndex = s.LastIndexOf("Name: ") + 6;
					string asName = s.Substring(startCatchIndex, s.Length - startCatchIndex);

					if (!assetList.Contains(asName))
						assetList.Add(asName);
				}
			}

			//用latestCRC有沒有更新來判定要不要下載(待...
			if (assetList.Count > 0)
			{
				/*StartCoroutine(LoadAs());*/
				StartCoroutine(TestLoad(assetList));
			}

			//foreach (var ab in assetList)
			//	Debug.Log("ab:" + ab);
		}
		else
			Debug.Log(mainbundleName + ".manifest has not found.");
	}
#endif

	private int loadCompleteNum = 0;

	private IEnumerator GetAssetBundles(Dictionary<string, uint> downlist)
	{
		//
		while (!Caching.ready)
			yield return null;

		loadCompleteNum = 0;

		string platform = string.Empty;

#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
#else
		string basePath = string.Format("file://{0}/AssetBundles/", Application.dataPath);
		platform = "StandaloneWindows";
#endif

		List<AssetLoader> loadQuene = new List<AssetLoader>();

		foreach (KeyValuePair<string, uint> kv in downlist)
		{
			string assetURL = string.Format("{0}/{1}/{2}", basePath, platform, kv.Key);
			var w = UnityWebRequest.GetAssetBundle(assetURL, kv.Value);
			AssetLoader asb = new AssetLoader(kv.Key, w);
			asb.OnDownloading += DownloadingListener;
			asb.OnDownloadComplete += DownloadCompleteListenr;
			loadQuene.Add(asb);

			asb.StartDownload();
		}

		while (loadCompleteNum < loadQuene.Count)
			yield return null;

		foreach (var asb in loadQuene)
			mBundleDict[asb.TargetName] = asb.GetContent();

		foreach (KeyValuePair<string, AssetBundle> kv in mBundleDict)
			Debug.Log(kv.Key + "/" + kv.Value);
	}

	private void DownloadingListener(string down, float progress)
	{
		Debug.Log(string.Format("[{0}]:{1}%", down, progress));
		//
	}

	private void DownloadCompleteListenr()
	{
		loadCompleteNum++;
	}
}
