# Parallel Genetic Algorithm
## Table of Contents
1. GA
2. Population
3. SubPopulationProcessor
4. Input Manager
5. Conceptual Idea
6. Setup
7. Additional Notes

### GA
The GA functions largely the same as a normal GA, although it is initalized as a Unity Monobehavior so that it can have two Population Monobehaviors as children. The GA currently uses CHC Crossover

### Population
The population is stored as an object inside of the GA so that it can issue RPC jobs to client nodes from the server node. (Node that Host serves as both client and server). The list of induviduals is interated through on a coroutine and passed as a struct containing the chromosme and index to any subpopulation that is currently inactive. Once all induvidual jobs are asigned it yeild waits in intervals of .1s for the remaining jobs to finish on client nodes, it will also timeout after 30s and continue if there is a connection failure.

### Sub-Population Processor
Any time a client connects to the server a player object is made for it with two scripts, PlayerController is just debug, but SPP(Sub-Population Processor) serves as the container for the evaluation job. An inactive SPP is passed a struct containing the induvidual to evaluate and applies that.

### Input Manager
Reads GA Paramaters and initalizes the GA, sets things into motion. 

### Conceptual Idea
1. Client Nodes connect to the host
2. Each client is given a game object that enrols itself in the sincronized list of all clients (host is considered both a client and a server, this means that it will also be in the list of processors and can recive tasks like any other client). 
3. The population bing evaluated is then passed out to inactive nodes via client-rpc calls initalzied with their id. 
4. They evaluate and return the fitness via server-rpc call, this call adds them to the back of the inactive queue, and updates their fitness.
5. Once Complete handing out tasks the host waits for all fitnesses to be returned to it/timeout.
6. CHC Crossover and repeat

PS: Client-rpc calls are generally broadcast to all clients, but can be specified to only go to one if the ID is added in the call
![Diagram of Client-RPC](https://docs-multiplayer.unity3d.com/img//sequence_diagrams/RPCs/ClientRPCs_CertainClients.png?text=LightMode)
```
SubPopulationProcessor waitingProcessor = InputHandler.inst.inactiveProcessors.Dequeue();

ClientRpcParams clientRpcParams = new ClientRpcParams
{
        Send = new ClientRpcSendParams
        {
            TargetClientIds = new ulong[]{waitingProcessor.myID.Value}
        }
};
ToHostMessage temp = new(members[i].chromosome,i);
waitingProcessor.RunProcessorClientRpc(temp,clientRpcParams);
```
![Diagram of Server-RPC](https://docs-multiplayer.unity3d.com/img//sequence_diagrams/RPCs/ServerRPCs_ClientHosts_CalledByClient.png?text=LightMode)
```
//Inside of Sub-Processor Client RPC
ToServerMessage toServer = new(fitness,toHostMessage.indexA,1);
ReadFitnessServerRpc(toServer);
```

### Implementation Details
1. All structs passed through rpcs messages must be an implementation of the INetworkSerializable interface with the method:
```
public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
{
        serializer.SerializeValue(ref staticVarA);
        serializer.SerializeValue(ref staticVarB);
        serializer.SerializeValue(ref staticVarC);
}
    /*a. Make sure to serial sinc all variables and be very careful with nonstaticly allocated elements*/
```
    
2. Initally network varriables will have only server write permission, if you want a Trusting P2P Framework use:
```
[ServerRpc(RequireOwnership =false)] //For Server rpc calls
private NetworkVariable<int> netThing = new(1/*Initalized Value*/,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner); //For NetVariables
```
### Setup
1. Zip and Send/Download Unity Build File to all clients (windows scp is very efficent for this)
2. Boot Up all exe files, can run multiple on one client for effectively threading.
3. Find ip4 address of computer that will be host server (ipconfig in cmd is easiest), and initalize all runtimes with that value.
4. Select one instance as host and all others as client
5. Select GA paramaters
6. Run (Note that nodes will turn green after running if their respective client sucessfully processed at least 1 fitness.)

### Aditional Notes
Multithreading currently does not work
Sever Hosting Mode also has issues
