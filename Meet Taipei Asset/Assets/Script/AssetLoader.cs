using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetLoader : ILoader, IEnumerator
{
	#region progress
	public string TargetName { get; set; }
	public float Progress { get { return mRequestOperation.progress; } }
	public bool IsFailed { get { return (null != mWebRequest) ? mWebRequest.isError : false; } }
	public bool IsDone { get { return mRequestOperation.isDone; } }
	#endregion

	#region ILoader
	void ILoader.DownloadFinish() { }
	#endregion

	#region IDisposable
	void IDisposable.Dispose() { }
	#endregion

	#region IEnumerator
	public object Current { get; set; }

	public bool MoveNext()
	{
		return IsDone;
	}

	public void Reset() { }
	#endregion

	private AssetBundle mContent = null;
	public UnityWebRequest mWWW
	{
		get { return mWebRequest; }
	}

	public UnityWebRequest mWebRequest = null;
	AsyncOperation mRequestOperation = null;
	//public AssetLoader(/*UnityWebRequest w*/)
	//{
	//	//mWebRequest = w;
	//}

	public IEnumerator DownloadStart(UnityWebRequest w,Action<object> OnFinish)
	{
		string urlPath = "https://www.dropbox.com/s/14t9wbb6u67n4dw/background?dl=1";
		mWebRequest = UnityWebRequest.Get(urlPath);
		//var vv = w;
		mRequestOperation = mWebRequest.Send();

		//IsDone = mRequestOperation.isDone;

		while (!mRequestOperation.isDone)
		{
			yield return null;
			Debug.Log("---- :" + mRequestOperation.progress);
		}

		OnFinish(mWebRequest);
	}
}