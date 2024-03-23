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
	// 摄像机世界空间坐标
	vec3 CameraPosition;
	// 光源颜色
	vec3 LightColor;
	// 片段切线空间坐标
	vec3 TangentPosition;
	// 摄像机切线空间坐标
	vec3 CameraTangentPosition;
	// 间接光强度
	float IndirectLightStrength;
#ifdef _DIRECTIONLIGHT_
	// 定向光朝向
	vec3 LightTangentDirection;
#endif
#ifdef _POINTLIGHT_
	vec3 LightPosition;
	vec3 LightTangentPosition;
	float AttenuationFactor;
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
};

vec4 BlinnPhongShading(vec4 BaseColor, vec3 Normal, PassToFrag passToFrag)
{
	// 光源方向
#ifdef _DIRECTIONLIGHT_
	vec3 LightDirection = vec3(-1.0) * passToFrag.LightTangentDirection;
#else

	vec3 LightDirection = normalize(passToFrag.LightTangentPosition - passToFrag.TangentPosition);
#endif
	// 摄像机方向
	vec3 CameraDirection = passToFrag.CameraTangentPosition - passToFrag.TangentPosition;
	
	// 漫反射光
	float Diffuse = max(dot(LightDirection, Normal), 0.0);
	vec3 DiffuseLight = Diffuse * passToFrag.LightColor * BaseColor.xyz;
	// 镜面光
	vec3 HalfVector = normalize(LightDirection + CameraDirection);
	float Specular = pow(max(dot(Normal, HalfVector), 0.0), 32.0);
	vec3 SpecularLight = Specular * BaseColor.xyz * passToFrag.LightColor;
	float factor = 1.0f;
#ifdef _POINTLIGHT_
	float Distance = length(passToFrag.LightTangentPosition - passToFrag.TangentPosition);
	factor =   passToFrag.AttenuationFactor / (Distance * Distance);

#endif
	return vec4((DiffuseLight + SpecularLight) * factor, BaseColor.a);
}
