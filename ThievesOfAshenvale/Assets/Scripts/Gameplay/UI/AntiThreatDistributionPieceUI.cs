using UnityEngine;

namespace Gameplay.UI
{
    public class AntiThreatDistributionPieceUI : DistributionPieceUI
    {
        [SerializeField] private GameObject jobMarker;
        
        public AntiThreatDistributionPool currntPool;
        public bool isPrivate;
        public Piece representative;

        public override void Start()
        {
            base.Start();
            if (isPrivate)
            {
                jobMarker.SetActive(false);
            }
        }

        public void Release(AntiThreatDistributionPool newPool)
        {
            if (newPool.acceptedPieces == representative.type)
            {
                currntPool.ChangeItem(gameObject, false);
                if (newPool == null)
                {
                    currntPool.ChangeItem(gameObject, true);
                }
                else
                {
                    newPool.ChangeItem(gameObject, true);
                }

                isGrabbed = false;
            }
            else
            {
                currntPool.ChangeItem(gameObject, true);
            }
        }

        public override void Grab()
        {
            isGrabbed = true;
            if (representative.type == GameMaster.PieceType.Assassin)
            {
                transform.parent = UIManager.Instance.antiThreatDistributionPools[0].transform;
            }
            else
            {
                transform.parent = UIManager.Instance.antiThreatDistributionPools[3].transform;
            }
        }
    }
}
