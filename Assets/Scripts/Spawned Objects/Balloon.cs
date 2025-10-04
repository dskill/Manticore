using UnityEngine;
using System.Collections;

public class Balloon : BaseObject {

	public float floatSpeed;
	private bool broken = false;
	private Transform mainCameraTransform;
	private bool frameComplete = false;
	
	void OnEnable()
	{
		EventManager.FrameCompleted += FrameCompleted;
	}
	
	void OnDisable()
	{
		EventManager.FrameCompleted -= FrameCompleted;
	}
	
	void Start()
	{
		mainCameraTransform = Camera.main.transform;
	}
	
	public void Break()
	{
		if (broken)
			return;
		
		broken = true;
		myTransform.parent = null;
		StartCoroutine(Float());
	}
	
	private IEnumerator Float()
	{
		bool done = false;
		while(true)
		{
			if (!done)
			{
				myTransform.position += Vector3.up * Time.deltaTime * floatSpeed;
				if (myTransform.position.y > 8)
					done = true;
			}
			
			if (frameComplete)
			{
				Vector3 offset = (myTransform.position-mainCameraTransform.position).normalized;
				Quaternion lookAtBall = Quaternion.LookRotation(offset);
				lookAtBall = Quaternion.RotateTowards(mainCameraTransform.rotation, lookAtBall, 2.5f * Time.deltaTime);
				mainCameraTransform.rotation = lookAtBall;
			}
			yield return null;
		}
	}
	
	private void FrameCompleted(int frame)
	{
		if (frame == 10)
		{
			Break();
			frameComplete = true;
		}
	}
}
