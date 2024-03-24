#version 300 es
precision mediump float;
#include <Common.glsl>
out vec4 glColor;


in PassToFrag passToFrag;

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

	glColor = BlinnPhongShading(BaseColor, Normal, passToFrag);
}