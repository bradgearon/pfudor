using UnityEngine;
using System.Collections;

public class GlowTrigger : MonoBehaviour
{
    public Material glowMaterial;
    public Material fallbackMaterial;
    public LayerMask glowMask;

    private bool glowEnabled = false;
    public tk2dSlicedSprite glowOverlay;
    private Renderer glowTarget;
    private Material targetMaterial;
    private tk2dSlicedSprite ob;


    void Start()
    {
        glowTarget = GetComponentInChildren<Renderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetMaterial == null)
        {
            var otherMask = (1 << other.gameObject.layer);
            if ((glowMask.value & otherMask) > 0)
            {
                targetMaterial = glowTarget.material;
                glowMaterial.mainTexture = glowTarget.material.mainTexture;
                glowTarget.material = glowMaterial;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("exit " + other);
        if (targetMaterial != null)
        {
            var otherMask = (1 << other.gameObject.layer);
            if ((glowMask.value & otherMask) > 0)
            {
                glowTarget.material = targetMaterial;
                targetMaterial = null;
            }
        }
    }

}
