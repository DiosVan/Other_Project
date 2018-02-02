using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

public class ManifestManager
{
	public event Action<Dictionary<string, uint>> OnManifestDone;

	private Dictionary<string, uint> mMainfestDict = new Dictionary<string, uint>();

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

		ManifestLoader mfLoader = new ManifestLoader(platform, manifestURL);

		while (!mfLoader.IsDone)
			yield return null;

		if (string.IsNullOrEmpty(mfLoader.ThisWWW.error))
		{
			//
			string[] lines = mfLoader.ThisWWW.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

			List<string> assetList = new List<string>();
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

					if (!assetList.Contains(asName))
						assetList.Add(asName);
				}
			}

			if (assetList.Count > 0)
			{
				int assetCount = assetList.Count;
				List<ManifestLoader> mfLoadQueue = new List<ManifestLoader>();
				foreach (var ms in assetList)
				{
					string asbManifestURL = string.Format("{0}/{1}/{2}.manifest", basePath, platform, ms);
					UrlAppendTimeStamp(ref asbManifestURL);
					
					ManifestLoader asbMfLoader = new ManifestLoader(ms, asbManifestURL);
					mfLoadQueue.Add(asbMfLoader);
				}

				//check all manifest isDone
				while (null != mfLoadQueue.Find(i => i.IsDone == false))
					yield return null;

				foreach (var m in mfLoadQueue)
				{
					string assetName = m.TargetName;
					uint assetCrc;

					string[] line = m.ThisWWW.downloadHandler.text.Split(new string[] { "\n" }, StringSplitOptions.None);

					if (ParseAssetManifest(line,/* out assetName,*/ out assetCrc))
						mMainfestDict[assetName] = assetCrc;
				}
			}

			foreach (KeyValuePair<string, uint> kv in mMainfestDict)
				Debug.Log(string.Format("assetName:{0}/crc:{1}", kv.Key, kv.Value));
		}

		if (null != OnManifestDone)
			OnManifestDone(mMainfestDict);
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
			//else if (s.Contains("Name: "))
			//{
			//	int startCatchIndex = s.LastIndexOf("Name: ") + 6;
			//	str = s.Substring(startCatchIndex, s.Length - startCatchIndex);
			//}

			if (/*!string.IsNullOrEmpty(str) &&*/ crc > 0)
				return true;
		}

		return false;
	}
}