using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// This class manages the game's musical state, i.e spawning NoteVectors, cursor position, etc
/// </summary>
public class MusicHandler : MonoBehaviour
{
    #region References

    // Many gameobject references and references to the song file
    public AudioClip SongClip;
    public VideoPlayer MovieClip;
    public LevelClass Level;
    public AudioSource MyAS;
    public SpawnButtons ButtonSpawner;
    public GameObject CursorObj;
    public GameObject GameCamera;
    public Animator CursorAN;
    public MotionPath CameraMP;
    public GameObject GamePath;
    public GameObject SliderObj;
    public GameObject SparksPrefab;

    // The current position of the beatmap (in seconds)
    public float SongPosition;

    // The current position of the beatmap (in beats)
    public float SongPosInBeats;

    // Song has finished playing
    public bool SongFinished = false;

    // The duration of a Beat
    [HideInInspector] public float BeatLength;

    [HideInInspector]
    // I literally have no fucking clue what this is
    public float FadeEnd;

    // How much time (in seconds) has passed since the song started
    [HideInInspector] public float TimeSinceSongStart;

    public float SongLength;
    public bool Paused = false;

    public float LengthInBeats;

    [HideInInspector]
    /// <summary>
    /// The NoteIndex of the next note to be spawned on Track 1.
    /// </summary>
    public int NextNoteIndex = 0;

    [HideInInspector]
    /// <summary>
    /// The NoteIndex of the next note to be spawned on Track 2.
    /// </summary>
    public int NextNoteIndex2 = 0;

    /// <summary>
    /// The button on the first line that has UpNext
    /// </summary>
    public Button FirstLineNextButton = null;

    /// <summary>
    /// The button on the second line that has UpNext
    /// </summary>
    public Button SecondLineNextButton = null;

    private FollowMotionPath cursorMP;
    private MotionPath gamePathMP;
    private GameObject sparksObj;

    private GameHandler gameHandler;

    #endregion

    /// <summary>
    /// Gets the position of the camera on it's BeatPath depending on the keyframes provided
    /// </summary>
    /// <returns></returns>
    public float GetCamPathProgress()
    {
        // Assumed Beat Map has atleast 2 elements. TODO: correct error handling
        var startInterval = 0;
        var endInterval = 0;
        for (var i = 1; i < Level.CameraKeyframes.Length; i++)
        {
            if (SongPosInBeats <= Level.CameraKeyframes[i].x)
            {
                endInterval = i;
                break;
            }

            startInterval = i;
        }

        // Give the percents at the bounds of the Beat interval
        var startPercent = Level.CameraKeyframes[startInterval].y;
        var endPercent = Level.CameraKeyframes[endInterval].y;
        // Compute the total number of beats in this interval
        var intervalBeatSize = Level.CameraKeyframes[endInterval].x - Level.CameraKeyframes[startInterval].x;
        // Linearly interpolate the current Beat position as a BeatPath percentage
        // You could use some other update rule here if you don't want linear interpolation
        var percent = startPercent + (endPercent - startPercent) * (SongPosInBeats - Level.CameraKeyframes[startInterval].x) /
            intervalBeatSize;
        return percent;
    }

    /// <summary>
    /// Causes the cursor animator to extend for hitting second line NoteVectors (purely visual)
    /// </summary>
    public void ExtendCursor(bool? state = null)
    {
        if (state == null)
            state = !gameHandler.cursorExtended;
        gameHandler.cursorExtended = state.Value;
        if (CursorAN.GetBool("Extended") != state)
        {
            CursorAN.SetBool("Extended", state.Value);
            CursorAN.SetTrigger("Extend");
        }
    }

    public void MoveSlider()
    {
        MyAS.time = SongLength * SliderObj.GetComponent<Slider>().value;
    }

    public List<ButtonClass> GetSpawnedButtons(bool firstTrack)
    {
        if (gameHandler == null) return new List<ButtonClass>();
        if (firstTrack) 
            return gameHandler.SongButtons.Where(btn => btn.Button != null).ToList();

        return gameHandler.SongButtons2.Where(btn => btn.Button != null).ToList();
    }

    public List<ButtonClass> GetButtonsInTimingWindow(bool firstTrack)
    {
        if (gameHandler == null) return new List<ButtonClass>();
        if (firstTrack) return gameHandler.SongButtons.Where(btn => ButtonWithinTimingWindow(btn.Button)).ToList();

        return gameHandler.SongButtons2.Where(btn => ButtonWithinTimingWindow(btn.Button)).ToList();
    }

    public bool ButtonWithinTimingWindow(Button btn)
    {
        if (btn != null)
            if (Button.GetRank(SongPosInBeats, Level.BPM, btn.Beat) != GameHandler.Rank.Missed)
                return true;
        return false;
    }

