using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiggerOnTimer : MonoBehaviour
{
    public Vector3 startScale;
    public float scalePerSecond;
    private bool end;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (end)
        {
            return;
        }

        var scaleScale = Time.deltaTime * scalePerSecond;

        var newScale = new Vector3(
            startScale.x * scaleScale + transform.localScale.x,
            startScale.y * scaleScale + transform.localScale.y, 1);

        if (newScale.x > .99f || newScale.y > .99f)
        {
            end = true;
            transform.localScale = Vector3.one;
        }
        
        transform.localScale = newScale;

        var bottomCenter = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, newScale.y));

        transform.position = new Vector3(bottomCenter.x, bottomCenter.y, 0);
    }
}
