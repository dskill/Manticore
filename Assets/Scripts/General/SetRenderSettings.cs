using UnityEngine;
using System.Collections;

public class SetRenderSettings : MonoBehaviour {
    public Color ambient_color = Color.red;
	public Color fog_color = Color.red;
	public float fog_density = 0.0F;
    void Update() {
        RenderSettings.ambientLight = ambient_color;
		RenderSettings.fogDensity = fog_density;
		RenderSettings.fogColor = fog_color;
    }
}