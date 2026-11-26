using Unity.Netcode;
using UnityEngine;


public class TempMenu : NetworkBehaviour
{
    [SerializeField] private GameObject panel;
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        panel.SetActive(false);
        
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        panel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}