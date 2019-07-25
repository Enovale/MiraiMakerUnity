using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class that holds information about a button concisely, for use in Ispector and the button array
[System.Serializable]
public class ButtonClass
{
    public GameObject btn;
    public KeyCode key;
    public KeyCode keyAlt;
    public Button btnClass;

    public ButtonClass(GameObject button, KeyCode code, KeyCode codeAlt, Button buttonClass)
    {
        // Button gameobject reference
        this.btn = button;
        // Keycode to hit button according to the input array
        this.key = code;
        this.keyAlt = codeAlt;
        this.btnClass = buttonClass;
    }
}

public class Button : MonoBehaviour
{
    // Which button it is
    public int type;

    // Which track its on
    public int track = 0;

    // The beat that this button resides on
    public float beat;
    // Which button in the list is this?
    public int index;

    // Range which are sustain notes
    public static int[] susRange = { 8, 12 };

    public bool upNext = false;
    // Sustain note
    public bool sus = false;
    public bool holdingSus = false;

    // ButtonClass reference, shouldn't be used, but just in case
    public ButtonClass btn;

    public Button pair = null;
    // Reference to the rank text prefab
    public GameObject rankPrefab;
    //Reference the game handlers
    private GameHandler gameHandler;
    private MusicHandler musicHandler;
    // Need reference to sprite renderer to change the type
    private new SpriteRenderer renderer;

    /// <summary>
    /// Checks if an integer is between a given range
    /// </summary>
    /// <param name="numberToCheck">The integer you are checking</param>
    /// <param name="bottom">The first number in the range</param>
    /// <param name="top">The top number in the range</param>
    /// <returns></returns>
    public static bool IsSustain(float numberToCheck)
    {
        numberToCheck = Mathf.RoundToInt(numberToCheck);
        return (numberToCheck >= susRange[0] && numberToCheck <= susRange[1]);
    }

    /// <summary>
    /// Hits the note in question based on the rate parameter
    /// </summary>
    /// <param name="songPosInBeats">Position in the song in beats</param>
    /// <param name="bpm">Beats per Minute of the song</param>
    /// <param name="rate">Accuracy rating of the hit</param>
    public void Hit(float songPosInBeats, float bpm, int rate)
    {
        GameObject rankText = Instantiate(rankPrefab, this.gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(rate);
        Destroy(this.gameObject);
        gameHandler.hits[rate]++;
    }

    /// <summary>
    /// Kills the button with a missed ranking
    /// </summary>
    public void Missed()
    {
        GameObject rankText = Instantiate(rankPrefab, this.gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(4);
        Destroy(this.gameObject);
        gameHandler.hits[4]++;
    }

    /// <summary>
    /// Initiates the buttons defaults and sets any needed information
    /// </summary>
    /// <param name="typein">Type of Button as an integer, see GameHandler</param>
    /// <param name="curbeat">Current beat of the song</param>
    /// <param name="indexin">The index of the button array that this button belongs to</param>
    public void Init(int typein, float curbeat, int indexin, int trackin, Button buttonClass)
    {
        // Create button
        beat = curbeat;
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];
        MusicHandler[] objects2 = FindObjectsOfType<MusicHandler>();
        musicHandler = objects2[0];
        Sprite[] types = objects[0].types;
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = types[typein];
        type = typein;
        index = indexin;
        track = trackin;
        // For future me, this range is the "Hold" sprites in the GameHandler's type array.
        sus = IsSustain(type);
        btn = new ButtonClass(this.gameObject, gameHandler.inputs[type], gameHandler.inputsAlt[type], buttonClass);
    }

    /// <summary>
    /// Determines what the current accuracy rating should be depending on the current state of the game.
    /// </summary>
    /// <param name="pos">Current song position in beats</param>
    /// <param name="bpm">Song Beats per Minute</param>
    /// <param name="beat">Beat of the note in question</param>
    /// <returns>The rank index</returns>
    public int GetRank(float pos, float bpm, float beat)
    {
        // Cool Rank
        if ((pos <= beat && pos > beat - ((bpm / 60) / 5)) || (pos >= beat && pos < beat + ((bpm / 60) / 5)))
        {
            return 0;
        }
        // Fine rank
        else if ((pos <= beat && pos > beat - ((bpm / 60) / 3)) || (pos >= beat && pos < beat + ((bpm / 60) / 3)))
        {
            return 1;
        }
        // Safe rank
        else if ((pos <= beat && pos > beat - ((bpm / 60) / 2)) || (pos >= beat && pos < beat + ((bpm / 60) / 2)))
        {
            return 2;
        }
        // Sad Rank
        else if ((pos <= beat && pos > beat - ((bpm / 60))) || (pos >= beat && pos < beat + ((bpm / 60))))
        {
            return 3;
        }
        // Missed
        else
        {
            return 4;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // If the button reaches the "fadeEnd" part of the path, kill it with a missed ranking
        if ((beat / musicHandler.lengthInBeats) <= musicHandler.fadeEnd)
        {
            Missed();
        }

        bool upNextTrack2 = false;

        // Prototype code; FUTURE ME, PLEASE OPTIMIZE
        foreach (ButtonClass altBtn in gameHandler.buttons2)
        {
            if (altBtn.btnClass.upNext == true && GetRank(altBtn.btnClass.beat, musicHandler.bpm, beat) != 4)
            {
                if (track == 0)
                {
                    pair = altBtn.btnClass;
                }
                upNextTrack2 = true;
                break;
            }
        }

        bool upNextTrack1 = false;

        // Prototype code; FUTURE ME, PLEASE OPTIMIZE
        foreach (ButtonClass altBtn in gameHandler.buttons)
        {
            if (altBtn.btnClass.upNext == true && GetRank(altBtn.btnClass.beat, musicHandler.bpm, beat) != 4)
            {
                if (track == 1)
                {
                    pair = altBtn.btnClass;
                }
                upNextTrack1 = true;
                break;
            }
        }

        // connectedClients.Find(i => i.endPoint.ToString() == e.RemoteEndPoint.ToString());

        KeyCode pressedKey;

        // Prototype code; FUTURE ME, PLEASE OPTIMIZE IF POSSIBLE
        int i = 0;
        foreach (KeyCode kcode in gameHandler.inputs)
        {
            if (Input.GetKeyDown(kcode))
            {
                pressedKey = kcode;
                if (upNextTrack2 == false && kcode != btn.key && kcode != btn.keyAlt && GetRank(musicHandler.songPosInBeats, musicHandler.bpm, beat) != 4 && upNext == true)
                {
                    Missed();
                }
            }
            i++;
        }

        // If the designated key is hit
        if (Input.GetKeyDown(btn.key) || Input.GetKeyDown(btn.keyAlt))
        {
            if(sus)
            {
                return;
            }

            if (track == 0 && upNextTrack2 == true)
            {
                print("killed");
                return;
            }
            else if (track == 1 && upNextTrack1 == true)
            {
                print("killed2");
                return;
            }
            // If this button is the next button that should be hit, get the rank and hit it
            float pos = musicHandler.songPosInBeats;
            float bpm = musicHandler.bpm;
            // If you would have missed the previous note anyway, hit this one
            if (upNext == false)
            {
                return;
            }
            Hit(pos, bpm, GetRank(pos, bpm, beat));
        }
        if(Input.GetKey(btn.key) || Input.GetKey(btn.keyAlt))
        {
            if(!sus)
            {
                return;
            }
            holdingSus = true;
        } else
        {
            if (!sus)
            {
                return;
            }
            holdingSus = false;
        }
    }
}
