using UnityEngine;
using System.Collections;

public class EventManager {
	
	public delegate void NullTypeHandler();
	public delegate void IntTypeHandler(int i);
	public delegate void IntFloatTypeHandler(int i, float f);
	public delegate void PawnTypeHandler(Pawn pawn);
	public delegate void PinTypeHandler(Pin pin);

	public static event NullTypeHandler BallReturned;
	public static event IntTypeHandler FrameStarted;
	public static event IntTypeHandler FrameCompleted;
	public static event IntFloatTypeHandler GoToWaypoint;
	public static event PinTypeHandler PinHit;
	
	static public void ballReturned()
	{
//		Debug.Log("Ball returned");
		
		if(BallReturned != null)
			BallReturned();
	}
	
	static public void frameStarted(int frame)
	{
		Debug.Log("Frame " + frame + " started.");
		
		if(FrameStarted != null)
			FrameStarted(frame);
	}
	
	static public void frameCompleted(int frame)
	{
		Debug.Log("Frame " + frame + " completed.");
		
		if(FrameCompleted != null)
			FrameCompleted(frame);
	}
	
	static public void goToWaypoint(int waypoint, float time)
	{
//		Debug.Log("Going to waypoint " + waypoint + " be there at " + time);
		
		if(GoToWaypoint != null)
			GoToWaypoint(waypoint, time);
	}
	
	static public void pinHit(Pin pin)
	{
//		Debug.Log("Pin Hit.");
		
		if(PinHit != null)
			PinHit(pin);
	}
}
