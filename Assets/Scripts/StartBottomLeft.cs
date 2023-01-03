using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBottomLeft : MonoBehaviour
{
    public Vector2 screenPadding;
    private SpriteRenderer spriteRenderer;
    public float moveAmount = 25f;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine("SlideInFlufflePuff");
    }

    IEnumerator SlideInFlufflePuff()
    {
        var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(
            spriteRenderer.sprite.rect.width - spriteRenderer.sprite.pivot.x - screenPadding.x,
            spriteRenderer.sprite.rect.height - spriteRenderer.sprite.pivot.y - screenPadding.y
        ));

        yield return new WaitForSeconds(1f);

        var moveRemaining = bottomLeft.x - transform.position.x;

        while(transform.position.x < bottomLeft.x)
        {
            var newX = transform.position.x + Time.deltaTime * moveAmount;

            transform.position = new Vector3(newX, bottomLeft.y, 0);
            moveRemaining = bottomLeft.x - transform.position.x;

            yield return new WaitForSeconds(0.001f);
        }
    }

}
