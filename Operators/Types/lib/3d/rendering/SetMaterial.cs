using System;
using SharpDX.Direct3D11;
using T3.Core.DataTypes;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Rendering;
using T3.Core.Resource;
using Buffer = SharpDX.Direct3D11.Buffer;
using Utilities = T3.Core.Utils.Utilities;
using Vector4 = System.Numerics.Vector4;


namespace T3.Operators.Types.Id_0ed2bee3_641f_4b08_8685_df1506e9af3c
{
    public class SetMaterial : Instance<SetMaterial>
    {
        [Output(Guid = "d80e1028-a48d-4171-8c8c-e6856bd2143d")]
        public readonly Slot<Command> Output = new Slot<Command>();

        public SetMaterial()
        {
            Output.UpdateAction = Update;
        }

        private Buffer _parameterBuffer = null;

        private void Update(EvaluationContext context)
        {
            // Parameters
            var parameterBufferContent = new PbrMaterialParams
                                             {
                                                 BaseColor = BaseColor.GetValue(context),
                                                 EmissiveColor = EmissiveColor.GetValue(context),
                                                 Roughness = Roughness.GetValue(context),
                                                 Specular = Specular.GetValue(context),
                                                 Metal = Metal.GetValue(context)
                                             };

            ResourceManager.SetupConstBuffer(parameterBufferContent, ref _parameterBuffer);
            var device = ResourceManager.Device;
            
            // Albedo
            var prevAlbedoColorMap = context.PbrMaterialTextures.AlbedoColorMap;
            Utilities.Dispose(ref _baseColorMapSrv);
            var baseTex = BaseColorMap.GetValue(context) ?? PbrContextSettings.WhitePixelTexture;
            _baseColorMapSrv = TryToCreate(device, baseTex, prevAlbedoColorMap, "albedo");
            
            context.PbrMaterialTextures.AlbedoColorMap = _baseColorMapSrv;

            // Normal
            var prevNormalMap = context.PbrMaterialTextures.NormalMap;
            Utilities.Dispose(ref _normalMapSrv);
            var normalTex = NormalMap.GetValue(context) ?? PbrContextSettings.NormalFallbackTexture;
            _normalMapSrv = TryToCreate(device, normalTex, prevNormalMap, "normal");
            
            context.PbrMaterialTextures.NormalMap = _normalMapSrv;

            // Roughness
            var prevRoughnessSpecularMetallicOcclusionMap = context.PbrMaterialTextures.RoughnessSpecularMetallicOcclusionMap;
            Utilities.Dispose(ref _rsmoMapSrv);
            var roughnessTex = RoughnessSpecularMetallicOcclusionMap.GetValue(context) ?? PbrContextSettings.RsmoFallbackTexture;
            _rsmoMapSrv = TryToCreate(device, roughnessTex, prevRoughnessSpecularMetallicOcclusionMap, "Roughness");

            context.PbrMaterialTextures.RoughnessSpecularMetallicOcclusionMap = _rsmoMapSrv;

            // Emissive
            var prevEmissiveColorMap = context.PbrMaterialTextures.EmissiveColorMap;
            Utilities.Dispose(ref _emissiveColorMapSrv);
            var emissiveTex = EmissiveColorMap.GetValue(context) ?? PbrContextSettings.WhitePixelTexture;
            _emissiveColorMapSrv = TryToCreate(device, emissiveTex, prevRoughnessSpecularMetallicOcclusionMap, "Emissive");

            context.PbrMaterialTextures.EmissiveColorMap = _emissiveColorMapSrv;

            var previousParameters = context.PbrMaterialParams;
            context.PbrMaterialParams = _parameterBuffer;
            
            SubTree.GetValue(context);
            
            context.PbrMaterialParams = previousParameters;
            context.PbrMaterialTextures.AlbedoColorMap = prevAlbedoColorMap;
            context.PbrMaterialTextures.NormalMap = prevNormalMap;
            context.PbrMaterialTextures.RoughnessSpecularMetallicOcclusionMap = prevRoughnessSpecularMetallicOcclusionMap;
            context.PbrMaterialTextures.EmissiveColorMap = prevEmissiveColorMap;
        }

        private ShaderResourceView _baseColorMapSrv;
        private ShaderResourceView _rsmoMapSrv;
        private ShaderResourceView _normalMapSrv;
        private ShaderResourceView _emissiveColorMapSrv;


        private ShaderResourceView TryToCreate(Device device, Resource tex, ShaderResourceView previous, string name)
        {
            try
            {
                var srv = new ShaderResourceView(device, tex);
                return srv;
            }
            catch (Exception e)
            {
                Log.Warning($"Failed to create SRV for {name} texture {e.Message}", this);
                return previous;
            }
        }
        
        
        [Input(Guid = "2a585a23-b60c-4c8b-8cfa-9ab2a8b04c7a")]
        public readonly InputSlot<Command> SubTree = new InputSlot<Command>();

        [Input(Guid = "9FF5ADE2-CFA7-4616-AD89-356F3248E04F")]
        public readonly InputSlot<Vector4> BaseColor = new InputSlot<Vector4>();

        [Input(Guid = "0EB51DF1-570A-4AC6-BAE6-5E03D6E66CEB")]
        public readonly InputSlot<Texture2D> BaseColorMap = new InputSlot<Texture2D>();

        [Input(Guid = "2C91C306-1815-4B22-A70F-746232F024D7")]
        public readonly InputSlot<Vector4> EmissiveColor = new InputSlot<Vector4>();

        [Input(Guid = "6D859756-0243-42C5-973D-6DE2DCDC5609")]
        public readonly InputSlot<Texture2D> EmissiveColorMap = new InputSlot<Texture2D>();
        
        [Input(Guid = "9D5CA726-28B0-4F3D-A5AA-F0AE3E2043A9")]
        public readonly InputSlot<float> Specular = new InputSlot<float>();

        [Input(Guid = "E14DCC43-7C18-4ED7-AD39-DFEAB10E3D33")]
        public readonly InputSlot<float> Roughness = new InputSlot<float>();

        [Input(Guid = "108FF533-F205-4989-B894-ACF48E3CC1DB")]
        public readonly InputSlot<float> Metal = new InputSlot<float>();

        [Input(Guid = "600BBC24-6B3A-4AC4-9CEB-702E71C839E9")]
        public readonly InputSlot<Texture2D> NormalMap = new InputSlot<Texture2D>();

        [Input(Guid = "C8003FBD-C6CE-440C-9F1F-6B15B5EE5274")]
        public readonly InputSlot<Texture2D> RoughnessSpecularMetallicOcclusionMap = new InputSlot<Texture2D>();

    }
}