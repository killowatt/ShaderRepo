#version 430

in vec4 vPosition;
in vec4 vColor;
in vec2 vTexCoord;
in vec3 vNormal;

//out vec4 color;
out vec2 texCoord;

uniform mat4 Model;
uniform mat4 View;
uniform mat4 Projection;

out vec4 pos;
out vec3 normal;
out mat4 ModelView;

void main()
{
	ModelView = View * Model;
	pos = vec4(ModelView * vPosition);
	normal = vNormal;

	texCoord = vTexCoord;
	gl_Position = Projection * View * Model * vPosition;
}