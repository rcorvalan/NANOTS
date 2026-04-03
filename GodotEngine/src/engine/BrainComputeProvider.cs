using Godot;
using System;

public class BrainComputeProvider
{
    private RenderingDevice rd;
    private Rid shader;
    private Rid pipeline;

    public uint InputCount = 21;
    public uint OutputCount = 12;
    public uint HiddenCount = 8;
    public uint AgentCount;

    public float[] RawOutputs;
    
    // RIDs Cacheados
    private Rid inputsBuffer;
    private Rid weightsBuffer;
    private Rid biasesBuffer;
    private Rid outputsBuffer;
    private Rid paramsUniformBuffer;
    private Rid uniformSet;

    public BrainComputeProvider(uint agentCount)
    {
        AgentCount = agentCount;
        RawOutputs = new float[AgentCount * OutputCount];

        // Se usa el GPU RenderingDevice Global en vez del Local para mayor compatibilidad
        rd = RenderingServer.GetRenderingDevice();
        
        try {
            string glslCode = System.IO.File.ReadAllText("src/engine/BrainCompute.glsl");
            var shaderSource = new RDShaderSource();
            shaderSource.SourceCompute = glslCode;
            shaderSource.Language = RenderingDevice.ShaderLanguage.Glsl;

            var spirv = rd.ShaderCompileSpirVFromSource(shaderSource);
            
            if (!string.IsNullOrEmpty(spirv.CompileErrorCompute)) {
                GD.PrintErr("!!! ERROR DE COMPILACION GLSL !!!");
                GD.PrintErr(spirv.CompileErrorCompute);
                return;
            }

            shader = rd.ShaderCreateFromSpirV(spirv);
            pipeline = rd.ComputePipelineCreate(shader);
            
            // Iniciar Memoria y Buffers (Tamaños pre-allocados estáticos)
            uint inSize = (uint)(AgentCount * InputCount * 4);
            uint wSize = (uint)(AgentCount * ((InputCount * HiddenCount) + (HiddenCount * OutputCount)) * 4);
            uint bSize = (uint)(AgentCount * (HiddenCount + OutputCount) * 4);
            uint outSize = (uint)(AgentCount * OutputCount * 4);

            inputsBuffer = rd.StorageBufferCreate(inSize);
            weightsBuffer = rd.StorageBufferCreate(wSize);
            biasesBuffer = rd.StorageBufferCreate(bSize);
            outputsBuffer = rd.StorageBufferCreate(outSize);

            uint[] paramsData = { InputCount, OutputCount, HiddenCount, AgentCount };
            byte[] paramsBytes = new byte[16];
            Buffer.BlockCopy(paramsData, 0, paramsBytes, 0, paramsBytes.Length);
            paramsUniformBuffer = rd.UniformBufferCreate((uint)paramsBytes.Length, paramsBytes);

            var uInputs = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 0 };
            uInputs.AddId(inputsBuffer);
            var uWeights = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 1 };
            uWeights.AddId(weightsBuffer);
            var uBiases = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 2 };
            uBiases.AddId(biasesBuffer);
            var uOutputs = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 3 };
            uOutputs.AddId(outputsBuffer);
            var uParams = new RDUniform { UniformType = RenderingDevice.UniformType.UniformBuffer, Binding = 4 };
            uParams.AddId(paramsUniformBuffer);

            uniformSet = rd.UniformSetCreate(new Godot.Collections.Array<RDUniform> { uInputs, uWeights, uBiases, uOutputs, uParams }, shader, 0);

            GD.Print("Shader Compute SPIR-V Compilado y Buffers cacheados con éxito");
        } catch (Exception e) {
            GD.PrintErr("ERROR AL CARGAR/COMPILAR SHADER: " + e.Message);
        }
    }

    public void ForwardProcess(float[] inputs, float[] weights, float[] biases)
    {
        if (!pipeline.IsValid) return;

        // 1. Actualizar memoria en VRAM (Sobrescribir Buffers Cacheados)
        byte[] inputsBytes = new byte[inputs.Length * 4];
        Buffer.BlockCopy(inputs, 0, inputsBytes, 0, inputsBytes.Length);
        rd.BufferUpdate(inputsBuffer, 0, (uint)inputsBytes.Length, inputsBytes);
        
        byte[] weightsBytes = new byte[weights.Length * 4];
        Buffer.BlockCopy(weights, 0, weightsBytes, 0, weightsBytes.Length);
        rd.BufferUpdate(weightsBuffer, 0, (uint)weightsBytes.Length, weightsBytes);
        
        byte[] biasesBytes = new byte[biases.Length * 4];
        Buffer.BlockCopy(biases, 0, biasesBytes, 0, biasesBytes.Length);
        rd.BufferUpdate(biasesBuffer, 0, (uint)biasesBytes.Length, biasesBytes);

        // 2. Despachar
        long computeList = rd.ComputeListBegin();
        rd.ComputeListBindComputePipeline(computeList, pipeline);
        rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        
        uint workgroupsX = (uint)Mathf.CeilToInt(AgentCount / 64.0f);
        rd.ComputeListDispatch(computeList, workgroupsX, 1, 1);
        rd.ComputeListEnd();

        rd.Submit();
        rd.Sync();

        // 3. Descargar resultados
        byte[] readBytes = rd.BufferGetData(outputsBuffer);
        Buffer.BlockCopy(readBytes, 0, RawOutputs, 0, readBytes.Length);
    }

    public float[] GetOutputsForAgent(int index)
    {
        float[] outNode = new float[OutputCount];
        for (int i = 0; i < OutputCount; i++) {
            outNode[i] = RawOutputs[index * OutputCount + i];
        }
        return outNode;
    }
}
