using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class WolfSpawner : MonoBehaviour
{
    public GameObject rainbowPrefab;
    public UILabel remainLabel;
    public UILabel landedLabel; 

    public int startWolves = 10;

    private int destroyedWolves = 0;
    private int landedWolves = 0;

    public float minTimeBetweenSpawns;		// The shortest possible time between spawns.
    public float maxTimeBetweenSpawns;		// The longest possible time between spawns.
    public float minSpeed;					// The lowest possible speed of the prop.
    public float maxSpeed;					// The highest possible speeed of the prop.

    public float maxScaleX;
    public float minScaleX;

    public float topPadY;
    public float bottomPadY;
    public float antiGravity;

    public bool drawDebug = false;

    private Vector3 screenMax;
    private Vector3 screenMin;
    private bool first;

    public Transform player;
    public float maxDistanceYFromPlayer;
    public GameObject cloudSpawner;
    public void StartSpawner()
    {
        // Start the Spawn coroutine.
        StartCoroutine("Spawn");
    }

    public void DecideVictory()
    {
        Stars.stars = new Stars
        {
            landed = landedWolves,
            won = destroyedWolves > landedWolves
        };

        SceneManager.LoadScene("stars");
    }

    private void CheckFinish()
    {
        if (destroyedWolves + landedWolves >= startWolves)
        {
            DecideVictory();
        }
    }

    public void OnWolfDestroy()
    {
        destroyedWolves++;
        UpdateRemain();

        CheckFinish();
    }

    private void UpdateRemain()
    {
        var remaining = startWolves - (destroyedWolves + landedWolves);
        remainLabel.text = remainLabel.text.Split(":")[0] + ": " + remaining;
    }

    public void OnWolfLand()
    {
        landedWolves++;
        
        landedLabel.text = landedLabel.text.Split(":")[0] + ": " + landedWolves;
        UpdateRemain();

        CheckFinish();
    }

    private void Start()
    {
        remainLabel.text = remainLabel.text.Split(":")[0] + ": " + startWolves;
    }

    void Update()
    {
        screenMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        screenMin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        var topEnd = Mathf.Clamp(screenMax.y - topPadY, screenMin.y, player.transform.position.y + maxDistanceYFromPlayer);
        screenMax = new Vector3(screenMax.x, topEnd);

        var bottomEnd = screenMin.y + bottomPadY;
        screenMin = new Vector3(screenMin.x, bottomEnd);
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


        var rigidBody = propInstance.GetComponent<Rigidbody2D>();
        // Set the prop's velocity to this speed in the x axis
        if (rigidBody != null)
        {
            int gravity = Random.Range(0, 2);
            rigidBody.velocity = new Vector2(speed, gravity == 1 ? antiGravity : 0);
            rigidBody.gravityScale = gravity;           
        }

        StartCoroutine("Spawn");
    }


}
