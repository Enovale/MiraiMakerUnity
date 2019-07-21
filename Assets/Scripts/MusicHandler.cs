using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages the game's musical state, i.e spawning notes, cursor position, etc
/// </summary>
public class MusicHandler : MonoBehaviour
{
    // Many gameobject references and references to the song file
    public AudioClip song;
    public AudioSource source;
    public SpawnButtons buttonSpawner;
    public GameObject cursor;
    public new GameObject camera;
    public GameObject gamePath;

    public GameObject sparksPrefab;
    private GameObject sparksObj;

    // The current position of the song (in seconds)
    public float songPosition;

    // The current position of the song (in beats)
    public float songPosInBeats;

    // The duration of a beat
    [HideInInspector]
    public float secPerBeat;

    public float fadeOffsetInBeats;

    [HideInInspector]
    public float fadeEnd;

    // How much time (in seconds) has passed since the song started
    [HideInInspector]
    public float dsptimesong;

    // The offset to the first beat of the song in seconds
    public float firstBeatOffset;

    public int beatsInAdvance = 3;

    public int pathBeatsInAdvance = 6;

    public float bpm = 205;

    public float lengthInBeats;

    // Keep all the position-in-beats of notes in the song
    public Vector2[] notes;

    // The index of the next note to be spawned
    [HideInInspector]
    public int nextIndex = 0;

    // Camera animation
    public Vector2[] cameraKeyframes;

    private GameHandler gameHandler;

    /// <summary>
    /// Gets the position of the camera on it's path depending on the keyframes provided
    /// </summary>
    /// <returns></returns>
    float GetPathProgress()
    {
        // Assumed Beat Map has atleast 2 elements. TODO: correct error handling
        int startInterval = 0;
        int endInterval = 0;
        for (int i = 1; i < cameraKeyframes.Length; i++)
        {
            if (songPosInBeats <= cameraKeyframes[i].x)
            {
                endInterval = i;
                break;
            }
            startInterval = i;
        }
        // Give the percents at the bounds of the beat interval
        float startPercent = cameraKeyframes[startInterval].y;
        float endPercent = cameraKeyframes[endInterval].y;
        // Compute the total number of beats in this interval
        float intervalBeatSize = cameraKeyframes[endInterval].x - cameraKeyframes[startInterval].x;
        // Linearly interpolate the current beat position as a path percentage
        // You could use some other update rule here if you don't want linear interpolation
        float percent = startPercent + (endPercent - startPercent) * (songPosInBeats - cameraKeyframes[startInterval].x) / intervalBeatSize;
        return percent;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Spawn the sparks on the end of the path
        sparksObj = Instantiate(sparksPrefab, new Vector3(0,0,0), new Quaternion(0,0,0,0));

        // Get reference to the Game Handler
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];

        // Calculate how many seconds is one beat
        // We will see the declaration of bpm later
        secPerBeat = 60f / bpm;

        lengthInBeats = (bpm / 60) * song.length;

        // Get the time that the song starts
        dsptimesong = (float)source.time;

        // Play the actual song
        source.clip = song;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //Debugging
        if (gameHandler.debugMode == true)
        {
            print(nextIndex);
        }

        // Calculate the position in seconds
        songPosition = (float)(source.time - dsptimesong - firstBeatOffset);

        // Calculate the position in beats
        songPosInBeats = songPosition / secPerBeat;

        // If it's time to spawn the next note based on the beatsInAdvance, do so
        if (nextIndex < notes.Length && notes[nextIndex].x < songPosInBeats + beatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            GameObject button = buttonSpawner.spawn(notes[nextIndex].x / lengthInBeats, notes[nextIndex].y, songPosInBeats + beatsInAdvance, nextIndex);
            gameHandler.buttons.Add(new ButtonClass(button, gameHandler.inputs[button.GetComponent<Button>().type]));

            nextIndex++;
        }

        // Move the cursor and camera respective to the current pos and keyframes
        Vector3 cursorPos = cursor.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(songPosInBeats / lengthInBeats);
        cursor.transform.position = cursorPos;
        float final = GetPathProgress();
        Vector3 cameraPos = camera.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(final);
        camera.transform.position = cameraPos;

        // Fadeout line after passing
        Gradient gradient = new Gradient();
        float gradient1 = (((songPosInBeats - fadeOffsetInBeats) / lengthInBeats) - (bpm / 60) * 0.01f);
        fadeEnd = (((songPosInBeats - fadeOffsetInBeats) / lengthInBeats));
        float end1 = (songPosInBeats + pathBeatsInAdvance) / lengthInBeats;
        float end2 = end1;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, gradient1), new GradientAlphaKey(1, fadeEnd), new GradientAlphaKey(1, end1), new GradientAlphaKey(0, end2), new GradientAlphaKey(0, 1.0f) }
        );
        gradient.mode = GradientMode.Blend;
        gamePath.GetComponent<MotionPath>().line.GetComponent<LineRenderer>().colorGradient = gradient;

        // Move sparks to the tip of the line
        Vector3 sparksPos = cursor.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(end1);
        sparksObj.transform.position = sparksPos;
    }
}
