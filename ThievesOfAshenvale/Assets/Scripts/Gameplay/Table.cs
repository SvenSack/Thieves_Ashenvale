using UnityEngine;

namespace Gameplay
{
    public class Table : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == 12)
            {
                SoundManager.Instance.PlayOneShot("event:/Effects/CardDrop", other.GetContact(0).point);
            }
        }
    }
}
