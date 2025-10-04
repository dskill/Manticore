using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Ball : BaseObject {

	public AudioClip[] clipsSlowRolling;
	public AudioClip[] clipsFastRolling;
	public AudioClip[] clipsHitPin;
	public AudioClip[] clipsHitWalls;

	public float slowSoundThreshold = 3f;
	public float fastSoundThreshold = 6f;

	protected bool _rolling = false;
	public bool rolling
	{
		get { return _rolling; }
		set { _rolling = value; if(!_rolling) audioSource.Stop(); }
	}

	// Cached components
	private AudioSource audioSource;
	private Rigidbody rb;

	protected override void Awake ()
	{
		base.Awake ();

		audioSource = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody>();
		audioSource.loop = true;
	}
	
	void Update()
	{
		if(!audioSource.isPlaying)
		{
			float velocityMagnitude = rb.velocity.magnitude;

			if(rolling && clipsFastRolling.Length > 0 && velocityMagnitude >= fastSoundThreshold)
			{
				audioSource.clip = clipsFastRolling[Random.Range(0, clipsFastRolling.Length)];
				audioSource.Play();
			}

			if(rolling && !audioSource.isPlaying && clipsSlowRolling.Length > 0 && velocityMagnitude >= slowSoundThreshold)
			{
				audioSource.clip = clipsSlowRolling[Random.Range(0, clipsSlowRolling.Length)];
				audioSource.Play();
			}

			if(velocityMagnitude < slowSoundThreshold)
				audioSource.Stop();
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.CompareTag("Pin"))
		{
			if(clipsHitPin.Length > 0)
				audioSource.PlayOneShot(clipsHitPin[Random.Range(0, clipsHitPin.Length)]);
		}
		if(collision.collider.CompareTag("Walls"))
		{
			if(clipsHitWalls.Length > 0)
				audioSource.PlayOneShot(clipsHitWalls[Random.Range(0, clipsHitWalls.Length)]);
		}
	}
}
