using System;
using System.Collections.Generic;
using Gameplay.CardManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Board : MonoBehaviour, IPunObservable
    {
        [SerializeField] private TextMeshProUGUI coinCounter;
        [SerializeField] private Tile[] tiles = new Tile[4];
        [SerializeField] private GameObject coinObject = null;
        [SerializeField] private GameMaster.Job thisJob;
        
        public Transform pieceLocation;
        public int coins;
        public List<Card> artifactHand = new List<Card>();
        public List<GameObject> pieces = new List<GameObject>();
        public PhotonView pv;
        public Participant jobHolder;
        public bool seleneClaimed;
        
        private List<GameObject> coinObjects = new List<GameObject>();

        private void Update()
        {
            for (var i = 0; i < artifactHand.Count; i++)
            {
                if (artifactHand[i] == null)
                {
                    artifactHand.RemoveAt(i);
                }
            }
            
            for (var i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null)
                {
                    pieces.RemoveAt(i);
                }
            }
            
            for (var i = 0; i < coinObjects.Count; i++)
            {
                if (coinObjects[i] == null)
                {
                    coinObjects.RemoveAt(i);
                }
            }
        }

        public void AddCoin(int amount)
        { // adds coins to this board
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, pieceLocation.position + new Vector3(.01f*Random.Range(-(float)coins,(float)coins), coins * .2f, .01f*Random.Range(-(float)coins,(float)coins)),
                    Quaternion.identity));
                coins++;
            }
            coinCounter.text = coins.ToString();
        }

        [PunRPC]
        public void ChangeJobHolder(byte boardIndex, byte newOwnerNumber)
        { // assigns a new owner to the jobboard
            Participant newHolder = GameMaster.Instance.FetchPlayerByNumber(newOwnerNumber);
            pv.TransferOwnership(newHolder.pv.Controller);
            jobHolder = newHolder;
            foreach (var tile in tiles)
            {
                tile.player = newHolder;
            }
            var transform1 = transform;
            transform1.position = newHolder.mySlot.jobLocations[boardIndex].position;
            transform1.rotation = newHolder.mySlot.jobLocations[boardIndex].rotation;

            if (pieces.Count > 0)
            {
                GameObject[] oldPieces = pieces.ToArray();
                foreach (var piece in oldPieces)
                {
                    AddPiece(piece.GetComponent<Piece>().type, false);
                    pieces.Remove(piece);
                    PhotonNetwork.Destroy(piece);
                }
            }

            if (coins > 0)
            {
                GameObject[] oldCoins = coinObjects.ToArray();
                coins = 0;
                AddCoin(oldCoins.Length);
                foreach (var coin in oldCoins)
                {
                    PhotonNetwork.Destroy(coin);
                }
            }

            if (artifactHand.Count > 0)
            {
                Card[] oldArtifacts = artifactHand.ToArray();
                foreach (var art in oldArtifacts)
                {
                    AddACard(art.cardIndex);
                    artifactHand.Remove(art);
                    PhotonNetwork.Destroy(art.gameObject);
                }
            }
        }

        public void RemoveCoins(int amount)
        { // removes coins from this board
            coins -= amount;
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Destroy(coinObjects[coinObjects.Count-1]);
                coinObjects.RemoveAt(coinObjects.Count-1);
            }
            coinCounter.text = coins.ToString();
        }
        
        public void AddPiece(GameMaster.PieceType type, bool setUsed)
        { // ads pieces to this board
            GameObject newPiece = GameMaster.Instance.CreatePiece(type);
            newPiece.transform.position = pieceLocation.position +
                                          new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
            newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
            Piece nPPiece = newPiece.GetComponent<Piece>();
            nPPiece.cam = jobHolder.mySlot.perspective;
            nPPiece.isPrivate = false;
            nPPiece.originBoard = this;
            if (setUsed)
            {
                nPPiece.ToggleUse();
            }
            pieces.Add(newPiece);
        }

        public GameObject LookForPiece(GameMaster.PieceType type, bool careUsed)
        { // looks for pieces matching criteria on this board
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

        public void SeleneClaim(Player newOwner)
        {
            pv.TransferOwnership(newOwner);
            seleneClaimed = true;
            Participant selenePlayer = GameMaster.Instance.FetchPlayerByPlayer(newOwner);
            string targetPlayer = UIManager.Instance.CreateCharPlayerString(selenePlayer);
            pv.RPC("ChangeJobHolder", RpcTarget.All, (int)thisJob, (byte)selenePlayer.playerNumber);
            string jobString = "" + thisJob;
            jobString = jobString.Insert(jobString.IndexOf('r')+1, " ");
            jobString = jobString.Insert(jobString.IndexOf('f')+1, " ");
            Participant[] participants = FindObjectsOfType<Participant>();
            foreach (var part in participants)
            {
                part.pv.RPC("RpcAddEvidence", RpcTarget.Others, targetPlayer + " has claimed the title " + jobString, targetPlayer + " claimed a job in round " + GameMaster.Instance.turnCounter, false, (byte)selenePlayer.playerNumber);
            }
        }
        
        public void DrawACard()
        { // draws a card to this board
            int newCardIndex = GameMaster.Instance.DrawCard(Decklist.Cardtype.Artifact);
            AddACard(newCardIndex);
        }

        private void AddACard(int cardIndex)
        {
            GameObject newCard = Decklist.Instance.CreateCard(Decklist.Cardtype.Artifact, cardIndex);
            newCard.transform.position = pieceLocation.position + new Vector3(.3f*artifactHand.Count,.3f,.2f*artifactHand.Count);
            newCard.transform.rotation = pieceLocation.rotation;
            Card cardPart = newCard.GetComponent<Card>();
            cardPart.hoverLocation = jobHolder.mySlot.hoverLocation;
            cardPart.isPrivate = false;
            artifactHand.Add(cardPart);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(seleneClaimed);
            }
            else
            {
                seleneClaimed = (bool) stream.ReceiveNext();
            }
        }
    }
}
