//=============================================================================
// 	[GLOBALS]
//=============================================================================

float4x4 World;
float4x4 Projection;
float4x4 View;
bool IsReflection;
float Opacity;

Texture2D ThumbTexture;
Texture2D DiffuseTexture;
Texture2D MaskTexture;

sampler2D DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
};


sampler2D ThumbSampler = sampler_state
{
	Texture = <ThumbTexture>;
};


sampler2D MaskSampler = sampler_state
{
	Texture = <MaskTexture>;
};


//=============================================================================
//	[STRUCTS]
//=============================================================================

struct VertexPositionTexture
{
    float4 Position : POSITION0;
	float2 UV       : TEXCOORD0;
};

//=============================================================================
// 	[FUNCTIONS]
//=============================================================================

//-----------------------------------------------------------------------------
// Textured Vertex Shader
//-----------------------------------------------------------------------------

VertexPositionTexture TexturedVertexShader(VertexPositionTexture input)
{
    VertexPositionTexture output;

    output.Position = mul(input.Position, World);    
    output.Position = mul(output.Position, View);
	output.Position = mul(output.Position, Projection);
	output.UV       = input.UV;

    return output;
}

float4 TexturedPixelShader(VertexPositionTexture input) : COLOR0
{
	float4 diff  = tex2D(DiffuseSampler, input.UV);
	float4 thumb = tex2D(ThumbSampler, input.UV);
	float4 mask  = tex2D(MaskSampler, input.UV);
	diff.rgb = saturate(diff.rgb + thumb.rgb).bgr;
	diff.a = mask.a;

	if (IsReflection)
		diff.a = (diff.a * input.UV.y) - 0.25;

    return diff;
}

//=============================================================================
//	[TECHNIQUES]
//=============================================================================

technique DefaultEffect
{
    Pass
    {
        VertexShader = compile vs_2_0 TexturedVertexShader();
        PixelShader  = compile ps_2_0 TexturedPixelShader();
    }
}
