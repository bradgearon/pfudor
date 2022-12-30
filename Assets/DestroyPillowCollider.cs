using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPillowCollider : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dead"))
        {
            Destroy(gameObject);
        }
    }
}
