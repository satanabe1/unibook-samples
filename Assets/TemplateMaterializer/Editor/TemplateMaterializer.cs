using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CodeBuilder;

/// <summary>
/// テンプレートから、コードを生成する
/// </summary>
public class TemplateMaterializer
{
	public const string CommandPrefix = "Assets/TemplateMaterializer";
	/// <summary>
	/// TemplateMaterializer/Templates
	/// </summary>
	[MenuItem(CommandPrefix + "/RecreateMenuItemCommands %t")]
	public static void RecreateMenuItemCommands ()
	{
		TypeDeclaration typeDeclaration = new TypeDeclaration () { Name = "TemplateMaterializerCommand" };
		// using
		typeDeclaration.UsingDeclarationList.Add (new UsingDeclaration () {
			UsingTarget = typeof(UnityEditor.MenuItem).Namespace,
		});
		typeDeclaration.UsingDeclarationList.Add (new UsingDeclaration () {
			UsingTarget = typeof(UnityEngine.Debug).Namespace,
		});
		// modifier
		typeDeclaration.ModifierList.Add (Modifier.Pubic);

		// method
		MethodDeclaration method = new MethodDeclaration () { Name = "Open" };
		method.MethodBody = "Debug.Log(\"hgoe\");";
		method.ModifierList.Add (Modifier.Pubic);
		method.ModifierList.Add (Modifier.Static);
		// method attribute
		AttributeText attribute = new AttributeText () { Name = typeof(MenuItem).Name };
		attribute.ParameterList.Add (CommandPrefix + "/AnyCommand");
		attribute.ParameterList.Add (false);
		attribute.ParameterList.Add (100);
		method.AttributeList.Add (attribute);

		typeDeclaration.MethodDeclarationList.Add (method);
		Debug.Log (typeDeclaration.BuildCode ());

		File.WriteAllText (
			EditorCommon.CombinePath (Application.dataPath, "TemplateMaterializer", "Editor", typeDeclaration.Name + ".design.cs"),
			typeDeclaration.BuildCode ());
		AssetDatabase.Refresh ();
	}

	public void GenerateCode (string templatePath, string distPath)
	{
	}
}
