using UnityEngine;

namespace Gameplay.UI
{
    public class DistributionPieceUI : MonoBehaviour
    {
        public DistributionPool currentPool;
        public bool isGrabbed;
        public float currentWidth { get; private set; }
        public float currentHeight { get; private set; }
        
        private RectTransform recT;
        private float originalWidth;
        private float originalHeight;

        public virtual void Start()
        {
            recT = GetComponent<RectTransform>();
            var rect = recT.rect;
            originalWidth = rect.width;
            originalHeight = rect.height;
            
            currentWidth = originalWidth;
            currentHeight = originalHeight;
        }


        public virtual void Grab()
        { // this and the following method are used to make the distribution UI work (the one where you can drag and drop UI elements into pools)
            isGrabbed = true;
            transform.parent = UIManager.Instance.workerDistributionPools[0].transform;
            ResetSize();
        }

        public virtual void Release(DistributionPool newPool)
        {
            currentPool.ChangeItem(gameObject, false);
            if (newPool == null)
            {
                currentPool.ChangeItem(gameObject, true);
            }
            else
            {
                newPool.ChangeItem(gameObject, true);
            }

            isGrabbed = false;
        }

        public void Resize(float newWidth)
        {
            float scaleFactor = originalWidth / originalHeight;
            currentHeight = newWidth * scaleFactor;
            currentWidth = newWidth;
            recT.localScale = new Vector3(currentWidth/originalWidth, currentHeight/originalHeight, 1);
        }

        public void ResetSize()
        {
            currentWidth = originalWidth;
            currentHeight = originalHeight;
            recT.localScale = new Vector3(1,1,1);
        }

        public virtual void Update()
        {
            if (isGrabbed)
            {
                transform.position = Vector3.Slerp(transform.position, Input.mousePosition, .5f);
            }
        }
    }
}
