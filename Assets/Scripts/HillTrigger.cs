using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log(collider.name + " " + collider.tag);
        if (collider.CompareTag("mob"))
        {
            Debug.Log("add mob to island ");
        }
    }

}
