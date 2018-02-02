using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	void Start()
	{
		LoadAssetHandler.Initial();
		LoadAssetHandler.Instance.GetManifest();
	}
}