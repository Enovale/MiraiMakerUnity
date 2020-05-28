using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    /// <summary>
    /// What kind of button is this?
    /// </summary>
    public int NoteType;

    /// <summary>
    /// Which of the two tracks this button is on; 0 = first, 1 = second
    /// </summary>
    public int Track = 0;

    /// <summary>
    /// Which Beat this button plays on
    /// </summary>
    public float Beat;

    /// <summary>
    /// Where in the button list is this
    /// </summary>
    public int NoteIndex;

    // Sustain note
    public bool Sustain = false;
    public bool HoldingNote = false;

    // ButtonClass reference, shouldn't be used, but just in case
    public ButtonClass BtnClass;

    // Reference to the rank text prefab
    public GameObject rankPrefab;

    // Range which are sustain notes
    private static readonly int[] susRange = {8, 12};

    //Reference the game handlers
    private GameHandler gameHandler;
    private MusicHandler musicHandler;

    // Need reference to sprite renderer to change the NoteType
    private new SpriteRenderer renderer;

    /// <summary>
    /// Checks if an integer is between a given range
    /// </summary>
    /// <param name="numberToCheck">The integer you are checking</param>
    /// <param name="bottom">The first number in the range</param>
    /// <param name="top">The top number in the range</param>
    /// <returns></returns>
    public static bool IsSustain(int numberToCheck)
    {
        return numberToCheck >= susRange[0] && numberToCheck <= susRange[1];
    }

    /// <summary>
    /// Hits the note in question based on the rate parameter
    /// </summary>
    /// <param name="songPosInBeats">Position in the song in beats</param>
    /// <param name="bpm">Beats per Minute of the song</param>
    /// <param name="rate">Accuracy rating of the hit</param>
    public void Hit(float songPosInBeats, float bpm, GameHandler.Rank? rate = null)
    {
        if (rate == null)
            rate = GetRank(songPosInBeats, bpm, Beat);
        var rankText = Instantiate(rankPrefab, gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(rate.Value);
        Destroy(gameObject);
        gameHandler.NoteHitCounter[(int) rate]++;
    }

    /// <summary>
    /// Kills the button with a missed ranking
    /// </summary>
    public void Missed()
    {
        var rankText = Instantiate(rankPrefab, gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(GameHandler.Rank.Missed);
        Destroy(gameObject);
        gameHandler.NoteHitCounter[4]++;
    }

    /// <summary>
    /// Initiates the buttons defaults and sets any needed information
    /// </summary>
    /// <param name="typein">Type of Button as an integer, see GameHandler</param>
    /// <param name="curbeat">Current Beat of the song</param>
    /// <param name="indexin">The NoteIndex of the button array that this button belongs to</param>
    public void Init(int typein, float curbeat, int indexin, int trackin, Button buttonClass)
    {
        // Create button
        Beat = curbeat;
        gameHandler = FindObjectOfType<GameHandler>();
        musicHandler = FindObjectOfType<MusicHandler>();
        var types = gameHandler.NoteTypes;
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = types[typein];
        NoteType = typein;
        NoteIndex = indexin;
        Track = trackin;
        // For future me, this range is the "Hold" sprites in the GameHandler's NoteType array.
        Sustain = IsSustain(NoteType);
        BtnClass = new ButtonClass(gameObject, gameHandler.NoteInputs[NoteType], gameHandler.NoteInputsAlt[NoteType], buttonClass);
    }

    /// <summary>
    /// Determines what the current accuracy rating should be depending on the current state of the game.
    /// </summary>
    /// <param name="pos">Current song position in beats</param>
    /// <param name="bpm">Song Beats per Minute</param>
    /// <param name="beat">Beat of the note in question</param>
    /// <returns>The rank NoteIndex</returns>
    public static GameHandler.Rank GetRank(float pos, float bpm, float beat)
    {
        if (pos <= beat && pos > beat - bpm / 60 / 5 || pos >= beat && pos < beat + bpm / 60 / 5)
            return GameHandler.Rank.Cool;
        if (pos <= beat && pos > beat - bpm / 60 / 4 || pos >= beat && pos < beat + bpm / 60 / 4)
            return GameHandler.Rank.Fine;
        if (pos <= beat && pos > beat - bpm / 60 / 3 || pos >= beat && pos < beat + bpm / 60 / 3)
            return GameHandler.Rank.Safe;
        if (pos <= beat && pos > beat - bpm / 60 / 2 || pos >= beat && pos < beat + bpm / 60 / 2)
            return GameHandler.Rank.Sad;
        return GameHandler.Rank.Missed;
    }

    private void Update()
    {
        // If the button reaches the "fadeEnd" part of the BeatPath, kill it with a missed ranking
        if (Beat / musicHandler.LengthInBeats <= musicHandler.FadeEnd) 
            Missed();
    }
}