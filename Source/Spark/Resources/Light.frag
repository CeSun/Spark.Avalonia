#include <Common.glsl>
out vec4 glColor;


in PassToFrag passToFrag;

uniform LightInfo Light;
uniform	sampler2D BaseColorTexture;
uniform float HasBaseColor;
uniform	sampler2D NormalTexture;
uniform float HasNormal;

void main()
{
	vec4 BaseColor;
	vec3 Normal = texture(NormalTexture, passToFrag.TexCoord).xyz;
	// 有无颜色贴图
	if (HasBaseColor > 0.0)
		BaseColor = texture(BaseColorTexture, passToFrag.TexCoord);
	else
		BaseColor = vec4(passToFrag.Color, 1.0f);
	// 有无法线贴图
	if (HasNormal> 0.0)
		Normal = normalize(Normal * 2.0 - 1.0);
	else
		Normal = passToFrag.Normal;
	// 绘制背面，反转法线
	if (gl_FrontFacing == false)
		Normal = -1.0 * Normal;

#ifdef _BLENDMODE_MASKED_
	if (BaseColor.a <= 0.1)
		discard;
#endif
	LightInfo tmp = Light;

	tmp.CameraPosition = passToFrag.CameraTangentPosition;
#ifdef _DIRECTIONLIGHT_
	// 定向光朝向
	tmp.Direction = passToFrag.LightTangentDirection;
#endif
#ifdef _POINTLIGHT_
	tmp.LightPosition = passToFrag.LightTangentPosition;
#endif

#ifdef _SPOTLIGHT_
	tmp.LightPosition = passToFrag.LightTangentPosition;
	tmp.Direction = passToFrag.LightTangentDirection;
#endif

	glColor = BlinnPhongShading(BaseColor, Normal, passToFrag.TangentPosition ,tmp);
}