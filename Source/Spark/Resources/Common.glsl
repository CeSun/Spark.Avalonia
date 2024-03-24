struct PassToFrag
{
	// Ƭ������ռ�����
	vec4 Position;
	// Ƭ������ռ䷨��
	vec3 Normal;
	// ������ɫ
	vec3 Color;
	// UV����
	vec2 TexCoord;
	// ���������ռ�����
	vec3 CameraPosition;
	// ��Դ��ɫ
	vec3 LightColor;
	// Ƭ�����߿ռ�����
	vec3 TangentPosition;
	// ��������߿ռ�����
	vec3 CameraTangentPosition;
	// ��ӹ�ǿ��
	float IndirectLightStrength;
#ifdef _DIRECTIONLIGHT_
	// ����⳯��
	vec3 LightTangentDirection;
#endif
#ifdef _POINTLIGHT_
	vec3 LightPosition;
	vec3 LightTangentPosition;
	float AttenuationFactor;
#endif
#ifdef _SPOTLIGHT_
	vec3 LightPosition;
	vec3 LightTangentPosition;
	float Distance;
	float InteriorCosine;
	float ExteriorCosine;
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

vec4 BlinnPhongShading(vec4 BaseColor, vec3 Normal, PassToFrag passToFrag)
{
	// ��Դ����
#ifdef _DIRECTIONLIGHT_
	vec3 LightDirection = vec3(-1.0) * passToFrag.LightTangentDirection;
#else
	vec3 LightDirection = normalize(passToFrag.LightTangentPosition - passToFrag.TangentPosition);
#endif
	// ���������
	vec3 CameraDirection = passToFrag.CameraTangentPosition - passToFrag.TangentPosition;
	
	// �������
	float Diffuse = max(dot(LightDirection, Normal), 0.0);
	vec3 DiffuseLight = Diffuse * passToFrag.LightColor * BaseColor.xyz;
	// �����
	vec3 HalfVector = normalize(LightDirection + CameraDirection);
#ifndef _SHADERMODEL_LAMBERT_
	float Specular = pow(max(dot(Normal, HalfVector), 0.0), 32.0);
	vec3 SpecularLight = Specular * BaseColor.xyz * passToFrag.LightColor;
#endif
	float factor = 1.0f;
#ifdef _POINTLIGHT_
	float Distance = length(passToFrag.LightTangentPosition - passToFrag.TangentPosition);
	factor = passToFrag.AttenuationFactor / (Distance * Distance);
#endif
#ifdef _SPOTLIGHT_
	float theta = dot(LightDirection, normalize(-1.0 * passToFrag.LightTangentDirection));
	float Epsilon = passToFrag.InteriorCosine - passToFrag.ExteriorCosine;
	factor = clamp((theta - passToFrag.ExteriorCosine) / Epsilon, 0.0, 1.0);

#endif
#ifdef _SHADERMODEL_BLINNPHONG_
	return vec4((DiffuseLight + SpecularLight) * factor, BaseColor.a);
#endif

#ifdef _SHADERMODEL_LAMBERT_
	return vec4(DiffuseLight * factor, BaseColor.a);
#endif

}