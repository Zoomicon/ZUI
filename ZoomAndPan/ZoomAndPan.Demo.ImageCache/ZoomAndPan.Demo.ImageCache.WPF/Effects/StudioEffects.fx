//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Brightness, Contrast, ToneMapping
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------
float ToneMapping : register(C0);
float Brightness : register(C1);
float Contrast : register(C2);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------
sampler2D implicitInputSampler : register(S0);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 pixelColor = tex2D(implicitInputSampler, uv);
	
	//contrast
    pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(Contrast, 0)) + 0.5f;
    
    //brightness
    pixelColor.rgb = pixelColor.rgb + Brightness;
	
	//tonemapping
    pixelColor.rgb *= pow(2.0f, ToneMapping);
    
    return pixelColor;
}
