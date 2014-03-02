using UnityEngine;
using System.Collections;

public class CloudSpawner : MonoBehaviour
{
    public tk2dSprite prefab;
    public Texture2D[] prefabs;

    public float minTimeBetweenSpawns;		// The shortest possible time between spawns.
    public float maxTimeBetweenSpawns;		// The longest possible time between spawns.
    public float minSpeed;					// The lowest possible speed of the prop.
    public float maxSpeed;					// The highest possible speeed of the prop.

    private Vector3 screenMax;
    private Vector3 screenMin;
    private Random random;


    void Start()
    {
        // Set the random seed so it's not the same each game.
        Random.seed = System.DateTime.Today.Millisecond;
        random = new Random();

        // Start the Spawn coroutine.
        StartCoroutine("Spawn");
    }

    void Update()
    {
        screenMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        screenMin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
    }


    IEnumerator Spawn()
    {
        // Create a random wait time before the prop is instantiated.
        float waitTime = Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);

        // Wait for the designated period.
        yield return new WaitForSeconds(waitTime);

        // If the prop is facing left, it should start on the right hand side, otherwise it should start on the left.
        float posX = screenMax.x;

        // Create a random y coordinate for the prop.
        float posY = Random.Range(screenMin.y, screenMax.y);

        // Set the position the prop should spawn at.
        Vector3 spawnPos = new Vector3(posX, posY, transform.position.z);

        // Instantiate the prop at the desired position.
        var propInstance = Instantiate(prefab, spawnPos, Quaternion.identity) as tk2dSprite;
        var spriteName = prefabs[Random.Range(0, prefabs.Length)].name;
        propInstance.SetSprite(spriteName);
        propInstance.transform.parent = transform;

        // Create a random speed.
        float speed = Random.Range(minSpeed, maxSpeed) * -1;

        StartCoroutine(Spawn());
        // Set the prop's velocity to this speed in the x axis
        if (propInstance.rigidbody2D != null)
        {
            propInstance.rigidbody2D.velocity = new Vector2(speed, 0);

            // While the prop exists...
            while (propInstance != null)
            {
                if (propInstance.transform.position.x < screenMin.x - 0.5f)
                {
                    // ... destroy the prop.
                    Destroy(propInstance.gameObject);
                }
                // Return to this point after the next update.
                yield return null;
            }
        }


    }


}
