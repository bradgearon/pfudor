using UnityEngine;
using System.Collections;

public class OidController : MonoBehaviour
{
    public float minVelocity = 5;
    public float maxVelocity = 20;
    public float randomness = 1;
    public int flockSize = 20;
    public GameObject prefab;
    public GameObject chasee;

    public Vector2 flockCenter;
    public Vector2 flockVelocity;

    private GameObject[] boids;

    void Start()
    {
        var collider = GetComponent<Collider2D>();
        
        boids = new GameObject[flockSize];
        for (var i = 0; i < flockSize; i++)
        {
            var position = new Vector2(
                Random.value * collider.bounds.size.x,
                Random.value * collider.bounds.size.y
            ) - (Vector2) collider.bounds.extents;

            GameObject boid = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            boid.transform.parent = transform;
            boid.transform.localPosition = position;
            boid.GetComponent<OidFlocking>().SetController(gameObject);
            boids[i] = boid;
        }
    }

    void Update()
    {
        var theCenter = Vector2.zero;
        var theVelocity = Vector2.zero;

        foreach (GameObject boid in boids)
        {
            theCenter = theCenter + (Vector2) boid.transform.localPosition;
            theVelocity = theVelocity + boid.GetComponent<Rigidbody2D>().velocity;
        }

        flockCenter = theCenter / (flockSize);
        flockVelocity = theVelocity / (flockSize);
    }
}