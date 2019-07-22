using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public static float frameTime { get; private set; } = 1f / 60f;

    public int[] hits = new int[5];

    private MusicHandler musicHandler;

    private void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to music handler
        if (FindObjectsOfType<MusicHandler>()[0] != null)
            musicHandler = FindObjectsOfType<MusicHandler>()[0];
        // Make sure this object doesnt unload, for the results screen
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Only run this code in-game
        if(musicHandler != null)
        {
            if(musicHandler.finished == true)
            {
                LoadScene(2);
            }
        }
    }
}
