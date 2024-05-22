out vec4 glColor;

uniform sampler2D ColorTexture;

uniform vec2 CameraRenderTargetSize;
uniform vec2 RealRenderTargetSize;

in vec2 OutTexCoord;

float CalcGrayscale(vec3 Color)
{
	return 0.213 * Color.r + 0.715 * Color.g + 0.072 * Color.b;
}


void main()
{
	float _Scale = 0.5;

	float ConsoleCharpness = 8.0f;

	vec2 TexCoord = OutTexCoord * vec2(CameraRenderTargetSize / RealRenderTargetSize);
	vec2 Offset = 1.0f / vec2(textureSize(ColorTexture, 0));
	vec4 MColor = texture(ColorTexture, TexCoord);
	vec4 NWColor = texture(ColorTexture, TexCoord + vec2(-Offset.x, Offset.y) / 2.0);
	vec4 NEColor = texture(ColorTexture, TexCoord + vec2(Offset.x, Offset.y) / 2.0);
	vec4 SWColor = texture(ColorTexture, TexCoord + vec2(-Offset.x, -Offset.y) / 2.0);
	vec4 SEColor = texture(ColorTexture, TexCoord + vec2(Offset.x, -Offset.y) / 2.0);

	float M = CalcGrayscale(MColor.xyz);
	float NW = CalcGrayscale(NWColor.xyz);
	float NE = CalcGrayscale(NEColor.xyz);
	float SW = CalcGrayscale(SWColor.xyz);
	float SE = CalcGrayscale(SEColor.xyz);

	float MaxLuma = max(max(NW, NE), max(SW, SE));
	float MinLuma = min(min(NW, NE), min(SW, SE));

	float Contrast = max(MaxLuma, M) - min(MinLuma, M);

	vec2 Direction;
	Direction.x = -((NW + NE) - (SW + SE));
	Direction.y = ((NE + SE) - (NW + SW));
	Direction = normalize(Direction);

	vec2 Direction1 = Direction * Offset * _Scale;
	vec4 N1 = texture(ColorTexture, TexCoord + Direction2);
	vec4 P1 = texture(ColorTexture, TexCoord - Direction2);

	vec4 Result = (N1 + P1) * 0.5f;

	if (Contrast > 0.5)
	{
		glColor = Result;
	}
	else 
	{
		glColor = MColor;
	}
}