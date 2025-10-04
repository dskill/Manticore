using UnityEngine;
using System.Collections;

public class Player : Pawn {
	public Transform arrow;
	private Renderer[] arrowRenderers;
	private float oldArrowScale = 0f;
	public Ball ball;
	public Transform ballMarker;
	public float minThrowDistance = 2f;
	public float maxThrowDistance = 10f;
	public float minThrowSpeed = 20f;
	public float maxThrowSpeed = 40f;
	public float ballSleepVelocity = 1f;
	public float horizontalMoveSpeed = 15f;
	private float ballSleepVelocitySqr;
	public float ballReturnLockoutTime = 3f;
	protected float ballRollingOutTime = 0f;
	public float ballReturnMinSpeed = 10f;
	public float ballReturnMaxSpeed = 20f;
	public float ballReturnDistanceThreshold = 0.1f;
	public float xMoveLimit = 10f;
	public float throwCameraEaseInTime = 1f;
	private float throwCameraStartTime;
	public float throwCameraFollowDistance = 5f;
	public float throwCameraFollowSpeed = 2f;
	public float throwCameraFollowRotationSpeed = 30f;
	public float ballReturnTransitionTime = 3f;
	private float ballReturnTransitionTimer = 0;
	private Vector3 ballReturnTransitionCameraStartPos;
	private Quaternion ballReturnTransitionCameraStartRot;
	public iTween.EaseType ballReturnTransitionEaseType = iTween.EaseType.easeInOutSine;
	private float ballReturnDistanceThresholdSqr;
	private float ballReturnCloseDistanceThresholdSqr;
	private float throwDistanceRange;
	private Rigidbody ballRigidbody;
	public float minAngle = 15f;
	
	public enum PlayerState { PickThrowPosition, AimThrow, ThrewBall, BallReturnStart, BallReturn }
	private PlayerState state;
	private int floorMask;
	private Camera mainCamera;
	private Transform mainCameraTransform;
	private Vector3 mainCameraInitialPos;
	private Quaternion mainCameraInitialRot;
	private bool cameraStopped = false;
	private bool cameraLeftBehind = false;
	
	public AudioClip[] clipsBallLaunch;
	public AudioClip[] clipsBallReturnLaunch;
	public AudioClip[] clipsArrowScaleDecrease;
	public AudioClip[] clipsArrowScaleIncrease;
	public AudioClip[] clipsStrafe;
	
	public float iosTiltAccelerationModifier = 1f;
	public float iosTiltDeadZone = 0.1f;
	private Vector3 lastMousePos = Vector3.zero;
	
	override protected void Awake()
	{
		base.Awake();
		
		floorMask = 1 << LayerMask.NameToLayer("Floor");
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;
		arrowRenderers = arrow.GetComponentsInChildren<Renderer>();
		for (int i=0;i<arrowRenderers.Length;i++)
			arrowRenderers[i].enabled = false;
		throwDistanceRange = maxThrowDistance-minThrowDistance;
		ballSleepVelocitySqr = ballSleepVelocity * ballSleepVelocity;
		ballRigidbody = ball.GetComponent<Rigidbody>();
		ballReturnDistanceThresholdSqr = ballReturnDistanceThreshold * ballReturnDistanceThreshold;
		ballReturnCloseDistanceThresholdSqr = ballReturnDistanceThreshold * 3;
		ballReturnCloseDistanceThresholdSqr *= ballReturnCloseDistanceThresholdSqr;
		
		mainCameraInitialPos = mainCameraTransform.position;
		mainCameraInitialRot = mainCameraTransform.rotation;
	}
	
	void Update()
	{
		switch (state)
		{
			case PlayerState.PickThrowPosition: UpdatePickThrowPosition(); break;
			case PlayerState.AimThrow: UpdateAimThrow(); break;
			case PlayerState.ThrewBall : UpdateThrewBall(); break;
			case PlayerState.BallReturnStart : UpdateBallReturnStart(); break;
			case PlayerState.BallReturn : UpdateBallReturn(); break;
		}
	}
	
	void FixedUpdate()
	{
		switch (state)
		{
			case PlayerState.ThrewBall : FixedUpdateThrewBall(); break;
			case PlayerState.BallReturn : FixedUpdateBallReturn(); break;
		}
	}
	
	private void UpdatePickThrowPosition()
	{
		MovePlayer(true);
		
		if (Input.GetMouseButtonDown(0))
		{
			for (int i=0;i<arrowRenderers.Length;i++)
				arrowRenderers[i].enabled = true;
			arrow.position = ballMarker.position;
			oldArrowScale = 0f;
			state = PlayerState.AimThrow;
		}
	}
	