    /// <summary>
    /// Returns the closest note out of both lines, if they are both the same Beat, returns null
    /// </summary>
    public Button GetClosestNote()
    {
        if (FirstLineNextButton == null || SecondLineNextButton == null)
            return FirstLineNextButton != null ? FirstLineNextButton : SecondLineNextButton;
        if (FirstLineNextButton.Beat > SecondLineNextButton.Beat)
            return SecondLineNextButton;
        else if (FirstLineNextButton.Beat < SecondLineNextButton.Beat)
            return FirstLineNextButton;
        else
            return null;
    }

    public void BeginGame()
    {
        #region Init References

        cursorMP = CursorObj.GetComponent<FollowMotionPath>();
        gamePathMP = GamePath.GetComponent<MotionPath>();

        // Get reference to the Game Handler
        gameHandler = FindObjectOfType<GameHandler>();

        #endregion

        CancelInvoke(nameof(MyUpdate));

        MovieClip.Prepare();

        gameHandler.ResetButtons();

        #region Clear Variables

        MyAS.Stop();
        MyAS.time = 0;
        BeatLength = 0;
        LengthInBeats = 0;
        SongLength = 0;
        TimeSinceSongStart = 0;
        SongPosInBeats = 0;
        SongPosition = 0;
        NextNoteIndex = 0;
        NextNoteIndex2 = 0;

        if (sparksObj != null) Destroy(sparksObj);

        #endregion

        // Spawn the debug slider if needed
        if (gameHandler.DebugMode) SliderObj.SetActive(true);

        // Calculate how many seconds is one Beat
        // We will see the declaration of BPM later
        BeatLength = 60f / Level.BPM;

        LengthInBeats = Level.BPM / 60 * (SongClip.length - Level.FirstBeatOffset);

        SongLength = SongClip.length - Level.FirstBeatOffset;

        // Get the time that the song starts
        TimeSinceSongStart = MyAS.time;

        // Play the actual song
        MyAS.clip = SongClip;
        MyAS.Play();

        if (!string.IsNullOrEmpty(Level.MoviePath))
        {
            Level.MoviePath = "file:///" + Path.GetDirectoryName(Application.dataPath) + "/" + Level.MoviePath;
            MovieClip.url = Level.MoviePath;
            MovieClip.Play();
            MovieClip.playbackSpeed = 1; // Evenetually the user will select this
        }

        // Spawn the sparks on the end of the BeatPath
        sparksObj = Instantiate(SparksPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));

