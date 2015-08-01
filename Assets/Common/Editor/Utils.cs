using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Obsolete]
public class Utils
{

	[MenuItem("Assets/RenameTest")]
	[MenuItem("Assets/RenameTest1")]
	[MenuItem("Hierarchy/RenameTest1")]
	[MenuItem("GameObject/MyMenu/Do Something", false, 0)]
	public static void RenameCommand ()
	{
		EditorCommon.EngageRenameMode (Selection.activeObject);
	}
}
