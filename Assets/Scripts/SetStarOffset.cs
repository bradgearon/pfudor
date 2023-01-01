using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStarOffset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(var mecanim in gameObject.GetComponentsInChildren<Animator>())
        {
            var offset = Random.Range(0, 2);
            mecanim.SetFloat("offset", offset);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
