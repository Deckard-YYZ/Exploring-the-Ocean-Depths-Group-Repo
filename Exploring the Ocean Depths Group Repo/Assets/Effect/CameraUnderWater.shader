Shader "Hidden/CameraUnderWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        _DepthMap("Texture", 2D) = "black" {}
        _DepthStart("Depth Start Distance", float) = 1
        _DepthEnd("Depth End Distance", float) = 300
        _DepthColor("Depth Color", float) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        
        //Disable backface culling(Cull off)
        //depth duffer updating during rendering (ZWrite off),
        //Always draw a pixel regradless of depth (ZTest Always)
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture, _MainTex, _DepthMap;
            float _DepthStart, _DepthEnd;
            fixed4 _DepthColor;
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos: TEXTCOORD1;
            };

            //We add an extra screenPos attribute to the vertext data, and compute the screen position of each vertex in the vert() function below.
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            

            
            //run on very pixel that is seen by the camera.
            //responsible for applying post-processing on the image camera sees
            fixed4 frag (v2f i) : SV_Target
            {
                //sample the pixel in i.screenPos from _CameraDepthTexture, then convert it to linear depth (depth stores non-linearly, we want it to clamped between 0 to 1)
                // float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.screenPos.xy));
                float depth = LinearEyeDepth(tex2D(_DepthMap, i.screenPos.xy));

                //collapse the depth to 0-1:       1 meaning the pixel is further than Depth_end, 0 as closer than _depthStart
                depth = saturate((depth - _DepthStart) / _DepthEnd);

                //Scale the intensity of the depth color based on the depth by lerping it between the original pixel color and our color based on the depthValue of the pixel
                //so the closer, the clear it appears, the further the blue it looks
                fixed4 col = tex2D(_MainTex, i.screenPos);
                //The math calculation is for low visibility in dimmer area
                //return lerp(col, _DepthColor, depth);
                return lerp(col, (0.5 * glstate_lightmodel_ambient + _DepthColor * 0.5) * glstate_lightmodel_ambient.w, depth);
                
                // fixed4 col = tex2D(_MainTex, i.uv);
                // // just invert the colors
                // col.rgb = 1 - col.rgb;
                // return col;
            }
            ENDCG
        }
    }
}
