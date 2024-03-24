#version 300 es
precision mediump float;
#include <Common.glsl>
layout(location=0) in vec3 Position;
layout(location=1) in vec3 Normal;
layout(location=2) in vec3 Tangent;
layout(location=3) in vec3 BitTangent;
layout(location=4) in vec3 Color;
layout(location=5) in vec2 TexCoord;

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Model;

out PassToFrag passToFrag;

uniform LightInfo lightInfo;

void main()
{
	vec3 T = normalize(vec3(Model * vec4(Tangent,   0.0)));
	vec3 B = normalize(vec3(Model * vec4(BitTangent, 0.0)));
	vec3 N = normalize(vec3(Model * vec4(Normal,    0.0)));
	mat3 TBN = mat3(T, B, N);

	passToFrag.Position = Projection * View * Model * vec4(Position, 1.0f);
	passToFrag.Normal = Normal;
	passToFrag.Color = Color;
	passToFrag.TexCoord = TexCoord;
	passToFrag.LightColor = lightInfo.Color;
	passToFrag.CameraPosition = lightInfo.CameraPosition;
	passToFrag.TangentPosition = TBN * vec3(Model * vec4(Position, 1.0f));
	passToFrag.CameraTangentPosition = TBN * passToFrag.CameraPosition;
	passToFrag.IndirectLightStrength = passToFrag.IndirectLightStrength;
#ifdef _DIRECTIONLIGHT_
	// 定向光朝向
	passToFrag.LightTangentDirection = TBN * lightInfo.Direction;
#endif
#ifdef _POINTLIGHT_
	passToFrag.LightPosition = lightInfo.LightPosition;
	passToFrag.LightTangentPosition = TBN * lightInfo.LightPosition;
	passToFrag.AttenuationFactor = lightInfo.AttenuationFactor;

#endif

#ifdef _SPOTLIGHT_
	passToFrag.LightPosition = lightInfo.LightPosition;
	passToFrag.LightTangentPosition = TBN * lightInfo.LightPosition;
	passToFrag.Distance = lightInfo.Distance;
	passToFrag.InteriorCosine = lightInfo.InteriorCosine;
	passToFrag.ExteriorCosine = lightInfo.ExteriorCosine;
	passToFrag.LightTangentDirection = TBN * lightInfo.Direction;
#endif
		
	gl_Position = passToFrag.Position;
}