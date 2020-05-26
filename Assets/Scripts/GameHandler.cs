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
    /// <summary>
    /// Enables a song position slider and seeing certain information in the console.
    /// </summary>
    public bool DebugMode = false;
    public Sprite[] NoteTypes;
    public List<ButtonClass> SongButtons;
    public List<ButtonClass> SongButtons2;
    public KeyCode[] NoteInputs;
    public KeyCode[] NoteInputsAlt;

    public bool cursorExtended = false;
    public bool cursorFlipped = false;

    /// <summary>
    /// The framerate of the renderer (not related to the game logic framerate)
    /// </summary>
    [HideInInspector]
    public double runningFrameRate { get; private set; }

    public static int FrameRate = 60;
    /// <summary>
    /// How long between each frame, because game logic doesn't need to be called every frame
    /// Calling update each frame will massively slow down the game because the faster the rendering
    /// the more times update gets called, and Update is pretty hefty
    /// </summary>
    public static float FrameTime => 1f / FrameRate;

    public int[] NoteHitCounter = new int[5];

    #region FPS
    // For fps calculation.
    private int frameCount;
    private float elapsedTime;
    #endregion

    private MusicHandler musicHandler;
    private bool _isMusicHandlerNotNull;

    public enum Rank
    {
        Cool = 0,
        Fine = 1,
        Safe = 2,
        Sad = 3,
        Missed = 4,
    }

    /// <summary>
    /// Simply a wrapper for the SceneManager loadscene
    /// </summary>
    /// <param name="scene">The scene index to load</param>
    private void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ResetButtons()
    {
        foreach (ButtonClass btn in SongButtons)
        {
            Destroy(btn.btn);
        }
        foreach (ButtonClass btn in SongButtons2)
        {
            Destroy(btn.btn);
        }
        SongButtons.Clear();
        SongButtons2.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to music handler
        if (FindObjectsOfType<MusicHandler>()[0] != null)
        {
            musicHandler = FindObjectsOfType<MusicHandler>()[0];
            _isMusicHandlerNotNull = true;
        }

        // Make sure this object doesn't unload, for the results screen
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Only run this code in-game
        if(_isMusicHandlerNotNull)
        {
            if(musicHandler.SongFinished == true)
            {
                LoadScene(2);
            }
        }

        // FPS calculation
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;
        if (elapsedTime > 0.5f)
        {
            runningFrameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
            frameCount = 0;
            elapsedTime = 0;
        }
    }
}
