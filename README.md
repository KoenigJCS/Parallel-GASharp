# Cuda Genetic Algorithms
## Table of Contents
1. GA
2. Conceptual Idea
3. Setup
4. Additional Notes

### GA
The GA functions largely the same as a normal GA, although an individual is implemented as a struct instead of a class for reasons seen later.
The primary changer is the usage of NVIDA cuda to take advantage of GPU processing which can solve much more fitness equations in parallel than the CPU.
![GPU Diagram](https://docs.nvidia.com/cuda/cuda-c-programming-guide/_images/gpu-devotes-more-transistors-to-data-processing.png)
*This can also be split into multiple files easily, my Visual Studio Instance was giving me trouble on compiling.*

### Conceptual Idea
1. Standard GA Runtime INIT+CHC Crossover Population
2. Memory containing individuals is copied to the GPU
3. Instead of a standard Eval (IF GPU option enum is set), a call is made to the GPU for the eval function
4. Individuals are copied back with updated fitness
5. Algorithm continues

### Implementation Details
This is the EvalGPU Call
```
cudaMemcpy(members_d,members,size,cudaMemcpyHostToDevice);
EvalGPU<<<(options.popSize*2+255)/256, 256>>>(members_d,options.popSize*2);
cudaMemcpy(members,members_d,size,cudaMemcpyDeviceToHost);
```
This copies the data from the members array (The array of Individuals) to the GPU. Note that size is computed above via
```
size_t size = options.popSize*2*sizeof(Individual);
```
**Note that it is very important to treat this like malloc or memcpy** as any internal pointers will not be deep copied, and an array can only be copied easily if it is statically allocated with non-pointer data (such as structs), whereas classes if contained in the array via pointers will not be copied.
Then ```EvalGPU<<<(options.popSize*2+255)/256, 256>>>(members_d,options.popSize*2)``` is called which is a ```__global__``` function and is passed as follows ```<<<numbOfCores,numbOfThreads>>>(standardFuncArgs);```

Execution Usually will look like this

![Parallel Execution Diagram](https://docs.nvidia.com/cuda/cuda-c-programming-guide/_images/heterogeneous-programming.png)

### Setup
1. Download NVIDA CUDA Tools: [https://developer.nvidia.com/cuda-toolkit](https://developer.nvidia.com/cuda-toolkit)
2. Instal
3. Make sure your NVIDIA Drivers are up to date
4. Place CHCGeneticAlgorithm.cu into visual studio, (it should integrate it into the .sln file) otherwise try VSCode and run the compile manually.
5. Run the exe on the command line and give it whatever runtime args you want for the GA *Currently not implemented :(*
6. Check Results in outfile 

### Additional Notes
- NSIGHT COMPUTE is very useful for seeing gpu errors and general debugging.
- This requires an NVIDIA GPU to run.
- ```extern std::mt19937 MyRandom;``` flips out, unsure why


