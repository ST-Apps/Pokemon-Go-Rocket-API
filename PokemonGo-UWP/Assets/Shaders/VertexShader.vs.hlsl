
cbuffer CommonBuffer : register(b0)
{
  matrix view;
  matrix projection;
};

cbuffer ModelBuffer : register(b1)
{
  matrix model;
};

struct VertexShaderInput
{
  float3 pos : POSITION;
  float2 tex : TEXCOORD;
};

struct VertexShaderOutput
{
  float2 tex : TEXCOORD;
  float4 pos : SV_POSITION;
};

VertexShaderOutput main(VertexShaderInput input)
{
  VertexShaderOutput output;
  float4 pos = float4(input.pos, 1.0f);

  // Transform the vertex position into projected space.
  pos = mul(pos, model);
  pos = mul(pos, view);
  pos = mul(pos, projection);
  output.pos = pos;

  // Pass through the color without modification.
  output.tex = input.tex;

  return output;
}