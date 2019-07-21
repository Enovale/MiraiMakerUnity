using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This manages the rank text when you hit a note
/// </summary>
public class RankText : MonoBehaviour
{
    // Reference the sprite itself and other vars
    public Sprite[] rankSprites;
    public GameObject spriteObj;
    private bool alive = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Change sprite to match rank and start animation
    /// </summary>
    /// <param name="rank"></param>
    public void Init(int rank)
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = rankSprites[rank];
    }

    // Update is called once per frame
    void Update()
    {
        // If the animation finishes, kill the object to save memory
        if (spriteObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("RankFloat"))
        {
            // Avoid any reload.
            alive = true;
        } else
        {
            alive = false;
        }
        if(alive == false)
        {
            Destroy(this.gameObject);
        }
    }
}
