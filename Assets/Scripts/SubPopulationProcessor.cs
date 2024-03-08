using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public struct ToHostMessage : INetworkSerializable
{
    //public NativeList<Individual> individualList;
    public int[] chromeosomeArray;
    public int indexA;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref indexA);
        serializer.SerializeValue(ref chromeosomeArray);
        //serializer.SerializeValue(ref indexB);
    }
    public ToHostMessage(int[] chromeosomeArray, int indexA)
    {
        this.chromeosomeArray = chromeosomeArray; 
        this.indexA = indexA;
        //this.indexB = indexB;
    }
} 

public struct ToServerMessage : INetworkSerializable
{
    public float fitness;
    public int indexA;
    public int status;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref indexA);
        serializer.SerializeValue(ref fitness);
        serializer.SerializeValue(ref status);
    }
    public ToServerMessage(float fitness = -1, int indexA=-1,int status=-1)
    {
        this.fitness = fitness;
        this.indexA = indexA;
        this.status = status;
    }

}

public class SubPopulationProcessor : NetworkBehaviour
{
    AXnEvaluator evaluator; 

    // Start is called before the first frame update
    void Start()
    {
        evaluator = new AXnEvaluator(-1.28f, 1.27f, 1, 2, 8);
        InputHandler.inst.processors.Add(this);
        Rand r = new Rand(0);
    }
    
    public NetworkVariable<ulong> myID = new(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
            myID.Value = NetworkManager.LocalClientId;
        base.OnNetworkSpawn();
    }

    [ClientRpc]
    public void RunProcessorClientRpc(ToHostMessage toHostMessage,ClientRpcParams clientRpcParams = default)
    {
        float fitness = EasyEval(toHostMessage.chromeosomeArray);
        ToServerMessage toServer = new(fitness,toHostMessage.indexA,1);
        ReadFitnessServerRpc(toServer);
    }

    public float EasyEval(int[] chromosome)
    {
        float fitness=0;
        for(int i =0; i<chromosome.Length;i++)
        {
            if(chromosome[i] ==  1)
            {
                fitness++;
            }
        }
        return fitness;
    }


    [ServerRpc(RequireOwnership =false)]
    public void ReadFitnessServerRpc(ToServerMessage toServerMessage)
    {
        gameObject.GetComponent<SpriteRenderer>().color=Color.green;
        InputHandler.inst.SetFintess(toServerMessage.fitness,toServerMessage.indexA,this);
    }
}
