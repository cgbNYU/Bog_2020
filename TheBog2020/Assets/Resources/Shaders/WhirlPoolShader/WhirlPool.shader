// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Chill/WhirlPool"
{
  Properties
  {
    _MainTex("Texture", 2D) = "white" {}
    _MotionTex("Motion", 2D) = "black" {}
    _Speed("Speed", float) = 0
    _Swirliness("Swirliness", float) = .75
    _Rotation("Rotation", float) = 0
    _Swirl("Swirl", float) = 0
    _SwirlIntensity("Swirl Intensity", float) = 5
    _Pivot("", Vector) = (0.5, 0.5, 1.0, 1.0)
    _CenterShadow("CenterShadow", Color) = (1,1,1,1)
    _CenterShadowSize("CenterSize", float) = 0.4
  }

  SubShader
  {
    Tags {
      "Queue" = "Transparent"
      "RenderType" = "Transparent"
      "PreviewType" = "Plane"
    }
    LOD 100
    ZWrite Off

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
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      sampler2D _MainTex;
      sampler2D _MotionTex;
      float _Speed;
      float _Rotation;
      float _Swirl;
      float _Swirliness;
      float _SwirlIntensity;
      fixed4 _Pivot;
      half4 _CenterShadow;
      float _CenterShadowSize;

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      float2 rotate( float magnitude , float2 p )
      {
        float sinTheta = sin(magnitude);
        float cosTheta = cos(magnitude);
        float2x2 rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
        return mul(p, rotationMatrix);
      }

      fixed4 frag(v2f i) : SV_Target
      {
        fixed4 motion = tex2D(_MotionTex, i.uv);

        float2 p = i.uv - _Pivot.xy;
        // Rotate based upon direction
        p = rotate(_Swirl * (motion.r * _Time), p);
        p = rotate(_Rotation * _Time * _Speed, p);

        // get the angle of our points, and divide it in half
        float a = atan2(p.y , p.x ) * 0.5;
        // the square root of the dot product will convert our angle into our distance from center
        float r = sqrt(dot(p,p));
        float2 uv;
        // x is equal to the square root modified by:
        // _Speed: The speed at which the pool twists.
        // _Swirliness: How many 'rings' on the x we have.
        uv.x = (_Time * _Speed) - 1/(r + _Swirliness);
        // uv.x = r;
        uv.y = _Pivot.z *a/3.1416;

        // Now we can get our color.
        fixed4 fragColor = tex2D(_MainTex, uv);

        // this adds a little circular blur based on the 'r' or distance from scener.
        float shadowIntensity =  1 - smoothstep(0,_CenterShadowSize, r);
        //Multiply in the alpha of our center shadow so that we can mix in our color as necessary.
        shadowIntensity *= _CenterShadow.a;

        // alpha blend the shadow and the fragment color.
        fragColor.rgb = ( (1 - shadowIntensity) * fragColor.rgb ) + (shadowIntensity * _CenterShadow);
        return fragColor;
      }
      ENDCG
    }
  }
}