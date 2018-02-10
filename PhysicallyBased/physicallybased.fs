#version 430
#define PI 3.14159265359

// Parameters
float reflectance = 0.5; // Must be between 0 and 1.
float roughness = 0.5; // effects how surface is reflected;
float ior = 1.2; // something like roughness? value between 1 to 3

// Properties
vec3 vertex;
vec3 normal;
vec3 halfVector = normalize(L + V);
vec3 lightVector = vec3(1.0, 1.0, 1.0);

// Specular Functions
vec3 D(vec3 H) // GGX/Trowbridge-Reitz
{
	float alpha = roughness * roughness;
	vec3 normalH = normal * H
	vec3 alphaTerm = (alpha * alpha - 1) + 1;
	return PI * (normalH * normalH) * (alphaTerm * alphaTerm);
}
vec3 G(vec3 L, vec3 V, vec3 H)
{
	float k = (roughness + 1) * (roughness + 1);
	vec3 firstTerm = normal * L / (normal * L) * (1 - k) + k;
	vec3 secondTerm = normal * V / (normal * V) * (1 - k) + k;
	return firstTerm * secondTerm;
}
vec3 F(vec3 V, vec3 H)
{
	float f0 = pow((ior - 1) / (ior + 1), 2);
	float u = dot(L, H);
	float F = f0 + (1 - f0) * pow(1 - u, 5); // could replace the power with (-5.55473(v*h)-6.98316(v*h)
	return vec3(F);
}

// Final Results
vec3 diffuse() // Lambert
{
	return vec3(reflectance / PI);
}
vec3 specular() // Cook-Torrance
{
	return D(halfVector) * F(vertex, halfVector) * G(lightVector, vertex, halfVector) / 4 * (normal * lightVector) * (normal * vertex);
}