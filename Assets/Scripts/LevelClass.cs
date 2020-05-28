using System;
using UnityEngine;

[Serializable]
public class LevelClass
{
    // Music handler should keep this level as an object and use that instead of keeping duplicate shit
    public string LevelName;
    public string SongPath;
    public string MoviePath;
    public float BPM;
    // The offset to the first Beat of the song in seconds
    public float FirstBeatOffset;
    public int BeatsInAdvance;
    public int PathBeatsInAdvance;
    public float FadeOffsetInBeats;
    // How many beats it takes until the cursor extends
    public float BeatsBetweenExtend;
#pragma warning disable CA2235 // Mark all non-serializable fields
    public Vector2[] NoteVectors;
    public Vector2[] NoteVectors2;
    // Camera animation
    public Vector2[] CameraKeyframes;
    public Vector3[] BeatPath;
    public Vector3[] CamPath;
#pragma warning restore CA2235 // Mark all non-serializable fields

    public static LevelClass CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<LevelClass>(jsonString);
    }
}
