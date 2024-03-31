#version 300 es
precision mediump float;

out vec4 glColor;
uniform float AmbientStrength;

uniform	sampler2D BaseColorTexture;
uniform float HasBaseColor;
in vec2 OutTexCoord;
in vec3 OutColor;

void main()
{
	vec4 BaseColor= vec4(OutColor, 1.0f);
	if (HasBaseColor > 0.0)
		BaseColor= texture(BaseColorTexture, OutTexCoord);
#ifndef _PREZ_ 
	if (BaseColor.a <= 0.0)
		discard;
#endif
	glColor = vec4(BaseColor.xyz * AmbientStrength, BaseColor.a);
}