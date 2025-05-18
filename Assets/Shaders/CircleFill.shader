Shader "Custom/CircleFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _FillAmount;
            fixed4 _BackgroundColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // ��UV����ת��Ϊ������Ϊԭ��ļ�����
                float2 center = float2(0.5, 0.5);
                float2 delta = i.uv - center;
                float angle = -atan2(delta.y, delta.x) / (2.0 * 3.14159265359) + 0.5;
                float radius = length(delta) * 2.0;
                
                // ��������������Ƿ���ʾ
                if (angle > _FillAmount || radius > 1.0) {
                    col = _BackgroundColor;
                    col.a = 0; // ��������Ϊ��͸��
                }
                
                return col;
            }
            ENDCG
        }
    }
}