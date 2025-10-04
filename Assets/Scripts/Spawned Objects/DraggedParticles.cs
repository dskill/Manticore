using UnityEngine;
using System.Collections;

public class DraggedParticles : MonoBehaviour {
	public ParticleSystem particlesPrefab;
	public Transform particlesMarker;
	public float decalHeight = 0.05f;
	private bool started = false;
	private ParticleSystem particles;
	private Transform particlesTransform;
	private int floorLayer;
	
	void OnEnable()
	{
		if (!particlesMarker)
			particlesMarker = transform;
		
		if (particlesPrefab)
		{
			particles = ObjectPoolManager.CreatePooled(particlesPrefab.gameObject, particlesMarker.position, Quaternion.identity).GetComponent<ParticleSystem>();
			particlesTransform = particles.transform;
		}
		floorLayer = LayerMask.NameToLayer("Floor");
	}
	
	void OnCollisionStay(Collision collision)
	{
		if (particlesTransform)
		{
			if (collision.gameObject.layer == floorLayer && collision.contacts.Length > 0)
			{
				Vector3 pos = collision.contacts[0].point;
				pos.y += decalHeight;
				particlesTransform.position = pos;
				particlesTransform.LookAt(pos + collision.contacts[0].normal);
				if (!started)
				{
					particles.Clear();
					particles.Stop();
					particles.Play();
					started = true;
				}
			}
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if (particlesTransform)
		{
			if (collision.gameObject.layer == floorLayer)
			{
				Vector3 pos = collision.contacts[0].point;
				pos.y += decalHeight;
				particlesTransform.position = pos;
				particlesTransform.LookAt(pos + collision.contacts[0].normal);
				if (!started)
				{
					particles.Clear();
					particles.Stop();
					particles.Play();
					started = true;
				}
			}
		}
	}
}
