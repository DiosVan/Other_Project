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

		string basePath = string.Format("file://{0}/AssetBundles/StandaloneWindows/", Application.dataPath);

		string bundleName = "StandaloneWindows";
		string url = basePath + bundleName;
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
			string[] lines = wwwManifest.downloadHandler.text.Split(new string[] { "CRC: " }, StringSplitOptions.None);
			latestCRC = uint.Parse(lines[1].Split(new string[] { "\n" }, StringSplitOptions.None)[0]);
		}
		else
			Debug.Log(bundleName + ".manifest has not found.");

		Debug.Log("latestCRC :" + latestCRC);
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
