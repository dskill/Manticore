using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SyncedPinPositions  {

	public List<PosAndTime> positions = new List<PosAndTime>();
	protected int index = 0;
	
	public PosAndTime GetNextPosAndTime()
	{
		if(positions.Count > 0)
		{
			PosAndTime ret_val = positions[index];
			index = (index + 1) % positions.Count;
			return ret_val;
		}
		else
		{
			Debug.LogError("Attempted to access an invalid SyncedPinPosition");
			return new PosAndTime();
		}
	}
}

[System.Serializable]
public class PosAndTime {
	public int node;
	public float time;
}
