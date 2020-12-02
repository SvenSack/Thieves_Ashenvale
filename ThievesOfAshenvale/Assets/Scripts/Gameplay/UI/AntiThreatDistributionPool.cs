using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.UI
{
    public class AntiThreatDistributionPool : MonoBehaviour
    {
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        [SerializeField] private GameObject populationPiecePrefab;
        
        public List<AntiThreatDistributionPieceUI> objectsHeld = new List<AntiThreatDistributionPieceUI>();
        public bool isFlex;
        public GameMaster.PieceType acceptedPieces;
        
        private float originalRowSize;
        private float originalColumSize;
        private int originalColumns;
        private float width;
        private float height;
        private bool flaggedForAdjustment;
        private float rowSize;
        private float columnSize;
        private Vector2 firstPostion;
    
        // this class is basically a copy paste of the normal distributionpools, I wanted to make it a child, but it had too little in common to be feasible that way (which sounds
        // like I planned poorly when writing those...
        void Start()
        {
            var rect = GetComponent<RectTransform>().rect;
            width = rect.width;
            height = rect.height;
            originalColumSize = columnSize;
            rowSize = (height*.9f) / rows;
            columnSize = (width*.9f) / columns;
            originalRowSize = rowSize;
            originalColumns = columns;
            firstPostion = new Vector2(-width/2+columnSize/2, height/2-rowSize/2);
        }

        private void Update()
        {
            if (flaggedForAdjustment)
            {
                AdjustPositions();
                flaggedForAdjustment = false;
            }
        }

        public void ChangeItem(GameObject item, bool isAdded)
        {
            AntiThreatDistributionPieceUI tapUI = item.GetComponent<AntiThreatDistributionPieceUI>();
            if (isAdded)
            {
                objectsHeld.Add(tapUI);
                item.transform.parent = transform;
                tapUI.currntPool = this;
            }
            else
            {
                objectsHeld.Remove(tapUI);
            }
            
            if (isFlex && objectsHeld.Count > 0)
            {
                if (objectsHeld.Count > columns * rows)
                {
                    if (objectsHeld[0].currentHeight * rows + 1 < height)
                    {
                        rows++;
                        rowSize = (height*.9f) / rows;
                    }
                    else
                    {
                        columns++;
                        columnSize = (width*.9f) / columns;
                    }
                    firstPostion = new Vector2(-width/2+columnSize/2, height/2-rowSize/2);
                }
                
                if (width < objectsHeld[0].currentWidth * columns)
                {
                    float betterWidth = width / columns;
                    foreach (var obj in objectsHeld)
                    {
                        obj.Resize(betterWidth);
                    }
                    firstPostion = new Vector2(-width/2+columnSize/2, height/2-rowSize/2);
                }
                
            }
            else if(isFlex)
            {
                tapUI.ResetSize();
                columnSize = originalColumSize;
                columns = originalColumns;
                rowSize = originalRowSize;
                firstPostion = new Vector2(-width/2+columnSize/2, height/2-rowSize/2);
            }

            flaggedForAdjustment = true;
        }

        public void DropPool()
        {
            foreach (var obj in objectsHeld)
            {
                Destroy(obj.gameObject);
            }
            objectsHeld = new List<AntiThreatDistributionPieceUI>();
        }
        
        private void AdjustPositions()
        {
            for (int i = 0; i < objectsHeld.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)columns);
                int column = i - columns*row;
                objectsHeld[i].transform.position = transform.position + new Vector3(firstPostion.x + columnSize*column, firstPostion.y - rowSize*row, -.1f);
            }
        }

        public void PopulatePool()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                if (piece.pv.IsMine)
                {
                    GameObject inst = null;
                    if (piece.type == acceptedPieces && !piece.isUsed)
                    {
                        Debug.LogAssertion("found one !");
                        inst = Instantiate(populationPiecePrefab, transform);
                        AntiThreatDistributionPieceUI tapUI = inst.GetComponent<AntiThreatDistributionPieceUI>();
                        tapUI.currntPool = this;
                        tapUI.representative = piece;
                        tapUI.isPrivate = piece.isPrivate;
                        objectsHeld.Add(tapUI);
                    }
                }
            }

            if (!flaggedForAdjustment)
            {
                flaggedForAdjustment = true;
            }
        }
    }
}
