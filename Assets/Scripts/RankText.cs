using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankText : MonoBehaviour
{

    public Sprite[] rankSprites;
    public GameObject spriteObj;
    private bool alive = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(int rank)
    {
        spriteObj.GetComponent<SpriteRenderer>().sprite = rankSprites[rank];
    }

    // Update is called once per frame
    void Update()
    {
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
