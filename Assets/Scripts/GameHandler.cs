using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{

    public Sprite[] types;
    public List<ButtonClass> buttons;
    public KeyCode[] inputs;
    private MusicHandler musicHandler;
    //private string keyDown = "";

    public List<float> test = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        musicHandler = FindObjectsOfType<MusicHandler>()[0];
    }

    // Update is called once per frame
    void Update()
    {

    }
}
