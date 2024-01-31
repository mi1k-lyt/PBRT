#version 460 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// material parameters
uniform vec3 albedo;
uniform float metallic;
uniform float roughness;
uniform float ao;
// Fresnel反射率
uniform float refl;
// 透明涂层
uniform float clearCoat;
uniform float clearCoatRoughness;

// IBL
uniform samplerCube irradianceMap;
uniform samplerCube prefilterMap;
uniform sampler2D brdfLUT;

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

uniform vec3 camPos;

const float PI = 3.14159265359;
// ----------------------------------------------------------------------------
float D_GGX(float NoH, float roughness)
{
    float a = roughness * roughness;
    float k = roughness / (1.0 - NoH * NoH + a * a);
    return k * k * (1.0 / PI);
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}


float V_SmithGGXCorrelated(float NoV, float NoL, float roughness) {
    float a2 = roughness * roughness;
    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

float V_SmithGGXCorrelatedFast(float NoV, float NoL, float roughness) {
    float a = roughness;
    float GGXV = NoL * (NoV * (1.0 - a) + a);
    float GGXL = NoV * (NoL * (1.0 - a) + a);
    return 0.5 / (GGXV + GGXL);
}

float V_Kelemen(float LoH) {
    return 0.25 / (LoH * LoH);
}

// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0) // 标量优化 F90 = 1.0
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float F_Schlick(float u, float F0, float F90) 
{
    return F0 + (F90 - F0) * pow(1.0 - u, 5.0);
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}  
// ----------------------------------------------------------------------------




// 兰伯特漫反射
float Fd_Lambert() 
{
    return 1.0 / PI;
}
// 迪士尼漫反射
float Fd_Disney(float NoV, float NoL, float LoH, float roughness)
{
    float F90 = 0.5 + 2.0 * roughness * LoH * LoH;
    float lightScatter = F_Schlick(NoL, 1.0, F90);
    float viewScatter  = F_Schlick(NoV, 1.0, F90);
    return lightScatter * viewScatter * (1.0 / PI);
}

// // 多重散射补偿
// vec3 tint(vec3 albedo)
// {
//     float luminance = dot(vec3(0.3f, 0.6f, 1.0f), albedo);
//     return (luminance > 0.0f) ? albedo : vec3(1.0f/luminance);
// }

// vec3 sheen()

void main()
{		
    vec3 N = normalize(Normal);
    vec3 V = normalize(camPos - WorldPos);
    vec3 R = reflect(-V, N);

    // calculate reflectance at normal incidence; if dia-electric (like plastic) use F0 
    // of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow) 
    vec3 reflt = vec3(refl);
    vec3 F0 = 0.16 * reflt * reflt * (1.0 - metallic) + albedo * metallic; 
    // F0 = mix(F0, albedo, metallic);

    // reflectance equation
    vec3 Lo = vec3(0.0);
    
    
    float NoV = max(dot(N, V), 0.0);

    vec2 brdf = texture(brdfLUT, vec2(NoV, roughness)).rg;

    vec3 energyCompensation = 1.0 + F0 * (1.0 / brdf.y - 1.0);

    for(int i = 0; i < 1; ++i) 
    {
        // calculate per-light radiance
        vec3 L = normalize(lightPositions[i] - WorldPos);
        vec3 H = normalize(V + L);
        float distance = length(lightPositions[i] - WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lightColors[i] * attenuation;

        float NoH = max(dot(N, H), 0.0);
        float NoL = max(dot(N, L), 0.0);
        float LoH = max(dot(L, H), 0.0);

        // Cook-Torrance BRDF
        float NDF  = D_GGX(NoH, roughness);
        float Visi = V_SmithGGXCorrelatedFast(NoV, NoL, roughness);
        vec3  F    = fresnelSchlick(LoH, F0);

        vec3  Fr   = (NDF * Visi) * F;
        //多重散射 补偿BRDF
        //Fr *= (0.001 * energyCompensation);

        vec3  diffuseColor = (1.0 - metallic) * albedo;
        //vec3  Fd = Fd_Lambert() * diffuseColor;
        vec3  Fd = Fd_Disney(NoV, NoL, LoH, roughness) * diffuseColor ;
        
        
        // kS is equal to Fresnel
        vec3 kS = F;
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS;
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        
        

        //透明涂层BRDF
        float Dc = D_GGX(NoH, clearCoatRoughness);
        float Vc = V_Kelemen(LoH);
        float Fc = F_Schlick(LoH, 0.04, 1.0) * clearCoat;
        float Frc = (Dc * Vc) * Fc;
        
        vec3 BRDF = ((Fd + Fr * (1.0 - Fc)) * (1.0 - Fc) + Frc);
        //vec3 BRDF = (kD * Fd + Fr);

        // add to outgoing radiance Lo
        Lo += (BRDF) * radiance * NoL;  // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
    }   
    
    // ambient lighting
    vec3 F = fresnelSchlickRoughness(NoV, F0, roughness);
    vec3 kS = F;
    vec3 kD = 1.0 - kS;

    
     
    vec3 irradiance = texture(irradianceMap, N).rgb;
    vec3 diffuse = irradiance * albedo * (1.0 - metallic); 

    // sample both the pre-filter map and the BRDF lut and combine them together as per the Split-Sum approximation to get the IBL specular part.
    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilteredColor = textureLod(prefilterMap, R,  roughness * MAX_REFLECTION_LOD).rgb;    
    
    vec3 specular = prefilteredColor * (F * brdf.x + brdf.y);

    vec3 ambient = (kD * diffuse + specular) * ao;
    //vec3 ambient = vec3(0.05) * albedo * ao;

    vec3 color = ambient + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));
    // gamma correct
    color = pow(color, vec3(1.0/2.2)); 

    FragColor = vec4(0.0, 1.0, 0.0, 1.0);//vec4(color, 1.0);
}