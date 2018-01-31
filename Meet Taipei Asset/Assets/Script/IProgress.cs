using System;
using System.Collections;

public interface IProgress : IEnumerator, IDisposable
{
	/// <summary> 目標名稱 </summary>
	string TargetName { get; }
	/// <summary> 完成且失敗 </summary>
	bool IsFailed { get; }
	/// <summary> 有完成 (但不一定是成功) </summary>
	bool IsDone { get; }
	/// <summary> 進度 </summary>
	float Progress { get; }
}