using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Netcode;
using UnityEngine;

public struct TestStruct : INetworkSerializable
{
    public int test1;
    public bool test2;
    public byte[] testbytes;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref test1);
        serializer.SerializeValue(ref test2);
    }
}

public class PlayerController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    [SerializeField] private float moveSpeed = 1f;
    void Update()
    {
        if(!IsOwner)
            return;
        
        Vector3 moveDir = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
            moveDir.y=5;
        if(Input.GetKey(KeyCode.A))
            moveDir.x=-5;
        if(Input.GetKey(KeyCode.S))
            moveDir.y=-5;
        if(Input.GetKey(KeyCode.D))
            moveDir.x=5;

        if(Input.GetKeyDown(KeyCode.K))
        {
            TestClientRpc();
        }
        if(Input.GetKeyDown(KeyCode.J))
        {
            TestServerRpc();
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            TestStruct test = new();
            int[] l = new int[5];
            for(int i =0;i<5;i++)
                l[i]=i;
            test.testbytes = new byte[l.Length];
            for(int i =0;i<l.Length;i++)
            {
                test.testbytes[i] = (byte) l[i];
            }
            Test2ServerRpc(test);
        }

        transform.position += moveDir*moveSpeed*Time.deltaTime;
    }

    [ClientRpc]
    void TestClientRpc()
    {
        Debug.Log(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership =false)]
    void TestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log(serverRpcParams.Receive.SenderClientId);
    }
    [ServerRpc(RequireOwnership =false)]
    void Test2ServerRpc(TestStruct test,ServerRpcParams serverRpcParams = default)
    {
        Debug.Log((int)test.testbytes[0]);
    }
}
