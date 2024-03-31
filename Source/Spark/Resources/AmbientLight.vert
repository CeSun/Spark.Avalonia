#version 300 es
precision mediump float;
layout(location=0) in vec3 Position;
layout(location=4) in vec3 Color;
layout(location=5) in vec2 TexCoord;

uniform mat4 Projection;
uniform mat4 View;
uniform mat4 Model;

#ifdef _SHADERMODEL_BLINNPHONG_LAMBERT_
out vec2 OutTexCoord;
out vec3 OutColor;
#endif

void main()
{
#ifdef _SHADERMODEL_BLINNPHONG_LAMBERT_
	OutTexCoord = TexCoord;
	OutColor = Color;
#endif
	gl_Position = Projection * View * Model * vec4(Position, 1.0f);
}