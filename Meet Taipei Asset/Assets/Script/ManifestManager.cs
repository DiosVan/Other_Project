#define USE_DROPBOX
//#define USE_LOCAL

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ManifestManager
{
	private List<string> mAssetList = new List<string>();

	private Dictionary<string, AssetRequest> mMainfestDict = new Dictionary<string, AssetRequest>();
	public Dictionary<string, AssetRequest> MainfestDict { get { return mMainfestDict; } }

	private Dictionary<string, string> mDropboxDict = new Dictionary<string, string>();

	private string mPlatform = string.Empty;

	public IEnumerator LoadManifest()
	{

#if USE_DROPBOX
		mDropboxDict["background"] = "https://www.dropbox.com/s/yhqzu2tqhmhbex4/";
		mDropboxDict["finalani_1"] = "https://www.dropbox.com/s/b431yjype0u4d6i/";
		mDropboxDict["avatar_ch1"] = "https://www.dropbox.com/s/e4xgv0n2xorkhv7/";
		mDropboxDict["avatar_ch2"] = "https://www.dropbox.com/s/joqklmteepjs5wm/";
#endif

		string manifestURL = string.Empty;

#if USE_LOCAL
#if UNITY_ANDROID
		mPlatform = "Android";
#else
		mPlatform = "StandaloneWindows";
#endif
		string basePath = string.Format("file://{0}/AssetBundles/{1}/", Application.dataPath, mPlatform);
		manifestURL = string.Format("{0}{1}.manifest", basePath, mPlatform);
#endif

#if USE_DROPBOX
		manifestURL = "https://www.dropbox.com/s/e8wxi1zrgn44yfl/AssetBundleList.txt?dl=1";
#endif

		UrlAppendTimeStamp(ref manifestURL);

		ManifestLoader mfLoader = new ManifestLoader();
		ManifestRequest mr = new ManifestRequest();
		mr.LoadPath = manifestURL;
		mr.LoadName = "Android";

		yield return LoadAssetHandler.Instance.StartCoroutine(mfLoader.DownloadProcess(mr, () => { return null; }));

#region parse json.txt to get asset list
		Dictionary<string, object> assetDict = (Dictionary<string, object>)MiniJSON.Json.Deserialize(mr.LoadRequest.downloadHandler.text);

		foreach (KeyValuePair<string, object> kv in assetDict)
		{
			AssetRequest asRq = new AssetRequest();
			var key = kv.Key.ToString().ToLower();
			var values = (List<object>)kv.Value;

			asRq.LoadName = key;
			asRq.H128 = Hash128.Parse(values[0].ToString());
			asRq.CRC = uint.Parse(values[1].ToString());

			mMainfestDict[asRq.LoadName] = asRq;
		}

		yield break;
#endregion

#region parse manifest to get asset list
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

		int requestCount = 0;
		if (mAssetList.Count > 0)
		{
			int assetCount = mAssetList.Count;
			List<ManifestLoader> mfLoadQueue = new List<ManifestLoader>();
			foreach (var ms in mAssetList)
			{
				if (!mDropboxDict.ContainsKey(ms.ToLower()))
					continue;

				string asbManifestURL = string.Empty;
#if USE_LOCAL
				asbManifestURL = string.Format("{0}{1}.manifest", basePath, ms);
				UrlAppendTimeStamp(ref asbManifestURL);
#endif

#if USE_DROPBOX
				asbManifestURL = string.Format("{0}{1}.manifest?dl=1", mDropboxDict[ms.ToLower()], ms.ToLower());
#endif

				ManifestLoader asbMfLoader = new ManifestLoader();
				ManifestRequest mmr = new ManifestRequest();
				mmr.LoadPath = asbManifestURL;
				mmr.LoadName = ms;

				LoadAssetHandler.Instance.StartCoroutine(asbMfLoader.DownloadProcess(mmr, () => { return AsbMfLoader_OnComplete(mmr); }));

				requestCount++;
			}
		}

		while (mMainfestDict.Count < requestCount)
			yield return null;
#endregion
	}

	private void UrlAppendTimeStamp(ref string url)
	{
		url += ((url.Contains("?")) ? "&" : "?") + "t=" + DateTime.Now.ToString("yyyyMMddHHmmss");
	}

	private ManifestRequest AsbMfLoader_OnComplete(ManifestRequest request)
	{
		AssetRequest asRq = new AssetRequest();
		asRq.LoadName = request.LoadName.ToLower();

		string[] line = request.LoadRequest.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

		if (ParseAssetManifest(line, ref asRq))
			mMainfestDict[request.LoadName] = asRq;

		return request;
	}

	private bool ParseAssetManifest(string[] lines, ref AssetRequest asRq)
	{
		var crc = lines[1];
		int startCatchIndex = crc.IndexOf("CRC: ") + 5;
		asRq.CRC = uint.Parse(crc.Substring(startCatchIndex, crc.Length - startCatchIndex));

		var hashRow = lines[5];
		asRq.H128 = Hash128.Parse(hashRow.Split(':')[1].Trim());

		if (asRq.CRC > 0 && asRq.H128 != (default(Hash128)))
			return true;

		return false;
	}
}

public class ManifestRequest : WebRequestBase { }