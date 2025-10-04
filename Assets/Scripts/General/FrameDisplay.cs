using UnityEngine;
using System.Collections;

public class FrameDisplay : MonoBehaviour {
	public tk2dTextMesh total;
	public tk2dTextMesh first;
	public tk2dTextMesh second;
	
	// Use this for initialization
	void Start () {
		SetScore("","","");
	}
	
	public void SetScore(string t, string f, string s)
	{
		total.text = t;
		total.Commit();
		first.text = f;
		first.Commit();
		second.text = s;
		second.Commit();
	}
}
