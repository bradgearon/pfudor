using UnityEngine;
using System.Collections;
using SmoothMoves;
public class PlayerControl : MonoBehaviour
{
    [HideInInspector]
    public bool jump = false;
    public int maxJump;

    public float gravity = -9.8f;
    public float jumpForce = 1000f;
    private BoneAnimation anim;
    private int jumpLeft = 2;
    private CharacterController2D controller;
    private bool inJump;
    private float jumpForceRemain;
    public Vector2 initialVelocity = new Vector2(15f, 0);
    private ScoreManager scoreManager;

    void Awake()
    {
        foreach (var render in GetComponentsInChildren<Renderer>())
        {
            render.sortingLayerName = "Character";
        }
        anim = GetComponentInChildren<BoneAnimation>();
        controller = GetComponent<CharacterController2D>();
        var fingerDownDetector = gameObject.AddComponent<FingerDownDetector>();
        fingerDownDetector.MessageTarget = gameObject;
        scoreManager = FindObjectOfType<ScoreManager>();

        var screenMin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
        transform.position = new Vector3(screenMin.x, transform.position.y);
    }

    void Update()
    {
        var xSpeed = 0f;
        if (initialVelocity.x > 0.2f)
        {
            var delta = initialVelocity * Time.deltaTime;
            initialVelocity -= delta;
            xSpeed = delta.x;
        }

        if (jumpLeft > 0)
        {
            jump = Input.GetButtonDown("Jump") || jump;
            if (jump)
            {
                jumpLeft--;
                anim.PlayQueued("Jump", QueueMode.PlayNow);
                jumpForceRemain = jumpForce;
                jump = false;
            }
        }
        var vSpeed = 0f;
        if (jumpForceRemain > 0.2f)
        {
            var jumpFrame = jumpForceRemain * Time.deltaTime;
            jumpForceRemain -= jumpFrame;
            vSpeed += jumpFrame;
        }
        else
        {
            jumpForceRemain = 0f;
        }
        if (!controller.collisionState.below)
        {
            vSpeed += gravity * Time.deltaTime;
        }
        controller.move(new Vector3(xSpeed, vSpeed));
    }

    void OnFingerDown(FingerDownEvent e)
    {
        Debug.Log("OnFingerDown");
        jump = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("enter " + other.name);
        if (other.CompareTag("platform"))
        {
            jumpLeft = maxJump;
        }
        else if (other.CompareTag("Dead"))
        {
            scoreManager.SendMessage("GameOver");
        }
    }


}
