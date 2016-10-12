using UnityEngine;
using System.Collections;

public class OidFlocking : MonoBehaviour
{
    private GameObject Controller;
    private bool inited = false;
    private float minVelocity;
    private float maxVelocity;
    private float randomness;
    private GameObject chasee;
    private OidController oidController;
    private Rigidbody2D oidRigidBody;

    void Start()
    {
        StartCoroutine("OidSteering");
    }

    IEnumerator OidSteering()
    {
        while (true)
        {
            if (inited)
            {
                oidRigidBody.velocity = oidRigidBody.velocity + Calc() * Time.deltaTime;

                // enforce minimum and maximum speeds for the boids
                float speed = oidRigidBody.velocity.magnitude;
                if (speed > maxVelocity)
                {
                    oidRigidBody.velocity = oidRigidBody.velocity.normalized * maxVelocity;
                }
                else if (speed < minVelocity)
                {
                    oidRigidBody.velocity = oidRigidBody.velocity.normalized * minVelocity;
                }
            }

            float waitTime = Random.Range(0.3f, 0.5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector2 Calc()
    {
        var randomize = new Vector2((Random.value * 2) - 1, (Random.value * 2) - 1);

        randomize.Normalize();
        
        var flockCenter = oidController.flockCenter;
        var flockVelocity = oidController.flockVelocity;
        Vector2 follow = chasee.transform.localPosition;

        flockCenter = flockCenter - (Vector2) transform.localPosition;
        flockVelocity = flockVelocity - oidRigidBody.velocity;
        follow = follow - (Vector2) transform.localPosition;

        return (flockCenter + flockVelocity + follow * 2 + randomize * randomness);
    }

    public void SetController(GameObject theController)
    {
        Controller = theController;
        oidController = Controller.GetComponent<OidController>();
        oidRigidBody = GetComponent<Rigidbody2D>();
        minVelocity = oidController.minVelocity;
        maxVelocity = oidController.maxVelocity;
        randomness = oidController.randomness;
        chasee = oidController.chasee;
        inited = true;
    }
}