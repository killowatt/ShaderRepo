#version 150

in vec4 vColor;
in vec2 texCoord;

out vec4 fColor;

uniform sampler2D tex;

in vec4 pos;
in vec3 normal;
in mat4 ModelView;

uniform vec4 Ambient;
uniform vec4 Diffuse;
uniform vec4 Specular;
uniform float Shininess;
uniform vec4 LightPosition;

void main()
{
	// LIGHT
	vec4 L = normalize(LightPosition - pos);

	// EYE
	vec4 E = normalize(-pos);

	// HALF ANGLE
	vec4 H = normalize(L + E);
	
	// Transform vertex normal into eye coordinates
	vec4 N = normalize(vec4(ModelView * vec4(normal.xyz, 0.0)));

	// Compute terms in the illumination equation
	vec4 ambient = Ambient;
	
	float Kd = max(dot(L, N), 0.0);
	vec4 diffuse = Kd * Diffuse;

	float Ks = pow(max(dot(N, H), 0.0), Shininess);
	vec4 specular = Ks * Specular;
	if (dot(L, N) < 0.0)
		specular = vec4(0.0, 0.0, 0.0, 1.0);

	vec4 FinalColor = ambient + diffuse + specular;
	fColor = texture(tex, texCoord) * FinalColor;
	fColor.a = 1.0;
}