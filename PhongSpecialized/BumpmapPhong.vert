#version 430

in vec4 vPosition;
in vec4 vColor;
in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBitangent;

//out vec4 color;
out vec2 texCoord;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

out vec3 LightDirection_cameraspace;
out vec3 Position_worldspace;
uniform vec3 LightPosition;
out vec3 EyeDirection_cameraspace;

out vec3 LightDirection_tangentspace;
out vec3 EyeDirection_tangentspace;

void main()
{
	vec3 bitangent = cross(vNormal,  vTangent);
	gl_Position = Projection * View * Model * vPosition;
	
	Position_worldspace = (Model * vPosition).xyz;
	
	vec3 vertexPosition_cameraspace = (View * Model * vPosition).xyz;
	EyeDirection_cameraspace = vec3(0, 0, 0) - vertexPosition_cameraspace;
	
	vec3 LightPosition_cameraspace = (View * vec4(LightPosition, 1)).xyz;
	LightDirection_cameraspace = LightPosition_cameraspace + EyeDirection_cameraspace;

	texCoord = vTexCoord;
	
	mat4 ModelView = View * Model;
	mat3 MV3x3 = mat3(ModelView);
	vec3 vertexTangent_cameraspace = MV3x3 * vTangent;
	vec3 vertexBitangent_cameraspace = MV3x3 * bitangent;
	vec3 vertexNormal_cameraspace = MV3x3 * vNormal;
	
	mat3 TBN = transpose(mat3(
		vertexTangent_cameraspace,
		vertexBitangent_cameraspace,
		vertexNormal_cameraspace
	));
	
	LightDirection_tangentspace = TBN * LightDirection_cameraspace;
	EyeDirection_tangentspace = TBN * EyeDirection_cameraspace;
}