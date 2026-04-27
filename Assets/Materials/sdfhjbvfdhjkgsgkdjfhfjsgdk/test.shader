// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "test"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_Color0("Color 0", Color) = (0.1529412,0.7450981,0.7019608,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color0;
		uniform sampler2D _Texture0;
		uniform float4 _Texture0_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color19 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 uv_Texture0 = i.uv_texcoord * _Texture0_ST.xy + _Texture0_ST.zw;
			o.Albedo = ( ( ( step( ( 1.0 - i.uv_texcoord.y ) , 0.57 ) * _Color0 ) + ( ( step( i.uv_texcoord.y , 0.43 ) * color19 ) + float4( 0,0,0,0 ) ) ) * tex2D( _Texture0, uv_Texture0 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
277;73;1253;666;1672.393;524.066;1.6;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1384.178,-197.378;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;15;-1076.739,-233.4651;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;19;-1140.348,-45.39797;Inherit;False;Constant;_Color1;Color 1;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;18;-1056.096,-148.4455;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.43;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;24;-914.9788,-359.2688;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-869.6344,-143.044;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;12;-869.3824,-334.955;Inherit;False;Property;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;0.1529412,0.7450981,0.7019608,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;10;-791.1883,-442.2468;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.57;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-616.3163,-141.0487;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-615.4739,-363.7108;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-687.7006,93.70006;Inherit;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;0;False;0;False;38976355894a036418fbfcc0e10152ba;a4c79b5e971bfcb4b8cb1a547dcc8f44;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-363.5215,-165.0636;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-425.6998,90.70007;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-107.742,-37.79724;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;129.7,-38.10001;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;test;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;1;2
WireConnection;18;0;1;2
WireConnection;24;0;15;0
WireConnection;20;0;18;0
WireConnection;20;1;19;0
WireConnection;10;0;24;0
WireConnection;33;0;20;0
WireConnection;11;0;10;0
WireConnection;11;1;12;0
WireConnection;21;0;11;0
WireConnection;21;1;33;0
WireConnection;5;0;2;0
WireConnection;13;0;21;0
WireConnection;13;1;5;0
WireConnection;0;0;13;0
ASEEND*/
//CHKSM=B8F4B8757458C14424A25D0BB1075E85C8B45CF6