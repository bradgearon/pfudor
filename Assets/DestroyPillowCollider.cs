using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPillowCollider : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("enter " + other.name);
        if (other.CompareTag("Dead"))
        {
            Destroy(gameObject);
        }
    }
}
