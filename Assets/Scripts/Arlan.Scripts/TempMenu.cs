using Unity.Netcode;
using UnityEngine;


public class TempMenu : NetworkBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform hostSpawnPoint;
    [SerializeField] private Transform clientSpawnPoint;
    public void StartClient()
    {
        
        NetworkManager.Singleton.StartClient();
        panel.SetActive(false);
        
    }
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
        NetworkManager.Singleton.StartHost();
        panel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        if(request.ClientNetworkId == NetworkManager.ServerClientId) response.Position = hostSpawnPoint.position;
        else response.Position = clientSpawnPoint.position;
        Debug.Log($"Client ID: {request.ClientNetworkId}, spawn: {response.Position}");
    }
}