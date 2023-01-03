using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobLandMessenger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.collider.name);
        Debug.Log(collision.otherCollider.name);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name+ " " + gameObject.name+ " " + other.tag);
        if (other.CompareTag("land"))
        {
            var wolves = GameObject.Find("wolves");
            if (wolves != null)
            {
                wolves.SendMessage("OnWolfLand");
            }
            Destroy(gameObject);
        }
    }
}
