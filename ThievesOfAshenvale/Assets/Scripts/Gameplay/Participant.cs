using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using Gameplay.CardManagement;
using TMPro;
using Random = UnityEngine.Random;

namespace Gameplay
{ 
    public partial class Participant : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private GameObject coinObject = null;
        [SerializeField] private GameObject healthObject = null;
        
        public PhotonView pv;
        public PlayerSlot mySlot;
        public int playerNumber = -1;
        public GameMaster.Character character;
        public GameMaster.Role role;
        public List<Card> aHand = new List<Card>();
        public List<Card> tHand = new List<Card>();
        public int coins = 0;
        public bool hasZeal;
        public List<GameObject> pieces = new List<GameObject>();
        public List<InformationPiece> informationHand = new List<InformationPiece>();
        public bool roleRevealed;
        public bool isLeader;
        public List<ThreatPiece> piecesThreateningMe = new List<ThreatPiece>();
        public bool isDead { get; private set; }
        public int[] officeCampaign = {0, 0, 0};
        public bool[] awaitingTrade = {false, false, false, false, false, false, false};
        
        private int health = 0;
        private TextMeshProUGUI coinCounter = null;
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<GameObject> healthObjects = new List<GameObject>();
        private bool isWaitingForNobleWin;
        private Coroutine nobleWin;
        private List<MoWTradeSecret> outStandingTrades = new List<MoWTradeSecret>();

        private void Start()
        {
            pv = GetComponent<PhotonView>();

            if (pv.IsMine)
            { 
                FindSlot(true);
                GameSetup();
            }
            else
            { 
                FindSlot(false);
            }
        }

