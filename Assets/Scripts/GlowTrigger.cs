using UnityEngine;
using System.Collections;

public class GlowTrigger : MonoBehaviour
{
    public Sprite glowSprite;
    public Sprite normSprite;

    public LayerMask glowMask;

    private bool glowEnabled = false;

    private SpriteRenderer glowTarget;

    void Start()
    {
        glowTarget = GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        glowTarget.sprite = glowSprite;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        glowTarget.sprite = normSprite;
    }

}
