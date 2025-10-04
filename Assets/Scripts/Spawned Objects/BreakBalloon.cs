using UnityEngine;
using System.Collections;

public class BreakBalloon : BaseObject {
	
	public Balloon balloon;
	
	public void Detach()
	{
		balloon.Break();
	}
}
