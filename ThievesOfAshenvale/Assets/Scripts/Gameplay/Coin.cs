using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;

    private bool soundPlayed;
    private void OnCollisionEnter(Collision other)
    {
        if (!other.collider.CompareTag("CursorFollower") && !soundPlayed)
        {
            soundPlayed = true;
            SoundManager.Instance.PlayOneShot("event:/Effects/Coins", other.GetContact(0).point);
            StartCoroutine(CheckForLanding());
        }
    }

    IEnumerator CheckForLanding()
    {
        yield return new WaitForSeconds(.7f);
        rigidBody.isKinematic = true;
    }
}
