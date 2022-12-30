using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootPillow : MonoBehaviour
{
    public GameObject pillowPrefab;
    public Vector2 pillowForce;
    public float torque;

    public void OnTap()
    {
        var pillow = Instantiate(pillowPrefab, gameObject.transform.position, Quaternion.AngleAxis(0, new Vector3(0, 1, 0)));
        var pillowRigidBody = pillow.GetComponent<Rigidbody2D>();

        pillowRigidBody.AddRelativeForce(pillowForce, ForceMode2D.Impulse);
        pillowRigidBody.AddTorque(torque);

    }
}
