# Parallel Genetic Algorithm
## Table of Contents
    1. GA
    2. Population
    3. SubPopulationProcessor
    4. Input Manager
    5. Conceptual Idea
    5. Additional Notes

### GA
The GA functions largely the same as a normal GA, although it is initalized as a Unity Monobehavior so that it can have two Population Monobehaviors as children. The GA currently uses CHC Crossover

### Population
The population is stored as an object inside of the GA so that it can issue RPC jobs to client nodes from the server node. (Node that Host serves as both client and server). The list of induviduals is interated through on a coroutine and passed as a struct containing the chromosme and index to any subpopulation that is currently inactive. Once all induvidual jobs are asigned it yeild waits in intervals of .1s for the remaining jobs to finish on client nodes, it will also timeout after 30s and continue if there is a connection failure.

### Sub-Population Processor
Any time a client connects to the server a player object is made for it with two scripts, PlayerController is just debug, but SPP(Sub-Population Processor) serves as the container for the evaluation job. An inactive SPP is passed a struct containing the induvidual to evaluate and applies that.

### Input Manager
Reads GA Paramaters and initalizes the GA, sets things into motion. 

### Conceptual Idea
    1. Client Nodes connect to the host, each is given a game object that enrols itself in the sincronized list of all clients (host is considered a client and server). 
    2. The population bing evaluated is then passed out to inactive nodes via client-rpc calls initalzied with their id. 
    3. They evaluate and return the fitness via server-rpc call, this call adds them to the back of the inactive queue, and updates their fitness.
    4. Once Complete handing out tasks the host waits for all fitnesses to be returned to it/timeout.
    5. CHC Crossover and repeat

### Implementation Details
    1. All structs passed through rpcs messages must be an implementation of the INetworkSerializable interface with the method:
```
public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
{
        serializer.SerializeValue(ref indexA);
        serializer.SerializeValue(ref fitness);
        serializer.SerializeValue(ref status);
}
```
        a. Make sure to serial sinc all variables and be very careful with nonstaticly allocated elements
    2. Initally network varriables will have only server write permission, if you want a Trusting P2P Framework use:
```
[ServerRpc(RequireOwnership =false)] //For Server rpc calls
private NetworkVariable<int> netThing = new(1/*Initalized Value*/,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner); //For NetVariables
```

### Aditional Notes
Multithreading currently does not work
Sever Hosting Mode also has issues