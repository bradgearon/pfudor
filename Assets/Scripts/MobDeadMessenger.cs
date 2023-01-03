using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

public class MobDeadMessenger : MonoBehaviour
{
    private GameObject wolves;

    private void Start()
    {
        wolves = GameObject.Find("wolves");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dead"))
        {
            if (wolves != null)
            {
                wolves.SendMessage("OnWolfDestroy");
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("mob"))
        {
            Debug.Log("mob destroy");
            Destroy(gameObject);
        }
    }
}
