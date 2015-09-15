using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ファイルの移動やリネームを監視する
/// </summary>
public class FileMoveWatcher : System.IDisposable
{
	/// <summary>
	/// 監視対象アセット設定
	/// </summary>
	public struct FileWatchEntry
	{
		/// <summary>
		/// 監視対象のGUID
		/// </summary>
		public readonly string guid;
		/// <summary>
		/// 監視開始時点でのPath
		/// </summary>
		public readonly string assetPath;
		/// <summary>
		/// 一つ目のstringが、監視開始時のPath.
		/// 二つ目のstringが、監視後に変更されたPath.
		/// onMoveの戻り値がtrueなら、監視を終了する.
		/// </summary>
		public readonly System.Func<string, string, bool> onMoved;
		/// <summary>
		/// GUID,Path,Callbackが正しく設定されているか
		/// </summary>
		public readonly bool isValid;

		public FileWatchEntry (string guid, System.Func<string, string, bool> onMove) :
			this(guid, AssetDatabase.GUIDToAssetPath(guid), onMove)
		{
		}

		public FileWatchEntry (string guid, string assetPath, System.Func<string, string, bool> onMove)
		{
			this.guid = guid;
			this.assetPath = assetPath;
			this.onMoved = onMove;
			this.isValid = (onMove != null)
				&& (string.IsNullOrEmpty (guid) == false)
				&& (string.IsNullOrEmpty (assetPath) == false);
		}
	}
	private bool _isInitialized = false;
	private bool _isDisposed = false;
	private List<FileWatchEntry> _fileWatchEntries;

	/// <summary>
	/// アセットのmoveを監視する. onMoveの戻り値がtrueなら、監視を終了する.
	/// </summary>
	public void Watch (Object asset, System.Func<string, string, bool> onMove)
	{
		if ((asset == null) || (onMove == null)) {
			return;
		}
		Watch (AssetDatabase.GetAssetPath (asset), onMove);
	}

	/// <summary>
	/// アセットのmoveを監視する. onMoveの戻り値がtrueなら、監視を終了する.
	/// </summary>
	public void Watch (string assetPath, System.Func<string, string, bool> onMove)
	{
		if (string.IsNullOrEmpty (assetPath) || (onMove == null)) {
			return;
		}
		Watch (new FileWatchEntry (AssetDatabase.AssetPathToGUID (assetPath), onMove));
	}

	/// <summary>
	/// アセットのmoveを監視する. 
	/// </summary>
	public void Watch (FileWatchEntry fileWatchEntry)
	{
		if (fileWatchEntry.isValid == false) {
			return;
		}
		WeakInitialize ();
		_fileWatchEntries = _fileWatchEntries ?? new List<FileWatchEntry> ();
		_fileWatchEntries.Add (fileWatchEntry);
	}

	public void Dispose ()
	{
		_isDisposed = true;
		_fileWatchEntries = null;
		if (_isInitialized) {
			EditorApplication.projectWindowChanged -= OnProjectWindowChanged;
		}
	}

	private void WeakInitialize ()
	{
		if (_isInitialized) {
			return;
		}
		_isInitialized = true;
		_fileWatchEntries = _fileWatchEntries ?? new List<FileWatchEntry> ();
		EditorApplication.projectWindowChanged += OnProjectWindowChanged;
	}

	private void OnProjectWindowChanged ()
	{
		if ((_fileWatchEntries == null) || (_fileWatchEntries.Count == 0)) {
			return;
		}
		var aliveWatchEntryList = new List<FileWatchEntry> (_fileWatchEntries.Count);
		foreach (var fileWatchEntry in _fileWatchEntries) {
			if (fileWatchEntry.isValid == false) {
				continue;
			}
			string currentAssetPath = AssetDatabase.GUIDToAssetPath (fileWatchEntry.guid);
			if (fileWatchEntry.assetPath == currentAssetPath) {
				// Pathが変わっていなかった場合は、aliveListに入れる.
				aliveWatchEntryList.Add (fileWatchEntry);
				continue;
			}
			bool isEndWatch = true;
			try {
				isEndWatch = fileWatchEntry.onMoved (fileWatchEntry.assetPath, currentAssetPath);
			} catch (System.Exception ex) {
				Debug.LogError (ex);
			}
			if (isEndWatch) {
				// 監視終了の場合.
				continue;
			}
			// Pathが変わっていて、監視継続の場合.
			aliveWatchEntryList.Add (new FileWatchEntry (fileWatchEntry.guid, currentAssetPath, fileWatchEntry.onMoved));
		}
		if (_isDisposed == false) {
			// コールバック中にDisposeされた場合にここに来る
			return;
		}
		_fileWatchEntries = aliveWatchEntryList;
	}
}
