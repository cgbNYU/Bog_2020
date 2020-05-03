// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VertexManipulationDemo"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0,0,0,0)
		_Frequency("Frequency", Float) = 0
		_OffsetTime("Offset Time", Float) = 0
		_Amplitude("Amplitude", Float) = 0
		_OffsetAmplitude("OffsetAmplitude", Float) = 0
		_FlapScalar("FlapScalar", Range( 0 , 10)) = 0
		_FlapTimeOffset("FlapTimeOffset", Range( 0 , 100)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			half filler;
		};

		uniform float _Frequency;
		uniform float _OffsetTime;
		uniform float _FlapTimeOffset;
		uniform float _Amplitude;
		uniform float _OffsetAmplitude;
		uniform float _FlapScalar;
		uniform float4 _Albedo;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float temp_output_16_0 = abs( ase_vertex3Pos.x );
			float4 appendResult13 = (float4(0.0 , ( ( ( sin( ( ( _Time.y * _Frequency ) + _OffsetTime + ( _FlapTimeOffset * temp_output_16_0 * -1 ) ) ) * _Amplitude ) + _OffsetAmplitude ) * temp_output_16_0 * temp_output_16_0 * _FlapScalar ) , 0.0 , 0.0));
			v.vertex.xyz += appendResult13.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Albedo.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
0;12;1920;1007;2477.072;422.3531;1.369532;True;False
Node;AmplifyShaderEditor.CommentaryNode;18;-1591.654,-226.0264;Inherit;False;1026.546;438.3535;Basic Sin Wave;10;3;5;6;7;8;4;9;10;11;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;14;-1856.145,396.6964;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-1528.108,-86.67307;Inherit;False;Property;_Frequency;Frequency;1;0;Create;True;0;0;False;0;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;16;-1646.508,417.7422;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;21;-1626.593,563.71;Inherit;False;Constant;_Int0;Int 0;7;0;Create;True;0;0;False;0;-1;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-1541.654,-176.0264;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1781.351,293.9122;Inherit;False;Property;_FlapTimeOffset;FlapTimeOffset;6;0;Create;True;0;0;False;0;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1363.108,-142.673;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1380.108,2.326948;Inherit;False;Property;_OffsetTime;Offset Time;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1385.556,280.2168;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;INT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-1192.108,-77.67304;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;4;-1046.108,-77.67304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1207.108,86.32708;Inherit;False;Property;_Amplitude;Amplitude;3;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-953.1085,96.3271;Inherit;False;Property;_OffsetAmplitude;OffsetAmplitude;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-901.1085,-77.67304;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-930.1207,472.932;Inherit;False;Property;_FlapScalar;FlapScalar;5;0;Create;True;0;0;False;0;0;0.2;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-717.1085,1.326948;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-443.7297,249.7765;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-338,-6.5;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;13;-286.1251,235.3713;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VertexManipulationDemo;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;14;1
WireConnection;6;0;3;0
WireConnection;6;1;5;0
WireConnection;19;0;20;0
WireConnection;19;1;16;0
WireConnection;19;2;21;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;8;2;19;0
WireConnection;4;0;8;0
WireConnection;10;0;4;0
WireConnection;10;1;9;0
WireConnection;12;0;10;0
WireConnection;12;1;11;0
WireConnection;15;0;12;0
WireConnection;15;1;16;0
WireConnection;15;2;16;0
WireConnection;15;3;17;0
WireConnection;13;1;15;0
WireConnection;0;0;1;0
WireConnection;0;11;13;0
ASEEND*/
//CHKSM=076B70108EB6A486CB712019BC39C622BF221793