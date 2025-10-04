using UnityEngine;
using System.Collections;

public class Metrics : BaseObject {

	override protected void Awake()
	{
		base.Awake();
		
		StartCoroutine(PostData("Game Started", 0, 0f));
	}
	
	void OnEnable()
	{
		EventManager.FrameCompleted += FrameCompleted;
	}
	
	void OnDisable()
	{
		EventManager.FrameCompleted -= FrameCompleted;
	}
	
	protected void FrameCompleted(int frame)
	{
		if(frame == 10)
			StartCoroutine(PostData("Game Completed", gameLoop.score.GetFrameScore(frame), 0f));
	}
	
	protected IEnumerator PostData(string method, int intValue, float floatValue)
	{
		string url = "http://bigfoot.robotpandaproductions.com/manticore_rpc.php";
		
		WWWForm form = new WWWForm();
		form.AddField("method", method);
		form.AddField("intValue", intValue);
		form.AddField("floatValue", floatValue.ToString());
				
		WWW www = new WWW(url, form);
		
		yield return www;
	}
}
