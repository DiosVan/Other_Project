﻿using System;
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

	public static void Initial()
	{
		if (null == Instance)
		{
			var go = new GameObject("AssetHandler", typeof(LoadAssetHandler));
			DontDestroyOnLoad(go);
			Instance = go.GetComponent<LoadAssetHandler>();

			Instance.mBundleDict = new Dictionary<string, AssetBundle>();
		}
	}

	public void DownloadAsset()
	{
		if (null == Instance)
			return;

		StartCoroutine(LoadBundleList());
	}

	private IEnumerator LoadBundleList()
	{
		while (!Caching.ready)
			yield return null;

#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
#else
		string basePath = string.Format("file://{0}/AssetBundles/StandaloneWindows/", Application.dataPath);
		string mainbundleName = "StandaloneWindows";
#endif

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
				/*StartCoroutine(LoadAs());*/StartCoroutine(TestLoad(assetList));

			//foreach (var ab in assetList)
			//	Debug.Log("ab:" + ab);
		}
		else
			Debug.Log(mainbundleName + ".manifest has not found.");
	}

	AssetLoader asb;
	private IEnumerator TestLoad(List<string> assetList)
	{
		//for (int i = 0; i < assetList.Count; i++)
		{
			
			string basePath = string.Format("file://{0}/AssetBundles/StandaloneWindows/", Application.dataPath);
			string url = basePath + assetList[0];
			string urlPath = "https://www.dropbox.com/s/14t9wbb6u67n4dw/background?dl=1";
			Debug.Log(url);

			//var w = UnityWebRequest.GetAssetBundle(urlPath);
			//var mRequestOperation = w.Send();

			////IsDone = mRequestOperation.isDone;

			//while (!mRequestOperation.isDone)
			//{
			//	yield return null;
			//	Debug.Log("---- :" + mRequestOperation.progress);
			//}

			var w = UnityWebRequest.GetAssetBundle(urlPath);
			asb = new AssetLoader(w);
			//var ssss = asb.mWWW.Send();
			//StartCoroutine(asb.DownloadStart(w, FF));
			//yield return null;
			//while (!asb.MoveNext())
			//	yield return null;

			//yield return asb;
			while (!asb.MoveNext())
			{
				Debug.Log(string.Format("[{0}]_{1}%", assetList[0], asb.Progress));
				yield return null;
			}
			var assetBundle = DownloadHandlerAssetBundle.GetContent(asb.mWWW);
			//Debug.Log(string.Format("[{0}]{1}_finish", assetList[i], asb.Content));
			Debug.Log("---------------- "+ assetBundle);
			

			
		}

		foreach (KeyValuePair<string, AssetBundle> kv in mBundleDict)
			Debug.Log(kv.Key + "/" + kv.Value);
	}

	private void FF(object www)
	{
		UnityWebRequest ee = (UnityWebRequest)www;
		DownloadHandlerAssetBundle.GetContent(ee);
	}

	private IEnumerator LoadAs()
	{
		string path = string.Format("file://{0}/AssetBundles/{1}", Application.dataPath, "background");
		string urlPath = "https://www.dropbox.com/s/14t9wbb6u67n4dw/background?dl=1";

		var request = UnityWebRequest.GetAssetBundle(urlPath);
		
		var requestHandler = request.Send();

		while (!requestHandler.isDone)
		{
			yield return null;
			Debug.Log(requestHandler.progress);
			// TODO: Show progress bar, op.progress
		}

		var assetBundle = DownloadHandlerAssetBundle.GetContent(request);

		//request.Dispose();
		

		string[] strr = assetBundle.GetAllAssetNames();

		for (int i = 0; i < strr.Length; i++)
			Debug.Log("??:" + strr[i]);
	}
}
