using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    [HideInInspector]
    public bool jump = false;
    public int maxJump;

    private Animator mecanim;

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
    private SceneManager sceneManager;
    private DefaultDown defaultDown;

    void Awake()
    {
        foreach (var render in GetComponentsInChildren<Renderer>())
        {
            render.sortingLayerName = "Character";
        }
        mecanim = GetComponentInChildren<Animator>();

        controller = GetComponent<CharacterController2D>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (Application.isEditor && Camera.main == null)
        {
            Application.LoadLevel("title");
            return;
        }

        var screenMin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
        transform.position = new Vector3(screenMin.x, transform.position.y);

        defaultDown = FindObjectOfType<DefaultDown>();
        defaultDown.playerControl = gameObject;
        scoreManager.ActivateAchievements();
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
                scoreManager.jumpCountLabel.text = jumpLeft.ToString();
                jumpForceRemain = jumpForce;
                inJump = true;
            }
        }

        mecanim.SetBool("jump", jump);

        var vSpeed = 0f;
        if (jumpForceRemain > 0f)
        {
            var jumpFrame = jumpForceRemain * Time.deltaTime;
            jumpForceRemain -= jumpFrame;
            vSpeed += jumpFrame;
            inJump = true;
        }
        else
        {
            jumpForceRemain = 0f;
        }

        if (!controller.collisionState.below)
        {
            vSpeed += gravity * Time.deltaTime;
        }

        mecanim.SetFloat("vSpeed", vSpeed);
        controller.move(new Vector3(xSpeed, vSpeed));

        if (controller.collisionState.becameGroundedThisFrame)
        {
            mecanim.SetBool("thisframe", true);
            jumpForceRemain = 0;
        }
        else
        {
            mecanim.SetBool("thisframe", false);
        }

        mecanim.SetBool("grounded", controller.collisionState.below);
        jump = false;
    }

    public void OnDefaultDown()
    {
        Debug.Log("default down");
        if (!(Time.timeScale > 0)) return;
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
            scoreManager.jumpCountLabel.text = jumpLeft.ToString();
        }
        else if (other.CompareTag("Dead"))
        {
            defaultDown.SendMessage("GameOver");
            scoreManager.SendMessage("GameOver");
        }
    }


}
