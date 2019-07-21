using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the game state, does not handle the music and note spawning portion
/// </summary>
public class GameHandler : MonoBehaviour
{
    // Mostly just stores objects right now.
    public Sprite[] types;
    public List<ButtonClass> buttons;
    public KeyCode[] inputs;
    public bool debugMode = false;
    private MusicHandler musicHandler;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to music handler
        musicHandler = FindObjectsOfType<MusicHandler>()[0];
    }

    // Update is called once per frame
    void Update()
    {

    }
}
