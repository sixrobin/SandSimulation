float hash21(float2 input)
{
    return frac(sin(dot(input.xyx, float3(127.1, 311.7, 74.7))) * 43758.5453123);
}
float2 hash22(float2 input)
{
    float a = dot(input.xyx, float3(127.1, 311.7, 74.7));
    float b = dot(input.yxx, float3(269.5, 183.3, 246.1));
    return frac(sin(float2(a, b)) * 43758.5453123);
}
float3 hash23(float2 input)
{
    float a = dot(input.xyx, float3(127.1, 311.7, 74.7));
    float b = dot(input.yxx, float3(269.5, 183.3, 246.1));
    float c = dot(input.xyy, float3(113.5, 271.9, 124.6));
    return frac(sin(float3(a, b, c)) * 43758.5453123);
}

float random(float x)  { return frac(sin(dot(x, 12.9898)) * 43758.5453); }
float random(float2 v) { return frac(sin(dot(v, float2(12.9898, 78.233))) * 43758.5453); }
float random(float3 v) { return frac(sin(dot(v, float3(12.9898, 78.233, 45.5432))) * 43758.5453); }