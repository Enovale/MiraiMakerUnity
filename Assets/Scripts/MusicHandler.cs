﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
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
    public Animator cursorAN;
    public MotionPath cameraMP;
    public GameObject gamePath;
    public GameObject sliderObj;

    public bool cursorFlip = false;

    public GameObject sparksPrefab;

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
    // How many beats it takes until the cursor extends
    public float beatsBetweenExtend;

    [HideInInspector]
    public float fadeEnd;
    // How much time (in seconds) has passed since the song started
    [HideInInspector]
    public float dsptimesong;
    // The offset to the first beat of the song in seconds
    public float firstBeatOffset;
    public int beatsInAdvance = 5;
    public int pathBeatsInAdvance = 10;
    public float bpm = 120;
    public float length;
    public bool paused = false;
    public float lengthInBeats;
    // Keep all the position-in-beats of notes in the song
    public Vector2[] notes;
    // Two note tracks needed for multi-line segments
    public Vector2[] notes2;
    [HideInInspector]
    /// <summary>
    /// The index of the next note to be spawned on track 1.
    /// </summary>
    public int nextIndex = 0;
    [HideInInspector]
    /// <summary>
    /// The index of the next note to be spawned on track 2.
    /// </summary>
    public int nextIndex2 = 0;

    /// <summary>
    /// The button on the first line that has UpNext
    /// </summary>
    public Button FirstLineNextButton = null;
    /// <summary>
    /// The button on the second line that has UpNext
    /// </summary>
    public Button SecondLineNextButton = null;

    // Camera animation
    public Vector2[] cameraKeyframes;

    private FollowMotionPath cursorMP;
    private MotionPath gamePathMP;
    private GameObject sparksObj;
    private bool cursorExtended = false;

    private GameHandler gameHandler;
    #endregion

    /// <summary>
    /// Gets the position of the camera on it's path depending on the keyframes provided
    /// </summary>
    /// <returns></returns>
    public float GetCamPathProgress()
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

    /// <summary>
    /// Causes the cursor animator to extend for hitting second line notes (purely visual)
    /// </summary>
    public void ExtendCursor(bool? state = null)
    {
        if (state == null)
            state = !cursorExtended;
        cursorExtended = state.Value;
        if (cursorAN.GetBool("Extended") != state)
        {
            cursorAN.SetBool("Extended", state.Value);
            cursorAN.SetTrigger("Extend");
        }
    }

    public void MoveSlider()
    {
        source.time = length * sliderObj.GetComponent<Slider>().value;
    }

    public List<ButtonClass> GetSpawnedButtons(bool firstTrack)
    {
        if (gameHandler == null) return new List<ButtonClass>();
        if (firstTrack)
        {
            return gameHandler.buttons.Where(btn => btn.btnClass != null).ToList();
        }

        return gameHandler.buttons2.Where(btn => btn.btnClass != null).ToList();
    }

    public List<ButtonClass> GetButtonsInTimingWindow(bool firstTrack)
    {
        if (gameHandler == null) return new List<ButtonClass>();
        if (firstTrack)
        {
            return gameHandler.buttons.Where(btn => ButtonWithinTimingWindow(btn.btnClass)).ToList();
        }

        return gameHandler.buttons2.Where(btn => ButtonWithinTimingWindow(btn.btnClass)).ToList();
    }

    public bool ButtonWithinTimingWindow(Button btn)
    {
        if(btn != null)
        {
            if(Button.GetRank(songPosInBeats, bpm, btn.beat) != GameHandler.Rank.Missed)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the closest note out of both lines, if they are both the same beat, returns null
    /// </summary>
    public Button GetClosestNote()
    {
        if (FirstLineNextButton == null || SecondLineNextButton == null) 
            return FirstLineNextButton != null ? FirstLineNextButton : SecondLineNextButton;
        if (FirstLineNextButton.beat > SecondLineNextButton.beat)
        {
            return SecondLineNextButton;
        }
        else if (FirstLineNextButton.beat < SecondLineNextButton.beat)
        {
            return FirstLineNextButton;
        }
        else
        {
            return null;
        }
    }

    public void BeginGame()
    {
        #region Init References
        cursorMP = cursor.GetComponent<FollowMotionPath>();
        gamePathMP = gamePath.GetComponent<MotionPath>();

        // Get reference to the Game Handler
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];
        #endregion

        CancelInvoke("MyUpdate");

        movie.Prepare();

        gameHandler.ResetButtons();

        #region Clear Variables
        source.Stop();
        source.time = 0;
        secPerBeat = 0;
        lengthInBeats = 0;
        length = 0;
        dsptimesong = 0;
        songPosInBeats = 0;
        songPosition = 0;
        nextIndex = 0;
        nextIndex2 = 0;

        if (sparksObj != null)
        {
            Destroy(sparksObj);
        }

        #endregion

        // Spawn the debug slider if needed
        if (gameHandler.debugMode)
        {
            sliderObj.SetActive(true);
        }

        // Calculate how many seconds is one beat
        // We will see the declaration of bpm later
        secPerBeat = 60f / bpm;

        lengthInBeats = (bpm / 60) * (song.length - firstBeatOffset);

        length = song.length - firstBeatOffset;

        // Get the time that the song starts
        dsptimesong = source.time;

        // Play the actual song
        source.clip = song;
        source.Play();

        if (!string.IsNullOrEmpty(moviePath))
        {
            moviePath = "file:///" + Path.GetDirectoryName(Application.dataPath) + "/" + moviePath;
            movie.url = moviePath;
            movie.Play();
            movie.playbackSpeed = 1; // Evenetually the user will select this
        }

        // Spawn the sparks on the end of the path
        sparksObj = Instantiate(sparksPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));

        InvokeRepeating("MyUpdate", 0, GameHandler.FrameTime); // Only run update a set amount of times per second
    }

    private void Update()
    {
        // Seek the movie (if it exists) to the part of the song we're at
        /*
        if (moviePath != "" && movie.isPlaying)
        {
            movie.frame = (long)(movie.frameCount * (songPosition / length));
        }
        */
    }

    /// <summary>
    /// Updates the lineRenderer's gradient so it fades away as the song progresses.
    /// Also updates the sparks on the line tip
    /// </summary>
    private void UpdateLineVisuals()
    {
        Gradient gradient = new Gradient();
        float gradient1 = ((songPosInBeats - fadeOffsetInBeats) / lengthInBeats) - bpm / 60 * 0.01f;
        fadeEnd = (songPosInBeats - fadeOffsetInBeats) / lengthInBeats;
        float end1 = (songPosInBeats + pathBeatsInAdvance) / lengthInBeats;
        float end2 = end1;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, gradient1), new GradientAlphaKey(1, fadeEnd), new GradientAlphaKey(1, end1), new GradientAlphaKey(0, end2), new GradientAlphaKey(0, 1.0f) }
        );
        gradient.mode = GradientMode.Blend;
        gamePathMP.line.GetComponent<LineRenderer>().colorGradient = gradient;

        // Move sparks to the tip of the line
        Vector3 sparksPos = cursorMP.motionPath.PointOnNormalizedPath(end1);
        Vector3 sparksNorm = cursorMP.motionPath.NormalOnNormalizedPath(end1);
        sparksObj.transform.position = sparksPos;
        sparksObj.transform.right = sparksNorm;
    }

    // Update is called once per frame
    private void MyUpdate()
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
        songPosition = source.time - dsptimesong - firstBeatOffset;

        // Calculate the position in beats
        songPosInBeats = songPosition / secPerBeat;

        // Handle extending the button if a second track button is coming up
        // This can probably be further optimized
        var trackTwoNotes = GetSpawnedButtons(false);
        bool anyTrackTwoNotes = trackTwoNotes.Any();
        float positionToExtend;
        if(anyTrackTwoNotes)
        {
            positionToExtend = trackTwoNotes.First().btnClass.beat - beatsBetweenExtend;
            if (songPosInBeats > positionToExtend && !cursorExtended)
            {
                ExtendCursor();
            }
        }
        else if (!anyTrackTwoNotes)
        {
            if (nextIndex2 > 0)
            {
                // Gets the last note in the second track and adds the beatsBetweenExtend
                positionToExtend = notes2[nextIndex2 - 1].x + beatsBetweenExtend;

                if (songPosInBeats > positionToExtend && cursorExtended)
                {
                    ExtendCursor();
                }
            }
        }

        // If it's time to spawn the next note based on the beatsInAdvance, do so
        if (nextIndex < notes.Length && notes[nextIndex].x < songPosInBeats + beatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            GameObject button = buttonSpawner.spawn(notes[nextIndex].x / lengthInBeats, notes[nextIndex].y, notes[nextIndex].x, nextIndex, 0);
            gameHandler.buttons.Add(new ButtonClass(button, gameHandler.inputs[button.GetComponent<Button>().type], gameHandler.inputsAlt[button.GetComponent<Button>().type], button.GetComponent<Button>()));

            nextIndex++;
        }

        // Same as before but for the second note track
        if (nextIndex2 < notes2.Length && notes2[nextIndex2].x < songPosInBeats + beatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            GameObject button = buttonSpawner.spawn(notes2[nextIndex2].x / lengthInBeats, notes2[nextIndex2].y, notes2[nextIndex2].x, nextIndex2, 1);
            gameHandler.buttons2.Add(new ButtonClass(button, gameHandler.inputs[button.GetComponent<Button>().type], gameHandler.inputsAlt[button.GetComponent<Button>().type], button.GetComponent<Button>()));

            nextIndex2++;
        }

        // Move debug slider to song positon
        if (gameHandler.debugMode)
        {
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.SetValueWithoutNotify(songPosInBeats / lengthInBeats);
        }

        // Fadeout line after passing
        UpdateLineVisuals();

        // Move the cursor and camera respective to the current pos and keyframes
        Vector3 cursorPos = cursorMP.motionPath.PointOnNormalizedPath(songPosInBeats / lengthInBeats);
        Vector3 cursorNorm = cursorMP.motionPath.NormalOnNormalizedPath(songPosInBeats / lengthInBeats);
        cursor.transform.position = cursorPos;
        if (cursorFlip == true) // Frankly i'm not sure why this is ever flipped, need to read code
        {
            cursor.transform.right = cursorNorm * -1;
        }
        else
        {
            cursor.transform.right = cursorNorm;
        }

        float final = GetCamPathProgress();
        Vector3 cameraPos = cameraMP.PointOnNormalizedPath(final);
        camera.transform.position = cameraPos;

        /* I'm Keeping this commented for now because even though I doubt i'll ever need this,
         * I have this feeling that I shouldnt go around thanos snapping all my old code.
        // Determines whether or not the button is the next that should be being hit
        foreach (ButtonClass button in gameHandler.buttons)
        {
            if (button.btn == null) continue;
            Button btn = button.btnClass;
            if (Button.GetRank(songPosInBeats, bpm, button.btnClass.beat) == GameHandler.Rank.Missed && btn.beat < songPosInBeats)
            {
                btn.upNext = false;
                //return;
            }
            else if ((gameHandler.buttons.Count != 0 && btn.index >= 0))
            {
                btn.upNext = true;
                FirstLineNextButton = btn;
                break;
            }
            else
            {
                btn.upNext = false;
            }
        }

        // Same thing for track 2
        foreach (ButtonClass button in gameHandler.buttons2)
        {
            if (button.btn == null) continue;
            Button btn = button.btnClass;
            if (Button.GetRank(songPosInBeats, bpm, button.btnClass.beat) == GameHandler.Rank.Missed && btn.beat < songPosInBeats)
            {
                btn.upNext = false;
                //return;
            }
            else if ((gameHandler.buttons2.Count != 0 && btn.index >= 0))
            {
                btn.upNext = true;
                SecondLineNextButton = btn;
                break;
            }
            else
            {
                btn.upNext = false;
            }
        }
        */
    }
}
