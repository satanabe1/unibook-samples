using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SandboxEditor
{
	public class ABFinder : EditorWindow
	{
		[MenuItem("Window/Finder/ABFinder")]
		public static void Open ()
		{
			GetWindow<ABFinder> ();
		}
		
		private Object _selected;
		private int _lastRenderedFrameCount;
		private AssetBundle _lastSelectedAssetBundle;
		private Object[] _assets;
		private Vector2 _scroll;
		
		void OnEnable ()
		{
			titleContent = new GUIContent (GetType ().Name);
		}
		
		void OnDisable ()
		{
			_assets = null;
			if (_lastSelectedAssetBundle != null) {
				_lastSelectedAssetBundle.Unload (true);
			}
		}
		
		void Update ()
		{
			if (_lastRenderedFrameCount == Time.renderedFrameCount) {
				return;
			}
			Repaint ();
		}
		
		void OnGUI ()
		{
			_lastRenderedFrameCount = Time.renderedFrameCount;
			if (_selected != Selection.activeObject) {
				_selected = Selection.activeObject;
				string selectedPath = AssetDatabase.GetAssetPath (Selection.activeObject);
				if (selectedPath.EndsWith (".unity3d")) {
					_assets = null;
					LoadAssets (selectedPath);
				}
			}
			
			DrawOpenFileButton ();
			EditorGUILayout.Space ();
			DrawSortButton ();
			
			if (_assets == null) {
				EditorGUILayout.LabelField ("null");
			} else {
				EditorGUILayout.LabelField ("Length:" + _assets.Length.ToString ());
				_scroll = EditorGUILayout.BeginScrollView (_scroll);
				foreach (var asset in _assets) {
					//  GUI.SetNextControlName (asset.ToString ());
					EditorGUILayout.BeginHorizontal ();
					int memorySizeKB = Profiler.GetRuntimeMemorySize (asset) / 1024;
					memorySizeKB = (memorySizeKB < 1) ? 1 : memorySizeKB;
					EditorGUILayout.PrefixLabel (memorySizeKB + " KB");
					EditorGUILayout.ObjectField (asset, asset.GetType (), false);
					if (asset is GameObject) {
						if (GUILayout.Button ("instantiate")) {
							Instantiate (asset as GameObject);
						}
					} else if (asset is TextAsset) {
						if (GUILayout.Button ("Console")) {
							TextAsset text = asset as TextAsset;
							Debug.Log (text.text);
						}
					} else if (asset is Texture) {
						if (GUILayout.Button ("Preview")) {
							TextureWindow.Open (asset as Texture);
						}
					}
					EditorGUILayout.EndHorizontal ();
				}
				EditorGUILayout.EndScrollView ();
				EditorGUILayout.Space ();
			}
			//      Debug.Log (GUI.GetNameOfFocusedControl ());
		}
		
		void DrawSortButton ()
		{
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Size")) {
				OrderBySize ();
			}
			if (GUILayout.Button ("Type")) {
				OrderByType ();
			}
			if (GUILayout.Button ("Name")) {
				OrderByName ();
			}
			EditorGUILayout.EndHorizontal ();
		}
		
		void DrawOpenFileButton ()
		{
			if (GUILayout.Button ("Open AB")) {
				string path = EditorUtility.OpenFilePanel ("Open AB", Application.dataPath, null);
				if (string.IsNullOrEmpty (path) == false) {
					LoadAssets (path);
					_selected = null;
					Selection.activeObject = null;
				}
			}
		}
		
		void OrderBySize ()
		{
			if (_assets != null) {
				var tmpList = new System.Collections.Generic.List<Object> (_assets);
				tmpList.Sort ((asset0, asset1) => Profiler.GetRuntimeMemorySize (asset1) - Profiler.GetRuntimeMemorySize (asset0));
				_assets = tmpList.ToArray ();
			}
		}
		
		void OrderByType ()
		{
			if (_assets != null) {
				var tmpList = new System.Collections.Generic.List<Object> (_assets);
				tmpList.Sort ((asset0, asset1) => {
					int result = asset0.GetType ().FullName.CompareTo (asset1.GetType ().FullName);
					if (result == 0) {
						result = asset0.ToString ().CompareTo (asset1.ToString ());
					}
					return result;
				});
				_assets = tmpList.ToArray ();
			}
		}
		
		void OrderByName ()
		{
			if (_assets != null) {
				var tmpList = new System.Collections.Generic.List<Object> (_assets);
				tmpList.Sort ((asset0, asset1) => asset0.ToString ().CompareTo (asset1.ToString ()));
				_assets = tmpList.ToArray ();
			}
		}
		
		void LoadAssets (string path)
		{
			if ((System.IO.File.Exists (path) == false) || (path.EndsWith (".unity3d") == false)) {
				return;
			}
			string assetBundleName = System.IO.Path.GetFileNameWithoutExtension (path);
			var loadedAssetBundles = Resources.FindObjectsOfTypeAll<AssetBundle> ();
			foreach (var loadedAssetBundle in loadedAssetBundles) {
				if (loadedAssetBundle.name == assetBundleName) {
					loadedAssetBundle.Unload (true);
				}
			}
			if (string.IsNullOrEmpty (path) == false) {
				if (System.IO.Directory.Exists (path)) {
					return;
				}
				byte[] bytes = System.IO.File.ReadAllBytes (path);
				if (_lastSelectedAssetBundle != null) {
					_lastSelectedAssetBundle.Unload (true);
					_lastSelectedAssetBundle = null;
				}
				_lastSelectedAssetBundle = AssetBundle.CreateFromMemoryImmediate (bytes);
				if (_lastSelectedAssetBundle != null) {
					_lastSelectedAssetBundle.name = assetBundleName;
					_assets = _lastSelectedAssetBundle.LoadAllAssets ();
				}
				OrderBySize ();
			}
		}
	}
	
	public class TextureWindow : EditorWindow
	{
		public static void Open (Texture texture)
		{
			if (texture == null) {
				return;
			}
			TextureWindow window = GetWindow<TextureWindow> ();
			window.SetTexture (texture);
		}
		
		private Texture _texture;
		private Rect _textureRect;
		
		public void SetTexture (Texture texture)
		{
			_texture = texture;
			_textureRect = new Rect ();
			_textureRect.width = _texture.width;
			_textureRect.height = _texture.height;
			_textureRect.x = 80f;
			_textureRect.y = 80f;
		}
		
		void OnEnable ()
		{
			titleContent = new GUIContent (GetType ().Name);
		}
		
		void OnGUI ()
		{
			if (_texture != null) {
				EditorGUILayout.LabelField (_texture.name);
				EditorGUILayout.LabelField ("width: " + _textureRect.width);
				EditorGUILayout.LabelField ("height: " + _textureRect.height);
				EditorGUI.DrawPreviewTexture (_textureRect, _texture);
			}
		}
	}
}
