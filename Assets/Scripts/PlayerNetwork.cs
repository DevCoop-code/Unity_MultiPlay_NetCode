using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;

    // NetworkVariable은 네트워크에 접속한 사람들이 공유할 변수를 설정
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<MyCustomData> randomCustomData = new NetworkVariable<MyCustomData>(new MyCustomData
    {
        _int = 56,
        _bool = true,
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; " + randomNumber.Value);
        };

        randomCustomData.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; " + newValue._int + " : " + newValue._bool + " : " + newValue.message);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            // Object Spawn
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);

            /*
             * ServerRPC 데이터 확인 코드 (Client --> Server)
             */
            //TestServerRPC(new ServerRpcParams());

            /*
             * ClientRPC 데이터 확인 코드 (Server --> Client)
             */
            // Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 }} : Client ID가 1인 Client에게만 보냄 보낼 Client를 지정할 수 있음 
            //TestClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });

            // Synchronize Data 확인 코드 
            /*
            randomNumber.Value = Random.RandomRange(0, 100);

            randomCustomData.Value = new MyCustomData
            {
                _int = 10,
                _bool = false,
                message = "Hello World!!",
            };
            */
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Destroy(spawnedObjectTransform.gameObject);
        }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    /*
     * RPC
     */
    [ServerRpc]
    private void TestServerRPC(ServerRpcParams serverRpcParams)
    {
        // serverRpcParams.Receive.SenderClientId : Can Figure out which client send u message
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRPC(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRPC ");
    }
}
