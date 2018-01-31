using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownLoadPorgress : IProgress
{
	public object Current { get { throw new NotImplementedException(); } }

	public bool IsDone { get { throw new NotImplementedException(); } }

	public bool IsFailed { get { throw new NotImplementedException(); } }

	public float Progress { get { throw new NotImplementedException(); } }

	public string TargetName { get { throw new NotImplementedException(); } }

	public void Dispose() { throw new NotImplementedException(); }

	public bool MoveNext() { throw new NotImplementedException(); }

	public void Reset() { throw new NotImplementedException(); }
}