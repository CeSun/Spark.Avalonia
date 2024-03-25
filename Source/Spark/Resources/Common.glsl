struct PassToFrag
{
	// 片段世界空间坐标
	vec4 Position;
	// 片段世界空间法线
	vec3 Normal;
	// 顶点颜色
	vec3 Color;
	// UV坐标
	vec2 TexCoord;
	// 片段切线空间坐标
	vec3 TangentPosition;
	// 摄像机切线空间坐标
	vec3 CameraTangentPosition;
#ifdef _DIRECTIONLIGHT_
	// 定向光朝向
	vec3 LightTangentDirection;
#endif
#ifdef _POINTLIGHT_
	vec3 LightTangentPosition;
#endif
#ifdef _SPOTLIGHT_
	vec3 LightTangentPosition;
	vec3 LightTangentDirection;
#endif
};


struct LightInfo
{
	vec3 CameraPosition;
	vec3 Color;
#ifdef _DIRECTIONLIGHT_
	vec3 Direction;
#endif

#ifdef _POINTLIGHT_
	vec3 LightPosition;
	float AttenuationFactor;
#endif

#ifdef _SPOTLIGHT_
	vec3 Direction;
	vec3 LightPosition;
	float Distance;
	float InteriorCosine;
	float ExteriorCosine;
#endif
};

vec4 BlinnPhongShading(vec4 BaseColor, vec3 Normal, vec3 FragPosition, LightInfo Light)
{
	// 光源方向
#ifdef _DIRECTIONLIGHT_
	vec3 LightDirection = vec3(-1.0) * Light.Direction;
#else
	vec3 LightDirection = normalize(Light.LightPosition - FragPosition);
#endif
	// 摄像机方向
	vec3 CameraDirection = Light.CameraPosition - FragPosition;
	
	// 漫反射光
	float Diffuse = max(dot(LightDirection, Normal), 0.0);
	vec3 DiffuseLight = Diffuse * Light.Color * BaseColor.xyz;
	// 镜面光
	vec3 HalfVector = normalize(LightDirection + CameraDirection);
#ifndef _SHADERMODEL_LAMBERT_
	float Specular = pow(max(dot(Normal, HalfVector), 0.0), 32.0);
	vec3 SpecularLight = Specular * BaseColor.xyz * Light.Color;
#endif
	float factor = 1.0f;
#ifdef _POINTLIGHT_
	float Distance = length(Light.LightPosition - FragPosition);
	factor = Light.AttenuationFactor / (Distance * Distance);
#endif
#ifdef _SPOTLIGHT_
	float theta = dot(LightDirection, normalize(-1.0 * Light.Direction));
	float Epsilon = Light.InteriorCosine - Light.ExteriorCosine;
	factor = clamp((theta - Light.ExteriorCosine) / Epsilon, 0.0, 1.0);

#endif
#ifdef _SHADERMODEL_BLINNPHONG_
	return vec4((DiffuseLight + SpecularLight) * factor, BaseColor.a);
#endif

#ifdef _SHADERMODEL_LAMBERT_
	return vec4(DiffuseLight * factor, BaseColor.a);
#endif

}