Shader "Custom/DrewTest" {

Properties {
    _MainTex ("Texture", 2D) = ""
}

SubShader {
    Tags {Queue = Transparent}
    Blend One One
    ZWrite On
    Pass {
        SetTexture[_MainTex]
    } 
}

}