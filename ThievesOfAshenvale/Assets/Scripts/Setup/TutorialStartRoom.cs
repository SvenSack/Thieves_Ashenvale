using Photon.Pun;
using UnityEngine;

namespace Setup
{
    public class TutorialStartRoom : MonoBehaviour
    {
        public void LoadNewRoom()
        { // this gets called by the start button press, and moves everyone to the game scene
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Tutorial Scene");
        }
    }
}
