using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : ILoader
{
	string IProgress.TargetName { get; }

	bool IProgress.IsFailed { get; }

	bool IProgress.IsDone { get; }

	float IProgress.Progress { get; }

	object IEnumerator.Current { get; }

	void IDisposable.Dispose()
	{
		throw new NotImplementedException();
	}

	void ILoader.DownloadFinish()
	{
		throw new NotImplementedException();
	}

	bool IEnumerator.MoveNext()
	{
		throw new NotImplementedException();
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}
}
