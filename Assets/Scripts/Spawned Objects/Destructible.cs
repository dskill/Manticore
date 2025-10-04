using UnityEngine;
using System.Collections;

public class Destructible : SpawnedObject {
	
	public float audioHitObjectThreshold = 2f;
	public AudioClip[] clipsHitObject;
	
	void OnEnable()
	{
		gameLoop.AddDestructible(this);
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(clipsHitObject.Length > 0 && GetComponent<AudioSource>() && collision.relativeVelocity.magnitude > audioHitObjectThreshold)
			GetComponent<AudioSource>().PlayOneShot(clipsHitObject[Random.Range(0, clipsHitObject.Length)]);
	}
}
