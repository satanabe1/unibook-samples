using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CodeBuilder;
using CustomTemplate;

/// <summary>
/// テンプレートから、コードを生成する
/// </summary>
public class TemplateMaterializer
{
	public const string MenuCommandPrefix = "Assets/TemplateMaterializer";
	public const string MaterializeCommandPrefix = "Assets/Create From Template";
	public const int MaterializeCommandPriority = 10;
	public const string TemplatesDirName = "Templates";
	public const string TemplateExtension = ".tmtemplate";
	public static string _projectRootPathCache;

	public static string ProjectRootPath {
		get {
			if (_projectRootPathCache != null) {
				return _projectRootPathCache;
			}
			_projectRootPathCache = Directory.GetParent (Application.dataPath).ToString ();
			return _projectRootPathCache;
		}
	}

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
		string[] templateFilePaths = FindTemplateFilePaths (TemplatesDirPath);
		Debug.Log (EditorCommon.CollectionToString (templateFilePaths));

		TypeDeclaration commandType = CreateEmptyCommandType ();
		foreach (var templateFilePath in templateFilePaths) {
			commandType.MethodDeclarationList.Add (CreateMaterializeCommandMethod (templateFilePath));
		}
		Debug.Log (commandType.BuildCode ());
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
		return Directory.GetFiles (dirPath, "*" + TemplateExtension, SearchOption.AllDirectories);
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
		attribute.ParameterList.Add (MaterializeCommandPriority);
		method.AttributeList.Add (attribute);
		return method;
	}

	public static void GenerateCode (string templatePath)
	{
		Debug.Log ("templatePath: " + templatePath);
		string distDirPath = Application.dataPath;
		if (Selection.activeObject != null) {
			distDirPath = Path.Combine (ProjectRootPath, AssetDatabase.GetAssetPath (Selection.activeObject));
		}
		if (File.Exists (distDirPath)) {
			distDirPath = Directory.GetParent (distDirPath).ToString ();
		}
		if (Directory.Exists (distDirPath) == false) {
			Directory.CreateDirectory (distDirPath);
		}
		// "Hoge.cs.template" -> "Hoge.cs"
		string templateNameWithoutMetaExtension = Path.GetFileNameWithoutExtension (templatePath);
		// "Hoge.cs" -> ".cs"
		string templateExtension = Path.GetExtension (templateNameWithoutMetaExtension);
		// "Assets/NewClass.cs"
		string distPath = Path.Combine (distDirPath, "NewClass" + templateExtension);
		GenerateCode (templatePath, distPath);
	}

	public static void GenerateCode (string templatePath, string distPath)
	{
		string fileExtension = Path.GetExtension (distPath);
		string distPathWithoutExtension = distPath.Substring (0, distPath.Length - fileExtension.Length);
		string uniqueDistPath = distPathWithoutExtension;
		for (var i = 1; File.Exists(uniqueDistPath) || File.Exists(uniqueDistPath + fileExtension); ++i) {
			uniqueDistPath = distPathWithoutExtension + i;
		}
		File.Copy (templatePath, uniqueDistPath);
		AssetDatabase.Refresh ();
		Object newClassFile = AssetDatabase.LoadAssetAtPath<Object> (uniqueDistPath.Substring (ProjectRootPath.Length + 1));
		Selection.activeObject = newClassFile;
		EditorCommon.DelayCall (2f / 60f, () => {
			Utils.RenameCommand (newClassFile);
			FileMoveWatcher renameWatcher = new FileMoveWatcher ();
			renameWatcher.Watch (Selection.activeObject, (from, to) => {
				Debug.Log ("Move " + from + " To " + to);
				renameWatcher.Dispose ();
				renameWatcher = null;
				if (DirtyTemplate (to)) {
					AssetDatabase.CopyAsset (to, to + fileExtension);
					Utils.SelectAssetPath (to + fileExtension);
					EditorCommon.DelayCall (2f / 60f, () => {
						AssetDatabase.DeleteAsset (to);
						AssetDatabase.Refresh ();
						EditorUtility.UnloadUnusedAssetsImmediate (true);
					});
				}
				return true;
			});
		});
	}

	private static bool DirtyTemplate (string templatePath)
	{
		if (string.IsNullOrEmpty (templatePath)) {
			return false;
		}
		if (File.Exists (templatePath) == false) {
			return false;
		}
		try {
			string className = Path.GetFileNameWithoutExtension (templatePath);
			// read
			string templateFullPath = Path.Combine (ProjectRootPath, templatePath);
			string templateText = File.ReadAllText (templateFullPath);
			// parse
			ITemplateParser parser = new RoughlyTemplateParser ();
			// customize
			TemplateCustomizer customizer = new TemplateCustomizer (parser.ParseTemplate (templateText));
			customizer.RenameClassName (className);
			string code = customizer.BuildCode ();
			// rewrite
			File.WriteAllText (templatePath, code);
			return true;
		} catch (System.Exception ex) {
			Debug.LogError (ex);
		}
		return false;
	}
}
