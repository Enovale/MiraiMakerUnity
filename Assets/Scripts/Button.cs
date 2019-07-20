using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    public int type;
    public float beat;
    public int index;
    private GameObject gameHandler;
    private new SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Hit(float songPosInBeats, float bpm)
    {
        
        if(songPosInBeats >= beat && songPosInBeats < (beat + (bpm / 60) / 4))
        {
            print("Did hit");
            Destroy(this);
        }
    }

    public void Init(int typein, float curbeat, int indexin)
    {
        // Create button
        beat = curbeat;
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        Sprite[] types = objects[0].types;
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = types[typein];
        type = typein;
        index = indexin;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
