using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Random = UnityEngine.Random;

public class Launcher : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
    private string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
    private enum ConnectionPath {host, join};

    private ConnectionPath currentPath;

    [SerializeField] private byte maxPlayersPerRoom = 6;
    [SerializeField] private int roomNameLength = 6;
    [SerializeField] private GameObject controlPanel = null;
    [SerializeField] private GameObject progressLabel = null;
    [SerializeField] private GameObject joinPopUp;
    [SerializeField] private TMP_InputField roomNameInput;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        joinPopUp.SetActive(false);
        
        if (PlayerPrefs.HasKey("RoomName"))
        {
            roomNameInput.text = PlayerPrefs.GetString("RoomName");
        }
    }

    public void Host()
    { // this gets called by the play button, and then does method calls and UI things
        PhotonNetwork.ConnectUsingSettings();
        currentPath = ConnectionPath.host;
        if (PhotonNetwork.IsConnected)
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
        }
    }

    private string CreateRoomName(int nameLength)
    {
        string output = "";
        for (int i = 0; i < nameLength; i++)
        {
            output += characters[Random.Range(0, characters.Length)];
        }
        return output;
    }

    public override void OnConnectedToMaster()
    {
        if (currentPath == ConnectionPath.host)
        {
            PhotonNetwork.CreateRoom(CreateRoomName(roomNameLength), new RoomOptions{MaxPlayers = maxPlayersPerRoom});
        }
        else if(currentPath == ConnectionPath.join)
        {
            PhotonNetwork.JoinRoom(roomNameInput.text);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // PhotonNetwork.RejoinRoom(roomNameInput.text);
    }

    public void Join()
    {
        Debug.Log(roomNameInput.text);
        if (roomNameInput.text.Length == roomNameLength)
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            joinPopUp.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
            currentPath = ConnectionPath.join;
        }
    }

    public void ToggleJoinPopUp()
    {
        progressLabel.SetActive(!progressLabel.activeSelf);
        joinPopUp.SetActive(!joinPopUp.activeSelf);
    }

    public void UpdateRoomNameInput()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        roomNameInput.text = roomNameInput.text.ToUpper();
        PlayerPrefs.SetString("RoomName", roomNameInput.text);
    }

    public override void OnDisconnected(DisconnectCause cause)
    { // if things break this is a fallback so things get reset instead of fully breaking
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        joinPopUp.SetActive(false);
        if (PlayerPrefs.HasKey("RoomName"))
        {
            roomNameInput.text = PlayerPrefs.GetString("RoomName");
        }
    }

    public override void OnJoinedRoom()
    { // this transfers us to a new scene, so players know that the actual room stuff behind the scenes worked
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.LoadLevel("Waiting Room");
        }
    }
}
