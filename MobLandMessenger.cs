using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobDeadMessenger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dead"))
        {
            var wolves = GameObject.Find("wolves");
            if (wolves != null)
            {
                wolves.SendMessage("OnWolfDestroy");
            }
            Destroy(gameObject);
        }
    }
}
