﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelClass
{
    public string levelName;
    public string songPath;
    public string moviePath;
    public float bpm;
    public float firstBeatOffset;
    public int beatsInAdvance;
    public int pathBeatsInAdvance;
    public float fadeOffsetInBeats;
    public float beatsBetweenExtend;
#pragma warning disable CA2235 // Mark all non-serializable fields
    public Vector2[] notes;
    public Vector2[] notes2;
    public Vector2[] cameraKeyframes;
    public Vector3[] path;
    public Vector3[] camPath;
#pragma warning restore CA2235 // Mark all non-serializable fields

    public static LevelClass CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<LevelClass>(jsonString);
    }
}

public class CustomSongLoader : MonoBehaviour
{

    public MusicHandler musicHandler;
    public string filePath;

    void Awake()
    {
        //WriteLevelToJSON();
        //LoadLevelFromJSON();
        print(Path.GetDirectoryName(Application.dataPath));
    }

    public void LoadLevelFromJSON()
    {
        StreamReader reader = new StreamReader(Path.GetDirectoryName(Application.dataPath) + "/" + filePath);
        string json = reader.ReadToEnd();
        reader.Close();
        LevelClass level = JsonUtility.FromJson<LevelClass>(json);
        musicHandler.bpm = level.bpm;
        musicHandler.levelName = level.levelName;
        musicHandler.notes = level.notes;
        musicHandler.notes2 = level.notes2;
        musicHandler.cameraKeyframes = level.cameraKeyframes;
        musicHandler.fadeOffsetInBeats = level.fadeOffsetInBeats;
        musicHandler.firstBeatOffset = level.firstBeatOffset;
        musicHandler.beatsBetweenExtend = level.beatsBetweenExtend;
        musicHandler.beatsInAdvance = level.beatsInAdvance;
        musicHandler.pathBeatsInAdvance = level.pathBeatsInAdvance;
        musicHandler.gamePath.GetComponent<MotionPath>().controlPoints = level.path;
        musicHandler.cameraMP.controlPoints = level.camPath;
        musicHandler.songPath = level.songPath;
        string url = Path.GetDirectoryName(Application.dataPath) + "/";
        string movieURL = url;
        url += level.songPath;
        movieURL = level.moviePath;
        musicHandler.moviePath = movieURL;
        StartCoroutine(LoadAudio(url, Path.GetFileNameWithoutExtension(level.songPath)));
    }

    IEnumerator LoadAudio(string url, string name)
    {
        string furl = "file:///" + url;
        AudioType type;
        print(Path.GetExtension(url).ToLower());
        switch (Path.GetExtension(url).ToLower())
        {
            case ".mp3":
                type = AudioType.MPEG;
                break;
            case ".wav":
                type = AudioType.WAV;
                break;
            case ".ogg":
                type = AudioType.OGGVORBIS;
                break;
            case ".xma":
                type = AudioType.XMA;
                break;
            default:
                type = AudioType.UNKNOWN;
                break;
        }
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(furl, type))
        {
            yield return www.SendWebRequest();
            //yield return www.Send();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip ac;
                switch (type)
                {
                    case AudioType.MPEG:/*
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        ac = NAudioPlayer.FromMp3Data(www.downloadHandler.data);
                        break;
#else
                        Debug.LogError("MP3 Playback only supported on windows at the moment. Please contact me if you know how to convert MP3 to WAV bytes universally in Unity.");
#endif*/
                        ac = NAudioPlayer.FromMp3Data(www.downloadHandler.data);
                        break;
                    default:
                        ac = DownloadHandlerAudioClip.GetContent(www);
                        break;
                }
                ac.name = name;
                musicHandler.song = ac;
                musicHandler.BeginGame();
            }
        }
    }


    public void WriteLevelToJSON()
    {
        StreamWriter writer = new StreamWriter(filePath);
        writer.Write(LevelToJson());
        writer.Close();
    }

    string LevelToJson()
    {
        LevelClass level = new LevelClass();
        level.levelName = musicHandler.levelName;
        level.bpm = musicHandler.bpm;
        level.notes = musicHandler.notes;
        level.notes2 = musicHandler.notes2;
        level.cameraKeyframes = musicHandler.cameraKeyframes;
        level.firstBeatOffset = musicHandler.firstBeatOffset;
        level.fadeOffsetInBeats = musicHandler.fadeOffsetInBeats;
        level.beatsInAdvance = musicHandler.beatsInAdvance;
        level.pathBeatsInAdvance = musicHandler.pathBeatsInAdvance;
        level.beatsBetweenExtend = musicHandler.beatsBetweenExtend;
        level.songPath = musicHandler.songPath;
        level.moviePath = musicHandler.moviePath;
        level.path = musicHandler.gamePath.GetComponent<MotionPath>().controlPoints;
        level.camPath = musicHandler.cameraMP.controlPoints;
        string json = JsonUtility.ToJson(level, true);
        return json;
    }
}
