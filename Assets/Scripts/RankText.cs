using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankText : MonoBehaviour
{

    public Sprite[] rankSprites;
    public GameObject spriteObj;

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
        
    }
}
