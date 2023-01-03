using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stars
{
    public static Stars stars;
    public int landed;
    public bool won;
}

public class StarsManager : MonoBehaviour
{
    public UILabel StarsLabel;
    public GameObject WolfDropper;
    // Start is called before the first frame update
    void Start()
    {
        var stars = Stars.stars;
        if (stars == null)
        {
            stars = new Stars
            {
                landed = 4,
            };
        }
        OnStarsUpdate(stars);
    }

    void OnStarsUpdate(Stars stars)
    {
        StarsLabel.enabled = true;
        StarsLabel.text = StarsLabel.text.Replace("<landed>", string.Empty + stars.landed);

        WolfDropper.SendMessage("OnDropWolves", stars.landed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
