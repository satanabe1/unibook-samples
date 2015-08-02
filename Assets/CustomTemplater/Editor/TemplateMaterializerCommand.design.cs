using UnityEditor;
using UnityEngine;
public class TemplateMaterializerCommand
{
	[MenuItem("Assets/Create From Template/Create DefaultCSharpCode_cs", false, 10)]
	public static void CreateDefaultCSharpCode_cs(){
		TemplateMaterializer.GenerateCode("Assets/CustomTemplater/Templates/DefaultCSharpCode.cs.tmtemplate");

	}
	[MenuItem("Assets/Create From Template/Create JsDefaultCode_js", false, 10)]
	public static void CreateJsDefaultCode_js(){
		TemplateMaterializer.GenerateCode("Assets/CustomTemplater/Templates/JsDefaultCode.js.tmtemplate");

	}
	[MenuItem("Assets/Create From Template/Create NamespaceCSharpCode_cs", false, 10)]
	public static void CreateNamespaceCSharpCode_cs(){
		TemplateMaterializer.GenerateCode("Assets/CustomTemplater/Templates/NamespaceCSharpCode.cs.tmtemplate");

	}
	[MenuItem("Assets/Create From Template/Create uGUICSharpCode_cs", false, 10)]
	public static void CreateuGUICSharpCode_cs(){
		TemplateMaterializer.GenerateCode("Assets/CustomTemplater/Templates/uGUICSharpCode.cs.tmtemplate");

	}
}

