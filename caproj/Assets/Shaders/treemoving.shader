Shader "tree/treemoving"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MyTex("Volume",3D)="white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };

            sampler2D _MainTex;
            sampler3D _MyTex;
            float4 _size;
            v2f vert (appdata v)
            {
                v2f o;
                float4 worldpos = mul(unity_ObjectToWorld,v.vertex);
                o.uv = v.uv;
                //worldpos = float4(worldpos.x + worldpos.y, worldpos.y, worldpos.z + worldpos.y, 1.0);
                float3 vel = tex3Dlod(_MyTex, float4(worldpos.xyz/_size.xyz, 0.0)).xyz;
                
                //height based move
                worldpos = worldpos + worldpos.y * float4(vel.x*vel.x,vel.y*vel.y,vel.z*vel.z, 0.0)*0.3;
                //distance based sway
                worldpos = worldpos + float4(vel.x, vel.y, vel.z, 0.0) * sin(_Time.y + worldpos.x + worldpos.z) * v.color.y * 0.02;
                worldpos = worldpos + float4(vel.x, vel.y, vel.z, 0.0) * sin(_Time.y+worldpos.x+worldpos.z) * v.color.x*0.05;
                
                o.vertex = mul(unity_MatrixVP, worldpos);
                //o.vertex= UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
