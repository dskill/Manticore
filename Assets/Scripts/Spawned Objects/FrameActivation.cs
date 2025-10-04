using UnityEngine;
using System.Collections;

public class FrameActivation : BaseObject {

	public bool activeFrame1 = false;
	public bool activeFrame2 = false;
	public bool activeFrame3 = false;
	public bool activeFrame4 = false;
	public bool activeFrame5 = false;
	public bool activeFrame6 = false;
	public bool activeFrame7 = false;
	public bool activeFrame8 = false;
	public bool activeFrame9 = false;
	public bool activeFrame10 = false;
	
	protected bool[] activeFrames = new bool[10];
	
	override protected void Awake()
	{
		base.Awake();
		
		activeFrames[0] = activeFrame1;
		activeFrames[1] = activeFrame2;
		activeFrames[2] = activeFrame3;
		activeFrames[3] = activeFrame4;
		activeFrames[4] = activeFrame5;
		activeFrames[5] = activeFrame6;
		activeFrames[6] = activeFrame7;
		activeFrames[7] = activeFrame8;
		activeFrames[8] = activeFrame9;
		activeFrames[9] = activeFrame10;
		
		EventManager.FrameStarted += FrameStarted;
		
		if(activeFrames[0])
			gameObject.SetActive(true);
		else
			gameObject.SetActive(false);
	}
	
	void OnDestroy()
	{
		EventManager.FrameStarted -= FrameStarted;
	}
	
	public void FrameStarted(int frame)
	{
		if(frame <= 1 || frame > 10)
			return;
		
		if(activeFrames[frame-1])
			gameObject.SetActive(true);
		else
			gameObject.SetActive(false);
	}
}
