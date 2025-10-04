using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(SplinePath))]
public class SplinePathEditor : Editor {
	
	public SplinePath _target;
	private Vector3 lastPos;
	
	void OnEnable()
	{
		_target = (SplinePath)target;
		lastPos = _target.gameObject.transform.position;
	}
	
	override public void OnInspectorGUI()
	{
//		EditorGUIUtility.LookLikeInspector();
		
		for(int i=0; i<_target.nodes.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			_target.nodes[i] = EditorGUILayout.Vector3Field("Node " + (i+1), _target.nodes[i]);
			if(GUILayout.Button("+", GUILayout.Width(20f)))
			{
				_target.nodes.Insert(i+1, InsertPosition(_target.nodes[i]));
				EditorUtility.SetDirty(_target);
			}
			if(GUILayout.Button("-", GUILayout.Width(20f)))
			{
				_target.nodes.RemoveAt(i);
				EditorUtility.SetDirty(_target);
			}
			EditorGUILayout.EndHorizontal();
		}
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}
	}
	
	void OnSceneGUI()
	{
//		Debug.Log("OnSceneGUI()");
		if(!_target.enabled)
			return;
		
		Undo.SetSnapshotTarget(_target, "Adjust Spline Path");
		
		var oldHandlesColor = Handles.color;
		
		Handles.color = Color.red;
		
		// Draw nodes
		for(int i=0; i<_target.nodes.Count; i++)
		{
			_target.nodes[i] = Handles.PositionHandle(_target.nodes[i], Quaternion.identity);
			Handles.Label(_target.nodes[i] + Vector3.up*2, (i+1).ToString());
		}
		
		if(_target.gameObject.transform.position != lastPos)
		{
			UpdateAllNodes(_target.gameObject.transform.position - lastPos);
			lastPos = _target.gameObject.transform.position;
		}
		
		Handles.color = oldHandlesColor;
		EditorUtility.SetDirty(_target);
		
	}
				
	private Vector3 InsertPosition(Vector3 originalPos)
	{
		Vector3 newPos = originalPos;
		
		newPos.x += 2;
		
		return newPos;
	}

	private void UpdateAllNodes(Vector3 move)
	{
		for(int i=0; i<_target.nodes.Count; i++)
		{
			_target.nodes[i] += move;
		}
	}
}
