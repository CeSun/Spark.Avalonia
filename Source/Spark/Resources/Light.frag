#version 300 es
precision mediump float;
#include <Common.glsl>
out vec4 glColor;


in PassToFrag passToFrag;

uniform LightInfo Light;
uniform	sampler2D BaseColorTexture;
uniform	sampler2D NormalTexture;

void main()
{
	vec4 BaseColor = texture(BaseColorTexture, passToFrag.TexCoord);
	vec3 Normal = texture(NormalTexture, passToFrag.TexCoord).xyz;
	Normal = normalize(Normal * 2.0 - 1.0);
#ifndef _PREZ_
	if (BaseColor.a <= 0.1)
		discard;
#endif
	LightInfo tmp = Light;

	tmp.CameraPosition = passToFrag.CameraTangentPosition;
#ifdef _DIRECTIONLIGHT_
	// 定向光朝向
	tmp.Direction = passToFrag.LightTangentDirection;
#endif
#ifdef _POINTLIGHT_
	tmp.LightPosition = passToFrag.LightTangentPosition;
#endif

#ifdef _SPOTLIGHT_
	tmp.LightPosition = passToFrag.LightTangentPosition;
	tmp.Direction = passToFrag.LightTangentDirection;
#endif

	glColor = BlinnPhongShading(BaseColor, Normal, passToFrag.TangentPosition ,tmp);
}