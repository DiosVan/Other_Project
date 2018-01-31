using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetScript : MonoBehaviour
{
	private static BuildTarget mTargetPlatform = EditorUserBuildSettings.activeBuildTarget;
	public enum CompressionType
	{
		Uncompress,
		LZMA,
		LZ4
	}

	[MenuItem("Custom Editor/Build AssetBundles Default(LZMA)", false, 0)]
	static void PackAndroidAsset()
	{
		ExecCreateAssetBundles(CompressionType.LZMA);
	}

	[MenuItem("Custom Editor/Build AssetBundles LZ4", false, 1)]
	static void PackIosAsset()
	{
		ExecCreateAssetBundles(CompressionType.LZ4);
	}

	[MenuItem("Custom Editor/Build AssetBundles Uncompress", false, 2)]
	static void PackWindowAsset()
	{
		ExecCreateAssetBundles(CompressionType.Uncompress);
	}

	static void ExecCreateAssetBundles(CompressionType compressionType)
	{
		BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.None;
		switch (compressionType)
		{
			case CompressionType.LZMA:
				break;
			case CompressionType.LZ4:
				buildOptions = BuildAssetBundleOptions.ChunkBasedCompression;
				break;
			case CompressionType.Uncompress:
				buildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
				break;
		}

		// AssetBundle 的資料夾名稱及副檔名
		string targetDir = "AssetBundles";
		string extensionName = ".assetBundles";

		//取得在 Project 視窗中選擇的資源(包含資料夾的子目錄中的資源)
		Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		string folderPath = string.Format("{0}/{1}", targetDir, mTargetPlatform.ToString());

		//建立存放 AssetBundle 的資料夾
		if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

		//Debug.Log(SelectedAsset.Length);

		foreach (Object obj in SelectedAsset)
		{
			//資源檔案路徑
			string sourcePath = AssetDatabase.GetAssetPath(obj);

			// AssetBundle 儲存檔案路徑
			string targetPath = string.Format("{0}{1}{2}", targetDir, Path.DirectorySeparatorChar, mTargetPlatform.ToString()) + Path.DirectorySeparatorChar + obj.name + extensionName;

			//取代檔案
			if (File.Exists(targetPath)) File.Delete(targetPath);

			//
			if (!(obj is GameObject) && !(obj is Texture2D) && !(obj is Material)) continue;
		}

		BuildPipeline.BuildAssetBundles(folderPath, buildOptions, mTargetPlatform);
	}
}
