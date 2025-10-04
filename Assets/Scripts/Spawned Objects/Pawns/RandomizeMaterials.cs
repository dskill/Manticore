using UnityEngine;
using System.Collections;

public class RandomizeMaterials : MonoBehaviour {

	public Material[] materials;
	
	void Awake()
	{
		if(materials.Length > 0)
		{
			MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer meshRenderer in meshRenderers)
			{
				meshRenderer.material = materials[Random.Range(0, materials.Length)];
			}
		}
	}
}
