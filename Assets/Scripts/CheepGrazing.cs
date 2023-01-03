using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheepGrazing : MonoBehaviour
{
    private Animator mecanim;

    void Start()
    {
        mecanim = GetComponent<Animator>();
        StartCoroutine("Graze");
    }

    // Update is called once per frame
    IEnumerator Graze()
    {
        for (; ; )
        {
            var eatingChance = Random.Range(0, 5);
            var eating = eatingChance > 2;
            var scared = Random.Range(0, 5) > 3;

            if (eating)
            {
                mecanim.Play("cheep-eating");
            }
            else if (scared)
            {
                mecanim.Play("cheep-scared");
            }

            yield return new WaitForSeconds(3);
        }
    }
}
