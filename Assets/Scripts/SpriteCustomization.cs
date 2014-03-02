using System.Collections.Generic;
using System.Linq;
using GooglePlayGames.BasicApi;
using UnityEngine;
using System.Collections;

public class SpriteCustomization : MonoBehaviour
{
    public Uni2DSprite ownerSprite;
    public GameObject horn;

    void activateHorn()
    {
        horn.SetActive(true);
    }

    void activateSunglasses()
    {
        var frame = ownerSprite.spriteAnimation.GetClipByIndex(0).frames[0];
        ownerSprite.SetFrame(frame);
    }




}
