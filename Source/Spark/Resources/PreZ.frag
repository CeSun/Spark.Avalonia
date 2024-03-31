#version 300 es
precision mediump float;

#ifdef _BLENDMODE_MASKED_ 
uniform	sampler2D BaseColorTexture;
uniform float HasBaseColor;
in vec2 OutTexCoord;
#endif



void main()
{
#ifdef _BLENDMODE_MASKED_ 
	vec4 BaseColor= vec4(1.0f);
	if (HasBaseColor > 0.0)
		BaseColor= texture(BaseColorTexture, OutTexCoord);
	if (BaseColor.a <= 0.0)
		discard;
#endif
}