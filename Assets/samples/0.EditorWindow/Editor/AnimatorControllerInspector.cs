using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5
[CustomEditor(typeof(UnityEditor.Animations.AnimatorController))]
#else // for Unity4
[CustomEditor(typeof(UnityEditorInternal.AnimatorController))]
#endif
public class AnimatorControllerInspector : Editor
{
	private List<AnimationClip> _animationClipList;

	private void OnEnable ()
	{
		_animationClipList = FindAnimationClips (target);
		_animationClipList.Sort ((clip1, clip2) => clip1.name.CompareTo (clip2.name));
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		if (_animationClipList != null) {
			EditorGUILayout.LabelField ("AnimationClips:");
			EditorGUI.indentLevel++;
			for (var i = 0; i < _animationClipList.Count; ++i) {
				EditorGUILayout.ObjectField (_animationClipList [i], typeof(AnimationClip), false);
			}
			EditorGUI.indentLevel--;
		}
	}

	private List<AnimationClip> FindAnimationClips (Object asset)
	{
#if UNITY_5
		RuntimeAnimatorController r = null;
		var animationClipList = new List<AnimationClip> ();
		var animatorController = asset as RuntimeAnimatorController;
		if (animatorController != null)
		{
			animationClipList.AddRange(animatorController.animationClips);
		}
		return animationClipList;
#else // for Unity4
		var animationClipList = new List<AnimationClip> ();
		var animatorController = asset as UnityEditorInternal.AnimatorController;
		if (animatorController == null) {
			return animationClipList;
		}
		for (var i = 0; i < animatorController.layerCount; ++i) {
			UnityEditorInternal.AnimatorControllerLayer layer = animatorController.GetLayer (i);
			UnityEditorInternal.StateMachine stateMachine = layer.stateMachine;
			for (var k = 0; k < stateMachine.stateCount; ++k) {
				var animationClip = stateMachine.GetState (k).GetMotion () as AnimationClip;
				if (animationClip != null) {
					animationClipList.Add(animationClip);
				}
			}
		}
		return animationClipList;
#endif
	}
}
