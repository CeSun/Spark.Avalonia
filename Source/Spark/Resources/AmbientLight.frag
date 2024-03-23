#version 300 es
precision mediump float;

out vec4 glColor;
uniform float AmbientStrength;

uniform sampler2D BaseColor;
in vec2 OutTexCoord;


void main()
{
	vec4 BaseColor = texture(BaseColor, OutTexCoord);
#ifndef _PREZ_ 
	if (BaseColor.a <= 0.0)
		discard;
#endif
	glColor = BaseColor * AmbientStrength;
}