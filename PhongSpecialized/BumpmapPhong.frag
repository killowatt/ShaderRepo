#version 430

in vec4 vColor;
in vec2 texCoord;

out vec4 fColor;

uniform sampler2D tex;

in vec3 Position_worldspace;
in vec3 EyeDirection_cameraspace;
uniform vec3 LightPosition;

//uniform sampler2D DiffuseTex; pretty much just tex
uniform sampler2D NormalTex;
uniform sampler2D SpecularTex;

in vec3 LightDirection_tangentspace;
in vec3 EyeDirection_tangentspace;

void main()
{
	vec3 LightColor = vec3(1, 1, 1);
	float LightPower = 40.0f;
	
	vec3 MaterialDiffuseColor = texture2D(tex, texCoord).rgb; // EDITABLE VALU 3 LINE
    vec3 MaterialAmbientColor = vec3(0.1, 0.1, 0.1) * MaterialDiffuseColor;
	vec3 MaterialSpecularColor = texture2D(SpecularTex, texCoord).rgb * 0.3;
	
	vec3 TextureNormal_tangentspace = normalize(texture2D(NormalTex, vec2(texCoord.x, texCoord.y)).rgb * 2.0 - 1.0); // EDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDIT this line if upside down
	
	float distance = length(LightPosition - Position_worldspace);
	
	vec3 n = TextureNormal_tangentspace;
	vec3 l = normalize(LightDirection_tangentspace);
	
	float cosTheta = clamp(dot(n, l), 0, 1);
	
	vec3 E = normalize(EyeDirection_tangentspace);
	vec3 R = reflect(-l, n);
	
	float cosAlpha = clamp(dot(E, R), 0, 1);
	
	float attenuation = 1.0/distance*distance;
	fColor = vec4(MaterialAmbientColor + (MaterialDiffuseColor * LightColor * cosTheta * attenuation) + (MaterialSpecularColor * LightColor * pow(cosAlpha, LightPower) * attenuation), 1.0);
	//fColor = vec4(MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) + MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance*distance), 1.0);
	
}