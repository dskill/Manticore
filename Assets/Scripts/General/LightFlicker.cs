using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {
    public float duration = 1.0F;
    public Color color0 = Color.red;
    public Color color1 = Color.blue;
    void Update() {
        //float t = Mathf.PingPong(Time.time, duration) / duration;
		float t = Mathf.Clamp(Mathf.Pow(Mathf.PerlinNoise(Time.time*duration, 1.0F) + 0.3F,1.5F),0.0F,1.0F);
        GetComponent<Light>().color = Color.Lerp(color0, color1, t);
    }
}