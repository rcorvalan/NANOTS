export class BrainCompute {
  constructor(maxAgents, maxNodes = 16) {
    this.maxAgents = maxAgents;
    this.maxNodes = maxNodes;
    this.device = null;
    this.pipeline = null;

    // Buffers and JS Arrays
    this.weightArray = new Float32Array(maxAgents * maxNodes * maxNodes);
    this.biasArray = new Float32Array(maxAgents * maxNodes);
    this.inputArray = new Float32Array(maxAgents * maxNodes); // We load inputs into first N nodes
    this.outputArray = new Float32Array(maxAgents * maxNodes); // State after activation
  }

  async init() {
    if (!navigator.gpu) {
      throw new Error("WebGPU is not supported in this browser. Please enable it or use a compatible browser.");
    }

    const adapter = await navigator.gpu.requestAdapter();
    if (!adapter) {
      throw new Error("No appropriate GPU adapter found.");
    }
    
    // We request the device, potentially utilizing RTX 5060 if available
    this.device = await adapter.requestDevice();

    const shaderCode = `
      // Data structures
      struct Weights {
        data: array<f32>,
      };
      struct Biases {
        data: array<f32>,
      };
      struct Inputs {
        data: array<f32>,
      };
      struct Outputs {
        data: array<f32>,
      };

      @group(0) @binding(0) var<storage, read> weightsBuffer: Weights;
      @group(0) @binding(1) var<storage, read> biasesBuffer: Biases;
      @group(0) @binding(2) var<storage, read> inputsBuffer: Inputs;
      @group(0) @binding(3) var<storage, read_write> outputsBuffer: Outputs;

      const MAX_NODES: u32 = ${this.maxNodes}u;

      // Activation function (Tanh nativo para evitar Infinity/NaN)
      fn activate(x: f32) -> f32 {
        return tanh(x);
      }

      @compute @workgroup_size(64)
      fn main(@builtin(global_invocation_id) global_id: vec3<u32>) {
        let agentId = global_id.x;
        
        // Prevención de Crash/OOB (Device Loss)
        if (agentId >= 1000u) {
            return;
        }
        
        let offsetW = agentId * MAX_NODES * MAX_NODES;
        let offsetN = agentId * MAX_NODES;

        // Simple single-pass evaluation for directed acyclic/cyclic graphs 
        // We will do 3 discrete passes (like recurrent network unfolding) to allow signal propagation
        var nodeValues: array<f32, ${this.maxNodes}>;
        
        // Init with inputs
        for (var i = 0u; i < MAX_NODES; i++) {
            nodeValues[i] = inputsBuffer.data[offsetN + i];
        }

        // Evaluate 3 times for deep propagation
        for(var step = 0u; step < 3u; step++) {
            var nextValues: array<f32, ${this.maxNodes}>;
            for(var i = 0u; i < MAX_NODES; i++) {
                var sum = biasesBuffer.data[offsetN + i];
                for(var j = 0u; j < MAX_NODES; j++) {
                    let weight = weightsBuffer.data[offsetW + (j * MAX_NODES + i)];
                    sum = sum + weight * nodeValues[j];
                }
                nextValues[i] = activate(sum);
            }
            // Overwrite with input clamping (Input nodes 0-4 shouldn't change)
            for(var i = 0u; i < 5u; i++) {
                nextValues[i] = inputsBuffer.data[offsetN + i];
            }
            nodeValues = nextValues;
        }

        // Write to output
        for (var i = 0u; i < MAX_NODES; i++) {
            outputsBuffer.data[offsetN + i] = nodeValues[i];
        }
      }
    `;

    const shaderModule = this.device.createShaderModule({ code: shaderCode });

    this.pipeline = this.device.createComputePipeline({
      layout: 'auto',
      compute: {
        module: shaderModule,
        entryPoint: 'main',
      },
    });

    // Create Buffers
    const createBuffer = (size, usage) => this.device.createBuffer({ size, usage });

    this.weightsBuffer = createBuffer(this.weightArray.byteLength, GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_DST);
    this.biasesBuffer = createBuffer(this.biasArray.byteLength, GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_DST);
    this.inputsBuffer = createBuffer(this.inputArray.byteLength, GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_DST);
    this.outputsBuffer = createBuffer(this.outputArray.byteLength, GPUBufferUsage.STORAGE | GPUBufferUsage.COPY_SRC);
    this.readBuffer = createBuffer(this.outputArray.byteLength, GPUBufferUsage.MAP_READ | GPUBufferUsage.COPY_DST);

    this.bindGroup = this.device.createBindGroup({
      layout: this.pipeline.getBindGroupLayout(0),
      entries: [
        { binding: 0, resource: { buffer: this.weightsBuffer } },
        { binding: 1, resource: { buffer: this.biasesBuffer } },
        { binding: 2, resource: { buffer: this.inputsBuffer } },
        { binding: 3, resource: { buffer: this.outputsBuffer } },
      ],
    });

    console.log(`[🟢 RTX] BrainCompute Shader (WebGPU) Inicializado para ${this.maxAgents} agentes.`);
  }

  // Upload weights to GPU (called infrequently, mostly during reproduction)
  updateWeightsAndBiases() {
    this.device.queue.writeBuffer(this.weightsBuffer, 0, this.weightArray);
    this.device.queue.writeBuffer(this.biasesBuffer, 0, this.biasArray);
  }

  // Set inputs for an agent. Used in JS main loop before compute pass
  setInputs(agentId, inputVec) {
    const offset = agentId * this.maxNodes;
    for(let i=0; i < inputVec.length; i++) {
      this.inputArray[offset + i] = inputVec[i];
    }
  }

  getOutputs(agentId) {
    const offset = agentId * this.maxNodes;
    // Assume specific output nodes (e.g. 13, 14, 15)
    return {
      moveX: this.outputArray[offset + 13],
      moveY: this.outputArray[offset + 14],
      comm: this.outputArray[offset + 15]
    };
  }

  async evaluate() {
    // 1. Upload input array
    this.device.queue.writeBuffer(this.inputsBuffer, 0, this.inputArray);

    // 2. Encode Commands
    const commandEncoder = this.device.createCommandEncoder();
    const passEncoder = commandEncoder.beginComputePass();
    passEncoder.setPipeline(this.pipeline);
    passEncoder.setBindGroup(0, this.bindGroup);
    
    // Dispatch workgroups (64 threads per group)
    const workgroups = Math.ceil(this.maxAgents / 64);
    passEncoder.dispatchWorkgroups(workgroups);
    passEncoder.end();

    // 3. Copy results to read buffer
    commandEncoder.copyBufferToBuffer(
      this.outputsBuffer, 0,
      this.readBuffer, 0,
      this.readBuffer.size
    );

    // 4. Submit and Wait
    this.device.queue.submit([commandEncoder.finish()]);

    await this.readBuffer.mapAsync(GPUMapMode.READ);
    const arrayBuffer = this.readBuffer.getMappedRange();
    this.outputArray.set(new Float32Array(arrayBuffer));
    this.readBuffer.unmap();
  }
}