	private void UpdateAimThrow()
	{
		ball.myTransform.position = ballMarker.position;
		
		if (Input.GetMouseButton(0))
			lastMousePos = Input.mousePosition;
		
		Ray ray = mainCamera.ScreenPointToRay(lastMousePos);
		float dist = 0;
		float angle = 0f;
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
		{
			Vector3 target = hit.point;
			target.y = arrow.position.y;
			if (target.z < arrow.position.z)
				target.z = arrow.position.z;
			arrow.LookAt(target);
			
			dist = Vector3.Distance(arrow.position, target);
			dist = Mathf.Min(dist, maxThrowDistance);
			angle = Vector3.Angle(arrow.transform.forward, Vector3.right);
			
			Vector3 scale = arrow.localScale;
			scale.z = dist;
			arrow.localScale = scale;
			
			if (dist < minThrowDistance || (angle < minAngle || angle > 180 - minAngle))
			{
				for (int i=0;i<arrowRenderers.Length;i++)
					arrowRenderers[i].enabled = false;
			}
			else
			{
				if(!GetComponent<AudioSource>().isPlaying)
				{
					if(clipsArrowScaleIncrease.Length > 0 && arrow.localScale.magnitude > oldArrowScale)
					{
						GetComponent<AudioSource>().clip = clipsArrowScaleIncrease[Random.Range(0, clipsArrowScaleIncrease.Length)];
						GetComponent<AudioSource>().Play();
					}
					
					if(clipsArrowScaleDecrease.Length > 0 && arrow.localScale.magnitude < oldArrowScale)
					{
						GetComponent<AudioSource>().clip = clipsArrowScaleDecrease[Random.Range(0, clipsArrowScaleDecrease.Length)];
						GetComponent<AudioSource>().Play();
					}							
				}
				oldArrowScale = arrow.localScale.magnitude;
				for (int i=0;i<arrowRenderers.Length;i++)
					arrowRenderers[i].enabled = true;
			}
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			if (dist < minThrowDistance || (angle < minAngle || angle > 180 - minAngle))
			{
				for (int i=0;i<arrowRenderers.Length;i++)
					arrowRenderers[i].enabled = false;
				state = PlayerState.PickThrowPosition;
			}
			else
			{
				ThrowBall(dist);
				oldArrowScale = 0f;
				for (int i=0;i<arrowRenderers.Length;i++)
					arrowRenderers[i].enabled = false;
				cameraStopped = false;
				cameraLeftBehind = false;
				throwCameraStartTime = Time.time;
				state = PlayerState.ThrewBall;
				gameLoop.HideRoundPhrase();
			}
		}
	}
	
	private void UpdateThrewBall()
	{
		ballRollingOutTime += Time.deltaTime;
		
		float speed = throwCameraFollowSpeed;
		float followDistance = throwCameraFollowDistance;
		
		if (Time.time < throwCameraStartTime + throwCameraEaseInTime)
		{
			float f = (Time.time - throwCameraStartTime) / throwCameraEaseInTime;
			speed = Mathf.Lerp(0, speed, f);
			followDistance = Mathf.Lerp(0, followDistance, f);
		}
		
		Vector3 offset = ball.myTransform.position-mainCameraTransform.position;
		float dist = offset.magnitude;
		offset.Normalize();
		if (dist >= followDistance && !cameraStopped)
		{
			cameraLeftBehind = true;
			dist -= speed * Time.deltaTime;
			//dist = Mathf.Max(throwCameraFollowDistance, dist);
			mainCameraTransform.position = ball.myTransform.position - offset * dist;
		}
		else
		{
			if (cameraLeftBehind)
				cameraStopped = true;
		}
		
		Quaternion lookAtBall = Quaternion.LookRotation(offset);
		lookAtBall = Quaternion.RotateTowards(mainCameraTransform.rotation, lookAtBall, throwCameraFollowRotationSpeed * Time.deltaTime);
		mainCameraTransform.rotation = lookAtBall;
		// never tilt/lean the camera
		Vector3 euler = mainCameraTransform.eulerAngles;
		euler.z = 0;
		mainCameraTransform.eulerAngles = euler;
	}
	
	private void FixedUpdateThrewBall()
	{
		if ( ballRollingOutTime > ballReturnLockoutTime && (Input.GetMouseButton(0) || ballRigidbody.IsSleeping() || ballRigidbody.velocity.sqrMagnitude < ballSleepVelocitySqr))
		{
			ballRigidbody.velocity = Vector3.zero;
			ballRigidbody.angularVelocity = Vector3.zero;
			ballRigidbody.Sleep();
			ballReturnTransitionTimer = Time.time + ballReturnTransitionTime;
			ballReturnTransitionCameraStartPos = mainCameraTransform.position;
			ballReturnTransitionCameraStartRot = mainCameraTransform.rotation;
			for (int i=0;i<arrowRenderers.Length;i++)
				arrowRenderers[i].enabled = true;
			Vector3 scale = arrow.localScale;
			scale.z = maxThrowDistance;
			arrow.localScale = scale;
			ball.rolling = false;
			state = PlayerState.BallReturnStart;
		}
	}
	
