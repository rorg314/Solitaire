using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// //Redundant class
/// </summary>
public class SpriteManager : MonoBehaviour
{
    public Dictionary<string, Sprite> cardSpritesMap { get; protected set; }

    public static SpriteManager Instance { get; protected set; }

    void Start() {
        LoadSprites();
    }

    public void LoadSprites() {
        //Dictionary of card sprites
        cardSpritesMap = new Dictionary<string, Sprite>();
        //Array of all sprites
        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Sprites");
        //Assign each sprite to correct card name
        foreach (Sprite s in cardSprites) {
            cardSpritesMap[s.name] = s;
            //Debug.Log(s.name);
        }

    }
}
