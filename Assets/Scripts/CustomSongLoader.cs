using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CustomSongLoader : MonoBehaviour
{
    public MusicHandler musicHandler;
    public string filePath;

    private void Awake()
    {
        //WriteLevelToJSON();
        //LoadLevelFromJSON();
        print(Path.GetDirectoryName(Application.dataPath));
    }

    public void LoadLevelFromJSON()
    {
        var reader = new StreamReader(Path.GetDirectoryName(Application.dataPath) + "/" + filePath);
        var json = reader.ReadToEnd();
        reader.Close();
        var level = JsonUtility.FromJson<LevelClass>(json);
        musicHandler.Level = level;
        musicHandler.GamePath.GetComponent<MotionPath>().controlPoints = level.BeatPath;
        musicHandler.CameraMP.controlPoints = level.CamPath;
        var url = Path.GetDirectoryName(Application.dataPath) + "/";
        url += level.SongPath;
        musicHandler.Level.SongPath = url;
        musicHandler.Level.MoviePath = level.MoviePath;
        StartCoroutine(LoadAudio(url, Path.GetFileNameWithoutExtension(level.SongPath)));
    }

    private IEnumerator LoadAudio(string url, string clipName)
    {
        var furl = "file:///" + url;
        AudioType type;
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

        using (var www = UnityWebRequestMultimedia.GetAudioClip(furl, type))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip ac;
                switch (type)
                {
                    case AudioType.MPEG: /*
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

                ac.name = clipName;
                musicHandler.SongClip = ac;
                musicHandler.BeginGame();
            }
        }
    }


    public void WriteLevelToJson()
    {
        var writer = new StreamWriter(filePath);
        writer.Write(LevelToJson());
        writer.Close();
    }

    private string LevelToJson()
    {
        var json = JsonUtility.ToJson(musicHandler.Level, true);
        return json;
    }
}