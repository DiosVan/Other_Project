using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

public class ManifestManager
{
	private List<string> mAssetList = new List<string>();

	private Dictionary<string, uint> mMainfestDict = new Dictionary<string, uint>();
	public Dictionary<string, uint> MainfestDict { get { return mMainfestDict; } }

	private int vcvv = 0;

	public IEnumerator LoadManifest()
	{
		string platform = string.Empty;

#if UNITY_ANDROID
		string basePath = string.Format("file://{0}/AssetBundles/Android/", Application.dataPath);
		string mainbundleName = "Android";
#else
		string basePath = string.Format("file://{0}/AssetBundles/", Application.dataPath);
		platform = "StandaloneWindows";
#endif

		string manifestURL = string.Format("{0}/{1}/{2}.manifest", basePath, platform, platform);
		UrlAppendTimeStamp(ref manifestURL);

		//Debug.Log("url :" + manifestURL);
		manifestURL = "https://www.dropbox.com/s/bz4hg8osne6f995/Android.manifest?dl=1";
		//ManifestLoader mfLoader = new ManifestLoader(platform, manifestURL);
		ManifestLoader mfLoader = new ManifestLoader();
		ManifestRequest mr = new ManifestRequest();
		mr.LoadPath = manifestURL;
		mr.LoadName = "Android";

		yield return LoadAssetHandler.Instance.StartCoroutine(mfLoader.DownloadProcess(mr, () => { return null; }));


		string[] lines = mr.LoadRequest.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

		mAssetList = new List<string>();
		foreach (var s in lines)
		{
			if (s.Contains("CRC: "))
			{
				int startCatchIndex = s.IndexOf("CRC: ") + 5;
				uint latestCRC = uint.Parse(s.Substring(startCatchIndex, s.Length - startCatchIndex));
			}

			if (s.Contains("Name: "))
			{
				int startCatchIndex = s.LastIndexOf("Name: ") + 6;
				string asName = s.Substring(startCatchIndex, s.Length - startCatchIndex);

				if (!mAssetList.Contains(asName))
					mAssetList.Add(asName);
			}
		}

		if (mAssetList.Count > 0)
		{
			int assetCount = mAssetList.Count;
			List<ManifestLoader> mfLoadQueue = new List<ManifestLoader>();
			foreach (var ms in mAssetList)
			{
				string asbManifestURL = string.Format("{0}/{1}/{2}.manifest", basePath, platform, ms);
				UrlAppendTimeStamp(ref asbManifestURL);

				ManifestLoader asbMfLoader = new ManifestLoader();
				ManifestRequest mmr = new ManifestRequest();
				mmr.LoadPath = asbManifestURL;
				mmr.LoadName = ms;

				LoadAssetHandler.Instance.StartCoroutine(asbMfLoader.DownloadProcess(mmr, () => { return AsbMfLoader_OnComplete(mmr); }));
			}
		}

		while (mMainfestDict.Count < mAssetList.Count)
			yield return null;
	}

	private ManifestRequest AsbMfLoader_OnComplete(ManifestRequest request)
	{
		uint assetCrc;
		string[] line = request.LoadRequest.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

		if (ParseAssetManifest(line, out assetCrc))
			mMainfestDict[request.LoadName] = assetCrc;

		return request;
	}

	private void UrlAppendTimeStamp(ref string url)
	{
		url += ((url.Contains("?")) ? "&" : "?") + "t=" + DateTime.Now.ToString("yyyyMMddHHmmss");
	}

	private bool ParseAssetManifest(string[] lines, /*out string str,*/ out uint crc)
	{
		//str = string.Empty;
		crc = 0;

		foreach (var s in lines)
		{
			if (s.Contains("CRC: "))
			{
				int startCatchIndex = s.IndexOf("CRC: ") + 5;
				crc = uint.Parse(s.Substring(startCatchIndex, s.Length - startCatchIndex));
			}

			if (crc > 0)
				return true;
		}

		return false;
	}
}

public class ManifestRequest : WebRequestBase { }