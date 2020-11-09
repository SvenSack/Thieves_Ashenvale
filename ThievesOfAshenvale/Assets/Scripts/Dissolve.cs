﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] private ParticleSystem parts;
    [SerializeField] private MeshRenderer meshRen;
    [SerializeField] private float dissolveTime;
    [SerializeField] private Material dissolveMaterial;

    private void Update()
    {
        if (Input.GetKeyDown("l"))
        {
            StartDissolve();
        }
    }

    public void StartDissolve()
    {
        dissolveMaterial.SetFloat("timeOffset", Time.time);
        meshRen.material = dissolveMaterial;
        parts.Play(true);
        StartCoroutine(RemoveAfter(dissolveTime));
    }

    private IEnumerator RemoveAfter(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        gameObject.SetActive(false);
    }
}