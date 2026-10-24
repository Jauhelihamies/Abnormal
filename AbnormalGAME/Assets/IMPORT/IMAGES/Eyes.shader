Shader "Custom/EyeBlinkShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlinkProgress ("Blink Progress", Range(0,1)) = 0
        _GlowBrightness ("Glow Brightness", Float) = 1
        _Feather ("Edge Softness", Range(0.01, 0.2)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _BlinkProgress;
            float _GlowBrightness;
            float _Feather;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                float uvY = IN.texcoord.y;
                float topLidEdge = 1.0 - (_BlinkProgress * 0.5);
                float topLid = smoothstep(topLidEdge, topLidEdge - _Feather, uvY);
                float bottomLidEdge = 0.0 + (_BlinkProgress * 0.5);
                float bottomLid = smoothstep(bottomLidEdge, bottomLidEdge + _Feather, uvY);
                float eyeVisibility = topLid * bottomLid;
                fixed4 finalColor = texColor * IN.color;
                finalColor.rgb *= _GlowBrightness;
                finalColor *= eyeVisibility; 

                finalColor.rgb *= finalColor.a; 
                return finalColor;
            }
            ENDCG
        }
    }
}
