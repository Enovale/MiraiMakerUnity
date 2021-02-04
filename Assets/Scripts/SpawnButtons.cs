using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actually spawns the buttons when the Music Handler demands it
/// </summary>
public class SpawnButtons : MonoBehaviour
{
    // Needs a reference to some game state vars
    public MotionPath path;
    public GameObject buttonPrefab;
    public float trackOffset = 2f;

    private MusicHandler musicHandler;
    private GameHandler gameHandler;

    // Start is called before the first frame update
    private void Start()
    {
        // Get reference to the Game Handler and Music Handler
        musicHandler = FindObjectOfType<MusicHandler>();
        gameHandler = FindObjectOfType<GameHandler>();
    }

    /// <summary>
    /// Spawns the button using a prefab and initiates it with the variables you give it. Also returns the button as a gameObject
    /// </summary>
    /// <param name="uv">Percentage of the BeatPath</param>
    /// <param name="type">Type of button</param>
    /// <param name="beat">Beat of button</param>
    /// <param name="indexin">Index of the button</param>
    /// <param name="track">The Track to spawn on (0-1)</param>
    /// <returns></returns>
    public GameObject Spawn(float uv, float type, bool sustain, float beat, int track)
    {
        var buttonPos = new Vector3(path.PointOnNormalizedPath(uv).x, path.PointOnNormalizedPath(uv).y,
            buttonPrefab.transform.position.z);
        var button = Instantiate(buttonPrefab, buttonPos, new Quaternion(0, 0, 0, 0));
        if (track != 0)
        {
            button.transform.up = path.NormalOnNormalizedPath(uv);
            var offset = transform.right * (trackOffset * -1);
            if (gameHandler.cursorFlipped)             
                offset = transform.right * (trackOffset * 1);
            button.transform.Translate(offset, Space.Self);
            button.transform.eulerAngles = new Vector3(0, 0, 0);
        }

        // If Type given does not exist, switch to Star(0)
        if (Mathf.RoundToInt(type) >= gameHandler.NoteTypes.Length)         
            type = 0;
        var btnClass = button.GetComponent<Button>();
        btnClass.Init(Mathf.RoundToInt(type), sustain, beat, track);
        return button;
    }
}