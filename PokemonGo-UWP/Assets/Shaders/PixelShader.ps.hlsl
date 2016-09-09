Texture2D pokemon : register(t0);
SamplerState pokemonSampler : register(s0);

struct PixelShaderInput
{
    float2 tex : TEXCOORD;
};

float4 main(PixelShaderInput input) : SV_TARGET
{
    float4 ret = pokemon.Sample(pokemonSampler, input.tex);
    clip(ret.a < 0.1f ? -1 : 1);
    return ret;
}