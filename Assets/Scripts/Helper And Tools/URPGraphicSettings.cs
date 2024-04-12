using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace CoreCraft.Core
{
    /// <summary>
    /// Enables getting/setting URP graphics settings properties that don't have built-in getters and setters.
    /// </summary>
    public static class URPGraphicSettings
    {
        private static FieldInfo MainLightCastShadows_FieldInfo;
        private static FieldInfo AdditionalLightCastShadows_FieldInfo;
        private static FieldInfo MainLightShadowmapResolution_FieldInfo;
        private static FieldInfo AdditionalLightShadowmapResolution_FieldInfo;
        private static FieldInfo Cascade2Split_FieldInfo;
        private static FieldInfo Cascade4Split_FieldInfo;
        private static FieldInfo SoftShadowsEnabled_FieldInfo;
        private static FieldInfo AdditionalLightsRenderingMode_FieldInfo;
        private static FieldInfo AdditionalLightsCookieResolution_FieldInfo;
        private static FieldInfo AdditionalLightsCookieFormat_FieldInfo;
        private static FieldInfo ReflectionProbeBlending_FieldInfo;
        private static FieldInfo ReflectionProbeBoxProjection_FieldInfo;

        static URPGraphicSettings()
        {
            var pipelineAssetType = typeof(UniversalRenderPipelineAsset);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            MainLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowsSupported", flags);
            AdditionalLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightShadowsSupported", flags);
            MainLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowmapResolution", flags);
            AdditionalLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsShadowmapResolution", flags);
            Cascade2Split_FieldInfo = pipelineAssetType.GetField("m_Cascade2Split", flags);
            Cascade4Split_FieldInfo = pipelineAssetType.GetField("m_Cascade4Split", flags);
            SoftShadowsEnabled_FieldInfo = pipelineAssetType.GetField("m_SoftShadowsSupported", flags);
            AdditionalLightsRenderingMode_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsRenderingMode", flags);
            AdditionalLightsCookieResolution_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsCookieResolution", flags);
            AdditionalLightsCookieFormat_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsCookieFormat", flags);
            ReflectionProbeBlending_FieldInfo = pipelineAssetType.GetField("m_ReflectionProbeBlending", flags);
            ReflectionProbeBoxProjection_FieldInfo = pipelineAssetType.GetField("m_ReflectionProbeBoxProjection", flags);
        }

        /// <summary>
        /// If enabled the main light can be a shadow casting light.
        /// </summary>
        public static bool MainLightCastShadows
        {
            get => (bool)MainLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => MainLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// If enabled shadows will be supported for spot lights.
        /// </summary>
        public static bool AdditionalLightCastShadows
        {
            get => (bool)AdditionalLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => AdditionalLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// Resolution of the main light shadowmap texture. If cascades are enabled, cascades will be packed into an atlas and this setting controls the maximum shadows atlas resolution.
        /// </summary>
        public static ShadowResolution MainLightShadowResolution
        {
            get => (ShadowResolution)MainLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => MainLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// All additional lights are packed into a single shadowmap atlas. This setting controls the atlas size.
        /// </summary>
        public static ShadowResolution AdditionalLightShadowResolution
        {
            get => (ShadowResolution)AdditionalLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => AdditionalLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        public static float Cascade2Split
        {
            get => (float)Cascade2Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => Cascade2Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        public static Vector3 Cascade4Split
        {
            get => (Vector3)Cascade4Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => Cascade4Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        public static bool SoftShadowsEnabled
        {
            get => (bool)SoftShadowsEnabled_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => SoftShadowsEnabled_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// Additional lights support.
        /// </summary>
        public static LightRenderingMode AdditionalLightsRenderingMode
        {
            get => (LightRenderingMode)AdditionalLightsRenderingMode_FieldInfo.GetValue(GraphicsSettings
                .currentRenderPipeline);
            set => AdditionalLightsRenderingMode_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// All additional lights are packed into a single cookie atlas. This setting controls the atlas size.
        /// </summary>
        public static LightCookieResolution AdditionalLightsCookieResolution
        {
            get => (LightCookieResolution)AdditionalLightsCookieResolution_FieldInfo.GetValue(GraphicsSettings
                .currentRenderPipeline);
            set => AdditionalLightsCookieResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// All additional lights are packed into a single cookie atlas. This setting controls the atlas format.
        /// </summary>
        public static LightCookieFormat AdditionalLightsCookieFormat
        {
            get => (LightCookieFormat)AdditionalLightsCookieFormat_FieldInfo.GetValue(GraphicsSettings
                .currentRenderPipeline);
            set => AdditionalLightsCookieFormat_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// If enabled smooth transitions will be created between reflection probes.
        /// </summary>
        public static bool ReflectionProbeBlending
        {
            get => (bool)ReflectionProbeBlending_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => ReflectionProbeBlending_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }

        /// <summary>
        /// If enabled reflections appear based on the object's position within the probe's box, while still using a single probe as the source of the reflection.
        /// </summary>
        public static bool ReflectionProbeBoxProjection
        {
            get => (bool)ReflectionProbeBoxProjection_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
            set => ReflectionProbeBoxProjection_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
        }
    }
}