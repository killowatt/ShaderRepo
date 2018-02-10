#define PI 3.14159265359
precision highp float;


// Parameters
float reflectance = 1.0; // Must be between 0 and 1.
float roughness = 1.0; // effects how surface is reflected;
float ior = 1.2; // something like roughness? value between 1 to 3

// FOR DA VINE
varying vec3 outVertex;
varying vec3 outNormal;

// Properties
vec3 vertex = vec3(1.0, 1.0, 1.0); // this is actually view vector, ignore vertex name
vec3 normal = outNormal;
vec3 lightVector = vec3(1.0);
vec3 halfVector = normalize(lightVector + vertex);

// Specular Functions
vec3 D(vec3 H) // GGX/Trowbridge-Reitz
{
  float alpha = roughness * roughness;
	vec3 normalH = normal * H;
	float alphaTerm = (alpha * alpha - 1.0) + 1.0;
	return alpha * alpha / PI * (normalH * normalH) * (alphaTerm * alphaTerm);
}
vec3 G(vec3 L, vec3 V, vec3 H)
{
	float k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
	vec3 firstTerm = normal * L / (normal * L) * (1.0 - k) + k;
	vec3 secondTerm = normal * V / (normal * V) * (1.0 - k) + k;
	return firstTerm * secondTerm;
}
vec3 F(vec3 V, vec3 H, vec3 L)
{
	float f0 = pow((ior - 1.0) / (ior + 1.0), 2.0);
	float u = dot(L, H);
	float F = f0 + (1.0 - f0) * pow(1.0 - u, 5.0); // could replace the power with (-5.55473(v*h)-6.98316(v*h)
	return vec3(F);
}

// Image-Based Lighting

float G1(float v, float k)
{
	return normal * v / (normal * v) * (1.0 - k) + k;
}
float G_Smith(float l, float v, float h)
{
	float k = (Roughness + 1) * (Roughness + 1) / 8;
	return G(l) * G(v);
}
float radicalInverse_VdC(uint bits)
{
	      bits = (bits << 16u) | (bits >> 16u);

                bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);

                bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);

                bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);

                bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);

                return float(bits) * 2.3283064365386963e-10; // / 0x100000000
                // gg copypaste
}
vec2 Hammersley(uint i, uint N)
{
	return vec2(float(i)/float(N), radicalInverse_VdC(i));
}
vec3 ImportanceSampleGGX(vec2 Xi, float Roughness, vec3 N)
{
	float a = roughness * roughness;

	float Phi = 2 * PI * Xi.x;
	float CosTheta = sqrt((1 - Xi.y) / (1 + (a*a - 1) * Xi.y));
	float SinTheta = sqrt(1 - CosTheta * CosTheta);

	vec3 H;
	H.x = SinTheta * cos(Phi);
	H.y = SinTheta * sin(Phi);
	H.z = CosTheta;

	vec3 UpVector = abs(N.z) < 0.999 ? vec3(0, 0, 1) : vec3(1, 0, 0);
	vec3 TangentX = normalize(cross(UpVector, N));
	vec3 TangentY = cross(N, TangentX);
	// Tangent to world space
	return TangentX * H.x + TangentY * H.y + N * H.z;
}
vec3 SpecularIBL(vec3 SpecularColor, float Roughness, vec3 N, vec3 V)
{
	vec3 SpecularLighting = 0;

	const uint NumSamples = 1024; // DEFAULT 1024
	for (uint i = 0; i < NumSamples; i++)
	{
		vec2 Xi = Hammersley(i, NumSamples);
		vec3 H = ImportanceSampleGGX(Xi, Roughness, N);
		vec3 L = 2 * dot(V, H) * H - V;

		float NoV = clamp(dot(N, V), 0.0, 1.0);
		float NoL = clamp(dot(N, L), 0.0, 1.0);
		float NoH = clamp(dot(N, H), 0.0, 1.0);
		float VoH = clmap(dot(V, H), 0.0, 1.0);

		if (NoL > 0)
		{
			vec3 SampleColor = EnvMap.SampleLevel(EnvMapSamplers, L, 0).rgb;

			float G = G_Smith(Roughness, NoV, NoL);
			float Fc = pow(1 - VoH, 5);
			float F = (1 - Fc) * SpecularColor + Fc;

			SpecularLighting += SampleColor * F * G * VoH / (NoH * NoV);
		}
		return SpecularLighting / NumSamples;
	}
}
vec3 PrefilterEnvMap(float Roughness, vec3 R)
{
	vec3 N = R;
	vec3 V = R;

	vec3 PrefilteredColor = 0;

	const uint NumSamples = 1024; // DEFAULT 1024
	for(uint i = 0; i < NumSamples; i++)
	{
		vec2 Xi = Hammersley(i, NumSamples);
		vec3 H = ImportanceSampleGGX(Xi, Roughness, N);
		vec3 L = 2 * dot(V, H) * H - V;

		float NoL = clamp(dot(N, L), 0.0, 1.0);
		if (NoL > 0)
		{
			PrefilteredColor += EnvMap.SampleLevel(EnvMapSampler, L, 0).rgb * NoL;
			TotalWeight += NoL;
		}
	}

	return PrefilteredColor / TotalWeight;
}
vec2 IntegrateBRDF(float Roughness, float NoV)
{
	vec3 V;
	V.x = sqrt(1.0 - NoV * NoV); // sin
	V.y = 0;
	V.z = NoV;

	float A = 0;
	float B = 0;

	const uint NumSamples = 1024; // DEFAULT 1024
	for(uint i = 0; i < NumSamples; i++)
	{
		vec2 Xi = Hammersley(i, NumSamples);
		vec3 H = ImportanceSampleGGX(Xi, Roughness, N);
		vec3 L = 2 * dot(V, H) * H - V;

		float NoL = clamp(L.z, 0.0, 1.0);
		float NoH = clamp(H.z, 0.0, 1.0);
		float VoH = clamp(dot(V, H), 0.0, 1.0);

		if (NoL > 0)
		{
			float G = G_Smith(Roughness, NoV, NoL);

			float G_Vis = G * VoH / (NoH * NoV);
			float Fc = pow(1 - VoH, 5);
			A += (1 - Fc) * G_Vis;
			B += Fc * G_Vis;
		}
	}

			return vec2(A, B) / NumSamples;
}
vec3 ApproximateSpecularIBL(vec3 SpecularColor, float Roughness, vec3 N, vec3 V)
{
	float NoV = clamp(dot(N, V), 0.0, 1.0);
	vec3 R = 2 * dot(V, N) * N - V;

	vec3 PrefilteredColor = PrefilterEnvMap(Roughness, R);
	vec2 EnvBRDF = IntegrateBRDF(Roughness, NoV);

	return PrefilteredColor * (SpecularColor * EnvBRDF.x + EnvBRDF.y);
}

// Final Results
vec3 diffuse() // Lambert
{
	return vec3(reflectance / PI) * dot(normal, lightVector);
}
vec3 specular() // Cook-Torrance
{
	return D(halfVector) * F(vertex, halfVector, lightVector) * G(lightVector, vertex, halfVector) / 4.0 * (normal * lightVector) * (normal * vertex);
}

void main()
{
  vertex = vec3(1.0, 1.0, 1.0);
  normal = outNormal;
  gl_FragColor = vec4(diffuse() * reflectance + specular(), 1.0);
}