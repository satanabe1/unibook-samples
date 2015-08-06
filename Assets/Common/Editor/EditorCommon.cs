using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using EditorCommonInternal;

public static class EditorCommon
{
	public static string CurrentProjectRootPath {
		get {
			return System.IO.Directory.GetParent (Application.dataPath).ToString ();
		}
	}

	public static string GetExtension (Object asset)
	{
		return (asset != null) ? System.IO.Path.GetExtension (AssetDatabase.GetAssetPath (asset)) : string.Empty;
	}

	public static void DrawButton (System.Action onClick)
	{
		if (onClick != null) {
			if (GUILayout.Button (onClick.Method.Name)) {
				onClick.Invoke ();
			}
		}
	}
        
	public static void DrawButton<Type> (Type label, System.Action onClick)
	{
		string text = (label == null) ? null : label.ToString ();
		if (GUILayout.Button (text)) {
			if (onClick != null) {
				onClick.Invoke ();
			}
		}
	}
    
	public static void DrawButton<Type> (Type label, System.Action<Type> onClick)
	{
		string text = (label == null) ? null : label.ToString ();
		if (GUILayout.Button (text)) {
			if (onClick != null) {
				onClick.Invoke (label);
			}
		}
	}
    
	public static void DrawHorizontal (System.Action drawContents)
	{
		EditorGUILayout.BeginHorizontal ();
		if (drawContents != null) {
			drawContents.Invoke ();
		}
		EditorGUILayout.EndHorizontal ();
	}
    
	public static void DrawVertical (System.Action drawContents)
	{
		EditorGUILayout.BeginVertical ();
		if (drawContents != null) {
			drawContents.Invoke ();
		}
		EditorGUILayout.EndVertical ();
	}
    
	public static void DrawIndent (System.Action drawContents)
	{
		++EditorGUI.indentLevel;
		if (drawContents != null) {
			drawContents.Invoke ();
		}
		--EditorGUI.indentLevel;
	}
    
	public static void DrawScroll (System.Action drawContents, ref Vector2 scrollPosition)
	{
		scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
		if (drawContents != null) {
			drawContents.Invoke ();
		}
		EditorGUILayout.EndScrollView ();
	}

	public static void DrawFoldout (System.Action drawContents, ref bool isFolding, string foldoutText)
	{
		isFolding = EditorGUILayout.Foldout (isFolding, foldoutText);
		if (isFolding) {
			drawContents.Invoke ();
		}
	}

	public static string CombinePath (params string[] paths)
	{
		if ((paths == null) || (paths.Length == 0)) {
			return string.Empty;
		}
		if (paths.Length == 1) {
			return paths [0];
		}
		string path = paths [0];
		for (var i = 1; i < paths.Length; ++i) {
			path = System.IO.Path.Combine (path, paths [i]);
		}
		return path;
	}

	public static string GetScriptPath<T> ()
	{
		return GetScriptPath (typeof(T));
	}

	public static string GetScriptPath (System.Type type)
	{
		string[] guids = AssetDatabase.FindAssets ("t:MonoScript");
		foreach (var guid in guids) {
			string scriptPath = AssetDatabase.GUIDToAssetPath (guid);
			MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript> (scriptPath);
			if (script != null) {
				if (script.GetClass () == type) {
					return scriptPath;
				}
			}
		}
		return string.Empty;
	}

	public static string CollectionToString<T> (ICollection<T> collection)
	{
		if ((collection == null) || (collection.Count == 0)) {
			return string.Empty;
		}
		var sb = new System.Text.StringBuilder ();
		foreach (var element in collection) {
			sb.Append (element).Append (", ");
		}
		return sb.ToString ();
	}

	public static void DelayCall (float delay, System.Action function)
	{
		if (function == null) {
			return;
		}
		double startTime = EditorApplication.timeSinceStartup;
		EditorApplication.CallbackFunction delayCaller = () => {
			double duration = EditorApplication.timeSinceStartup - startTime;
//			Debug.Log ("DelayCall Duration : " + duration);
			if (duration > delay) {
				function.Invoke ();
			}
		};
		function += () => {
			EditorApplication.update -= delayCaller;
		};
		EditorApplication.update += delayCaller;
	}

	public static void EditAssetName(string defaultName, Texture2D icon, System.Action<string> onEditName)
	{
		if (onEditName == null)
		{
			return;
		}
		OnAssetRenamedCallback renamedCallback = OnAssetRenamedCallback.CreateEventInstance(onEditName);
		UnityEditor.ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, renamedCallback, defaultName, icon, string.Empty);
	}

	public static void RenameCommand (Object obj)
	{
		EngageRenameMode (obj);
	}
	
	public static void EngageRenameMode (Object go)
	{
		Selection.activeObject = go;
		//		GetFocusedWindow ("Hierarchy").SendEvent (Events.Rename);
		GetFocusedWindow ("Project").SendEvent (Events.Rename);
		//		UnityEditor.EditorWindow.focusedWindow.SendEvent(Events.Rename);
	}
	
	public static EditorWindow GetFocusedWindow (string window)
	{
		FocusOnWindow (window);
		return EditorWindow.focusedWindow;
	}
	
	public static void FocusOnWindow (string window)
	{
		EditorApplication.ExecuteMenuItem ("Window/" + window);
	}

	public static void SelectAssetPath (string assetPath)
	{
		Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object> (assetPath);
	}

	public static string GetParentDirectoryPath (string path)
	{
		return System.IO.Directory.GetParent (path).ToString ();
	}

	public static class Events
	{
#if UNITY_EDITOR_WIN
		public static Event Rename = new Event () { keyCode = KeyCode.F2, type = EventType.KeyDown };
#else
		public static Event Rename = new Event () { keyCode = KeyCode.Return, type = EventType.KeyDown };
#endif
	}

	public static void OpenFolder (string path)
	{
#if UNITY_EDITOR_OSX
		System.Diagnostics.Process.Start("open", path);
#elif UNITY_EDITOR_WIN
		System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
#else
		EditorUtility.RevealInFinder (path);
#endif
	}
}

namespace EditorCommonInternal
{
	public class OnAssetRenamedCallback : UnityEditor.ProjectWindowCallback.EndNameEditAction
	{
		private System.Action<string> _editNameCallback;
	
		public static OnAssetRenamedCallback CreateEventInstance (System.Action<string> editNameCallback)
		{
			var renamedEvent = ScriptableObject.CreateInstance<OnAssetRenamedCallback> ();
			renamedEvent._editNameCallback = editNameCallback;
			return renamedEvent;
		}

		public override void Action (int instanceId, string pathName, string resourceFile)
		{
			if (_editNameCallback != null) {
				_editNameCallback.Invoke (pathName);
			}
		}
	}
}

