out vec4 glColor;

uniform sampler2D ColorTexture;

uniform vec2 CameraRenderTargetSize;
uniform vec2 RealRenderTargetSize;

in vec2 OutTexCoord;

float CalcGrayscale(vec3 Color)
{
	return 0.213 * Color.r + 0.715 * Color.g + 0.072 * Color.b;
}

float MaxValue4(float v1, float v2, float v3, float v4)
{
	float Max = v1;
	if (v2 > Max)
		Max = v2;
	if (v3 > Max)
		Max = v3;
	if (v4 > Max)
		Max = v4;
	return Max;
}
float MaxValue5(float v1, float v2, float v3, float v4, float v5)
{
	float Max = MaxValue4(v1, v2, v3, v4);
	if (v5 > Max)
		Max = v5;
	return Max;
}


float MinValue4(float v1, float v2, float v3, float v4)
{
	float Min = v1;
	if (v2 < Min)
		Min = v2;
	if (v3 < Min)
		Min = v3;
	if (v4 < Min)
		Min = v4;
	return Min;
}
float MinValue5(float v1, float v2, float v3, float v4, float v5)
{
	float Min = MinValue4(v1, v2, v3, v4);
	if (v5 < Min)
		Min = v5;
	return Min;
}


void main()
{
	float ConsoleCharpness = 8.0f;

	vec2 TexCoord = OutTexCoord * vec2(CameraRenderTargetSize / RealRenderTargetSize);
	vec2 Offset = 1.0f / vec2(textureSize(ColorTexture, 0));
	vec4 MColor = texture(ColorTexture, TexCoord);
	vec4 NWColor = texture(ColorTexture, TexCoord + vec2(-Offset.x, Offset.y));
	vec4 NEColor = texture(ColorTexture, TexCoord + vec2(Offset.x, Offset.y));
	vec4 SWColor = texture(ColorTexture, TexCoord + vec2(-Offset.x, -Offset.y));
	vec4 SEColor = texture(ColorTexture, TexCoord + vec2(Offset.x, -Offset.y));

	float M = CalcGrayscale(MColor.xyz);
	float NW = CalcGrayscale(NWColor.xyz);
	float NE = CalcGrayscale(NEColor.xyz);
	float SW = CalcGrayscale(SWColor.xyz);
	float SE = CalcGrayscale(SEColor.xyz);

	float Max = MaxValue5(M, NW, NE, SW, SE);
	float Min = MinValue5(M, NW, NE, SW, SE);

	if (Max - Min > 0.3f)
	{
		glColor = (NWColor + NEColor + SWColor + SEColor + MColor) * 0.2;

	}
	else
	{
		glColor = MColor;
	}
	// glColor = vec4(Contrast, Contrast, Contrast, 1.0f);
	// glColor = Color;
}