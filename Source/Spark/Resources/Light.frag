#version 300 es
precision mediump float;
#include <Common.glsl>
out vec4 glColor;


in PassToFrag passToFrag;

#ifdef _SHADERMODEL_BLINNPHONG_
uniform	sampler2D BaseColorTexture;
uniform	sampler2D NormalTexture;
#endif

void main()
{
	vec4 BaseColor = texture(BaseColorTexture, passToFrag.TexCoord);
#ifndef _PREZ_
	if (BaseColor.a <= 0.1)
		discard;
#endif

#ifdef _SHADERMODEL_BLINNPHONG_
		glColor = BlinnPhongShading(BaseColor, passToFrag.Normal, passToFrag);
#endif
}