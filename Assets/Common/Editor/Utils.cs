using UnityEngine;
using UnityEditor;
using System.Collections;

public class Utils
{
	public static void SelectAssetPath(string path)
	{
		Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
	}

	[MenuItem("Assets/RenameTest")]
	[MenuItem("Assets/RenameTest1")]
	[MenuItem("Hierarchy/RenameTest1")]
	[MenuItem("GameObject/MyMenu/Do Something", false, 0)]
	public static void RenameCommand ()
	{
		EngageRenameMode (Selection.activeObject);
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
	
	public static class Events
	{
#if UNITY_EDITOR_WIN
		public static Event Rename = new Event () { keyCode = KeyCode.F2, type = EventType.KeyDown };
#else
		public static Event Rename = new Event () { keyCode = KeyCode.Return, type = EventType.KeyDown };
#endif
	}
}
