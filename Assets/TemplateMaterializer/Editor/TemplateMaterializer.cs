using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CodeBuilder;

/// <summary>
/// テンプレートから、コードを生成する
/// </summary>
public class TemplateMaterializer
{
	public const string MenuCommandPrefix = "Assets/TemplateMaterializer";
	public const string MaterializeCommandPrefix = "Assets/Create";
	public const string TemplatesDirName = "Templates";
	public const string TemplateExtention = ".cs.template";
	private static string _scriptDirPathCache;

	public static string ScriptDirPath {
		get {
			if (_scriptDirPathCache != null) { // not string.Empty
				return _scriptDirPathCache;
			}
			string scriptPath = EditorCommon.GetScriptPath<TemplateMaterializer> ();
			_scriptDirPathCache = (string.IsNullOrEmpty (scriptPath)) ? string.Empty : Path.GetDirectoryName (scriptPath);
			return _scriptDirPathCache;
		}
	}

	public static string TemplateMaterializerRootDirPath {
		get { return Directory.GetParent (ScriptDirPath).ToString (); }
	}

	public static string TemplatesDirPath {
		get { return EditorCommon.CombinePath (TemplateMaterializerRootDirPath, TemplatesDirName); }
	}

	/// <summary>
	/// TemplateMaterializer/Templates
	/// </summary>
	[MenuItem(MenuCommandPrefix + "/RecreateMenuItemCommands %t")]
	public static void RecreateMenuItemCommands ()
	{
		DumpStatics ();
		string[] templateFilePaths = FindTemplateFilePaths (TemplatesDirPath);
		Debug.LogWarning (EditorCommon.CollectionToString (templateFilePaths));

		TypeDeclaration commandType = CreateEmptyCommandType ();
		foreach (var templateFilePath in templateFilePaths) {
			commandType.MethodDeclarationList.Add (CreateMaterializeCommandMethod (templateFilePath));
		}
		Debug.LogError (commandType.BuildCode ());
		string code = CodeIndenter.Pretty (commandType.BuildCode ());

		File.WriteAllText (EditorCommon.CombinePath (ScriptDirPath, commandType.Name + ".design.cs"), code);
		AssetDatabase.Refresh ();
	}

	public static void DumpStatics ()
	{
		Debug.Log ("ScriptPath:" + ScriptDirPath);
		Debug.Log ("TemplateMaterializerRootDirPath: " + TemplateMaterializerRootDirPath);
		Debug.Log ("TemplatesDirPath: " + TemplatesDirPath);
	}

	private static string[] FindTemplateFilePaths (string dirPath)
	{
		if (Directory.Exists (dirPath) == false) {
			return new string[0];
		}
		return Directory.GetFiles (dirPath, "*" + TemplateExtention, SearchOption.AllDirectories);
	}

	private static TypeDeclaration CreateEmptyCommandType ()
	{
		TypeDeclaration typeDeclaration = new TypeDeclaration () { Name = "TemplateMaterializerCommand" };
		// using UnityEditor;
		typeDeclaration.UsingDeclarationList.Add (new UsingDeclaration () { UsingTarget = typeof(UnityEditor.MenuItem).Namespace });
		// using UnityEngine;
		typeDeclaration.UsingDeclarationList.Add (new UsingDeclaration () { UsingTarget = typeof(UnityEngine.Debug).Namespace });
		// modifier
		typeDeclaration.ModifierList.Add (Modifier.Pubic);
		return typeDeclaration;
	}

	private static MethodDeclaration CreateMaterializeCommandMethod (string templateFilePath)
	{
		string templateName = Path.GetFileNameWithoutExtension (templateFilePath).Replace ('.', '_').Replace (' ', '_');
		StringBuilder methodBody = new StringBuilder ();
		methodBody.Append (InvocationHelper.CreateMethodInvocation<object> (Debug.Log, "\"" + templateFilePath + "\"")).AppendLine (";");
		methodBody.Append (InvocationHelper.CreateMethodInvocation<string> (GenerateCode, templateFilePath)).AppendLine (";");
		// public static void CreateHoge() {}
		MethodDeclaration method = new MethodDeclaration () { Name = "Create" +  templateName};
		method.MethodBody = methodBody.ToString ();
		method.ModifierList.Add (Modifier.Pubic);
		method.ModifierList.Add (Modifier.Static);
		// method attribute [MenuItem("hoge", false, 90)]
		AttributeText attribute = new AttributeText () { Name = typeof(MenuItem).Name};
		attribute.ParameterList.Add (MaterializeCommandPrefix + "/Create " + templateName);
		attribute.ParameterList.Add (false);
		attribute.ParameterList.Add (90);
		method.AttributeList.Add (attribute);
		return method;
	}

	public static void GenerateCode (string templatePath)
	{
		Debug.Log ("hoge: " + templatePath);
	}

	public static void GenerateCode (string templatePath, string distPath)
	{
		Debug.Log ("hoge: " + templatePath);
	}
}
