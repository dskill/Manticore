using UnityEngine;
using System.Collections;

public class MoveAlongSpline : MonoBehaviour {

	public SplinePath spline = null;
	public float speed = 1.0f;
	private int nodeIdx = 0;
	private int direction = 1;
	public bool moveOnStart = true;
	public iTween.LoopType loopType = iTween.LoopType.none;
	private bool started = false;
	private int nodeCount;
	private bool canMoveAlongSpline = true;
	private bool movingToWaypoint = false;
	private PosAndTime posAndTime = new PosAndTime();
	
	protected void OnEnable()
	{
		EventManager.GoToWaypoint += GoToWaypoint;
	}
	
	protected void OnDisable()
	{
		EventManager.GoToWaypoint -= GoToWaypoint;
	}
	
	protected void Start()
	{
		if(spline && moveOnStart)
			StartMove(spline);
	}
	
	private void StartMove(SplinePath path)
	{
		spline = path;
		nodeCount = spline.nodes.Count;
		
		if (nodeCount == 0)
		{
			Debug.LogWarning("Spline Path not set.", gameObject);
			return;
		}
		
		if (nodeIdx >= nodeCount || nodeIdx < 0)
		{
			Debug.LogError("node idx out of bounds: " + nodeIdx, gameObject);
			return;
		}
		
		if(!started)
		{
			StartCoroutine(Move(spline.nodes[0], true));
			started = true;
			return;
		}
		
		if (nodeCount == 1)
		{
			return;
		}
		
		nodeIdx += direction;
		
		StartCoroutine(Move(spline.nodes[nodeIdx], true));
	}
	
	private IEnumerator Move(Vector3 position, bool automaticallyAdvanceNodes)
	{		
		while(canMoveAlongSpline)
		{
			if(Vector3.Distance(position, transform.position) <= 0.2f)
			{
				if(!automaticallyAdvanceNodes)
					break;
				
				if ((direction < 0 && nodeIdx + direction == -1) || (direction > 0 && nodeIdx + direction == nodeCount))
					SplineComplete();
				
				nodeIdx += direction;
				
				position = spline.nodes[nodeIdx];
				
			
				if(loopType == iTween.LoopType.none)
					break;
			}
			else
			{
				var nextMoveSpeed = (position - transform.position).normalized * speed;
				
				GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + nextMoveSpeed;
				
				GetComponent<Rigidbody>().velocity = Vector3.Max(Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, speed), nextMoveSpeed);
				
//				print(rigidbody.velocity + " " + Vector3.Distance(position, transform.position));
				
				yield return null;
			}
		}
	}
	
	protected void FixedUpdate()
	{
		Vector3 position = spline.nodes[posAndTime.node];
		
		if(movingToWaypoint && canMoveAlongSpline)
		{
			var nextMoveSpeed = (position - transform.position).normalized * speed;
				
			GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + nextMoveSpeed;
				
			GetComponent<Rigidbody>().velocity = Vector3.Max(Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, speed), nextMoveSpeed);
		}
	}
	
	protected void GoToWaypoint(int waypoint, float time)
	{
		speed = Vector3.Distance(spline.nodes[waypoint], transform.position) / time;
		
		movingToWaypoint = true;
		posAndTime.node = waypoint;
		posAndTime.time = time;
	}
	
	private void NodeComplete()
	{
		print("Node Complete");
		StartMove(spline);
	}
	
	private void StartMove()
	{
		nodeIdx = 0;
		started = false;
		StartMove(spline);
	}
	
	public void SplineComplete()
	{		
		switch(loopType)
		{
			case iTween.LoopType.none : break;
			case iTween.LoopType.loop : nodeIdx = 0; break;
			case iTween.LoopType.pingPong : direction = -direction; break;
		}
	}
	
	public void StopSplineMovement()
	{
		canMoveAlongSpline = false;
	}
}
