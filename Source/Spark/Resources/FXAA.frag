out vec4 glColor;

uniform sampler2D ColorTexture;

uniform vec2 CameraRenderTargetSize;
uniform vec2 RealRenderTargetSize;

in vec2 OutTexCoord;

float CalcGrayscale(vec3 Color)
{
	return 0.213 * Color.r + 0.715 * Color.g + 0.072 * Color.b;
}

float MaxValue(float v1, float v2, float v3, float v4, float v5)
{
	float Max = v1;
	if (v2 > Max)
		Max = v2;
	if (v3 > Max)
		Max = v3;
	if (v4 > Max)
		Max = v4;
	if (v5 > Max)
		Max = v5;
	return Max;
}

float MinValue(float v1, float v2, float v3, float v4, float v5)
{
	float Min = v1;
	if (v2 < Min)
		Min = v2;
	if (v3 < Min)
		Min = v3;
	if (v4 < Min)
		Min = v4;
	if (v5 < Min)
		Min = v5;
	return Min;
}

void main()
{
	vec2 TexCoord = OutTexCoord * vec2(CameraRenderTargetSize / RealRenderTargetSize);
	vec2 Offset = 1.0f / vec2(textureSize(ColorTexture, 0));
	vec4 MColor = texture(ColorTexture, TexCoord);
	vec4 NColor = texture(ColorTexture, TexCoord - vec2(0, Offset.y));
	vec4 SColor = texture(ColorTexture, TexCoord + vec2(0, Offset.y));
	vec4 WColor = texture(ColorTexture, TexCoord - vec2(Offset.x, 0));
	vec4 EColor = texture(ColorTexture, TexCoord + vec2(Offset.x, 0));

	float M = CalcGrayscale(MColor.xyz);
	float N = CalcGrayscale(NColor.xyz);
	float S = CalcGrayscale(SColor.xyz);
	float W = CalcGrayscale(WColor.xyz);
	float E = CalcGrayscale(EColor.xyz);

	float Max = MaxValue(M, N, S, W, E);
	float Min = MinValue(M, N, S, W, E);
	float Contrast = Max - Min;

	if (Max - Min > 0.3f)
	{
		glColor = vec4(1.0f);
	}
	else
	{
		glColor = vec4(0.0f);
	}
	// glColor = vec4(Contrast, Contrast, Contrast, 1.0f);
	// glColor = Color;
}