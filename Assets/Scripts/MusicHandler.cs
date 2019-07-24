using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// This class manages the game's musical state, i.e spawning notes, cursor position, etc
/// </summary>
public class MusicHandler : MonoBehaviour
{
    #region References
    // Many gameobject references and references to the song file
    public AudioClip song;
    public VideoPlayer movie;
    public string songPath;
    public string moviePath;
    public string levelName = "testLevel";
    public AudioSource source;
    public SpawnButtons buttonSpawner;
    public GameObject cursor;
    public new GameObject camera;
    private FollowMotionPath cursorMP;
    public Animator cursorAN;
    private FollowMotionPath cameraMP;
    public GameObject gamePath;
    private MotionPath gamePathMP;
    public GameObject sliderObj;

    public bool cursorFlip = false;

    public GameObject sparksPrefab;
    private GameObject sparksObj;

    // The current position of the song (in seconds)
    public float songPosition;
    // The current position of the song (in beats)
    public float songPosInBeats;
    // Song has finished playing
    public bool finished = false;
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
    public float length;
    public bool paused = false;
    public float lengthInBeats;
    // Keep all the position-in-beats of notes in the song
    public Vector2[] notes;
    // Two note tracks needed for multi-line segments
    public Vector2[] notes2;
    // The index of the next note to be spawned
    [HideInInspector]
    public int nextIndex = 0;
    // Camera animation
    public Vector2[] cameraKeyframes;
    private GameHandler gameHandler;
    #endregion

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

    public void ExtendCursor()
    {
        cursorAN.SetTrigger("Extend");
    }

    public void MoveSlider()
    {
        source.time = length * sliderObj.GetComponent<Slider>().value;
    }
    
    public void BeginGame()
    {
        #region Init References
        cameraMP = camera.GetComponent<FollowMotionPath>();
        cursorMP = cursor.GetComponent<FollowMotionPath>();
        gamePathMP = gamePath.GetComponent<MotionPath>();

        // Spawn the sparks on the end of the path
        sparksObj = Instantiate(sparksPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));

        // Get reference to the Game Handler
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];
        #endregion

        movie.Prepare();

        // Spawn the debug slider if needed
        if (gameHandler.debugMode)
        {
            sliderObj.SetActive(true);
        }

        // Calculate how many seconds is one beat
        // We will see the declaration of bpm later
        secPerBeat = 60f / bpm;

        lengthInBeats = (bpm / 60) * song.length;

        length = song.length;

        // Get the time that the song starts
        dsptimesong = (float)source.time;

        // Play the actual song
        source.clip = song;
        source.Play();

        if(moviePath != "")
        {
            movie.url = moviePath;
            movie.Play();
        }

        InvokeRepeating("MyUpdate", 0, GameHandler.frameTime);
    }

    private void Update()
    {
        // Seek the movie (if it exists) to the part of the song we're at
        if (moviePath != "" && movie.isPlaying)
        {
            movie.frame = (long)(movie.frameCount * (songPosition / length));
        }
    }

    // Update is called once per frame
    void MyUpdate()
    {
        //Debugging
        if (gameHandler.debugMode == true)
        {
            //print(nextIndex);
        }

        // Set finished to true if the song is over
        if (source.isPlaying != true && paused == false)
        {
            finished = true;
        }

        // Calculate the position in seconds
        songPosition = (float)(source.time - dsptimesong - firstBeatOffset);

        // Calculate the position in beats
        songPosInBeats = songPosition / secPerBeat;

        // If it's time to spawn the next note based on the beatsInAdvance, do so
        if (nextIndex < notes.Length && notes[nextIndex].x < songPosInBeats + beatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            GameObject button = buttonSpawner.spawn(notes[nextIndex].x / lengthInBeats, notes[nextIndex].y, notes[nextIndex].x, nextIndex);
            gameHandler.buttons.Add(new ButtonClass(button, gameHandler.inputs[button.GetComponent<Button>().type], button.GetComponent<Button>()));

            nextIndex++;
        }

        // Move debug slider to song positon
        if (gameHandler.debugMode)
        {
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.SetValueWithoutNotify(songPosInBeats / lengthInBeats);
        }

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
        gamePathMP.line.GetComponent<LineRenderer>().colorGradient = gradient;

        // Move the cursor and camera respective to the current pos and keyframes
        Vector3 cursorPos = cursorMP.motionPath.PointOnNormalizedPath(songPosInBeats / lengthInBeats);
        Vector3 cursorNorm = cursorMP.motionPath.NormalOnNormalizedPath(songPosInBeats / lengthInBeats);
        cursor.transform.position = cursorPos;
        if (cursorFlip == true)
        {
            cursor.transform.right = cursorNorm * -1;
        } else
        {
            cursor.transform.right = cursorNorm;
        }

        float final = GetPathProgress();
        Vector3 cameraPos = cameraMP.motionPath.PointOnNormalizedPath(final);
        camera.transform.position = cameraPos;

        // Move sparks to the tip of the line
        Vector3 sparksPos = cursorMP.motionPath.PointOnNormalizedPath(end1);
        Vector3 sparksNorm = cursorMP.motionPath.NormalOnNormalizedPath(end1);
        sparksObj.transform.position = sparksPos;
        sparksObj.transform.right = sparksNorm;

        // Determines whether or not the button is the next that should be being hit
        foreach (ButtonClass button in gameHandler.buttons)
        {
            if (button.btn == null) continue;
            Button btn = button.btnClass;
            if ((gameHandler.buttons.Count != 0 && btn.index != 0) && btn.GetRank(songPosInBeats, bpm, button.btnClass.beat) != 4)
            {
                btn.upNext = true;
                break;
            } else
            {
                btn.upNext = false;
            }
        }

    }
}
