#[compute]
#version 450

// Procesamos a los agentes estructurados en bloques de 64 invocaciones
layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

layout(set = 0, binding = 0, std430) readonly buffer InputsBuffer {
    float data[];
} inputs;

layout(set = 0, binding = 1, std430) readonly buffer WeightsBuffer {
    float data[];
} weights;

layout(set = 0, binding = 2, std430) readonly buffer BiasesBuffer {
    float data[];
} biases;

layout(set = 0, binding = 3, std430) writeonly buffer OutputsBuffer {
    float data[];
} outputs;

layout(set = 0, binding = 4, std140) uniform ParamsBlock {
    uint inputCount;
    uint outputCount;
    uint hiddenCount;
    uint agentCount;
} params;

float tanh_fast(float x) {
    float e2x = exp(2.0 * clamp(x, -15.0, 15.0));
    return (e2x - 1.0) / (e2x + 1.0);
}

float relu(float x) {
    return max(0.0, x);
}

void main() {
    uint agentId = gl_GlobalInvocationID.x;
    if (agentId >= params.agentCount) return;

    uint inNodes = params.inputCount;
    uint hidNodes = params.hiddenCount;
    uint outNodes = params.outputCount;

    // Offsets para este agente
    uint W_in_hid_size = inNodes * hidNodes;
    uint W_hid_out_size = hidNodes * outNodes;
    uint agentWeightOffset = agentId * (W_in_hid_size + W_hid_out_size);
    uint agentBiasOffset = agentId * (hidNodes + outNodes);

    // Calcular Hidden Layer
    float hiddenNodes[64]; // max 64 hidden nodes
    for (uint h = 0; h < hidNodes; h++) {
        float sum = 0.0;
        for (uint i = 0; i < inNodes; i++) {
            float w = weights.data[agentWeightOffset + (i * hidNodes + h)];
            float x = inputs.data[agentId * inNodes + i];
            sum += x * w;
        }
        sum += biases.data[agentBiasOffset + h];
        hiddenNodes[h] = relu(sum);
    }

    // Calcular Outputs Layer
    for (uint o = 0; o < outNodes; o++) {
        float sum = 0.0;
        for (uint h = 0; h < hidNodes; h++) {
            float w = weights.data[agentWeightOffset + W_in_hid_size + (h * outNodes + o)];
            float x = hiddenNodes[h];
            sum += x * w;
        }
        sum += biases.data[agentBiasOffset + hidNodes + o];
        outputs.data[agentId * outNodes + o] = tanh_fast(sum);
    }
}
