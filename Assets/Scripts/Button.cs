using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    public int type;
    private GameObject gameHandler;
    private new SpriteRenderer renderer;
    public float beat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(int typein, float curbeat)
    {
        // Create button
        beat = curbeat;
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        Sprite[] types = objects[0].types;
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = types[typein];
        type = typein;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
