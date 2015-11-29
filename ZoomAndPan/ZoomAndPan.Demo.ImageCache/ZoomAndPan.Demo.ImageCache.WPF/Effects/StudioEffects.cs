using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ZoomAndPanSample.Effects
{
    public class StudioEffects
            : ShaderEffect
    {
        #region Dependency Properties

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.

        /// <summary>
        /// The explict input for this pixel shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(StudioEffects), 0);

        /// <summary>
        /// This property is mapped to the ToneMapping variable within the pixel shader. 
        /// </summary>
        public static readonly DependencyProperty ToneMappingProperty = DependencyProperty.Register("ToneMapping", typeof(double), typeof(StudioEffects), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        /// <summary>
        /// This property is mapped to the Brightness variable within the pixel shader. 
        /// </summary>
        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(StudioEffects), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        /// <summary>
        /// This property is mapped to the Contrast variable within the pixel shader. 
        /// </summary>
        public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(StudioEffects), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(2)));

        #endregion

        #region Member Data

        /// <summary>
        /// A refernce to the pixel shader used.
        /// </summary>
        private static readonly PixelShader pixelShader = new PixelShader();

        #endregion

        #region Constructors


        /// <summary>
        /// Creates an instance of the shader from the included pixel shader.
        /// </summary>
        static StudioEffects()
        {
            pixelShader.UriSource = Helpers.MakePackUri("Effects/StudioEffects.ps");
        }

        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public StudioEffects()
        {
            this.PixelShader = pixelShader;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ToneMappingProperty);
            UpdateShaderValue(BrightnessProperty);
            UpdateShaderValue(ContrastProperty);
        }

        #endregion

        /// <summary>
        /// Gets or sets the Input shader sampler.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ToneMapping variable within the shader.
        /// </summary>
        public double ToneMapping
        {
            get { return (double)GetValue(ToneMappingProperty); }
            set { SetValue(ToneMappingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Brightness variable within the shader.
        /// </summary>
        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Contrast variable within the shader.
        /// </summary>
        public double Contrast
        {
            get { return (double)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }
    }
}