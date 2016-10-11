using UnityEngine;
using System.Collections;

public class RainbowSpawner : MonoBehaviour
{
    public GameObject rainbowPrefab;

    public float minTimeBetweenSpawns;		// The shortest possible time between spawns.
    public float maxTimeBetweenSpawns;		// The longest possible time between spawns.
    public float minSpeed;					// The lowest possible speed of the prop.
    public float maxSpeed;					// The highest possible speeed of the prop.

    public float maxScaleX;
    public float minScaleX;

    public float topPadY;
    public float bottomPadY;

    public bool drawDebug = false;

    private Vector3 screenMax;
    private Vector3 screenMin;
    private bool first;

    public Transform player;
    public float maxDistanceYFromPlayer;

    void Start()
    {
        // Start the Spawn coroutine.
        StartCoroutine("Spawn");
    }

    void Update()
    {
        screenMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        screenMin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        var topEnd = Mathf.Clamp(screenMax.y - topPadY, screenMin.y, player.transform.position.y + maxDistanceYFromPlayer);
        screenMax = new Vector3(screenMax.x, topEnd);

        var bottomEnd = screenMin.y + bottomPadY;
        screenMin = new Vector3(screenMin.x, bottomEnd);

        if (drawDebug)
        {
            Debug.DrawLine(new Vector3(1, bottomEnd), new Vector3(1, topEnd), Color.yellow);
        }
    }

    void OnGUI()
    {
        if (drawDebug)
        {
            GUILayout.Label("Top: " + screenMax.y);
            GUILayout.Label("top pad: " + (screenMax.y - topPadY));

            GUILayout.Label("Bottom: " + screenMin.y);
            GUILayout.Label("player: " + player.transform.position);
            GUILayout.Label("player max: " + (player.transform.position.y + maxDistanceYFromPlayer));
        }
    }

    IEnumerator Spawn()
    {
        if (!first)
        {
            // Create a random wait time before the prop is instantiated.
            float waitTime = Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);

            // Wait for the designated period.
            yield return new WaitForSeconds(waitTime);
        }
        first = false;

        // If the prop is facing left, it should start on the right hand side, otherwise it should start on the left.
        float posX = screenMax.x;

        // Create a random y coordinate for the prop.
        float posY = Random.Range(screenMin.y, screenMax.y);
        float scaleX = Random.Range(minScaleX, maxScaleX);
        float speed = Random.Range(minSpeed, maxSpeed) * -1;

        // Set the position the prop should spawn at.
        Vector3 spawnPos = new Vector3(posX, posY, transform.position.z);

        // Instantiate the prop at the desired position.
        var propInstance = Instantiate(rainbowPrefab, spawnPos, Quaternion.identity) as GameObject;
        
        propInstance.transform.parent = transform;
        propInstance.transform.localScale = new Vector3(scaleX, propInstance.transform.localScale.y, propInstance.transform.localScale.z);

        StartCoroutine(Spawn());
        // Set the prop's velocity to this speed in the x axis
        if (propInstance.GetComponent<Rigidbody2D>() != null)
        {
            propInstance.GetComponent<Rigidbody2D>().velocity = new Vector2(speed, 0);

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
