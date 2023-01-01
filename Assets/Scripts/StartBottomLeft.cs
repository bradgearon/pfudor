using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBottomLeft : MonoBehaviour
{
    public Vector2 screenPadding;
    // Start is called before the first frame update
    void Awake()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(
            spriteRenderer.sprite.rect.width - spriteRenderer.sprite.pivot.x - screenPadding.x,
            spriteRenderer.sprite.rect.height - spriteRenderer.sprite.pivot.y - screenPadding.y
        ));

        transform.position = new Vector3(bottomLeft.x, bottomLeft.y, 0);
    }

}
