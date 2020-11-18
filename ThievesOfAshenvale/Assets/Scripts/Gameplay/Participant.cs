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

            for (var i = 0; i < aHand.Count; i++)
            {
                if (aHand[i] == null)
                {
                    aHand.RemoveAt(i);
                }
            }

            for (var i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null)
                {
                    pieces.RemoveAt(i);
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
                    healthObjects[healthObjects.Count-1].GetComponent<Dissolve>().StartDissolve();
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

        public bool PayAndReset()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            bool payNeeded = false;
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    if (piece.pv.IsMine)
                    {
                        if (piece.type != GameMaster.PieceType.Worker)
                        {
                            payNeeded = true;
                        }
                        else
                        {
                            PhotonNetwork.Destroy(piece.pv);
                        }
                    }
                }
            }
            if (payNeeded)
            {
                UIManager.Instance.StartSelection(UIManager.SelectionType.PostTurnPay, null);
            }

            return payNeeded;
        }

        public void LeaderTurnStart()
        {
            switch (GameMaster.Instance.turnCounter)
            {
                case 0:
                case 2:
                case 4:
                    UIManager.Instance.StartSelection(UIManager.SelectionType.JobAssignment, null);
                    goto case 3; 
                case 1:
                case 3:
                    int deadPlayers = 0;
                    foreach (var part in FindObjectsOfType<Participant>())
                    {
                        if (part.isDead)
                        {
                            deadPlayers++;
                        }
                    }
                    int threatAmount = Mathf.CeilToInt((GameMaster.Instance.seatsClaimed-deadPlayers) / 2f);
                    if (GameMaster.Instance.turnCounter == 0)
                    {
                        threatAmount = 0;
                    }

                    if (GameMaster.Instance.roleRevealTurns[2] == GameMaster.Instance.turnCounter &&
                        GameMaster.Instance.turnCounter > 0)
                    {
                        threatAmount++;
                    }

                    for (int i = 0; i < threatAmount; i++)
                    {
                        byte newThreatIndex = (byte) GameMaster.Instance.DrawCard(Decklist.Cardtype.Threat);
                        for (int j = 0; j < GameMaster.Instance.seatsClaimed; j++)
                        {
                            GameMaster.Instance.FetchPlayerByNumber(j).pv
                                .RPC("DrawTCard", RpcTarget.All, newThreatIndex);
                        }

                        GameObject newThreat = PhotonNetwork.Instantiate(GameMaster.Instance.threatObject.name,
                            Vector3.zero, Quaternion.identity);
                        Threat nT = newThreat.GetComponent<Threat>();
                        nT.GetComponent<PhotonView>().RPC("SetThreat", RpcTarget.All, newThreatIndex);
                    }

                    UIManager.Instance.StartSelection(UIManager.SelectionType.WorkerAssignment, null);
                    break;
                case 5:
                    EndTheGame();
                    break;
            }
        }

        public void EndOfTurnEffects()
        {
            StartCoroutine(EndOfTurnEffect());
        }

        private IEnumerator EndOfTurnEffect()
        {
            bool selectionMade = false;
            switch (character)
            {
                case GameMaster.Character.Poisoner:
                    UIManager.Instance.StartSelection(UIManager.SelectionType.Poisoner, null);
                    break;
                case GameMaster.Character.Scion:
                    AddCoin(2);
                    break;
                case GameMaster.Character.Seducer:
                    UIManager.Instance.StartSelection(UIManager.SelectionType.Seducer, null);
                    break;
                case GameMaster.Character.PitFighter:
                    AddPiece(GameMaster.PieceType.Thug, false);
                    break;
                case GameMaster.Character.OldFox:
                    if (GameMaster.Instance.turnCounter % 2 == 0 || GameMaster.Instance.turnCounter == 0)
                    {
                        foreach (var board in GameMaster.Instance.jobBoards)
                        {
                            board.GetComponent<Board>().seleneClaimed = false;
                        }

                        UIManager.Instance.StartSelection(UIManager.SelectionType.SeleneJobClaim, null);
                    }
                    break;
            }

            if (role == GameMaster.Role.Gangster && roleRevealed)
            {
                AddPiece(GameMaster.PieceType.Thug, false);
            }

            if (role == GameMaster.Role.Vigilante && roleRevealed)
            {
                AddPiece(GameMaster.PieceType.Assassin, false);
            }

            if (role == GameMaster.Role.Noble && roleRevealed)
            {
                AddCoin(2);
            }
            
            if (officeCampaign[0] != 0)
            {
                UIManager.Instance.RunForOffice();
            }

            foreach (var tile in FindObjectsOfType<Tile>())
            {
                if (tile.isUsed)
                {
                    tile.ToggleUsed();
                }
            }

            while (UIManager.Instance.isSelecting)
            {
                yield return new WaitForSeconds(0.5f);
            }
            UIManager.Instance.EndTurn(false);
        }

        public bool DealWithThreatPieces()
        {
            if (piecesThreateningMe.Count != 0)
            {
                UIManager.Instance.StartSelection(UIManager.SelectionType.ThreatenedPlayerResolution, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LeaderThreatResolution()
        {
            for (var i = 0; i < tHand.Count; i++)
            {
                var card = tHand[i];
                if (!card.threat.Resolve())
                {
                    for (int e = 0; e < GameMaster.Instance.seatsClaimed; e++)
                    {
                        GameMaster.Instance.FetchPlayerByNumber(e).pv
                            .RPC("RpcRemoveHealth", RpcTarget.All, (byte) 1);
                    }
                }

                card.threat.pv.TransferOwnership(pv.Owner);
                PhotonNetwork.Destroy(card.threat.pv);
                for (int j = 0; j < GameMaster.Instance.seatsClaimed; j++)
                {
                    GameMaster.Instance.FetchPlayerByNumber(j).pv
                        .RPC("RpcRemoveTCard", RpcTarget.All, (byte) i);
                }

            }
            UIManager.Instance.EndTurn(false);
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
                GameMaster.Instance.EndTurn();
            }
            if (role == GameMaster.Role.Leader)
            {
                UIManager.Instance.RevealRole();
                isLeader = true;
            }

            Participant[] participants = FindObjectsOfType<Participant>();
            string playerName = UIManager.Instance.CreateCharPlayerString(this);
            foreach (var part in participants)
            {
                part.pv.RPC("RpcAddEvidence", RpcTarget.OthersBuffered, character + "/n" + Decklist.Instance.characterCards[(int)character].text + "/n Starting assets: " + coins + " coins and " + health + " health", playerName + " basic information", false, playerNumber);
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