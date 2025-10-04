using UnityEngine;
using System.Collections;

public class DestroyOnFrameEnd : BaseObject {

	void OnEnable()
	{
		EventManager.FrameCompleted += FrameCompleted;
	}
	
	void OnDisable()
	{
		EventManager.FrameCompleted -= FrameCompleted;
	}
	
	void FrameCompleted(int frame)
	{
		Destroy(gameObject);
	}
}
