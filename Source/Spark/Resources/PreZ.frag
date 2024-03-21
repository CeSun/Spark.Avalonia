#version 300 es
precision highp float;

#ifdef _BLENDMODE_MASKED_
uniform sampler2D BaseColor;
in vec2 OutTexCoord;
#endif


void main()
{
#ifdef _BLENDMODE_MASKED_
	if (texture(BaseColor, OutTexCoord).a <= 0.0)
		discard;
#endif
}