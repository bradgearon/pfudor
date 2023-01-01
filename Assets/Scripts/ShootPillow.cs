using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ShootPillow : MonoBehaviour
{
    public GameObject pillowPrefab;
    public Vector2 pillowForce;
    public float torque;

    public float bandiness = 0.4f;

    public void OnDrag(DragGesture gesture)
        {

        // only handle release now
        if (gesture.State != GestureRecognitionState.Ended)
        {
            return;
        }

        // will have to handle drag move also but for feedback
        var pillow = Instantiate(pillowPrefab, gameObject.transform.position, Quaternion.AngleAxis(0, new Vector3(0, 1, 0)));
        var pillowRigidBody = pillow.GetComponent<Rigidbody2D>();

        var slingedPillowForce = new Vector2(
                pillowForce.x * bandiness * -gesture.TotalMove.x,
                pillowForce.y * bandiness * -gesture.TotalMove.y
            );

        pillowRigidBody.AddRelativeForce(slingedPillowForce, ForceMode2D.Impulse);
        // pillowRigidBody.AddTorque(torque);
    }

}
