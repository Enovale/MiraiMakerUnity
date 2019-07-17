using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{

    public AudioClip song;
    public AudioSource source;
    public SpawnButtons buttonSpawner;
    public GameObject cursor;
    public new GameObject camera;
    public GameObject gamePath;

    public GameObject sparksPrefab;
    private GameObject sparksObj;

    //the current position of the song (in seconds)
    public float songPosition;

    //the current position of the song (in beats)
    public float songPosInBeats;

    //the duration of a beat
    [HideInInspector]
    public float secPerBeat;

    //how much time (in seconds) has passed since the song started
    [HideInInspector]
    public float dsptimesong;

    public int beatsInAdvance = 3;

    public int pathBeatsInAdvance = 6;

    public float bpm = 205;

    public float lengthInBeats;

    //keep all the position-in-beats of notes in the song
    public Vector2[] notes;

    //the index of the next note to be spawned
    int nextIndex = 0;

    // Camera animation
    public Vector2[] cameraKeyframes;

    private GameHandler gameHandler;

    /// Takes float beat input to match Vector2 type.
    float GetPathProgress()
    {
        // Assumed beatMap has atleast 2 elements. TODO: correct error handling
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

        sparksObj = Instantiate(sparksPrefab, new Vector3(0,0,0), new Quaternion(0,0,0,0));

        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];

        //calculate how many seconds is one beat
        //we will see the declaration of bpm later
        secPerBeat = 60f / bpm;

        lengthInBeats = (bpm / 60) * song.length;

        //record the time when the song starts
        dsptimesong = (float)source.time;

        source.clip = song;

        source.Play();

        
        //cursor.GetComponent<FollowMotionPath>().speed = cursor.GetComponent<FollowMotionPath>().motionPath.length / song.length;
        //camera.GetComponent<FollowMotionPath>().speed = (bpm / 60) / lengthInBeats;
    }

    // Update is called once per frame
    void Update()
    {
        //calculate the position in seconds
        songPosition = (float)(source.time - dsptimesong);

        //calculate the position in beats
        songPosInBeats = songPosition / secPerBeat;

        if (nextIndex < notes.Length && notes[nextIndex].x < songPosInBeats + beatsInAdvance)
        {
            GameObject button = buttonSpawner.spawn(notes[nextIndex].x / lengthInBeats, notes[nextIndex].y, songPosInBeats);
            gameHandler.buttons.Add(button);
            //initialize the fields of the music note

            nextIndex++;
        }
        Vector3 cursorPos = cursor.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(songPosInBeats / lengthInBeats);
        cursor.transform.position = cursorPos;
        float final = GetPathProgress();
        Vector3 cameraPos = camera.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(final);
        camera.transform.position = cameraPos;

        // Fadeout line after passing
        Gradient gradient = new Gradient();
        float gradient1 = ((songPosInBeats / lengthInBeats) - (bpm / 60) * 0.01f);
        float gradient2 = ((songPosInBeats / lengthInBeats));
        float end1 = (songPosInBeats + pathBeatsInAdvance) / lengthInBeats;
        float end2 = end1;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0, gradient1), new GradientAlphaKey(1, gradient2), new GradientAlphaKey(1, end1), new GradientAlphaKey(0, end2), new GradientAlphaKey(0, 1.0f) }
        );
        gradient.mode = GradientMode.Blend;
        gamePath.GetComponent<MotionPath>().line.GetComponent<LineRenderer>().colorGradient = gradient;
        Vector3 sparksPos = cursor.GetComponent<FollowMotionPath>().motionPath.PointOnNormalizedPath(end1);
        sparksObj.transform.position = sparksPos;
    }
}