        InvokeRepeating("MyUpdate", 0, GameHandler.FrameTime); // Only run update a set amount of times per second
    }

    private void Update()
    {
        // Seek the movie (if it exists) to the part of the song we're at
        /*
        if (MoviePath != "" && movie.isPlaying)
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
        var gradient = new Gradient();
        var gradient1 = (SongPosInBeats - Level.FadeOffsetInBeats) / LengthInBeats - Level.BPM / 60 * 0.01f;
        FadeEnd = (SongPosInBeats - Level.FadeOffsetInBeats) / LengthInBeats;
        var end1 = (SongPosInBeats + Level.PathBeatsInAdvance) / LengthInBeats;
        var end2 = end1;
        gradient.SetKeys(
            new GradientColorKey[] {new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f)},
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0, gradient1), new GradientAlphaKey(1, FadeEnd), new GradientAlphaKey(1, end1),
                new GradientAlphaKey(0, end2), new GradientAlphaKey(0, 1.0f)
            }
        );
        gradient.mode = GradientMode.Blend;
        gamePathMP.line.GetComponent<LineRenderer>().colorGradient = gradient;

        // Move sparks to the tip of the line
        var sparksPos = cursorMP.motionPath.PointOnNormalizedPath(end1);
        var sparksNorm = cursorMP.motionPath.NormalOnNormalizedPath(end1);
        sparksObj.transform.position = sparksPos;
        sparksObj.transform.right = sparksNorm;
    }

    // Update is called once per frame
    private void MyUpdate()
    {
        //Debugging
        if (gameHandler.DebugMode == true)
        {
            //print(nextIndex);
        }

        // Set finished to true if the song is over
        if (MyAS.isPlaying != true && Paused == false) SongFinished = true;

        // Calculate the position in seconds
        SongPosition = MyAS.time - TimeSinceSongStart - Level.FirstBeatOffset;

        // Calculate the position in beats
        SongPosInBeats = SongPosition / BeatLength;

        // Handle extending the button if a second Track button is coming up
        // This can probably be further optimized
        var trackTwoNotes = GetSpawnedButtons(false);
        var anyTrackTwoNotes = trackTwoNotes.Any();
        float positionToExtend;
        if (anyTrackTwoNotes)
        {
            positionToExtend = trackTwoNotes.First().Button.Beat - Level.BeatsBetweenExtend;
            if (SongPosInBeats > positionToExtend && !gameHandler.cursorExtended) ExtendCursor();
        }
        else if (!anyTrackTwoNotes)
        {
            if (NextNoteIndex2 > 0)
            {
                // Gets the last note in the second Track and adds the BeatsBetweenExtend
                positionToExtend = Level.NoteVectors2[NextNoteIndex2 - 1].x + Level.BeatsBetweenExtend;

                if (SongPosInBeats > positionToExtend && gameHandler.cursorExtended) ExtendCursor();
            }
        }

        // If it's time to spawn the next note based on the BeatsInAdvance, do so
        if (NextNoteIndex < Level.NoteVectors.Length && Level.NoteVectors[NextNoteIndex].x < SongPosInBeats + Level.BeatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            var button = ButtonSpawner.spawn(Level.NoteVectors[NextNoteIndex].x / LengthInBeats, Level.NoteVectors[NextNoteIndex].y,
               Level. NoteVectors[NextNoteIndex].x, NextNoteIndex, 0);
            gameHandler.SongButtons.Add(new ButtonClass(button,
                gameHandler.NoteInputs[button.GetComponent<Button>().NoteType],
                gameHandler.NoteInputsAlt[button.GetComponent<Button>().NoteType], button.GetComponent<Button>()));

            NextNoteIndex++;
        }

        // Same as before but for the second note Track
        if (NextNoteIndex2 < Level.NoteVectors2.Length && Level.NoteVectors2[NextNoteIndex2].x < SongPosInBeats + Level.BeatsInAdvance)
        {
            // Spawn it and initialize the fields of the music note
            var button = ButtonSpawner.spawn(Level.NoteVectors2[NextNoteIndex2].x / LengthInBeats,
                Level.NoteVectors2[NextNoteIndex2].y, Level.NoteVectors2[NextNoteIndex2].x, NextNoteIndex2, 1);
            gameHandler.SongButtons2.Add(new ButtonClass(button,
                gameHandler.NoteInputs[button.GetComponent<Button>().NoteType],
                gameHandler.NoteInputsAlt[button.GetComponent<Button>().NoteType], button.GetComponent<Button>()));

            NextNoteIndex2++;
        }

        // Move debug slider to song positon
        if (gameHandler.DebugMode)
        {
            var slider = SliderObj.GetComponent<Slider>();
            slider.SetValueWithoutNotify(SongPosInBeats / LengthInBeats);
        }

        // Fadeout line after passing
        UpdateLineVisuals();

        // Move the cursor and camera respective to the current pos and keyframes
        var cursorPos = cursorMP.motionPath.PointOnNormalizedPath(SongPosInBeats / LengthInBeats);
        var cursorNorm = cursorMP.motionPath.NormalOnNormalizedPath(SongPosInBeats / LengthInBeats);
        CursorObj.transform.position = cursorPos;
        if (gameHandler.cursorFlipped == true) // Frankly i'm not sure why this is ever flipped, need to read code
            CursorObj.transform.right = cursorNorm * -1;
        else
            CursorObj.transform.right = cursorNorm;

        var final = GetCamPathProgress();
        var cameraPos = CameraMP.PointOnNormalizedPath(final);
        GameCamera.transform.position = cameraPos;

        /* I'm Keeping this commented for now because even though I doubt i'll ever need this,
         * I have this feeling that I shouldnt go around thanos snapping all my old code.
        // Determines whether or not the button is the next that should be being hit
        foreach (ButtonClass button in gameHandler.buttons)
        {
            if (button.ButtonObj == null) continue;
            Button ButtonObj = button.Button;
            if (Button.GetRank(songPosInBeats, BPM, button.Button.Beat) == GameHandler.Rank.Missed && ButtonObj.Beat < songPosInBeats)
            {
                ButtonObj.upNext = false;
                //return;
            }
            else if ((gameHandler.buttons.Count != 0 && ButtonObj.NoteIndex >= 0))
            {
                ButtonObj.upNext = true;
                FirstLineNextButton = ButtonObj;
                break;
            }
            else
            {
                ButtonObj.upNext = false;
            }
        }

        // Same thing for Track 2
        foreach (ButtonClass button in gameHandler.buttons2)
        {
            if (button.ButtonObj == null) continue;
            Button ButtonObj = button.Button;
            if (Button.GetRank(songPosInBeats, BPM, button.Button.Beat) == GameHandler.Rank.Missed && ButtonObj.Beat < songPosInBeats)
            {
                ButtonObj.upNext = false;
                //return;
            }
            else if ((gameHandler.buttons2.Count != 0 && ButtonObj.NoteIndex >= 0))
            {
                ButtonObj.upNext = true;
                SecondLineNextButton = ButtonObj;
                break;
            }
            else
            {
                ButtonObj.upNext = false;
            }
        }
        */
    }
}