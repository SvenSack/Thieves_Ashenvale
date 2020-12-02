using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class LobbyCode : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyCode;
    
    // Start is called before the first frame update
    void Start()
    {
        lobbyCode.text += PhotonNetwork.CurrentRoom.Name;
    }
}
