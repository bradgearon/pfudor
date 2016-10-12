using UnityEngine;
using System.Collections;

public class OidWatcher : MonoBehaviour
{
    private Transform target;

    void Start()
    {
        target = transform.parent.GetComponent<OidController>().chasee.transform;
    }

    void LateUpdate()
    {
        if (target)
        {
            Vector3 dir = target.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}