        private void Update()
        {
            if (pv.IsMine && coinCounter.text != coins.ToString())
            {
                coinCounter.text = coins.ToString();
            }

            foreach (var card in aHand)
            {
                if (card == null)
                {
                    aHand.Remove(card);
                }
            }
            
            foreach (var piece in pieces)
            {
                if (piece == null)
                {
                    pieces.Remove(piece);
                }
            }

            if (pv.IsMine && GameMaster.Instance.isTesting)
            {
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject newPiece = GameMaster.Instance.CreatePiece((GameMaster.PieceType) i);
                        newPiece.transform.position = mySlot.pieceLocation.position +
                                                      new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
                        newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
                        newPiece.GetComponent<Piece>().cam = mySlot.perspective;
                    }
                }

                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                        GameMaster.Instance.EndTurn(false);
                }
            }
        }

        #region Setup
        // this stuff is used to set up the participant and all game start data
        private void FindSlot(bool claimSlot)
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (Equals(PhotonNetwork.PlayerList[i], pv.Controller))
                    playerNumber = i;
            }

            PhotonView slot = GameMaster.Instance.playerSlots[playerNumber];
            if (claimSlot)
            {
                slot.TransferOwnership(pv.Controller);
            }
            mySlot = slot.GetComponent<PlayerSlot>();
            mySlot.player = this;
            mySlot.Board.SetActive(true);
            foreach (var tile in mySlot.publicTiles)
            {
                tile.player = this;
            }
            GameMaster.Instance.seatsClaimed++;
        }

        private void GameSetup()
        {
            mySlot.perspective.enabled = true;
            coinCounter = mySlot.coinCounter;
            CursorFollower.Instance.playerCam = mySlot.perspective;
            UIManager.Instance.participant = this;
            UIManager.Instance.playerCamera = mySlot.perspective;
            UIManager.Instance.player = pv.Controller;
        }

        #endregion

        #region Interfacing
        // this stuff is what other classes, mostly the tiles and UImanager, use to interface with the values of the class
        public void AddCoin(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, mySlot.coinLocation.position + new Vector3(.01f*Random.Range(-(float)coins,coins), coins * .2f, .01f*Random.Range(-(float)coins,(float)coins)),
                    Quaternion.identity));
                coins++;
            }
            
        }

        public void RemoveCoins(int amount)
        {
            coins -= amount;
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Destroy(coinObjects[coinObjects.Count-1]);
                coinObjects.RemoveAt(coinObjects.Count-1);
            }
        }

        public void DrawACard(Decklist.Cardtype type)
        {
            int newCardIndex = GameMaster.Instance.DrawCard(type);
            GameObject newCard = Decklist.Instance.CreateCard(type, newCardIndex);
            int handSize = aHand.Count;
            newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.4f*handSize,.3f,0);
            newCard.transform.rotation = mySlot.aACardLocation.rotation;
            Card cardPart = newCard.GetComponent<Card>();
            cardPart.hoverLocation = mySlot.hoverLocation;
            aHand.Add(cardPart);
        }

        private void AddHealth(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                healthObjects.Add(PhotonNetwork.Instantiate(healthObject.name, mySlot.healthLocation.position + new Vector3(0.3f*((health+1)/2*Mathf.Pow(-1, health)), .1f, 0),
                    Quaternion.identity));
                health++;
            }
        }

        public void DropOutstandingTS(MoWTradeSecret secret)
        {
            outStandingTrades.Remove(secret);
        }

        public void RemoveHealth(byte amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (health > 0)
                {
                    PhotonNetwork.Destroy(healthObjects[healthObjects.Count-1]);
                    healthObjects.RemoveAt(healthObjects.Count-1);
                }
                health--;
            }

            if (health < 1)
            {
                GameOver(false);
                if (role == GameMaster.Role.Leader)
                {
                    GameMaster.Instance.MakeNewLeader();
                }
                Participant[] participants = FindObjectsOfType<Participant>();
                List<Participant> alivePlayers = new List<Participant>();
                foreach (var p in participants)
                {
                    if (!p.isDead)
                    {
                        alivePlayers.Add(p);
                    }
                }

                if (alivePlayers.Count == 1)
                {
                    alivePlayers[0].EndTheGame();
                }
            }
        }

        public void AddPiece(GameMaster.PieceType type, bool setUsed)
        {
            GameObject newPiece = GameMaster.Instance.CreatePiece(type);
            newPiece.transform.position = mySlot.pieceLocation.position +
                                          new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
            newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
            Piece nPPiece = newPiece.GetComponent<Piece>();
            nPPiece.cam = mySlot.perspective;
            pieces.Add(newPiece);
            if (setUsed)
            {
                nPPiece.ToggleUse();
            }
        }

        public void EndTheGame()
        {
            GameMaster.Instance.mustWait = true;
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("EndGame", RpcTarget.AllBuffered);
            }
        }

        private void DenyNobleWin()
        {
            isWaitingForNobleWin = false;
            StopCoroutine(nobleWin);
            GameOver(false);
        }

        IEnumerator WaitForTradeConfirm(byte amountOfWorkers, byte amountOfThugs, byte amountOfAssassins,
            byte amountPoisoned, List<byte> artifactsIndices, List<byte> actionsIndices, int coinAmount, byte playerIndex)
        {
            while (awaitingTrade[playerIndex])
            {
                yield return new WaitForSeconds(1);
            }
            for (int i = 0; i < amountOfWorkers; i++)
            {
                AddPiece(GameMaster.PieceType.Worker, false);
            }
            for (int i = 0; i < amountOfThugs; i++)
            {
                AddPiece(GameMaster.PieceType.Thug, false);
            }
            for (int i = 0; i < amountOfAssassins; i++)
            {
                AddPiece(GameMaster.PieceType.Assassin, false);
            }
            if (amountPoisoned > 0)
            {
                LookForPiece(GameMaster.PieceType.Assassin, true).GetComponent<Piece>().ActivatePoison();
            }
            foreach (var ind in artifactsIndices)
            {
                RpcHandCard(ind, 3); 
            }
            foreach (var ind in actionsIndices)
            {
                RpcHandCard(ind, 2); 
            }
            AddCoin(coinAmount);
        }

        IEnumerator WaitForNobleWin()
        {
            isWaitingForNobleWin = true;
            yield return new WaitForSeconds(5f);
            if (isWaitingForNobleWin)
            {
                GameOver(true);
            }
        }

        public void GameOver(bool hasWon)
        {
            isDead = true;
            UIManager.Instance.dead = true;
            UIManager.Instance.ResetAfterSelect();
            if (hasWon)
            {
                // TODO add end screen
            }
        }

        #endregion
        
        public GameObject LookForPiece(GameMaster.PieceType type, bool careUsed)
        {
            foreach (var element in pieces)
            {
                Piece piece = element.GetComponent<Piece>();
                if (piece.type == type)
                {
                    if (careUsed && !piece.isUsed)
                    {
                        return element;
                    }
                    else if(!careUsed)
                    {
                        return element;
                    }
                }
            }
            return null;
        }
        
        IEnumerator PrepAndStart()
        {
            yield return new WaitForSeconds(1f);
            UIManager.Instance.UpdateSelectionNames();
            if (PhotonNetwork.IsMasterClient)
            {
                GameMaster.Instance.EndTurn(true);
            }
            if (role == GameMaster.Role.Leader)
            {
                UIManager.Instance.RevealRole();
                isLeader = true;
            }

            if (role == GameMaster.Role.Noble)
            {
                string head = "The Noble is suspicious";
                string[] nobleActions =
                {
                    "They slept with the servant girl",
                    "They made a pact with demons for power",
                    "They killed their father to inherit his wealth",
                    "They embezzle most of the cities taxes",
                    "They are a member of the Thieves Guild",
                    "They once drunkenly gambled away their palace"
                };
                for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                {
                    if (i != playerNumber)
                    {
                        string content = nobleActions[Random.Range(0, 5)];
                        GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcAddEvidence", RpcTarget.Others, content, head, true, playerNumber);
                    }
                }
            }
        }
        

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(coins);
                stream.SendNext(isDead);
                stream.SendNext(isLeader);
            }
            else
            {
                coins = (int)stream.ReceiveNext();
                isDead = (bool) stream.ReceiveNext();
                isLeader = (bool) stream.ReceiveNext();
            }
        }
    }
}

public class MoWTradeSecret
{
    public string content;
    public string header;
    public byte targetedPlayer;
    public byte secondaryPlayer;

    public MoWTradeSecret(string _content, string _header, byte _targetedPlayer, byte _secondaryPlayer)
    {
        content = _content;
        header = _header;
        targetedPlayer = _targetedPlayer;
        secondaryPlayer = _secondaryPlayer;
    }
}