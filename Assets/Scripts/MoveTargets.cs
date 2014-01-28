using UnityEngine;
using System.Collections;

public class MoveTargets : MonoBehaviour
{
    public float parallaxScale = 100f;
    public float smoothing = 1f;
    public Transform[] targets;

    void Start()
    {

    }

    void Update()
    {
        float parallax = Time.deltaTime * parallaxScale;
        for (int i = 0; i < targets.Length; i++)
        {
            float backgroundTargetPosX = targets[i].position.x + parallax;
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, targets[i].position.y, targets[i].position.z);
            targets[i].position = Vector3.Lerp(targets[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
        }
    }
}
