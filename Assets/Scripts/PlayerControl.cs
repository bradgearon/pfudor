using UnityEngine;
using System.Collections;
using SmoothMoves;
public class PlayerControl : MonoBehaviour
{
    [HideInInspector]
    public bool jump = false;				// Condition for whether the player should jump.
    public int maxJump;

    public float jumpForce = 1000f;			// Amount of force added when the player jumps.
    private BoneAnimation anim;					// Reference to the player's animator component.
    private int jumpLeft = 2;
    private CharacterController2D controller;
    private bool inJump;
    private float jumpForceRemain;
    public Vector2 initialVelocity = new Vector2(15f, 0);

    void Awake()
    {
        foreach (var render in GetComponentsInChildren<Renderer>())
        {
            render.sortingLayerName = "Character";
        }
        anim = GetComponentInChildren<BoneAnimation>();
        controller = GetComponent<CharacterController2D>();
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
            vSpeed -= 9.8f * Time.deltaTime;
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

        if (other.CompareTag("Dead"))
        {
            Application.Quit();
        }
    }


}
