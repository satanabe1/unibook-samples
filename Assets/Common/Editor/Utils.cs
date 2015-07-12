using UnityEngine;
using UnityEditor;
using System.Collections;

public class Utils
{
	[InitializeOnLoad]
	public class CodeMaterializer
	{
		[MenuItem("Assets/CreateCode")]
		public static void MaterializeDefaultTemplate ()
		{
//			Debug.LogError (Selection.activeObject);
			UnityEditorInternal.InternalEditorUtility.ExecuteCommandOnKeyWindow ("Quit");
			UnityEditor.EditorWindow.focusedWindow.SendEvent (new Event () {
				keyCode = KeyCode.P,
				type = EventType.KeyDown,
//				modifiers = EventModifiers.Command,
				command = true,
			});
//			UnityEditor.EditorApplication.Exit(0);
			var eventKey = new Event ();
			eventKey.command = true;
			eventKey.control = true;
			Debug.Log("hoge");
		}

		public static void MaterializeCustomTemplate (string templatePath, string outPath)
		{

		}
	}

	[MenuItem("Assets/RenameTest")]
	[MenuItem("Assets/RenameTest1")]
	[MenuItem("Hierarchy/RenameTest1")]
	[MenuItem("GameObject/MyMenu/Do Something", false, 0)]
	public static void RenameCommand ()
	{
		EngageRenameMode (Selection.activeObject);
	}

	public static void EngageRenameMode (Object go)
	{
		SelectObject (go);
//		GetFocusedWindow ("Hierarchy").SendEvent (Events.Rename);
		GetFocusedWindow ("Project").SendEvent (Events.Rename);
//		UnityEditor.EditorWindow.focusedWindow.SendEvent(Events.Rename);
	}
	
	public static void SelectObject (Object obj)
	{
		Selection.objects = new Object[] { obj };
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
