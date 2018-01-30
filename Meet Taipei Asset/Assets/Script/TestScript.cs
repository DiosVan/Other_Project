using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		LoadAssetHandler.Initial();

		LoadAssetHandler.Instance.DownloadAsset();
	}
	
	
}
