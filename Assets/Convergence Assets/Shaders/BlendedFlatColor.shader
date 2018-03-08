Shader "BlendedFlatColor" {
    Properties {
        _Color ("Main Color, Alpha", Color) = (1,1,1,1)
    }
    Category {
        ZWrite Off
        Lighting Off
        Tags {Queue=Transparent}
        Blend SrcAlpha OneMinusSrcAlpha
        Color [_Color]
        SubShader {
            Pass {
                Cull Off
            }
        }
    }
}