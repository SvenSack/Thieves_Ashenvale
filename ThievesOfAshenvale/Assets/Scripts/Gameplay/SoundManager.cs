using System;
using UnityEngine;

namespace Gameplay
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }


        public void PlayOneShot(string path)
        {
            // Debug.Log("playing sound at " + path);
            FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
        }
        
        public void PlayOneShot(string path, Vector3 position)
        {
            // Debug.Log("playing sound at " + path);
            FMODUnity.RuntimeManager.PlayOneShot(path, position);
        }
    }
}