	private void UpdateBallReturnStart()
	{
		MovePlayer(false);
		
		Vector3 offset = ballMarker.position - ball.myTransform.position;
		offset.Normalize();
		
		Vector3 arrowOffset = (ball.myTransform.position-ballMarker.position).normalized;
		arrow.position = ball.myTransform.position + arrowOffset;
		arrow.LookAt(ballMarker);
		
		if (Input.GetMouseButton(0))
		{
			ballRigidbody.velocity = offset * ballReturnMinSpeed;
			ballReturnTransitionTimer = Time.time + ballReturnTransitionTime;
			ballReturnTransitionCameraStartPos = mainCameraTransform.position;
			ballReturnTransitionCameraStartRot = mainCameraTransform.rotation;
			for (int i=0;i<arrowRenderers.Length;i++)
				arrowRenderers[i].enabled = false;
			
			if(clipsBallReturnLaunch.Length > 0)
				GetComponent<AudioSource>().PlayOneShot(clipsBallReturnLaunch[Random.Range(0, clipsBallReturnLaunch.Length)]);
			
			state = PlayerState.BallReturn;
		}
	}
	
	private void FixedUpdateBallReturn()
	{
		MovePlayer(false);
		
		Vector3 offset = ballMarker.position - ball.myTransform.position;
		bool done = false;
		float sqrMag = offset.sqrMagnitude;
		if (sqrMag < ballReturnDistanceThresholdSqr)
			done = true;
			
		offset.Normalize();
		
		float speed;
		if (Input.GetMouseButton(0) && sqrMag > ballReturnCloseDistanceThresholdSqr)
			speed = ballReturnMaxSpeed;
		else
			speed = ballReturnMinSpeed;
		
		ballRigidbody.velocity = offset * speed;
		
		if (done)
		{
			EventManager.ballReturned();
			
			ball.myTransform.position = ballMarker.position;
			ballRigidbody.velocity = Vector3.zero;
			ballRigidbody.angularVelocity = Vector3.zero;
			ballRigidbody.Sleep();
			state = PlayerState.PickThrowPosition;
		}
	}
	
	private void UpdateBallReturn()
	{
		if (Time.time > ballReturnTransitionTimer)
		{
			mainCameraTransform.position = mainCameraInitialPos;
			mainCameraTransform.rotation = mainCameraInitialRot;
			return;
		}
		
		float f = 1 - ((ballReturnTransitionTimer - Time.time)/ballReturnTransitionTime);
		f = iTween.CallEasingFunction(ballReturnTransitionEaseType, 0, 1, f);
		mainCameraTransform.position = Vector3.Lerp(ballReturnTransitionCameraStartPos, mainCameraInitialPos, f);
		mainCameraTransform.rotation = Quaternion.Lerp(ballReturnTransitionCameraStartRot, mainCameraInitialRot, f);
	}
	
	private void ThrowBall(float dist)
	{
		ballRollingOutTime = 0f;
		float f = (dist - minThrowDistance) / throwDistanceRange;
		float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, f);
		ballRigidbody.velocity = arrow.forward * speed;
		ball.rolling = true;
		GetComponent<AudioSource>().Stop();
		if(clipsBallLaunch.Length > 0)
			GetComponent<AudioSource>().PlayOneShot(clipsBallLaunch[Random.Range(0, clipsBallLaunch.Length)]);
	}
	
	private void MovePlayer(bool updateBall)
	{

		float distanceMoved = 0f;
		float xDelta = 0f;
		Vector3 pos = myTransform.position;
		
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if(Mathf.Abs(Input.acceleration.y) > iosTiltDeadZone)
			{
				xDelta = Mathf.Clamp(-Input.acceleration.y * iosTiltAccelerationModifier, -horizontalMoveSpeed * Time.deltaTime, horizontalMoveSpeed * Time.deltaTime);
			}
		}
		else
		{
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
			{
				xDelta = Mathf.Clamp(hit.point.x - pos.x, -horizontalMoveSpeed*Time.deltaTime, horizontalMoveSpeed*Time.deltaTime);
			}
		}
		
		if(xDelta != 0f)
		{
			pos.x = Mathf.Clamp(pos.x + xDelta, -xMoveLimit, xMoveLimit);
			distanceMoved = Mathf.Abs(pos.x - myTransform.position.x);
			myTransform.position = pos;
			
			if(updateBall)
			{
				ball.myTransform.position = ballMarker.position;
				
				if(distanceMoved > 0.01f && clipsStrafe.Length > 0 && !GetComponent<AudioSource>().isPlaying)
				{
					GetComponent<AudioSource>().clip = clipsStrafe[Random.Range(0, clipsStrafe.Length)];
					GetComponent<AudioSource>().Play();
				}
			}
		}
	}
}
