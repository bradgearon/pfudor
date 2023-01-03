using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPillowCollider : MonoBehaviour
{
    private Animator mecanim;
    private void Start()
    {
        mecanim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("mob"))
        {
            collision.collider.attachedRigidbody.gravityScale = 1;
            Debug.Log("squish tigger");
            mecanim.Play("pillow-squish");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dead"))
        {
            Destroy(gameObject);
        }
    }
}
