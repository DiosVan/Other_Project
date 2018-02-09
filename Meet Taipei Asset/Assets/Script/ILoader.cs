using System;
using System.Collections;

public interface ILoader<T> : IProgress
{
	IEnumerator DownloadProcess(T loadRequest, Func<T> callBack);
	void DownloadFinish();
}