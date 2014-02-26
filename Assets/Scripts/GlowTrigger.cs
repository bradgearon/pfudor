using UnityEngine;
using System.Collections;

public class GlowTrigger: MonoBehaviour
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
        if (!SystemInfo.supportsImageEffects)
        {
            glowEnabled = true;
        }
        glowTarget = GetComponentInChildren<Renderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetMaterial == null)
        {
            var otherMask = (1 << other.gameObject.layer);
            if ((glowMask.value & otherMask) > 0)
            {
                if (glowEnabled)
                {
                    Debug.Log("enabled true");
                    if (ob == null)
                    {
                        ob = Instantiate(glowOverlay) as tk2dSlicedSprite;
                        ob.scale = Vector3.one;
                        ob.transform.parent = glowTarget.transform;
                        var bounds = ob.GetUntrimmedBounds();
                        ob.transform.localPosition = Vector3.zero;
                        var horizontalPad = ob.borderBottom * bounds.size.y + ob.borderTop * bounds.size.y;
                        ob.dimensions = new Vector2((glowTarget.bounds.size.x + bounds.size.x / 2) * 100, (glowTarget.bounds.size.y + horizontalPad) * 100);
                    }
                    ob.gameObject.SetActive(true);
                }
                else
                {
                    targetMaterial = glowTarget.material;
                    glowMaterial.mainTexture = glowTarget.material.mainTexture;
                    glowTarget.material = glowMaterial;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("exit " + other);
        if (glowEnabled || targetMaterial != null)
        {
            var otherMask = (1 << other.gameObject.layer);
            if ((glowMask.value & otherMask) > 0)
            {
                if (glowEnabled)
                {
                    Debug.Log("enabled false");
                    ob.gameObject.SetActive(false);
                }
                else
                {
                    glowTarget.material = targetMaterial;
                    targetMaterial = null;
                }
            }
        }
    }

}
