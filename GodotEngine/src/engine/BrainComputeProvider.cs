using Godot;
using System;

public class BrainComputeProvider
{
    private RenderingDevice rd;
    private Rid shader;
    private Rid pipeline;

    public uint InputCount = 15;
    public uint OutputCount = 5;
    public uint HiddenCount = 8;
    public uint AgentCount;

    public float[] RawOutputs;

    public BrainComputeProvider(uint agentCount)
    {
        AgentCount = agentCount;
        RawOutputs = new float[AgentCount * OutputCount];

        rd = RenderingServer.CreateLocalRenderingDevice();
        
        // Cargar shader compilado
        var shaderFile = ResourceLoader.Load<RDShaderFile>("res://src/engine/BrainCompute.glsl");
        var spirv = shaderFile.GetSpirV();
        shader = rd.ShaderCreateFromSpirV(spirv);
        pipeline = rd.ComputePipelineCreate(shader);
    }

    public void ForwardProcess(float[] inputs, float[] weights, float[] biases)
    {
        // 1. Crear buffers de Bytes
        byte[] inputsBytes = new byte[inputs.Length * 4];
        Buffer.BlockCopy(inputs, 0, inputsBytes, 0, inputsBytes.Length);
        
        byte[] weightsBytes = new byte[weights.Length * 4];
        Buffer.BlockCopy(weights, 0, weightsBytes, 0, weightsBytes.Length);
        
        byte[] biasesBytes = new byte[biases.Length * 4];
        Buffer.BlockCopy(biases, 0, biasesBytes, 0, biasesBytes.Length);
        
        byte[] outputsBytes = new byte[RawOutputs.Length * 4];

        // Params Block (std140 require alineación 16 bytes p/uintegers en Godot, 
        // pero usaremos 4 uint seguidos, son 16 bytes = 1 vec4)
        uint[] paramsData = { InputCount, OutputCount, HiddenCount, AgentCount };
        byte[] paramsBytes = new byte[16];
        Buffer.BlockCopy(paramsData, 0, paramsBytes, 0, paramsBytes.Length);

        // 2. Subir a la GPU
        Rid inputsBuffer = rd.StorageBufferCreate((uint)inputsBytes.Length, inputsBytes);
        Rid weightsBuffer = rd.StorageBufferCreate((uint)weightsBytes.Length, weightsBytes);
        Rid biasesBuffer = rd.StorageBufferCreate((uint)biasesBytes.Length, biasesBytes);
        Rid outputsBuffer = rd.StorageBufferCreate((uint)outputsBytes.Length, outputsBytes);
        Rid paramsUniformBuffer = rd.UniformBufferCreate((uint)paramsBytes.Length, paramsBytes);

        // 3. Crear Uniforms Binding
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

        Rid uniformSet = rd.UniformSetCreate(new Godot.Collections.Array<RDUniform> { uInputs, uWeights, uBiases, uOutputs, uParams }, shader, 0);

        // 4. Despachar
        long computeList = rd.ComputeListBegin();
        rd.ComputeListBindComputePipeline(computeList, pipeline);
        rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        
        // workgroups (bloques de 64 hilos que definimos en glsl)
        uint workgroupsX = (uint)Mathf.CeilToInt(AgentCount / 64.0f);
        rd.ComputeListDispatch(computeList, workgroupsX, 1, 1);
        rd.ComputeListEnd();

        rd.Submit();
        rd.Sync(); // Esperar ejecución

        // 5. Descargar resultados
        byte[] readBytes = rd.BufferGetData(outputsBuffer);
        Buffer.BlockCopy(readBytes, 0, RawOutputs, 0, readBytes.Length);

        // Liberar memoria en GPU explícitamente para no acumular memory leak
        rd.FreeRid(inputsBuffer);
        rd.FreeRid(weightsBuffer);
        rd.FreeRid(biasesBuffer);
        rd.FreeRid(outputsBuffer);
        rd.FreeRid(paramsUniformBuffer);
        rd.FreeRid(uniformSet);
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
