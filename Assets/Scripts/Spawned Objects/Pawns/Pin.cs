using UnityEngine;
using System.Collections;

public class Pin : Pawn {
	public ParticleSystem contactInPlacePrefab;
	private ParticleSystem contactInPlace;
	public ParticleSystem floorInPlacePrefab;
	private ParticleSystem floorInPlace;
	public ParticleSystem floorFollowsPinPrefab;
	private Animation anim;
	public float initialAnimDelayRange = 0.4f;
	public float decalHeight = 0.05f;
	public Rigidbody[] pieces;
	public bool canBeDestroyed = false;
	public float destructionThreshold = 2f;
	
	protected bool pinHit = false;
	public bool hasBeenHit
	{
		get { return pinHit; }
	}
	
	public float audioHitThreshold = 2f;
	public AudioClip[] clipsPinHit;
	public AudioClip[] clipsPinDestroyed;

	protected void OnEnable()
	{
		if(gameLoop)
			gameLoop.activePins.Add(this);
		
		EventManager.FrameCompleted += FrameCompleted;
		
		if (contactInPlace)
			contactInPlace.gameObject.SetActive(false);

		if (floorInPlace)
			floorInPlace.gameObject.SetActive(false);
		
		if(anim)
			StartCoroutine(StartAnim());
		
	}
	
	protected void OnDisable()
	{
		if(gameLoop)
			gameLoop.activePins.Remove(this);
		
		EventManager.FrameCompleted -= FrameCompleted;
	}
	
	override protected void Awake()
	{
		base.Awake();
		
		if (contactInPlacePrefab)
		{
			contactInPlace = ObjectPoolManager.CreatePooled(contactInPlacePrefab.gameObject, myTransform.position, Quaternion.identity).GetComponent<ParticleSystem>();
			InitParticles(contactInPlace);
		}
		
		if (floorInPlacePrefab)
		{
			floorInPlace = ObjectPoolManager.CreatePooled(floorInPlacePrefab.gameObject, myTransform.position, Quaternion.identity).GetComponent<ParticleSystem>();
			InitParticles(floorInPlace);
		}
		
		anim = GetComponentInChildren<Animation>();
	}
	
	private void InitParticles(ParticleSystem system)
	{
		system.transform.parent = myTransform;
		system.playOnAwake = false;
		system.Stop();
		system.Clear();
		system.gameObject.SetActive(false);
	}
	
	protected void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Ball"))
		{
			PinHit(collision);
		}		
		else if(collision.gameObject.CompareTag("Pin"))
		{
			Pin otherPin = collision.gameObject.GetComponent<Pin>();
			
			if(otherPin && otherPin.hasBeenHit)
				PinHit(collision);
		}
		else
		{
			if(clipsPinHit.Length > 0 && collision.relativeVelocity.magnitude >= audioHitThreshold)
				GetComponent<AudioSource>().PlayOneShot(clipsPinHit[Random.Range(0, clipsPinHit.Length)]);
		}
	}
	
	protected IEnumerator StartAnim()
	{
		if(anim)
		{
			yield return new WaitForSeconds(Random.Range(0f, initialAnimDelayRange));
			anim.Play();
		}
	}
	
	public void DebugPinHit()
	{
		if(pinHit)
			return;
		
		pinHit = true;
		
		if(anim)
			anim.Stop();
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		SendMessage("StopSplineMovement");
		gameLoop.activePins.Remove(this);
		EventManager.pinHit(this);
	}
	
	protected void PinHit(Collision collision)
	{
		if(pinHit)
		{
			if(clipsPinHit.Length > 0)
				GetComponent<AudioSource>().PlayOneShot(clipsPinHit[Random.Range(0, clipsPinHit.Length)]);
			return;
		}
		
		pinHit = true;
		
		BreakBalloon breakBalloon = GetComponent<BreakBalloon>();
		if(breakBalloon)
			breakBalloon.Detach();
		
		if (anim)
			anim.Stop();
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		SendMessage("StopSplineMovement");
		gameLoop.activePins.Remove(this);
		EventManager.pinHit(this);
		
		if (contactInPlace)
		{
			contactInPlace.gameObject.SetActive(true);
			contactInPlace.transform.position = collision.contacts[0].point;
			contactInPlace.transform.LookAt(contactInPlace.transform.position + collision.contacts[0].normal);
			contactInPlace.transform.parent = null;
			contactInPlace.Play();
		}
		
		if (floorInPlace)
		{
			floorInPlace.gameObject.SetActive(true);
			Vector3 pos = floorInPlace.transform.position;
			pos.y = decalHeight;
			floorInPlace.transform.position = pos;
			floorInPlace.transform.parent = null;
			floorInPlace.Play();
		}
		
		if(canBeDestroyed && collision.relativeVelocity.magnitude > destructionThreshold)
		{
			gameLoop.PlaySplatter();
			Collider[] colliders = GetComponentsInChildren<Collider>();
			foreach(Collider col in colliders)
				col.enabled = false;
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			foreach(Renderer r in renderers)
				r.enabled = false;
			Destroy(gameObject, 1f);
			int i=0;
			foreach(Rigidbody rb in pieces)
			{
				GameObject go = ObjectPoolManager.CreatePooled(rb.gameObject, myTransform.position + Vector3.up + (Vector3.up * i * 0.5f), myTransform.rotation);
				go.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
				i++;
			}
			
			if(clipsPinDestroyed.Length > 0)
				GetComponent<AudioSource>().PlayOneShot(clipsPinDestroyed[Random.Range(0, clipsPinDestroyed.Length)]);
		}
		else
		{
			if(clipsPinHit.Length > 0)
				GetComponent<AudioSource>().PlayOneShot(clipsPinHit[Random.Range(0, clipsPinHit.Length)]);
		}
	}
	
	protected void FrameCompleted(int frame)
	{
		if(hasBeenHit)
			ObjectPoolManager.DestroyPooled(myGameObject);
	}
}
