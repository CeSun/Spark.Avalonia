﻿#version 300 es
precision mediump float;

#ifdef _BLENDMODE_MASKED_ 
uniform sampler2D BaseColor;
in vec2 OutTexCoord;
#endif



void main()
{
#ifdef _BLENDMODE_MASKED_ 
	vec4 BaseColor = texture(BaseColor, OutTexCoord);
	if (BaseColor.a <= 0.0)
		discard;
#endif
}