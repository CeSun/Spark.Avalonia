
out vec4 glColor;

uniform	sampler2D BaseColorTexture;
uniform float HasBaseColor;
in vec2 OutTexCoord;
in vec3 OutColor;

void main()
{
	vec4 BaseColor= vec4(OutColor, 1.0f);
	if (HasBaseColor > 0.0)
		BaseColor= texture(BaseColorTexture, OutTexCoord);
	glColor = vec4(BaseColor.xyz, BaseColor.a);
}