using UnityEngine;
using System.Collections;
using SmoothMoves;
public class PlayerControl : MonoBehaviour
{
    [HideInInspector]
    public bool jump = false;
    public int maxJump;

    private Animator mecanim;
    private BoneAnimation anim;
    private Uni2DAnimationPlayer ap;
    private CharacterController2D controller;

    public Vector2 initialVelocity = new Vector2(15f, 0);
    public float gravity = -9.8f;
    public float jumpForce = 1000f;

    private int jumpLeft = 2;
    private bool inJump;
    private float jumpForceRemain;
    private ScoreManager scoreManager;
    private bool isSM;
    private bool grounded;

    void Start()
    {

    }

    void Awake()
    {
        foreach (var render in GetComponentsInChildren<Renderer>())
        {
            render.sortingLayerName = "Character";
        }
        anim = GetComponentInChildren<BoneAnimation>();
        isSM = anim != null;
        if (!isSM)
        {
            mecanim = GetComponentInChildren<Animator>();
        }
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

        mecanim.SetBool("jump", jump);
        if (jumpLeft > 0)
        {
            jump = Input.GetButtonDown("Jump") || jump;
            if (jump)
            {
                jumpLeft--;
                if (isSM)
                {
                    anim.PlayQueued("Jump", QueueMode.PlayNow);
                }
                else
                {
                    Debug.Log("jumpup");
                }
                jumpForceRemain = jumpForce;
                inJump = true;
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

        if (controller.isGrounded)
        {
            mecanim.SetBool("grounded", true);
        }
        else
        {
            mecanim.SetBool("grounded", false);
        }

        controller.move(new Vector3(xSpeed, vSpeed));
        mecanim.SetFloat("vSpeed", vSpeed, 1f, Time.deltaTime);

        jump = false;
    }

    void OnFingerDown(FingerDownEvent e)
    {
        Debug.Log("OnFingerDown");
        if (jumpLeft > 0)
        {
            jump = true;
        }
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
