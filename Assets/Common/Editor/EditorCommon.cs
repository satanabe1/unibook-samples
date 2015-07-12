﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class EditorCommon
{
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
				onClick ();
			}
		}
	}
    
	public static void DrawButton<Type> (Type label, System.Action<Type> onClick)
	{
		string text = (label == null) ? null : label.ToString ();
		if (GUILayout.Button (text)) {
			if (onClick != null) {
				onClick (label);
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

	public static string ListToString<T> (List<T> list)
	{
		if ((list == null) || (list.Count == 0)) {
			return string.Empty;
		}
		var sb = new System.Text.StringBuilder ();
		list.ForEach ((node) => sb.Append (node).Append (", "));
		return sb.ToString ();
	}
        
	public static void OpenFolder (string path)
	{
		#if UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", path);
		#elif UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", "/select," + module.FolderPath);
		#else
		EditorUtility.RevealInFinder (module.FolderPath);
		#endif
	}
}