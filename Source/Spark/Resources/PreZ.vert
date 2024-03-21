﻿#version 300 es
precision highp float;
layout(location=0) in vec3 Position;
layout(location=5) in vec2 TexCoord;

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Model;

#ifdef _BLENDMODE_MASKED_
out vec2 OutTexCoord;
#endif


void main()
{
#ifdef _BLENDMODE_MASKED_
	OutTexCoord = TexCoord;
#endif
	gl_Position = Projection * View * Model * vec4(Position, 1.0f);
}