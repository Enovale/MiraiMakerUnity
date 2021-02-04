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

    public bool Sustain;

    // ButtonClass reference, shouldn't be used, but just in case
    public SerializedButton SerializedButton;

    // Reference to the rank text prefab
    public GameObject rankPrefab;

    //Reference the game handlers
    private GameHandler _gameHandler;
    private MusicHandler _musicHandler;

    // Need reference to sprite renderer to change the NoteType
    private SpriteRenderer _renderer;

    /// <summary>
    /// Hits the note in question based on the rate parameter
    /// </summary>
    /// <param name="songPosInBeats">Position in the song in beats</param>
    /// <param name="bpm">Beats per Minute of the song</param>
    /// <param name="rate">Accuracy rating of the hit</param>
    public void Hit(float songPosInBeats, float bpm, Rank? rate = null)
    {
        rate ??= GetRank(songPosInBeats, bpm, Beat);
        var rankText = Instantiate(rankPrefab, gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(rate.Value);
        Destroy(gameObject);
        _gameHandler.NoteHitCounter[(int) rate]++;
    }

    /// <summary>
    /// Kills the button with a missed ranking
    /// </summary>
    public void Missed()
    {
        var rankText = Instantiate(rankPrefab, gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(Rank.Missed);
        Destroy(gameObject);
        _gameHandler.NoteHitCounter[4]++;
    }

    /// <summary>
    /// Initiates the buttons defaults and sets any needed information
    /// </summary>
    public void Init(int type, bool sustain, float curbeat, int track)
    {
        // Create button
        Beat = curbeat;
        _gameHandler = FindObjectOfType<GameHandler>();
        _musicHandler = FindObjectOfType<MusicHandler>();
        var types = sustain ? _gameHandler.HoldTypes : _gameHandler.NoteTypes;
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = types[type];
        NoteType = type;
        Track = track;
        Sustain = sustain;
        SerializedButton = new SerializedButton(gameObject, _gameHandler.NoteInputs[NoteType], _gameHandler.NoteInputsAlt[NoteType], this);
    }

    /// <summary>
    /// Determines what the current accuracy rating should be depending on the current state of the game.
    /// </summary>
    /// <param name="pos">Current song position in beats</param>
    /// <param name="bpm">Song Beats per Minute</param>
    /// <param name="beat">Beat of the note in question</param>
    /// <returns>The rank NoteIndex</returns>
    public static Rank GetRank(float pos, float bpm, float beat)
    {
        if (pos <= beat && pos > beat - bpm / 60 / 5 || pos >= beat && pos < beat + bpm / 60 / 5)
            return Rank.Cool;
        if (pos <= beat && pos > beat - bpm / 60 / 4 || pos >= beat && pos < beat + bpm / 60 / 4)
            return Rank.Fine;
        if (pos <= beat && pos > beat - bpm / 60 / 3 || pos >= beat && pos < beat + bpm / 60 / 3)
            return Rank.Safe;
        if (pos <= beat && pos > beat - bpm / 60 / 2 || pos >= beat && pos < beat + bpm / 60 / 2)
            return Rank.Sad;
        return Rank.Missed;
    }

    private void Update()
    {
        // If the button reaches the "fadeEnd" part of the BeatPath, kill it with a missed ranking
        if (Beat / _musicHandler.LengthInBeats <= _musicHandler.FadeEnd) 
            Missed();
    }
}