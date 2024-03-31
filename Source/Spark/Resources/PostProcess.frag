out vec4 glColor;

uniform sampler2D ColorTexture;

uniform vec2 CameraRenderTargetSize;
uniform vec2 RealRenderTargetSize;

in vec2 OutTexCoord;

void main()
{
	vec4 Color = texture(ColorTexture, OutTexCoord * vec2(CameraRenderTargetSize / RealRenderTargetSize));

	// HDR 映射 LDR
	Color =vec4( Color.xyz / (Color.xyz + vec3(1.0)) ,Color.w);
	// 伽马矫正
	Color = vec4(pow(Color.xyz, vec3(1.0f/2.2f))  ,Color.w);
	
	glColor = Color;
}