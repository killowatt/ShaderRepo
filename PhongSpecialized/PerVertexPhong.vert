#version 430

in vec4 vPosition;
in vec4 vColor;
in vec2 vTexCoord;
in vec3 vNormal;

out vec4 color;
out vec2 texCoord;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

uniform vec4 Ambient;
uniform vec4 Diffuse;
uniform vec4 Specular;
uniform float Shininess;
uniform vec4 LightPosition;

void main()
{
	mat4 ModelView = View * Model;
	vec4 pos = vec4(ModelView * vPosition);
	
	// LIGHT
	vec4 L = normalize(LightPosition - pos);

	// EYE
	vec4 E = normalize(-pos);

	// HALF ANGLE
	vec4 H = normalize(L + E);
	
	// Transform vertex normal into eye coordinates
	vec4 N = normalize(vec4(ModelView * vec4(vNormal.xyz, 0.0)));

	// Compute terms in the illumination equation
	vec4 ambient = Ambient;
	
	float Kd = max(dot(L, N), 0.0);
	vec4 diffuse = Kd * Diffuse;

	float Ks = pow(max(dot(N, H), 0.0), Shininess);
	vec4 specular = Ks * Specular;
	if (dot(L, N) < 0.0)
		specular = vec4(0.0, 0.0, 0.0, 1.0);

	color = ambient + diffuse + specular + vColor;
	color.a = 1.0;
	texCoord = vTexCoord;
	gl_Position = Projection * View * Model * vPosition;
}