using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class NewBehaviourScript : MonoBehaviour
{
	public float alpha;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		var renderer = GetComponent<CanvasRenderer> ();
		renderer.SetAlpha (alpha);
		Debug.Log(transform.hasChanged);
		transform.hasChanged = false;
	}
}
