using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject HostMenu;
    [SerializeField] private GameObject GAMenuRoot;
    [SerializeField] private GameObject ConnectionMenuRoot;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private UnityTransport unityTransport;
    [SerializeField] private Button SetIPButton;
    [SerializeField] private TMP_InputField IPValue;
    // Start is called before the first frame update
    
     /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        IPValue.text=Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].MapToIPv4().ToString();
        GAMenuRoot.SetActive(false);
        HostButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartHost();
            HostMenu.SetActive(false);
            GAMenuRoot.SetActive(true);
        });
        ServerButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartServer();
            HostMenu.SetActive(false);
            GAMenuRoot.SetActive(true);
        });
        ClientButton.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartClient();
            HostMenu.SetActive(false);
            GAMenuRoot.SetActive(false);
        });
        SetIPButton.onClick.AddListener(() => 
        {
            unityTransport.ConnectionData.Address=IPValue.text;
            Debug.Log(IPValue.text);
            ConnectionMenuRoot.SetActive(false);
        });
        //unityTransport.SetRelayServerData
    }

    
}
