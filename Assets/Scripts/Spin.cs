using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {
    private float speed = -400f;
    private UISprite sprite;

    void Start()
    {
        sprite = GetComponent<UISprite>();
    }

	void Update()
    {
        if (sprite.isActiveAndEnabled)
        {
            transform.Rotate(Vector3.forward, speed * Time.deltaTime);
        }
    }
}
