using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    private GameHandler gameHandler;
    private MusicHandler musicHandler;

    private void Awake()
    {
        gameHandler = FindObjectsOfType<GameHandler>()[0];
        musicHandler = FindObjectsOfType<MusicHandler>()[0];
    }

    private void Update()
    {
        if (musicHandler == null) return;

        HandleInput();
    }

    /// <summary>
    /// Handle note registration.
    /// 
    /// Here's how it works:
    /// "Next" refers to the earliest note in the timing window.
    /// On key press:
    /// - Is the next note a double note? (two notes on the same beat for each track)
    ///   - No: Handle normally, end method
    /// - Get the next note on track one
    ///   - Is there one that matches the key we pressed?
    ///     - Yes: Hit it
    ///     - Hit both inputs at once: Hit the first, then go to No
    ///     - No: Is there one on the second track?
    ///       - No: Miss the next note on track 1, track 2 if there are none
    ///       - Yes: Hit it
    /// </summary>
    private void HandleInput()
    {
        if (Input.anyKeyDown)
        {
            List<ButtonClass> trackOneNotes = musicHandler.GetButtonsInTimingWindow(true);
            List<ButtonClass> trackTwoNotes = musicHandler.GetButtonsInTimingWindow(false);

            var pos = musicHandler.SongPosInBeats;
            var bpm = musicHandler.BPM;
            if (trackOneNotes.Any() && trackTwoNotes.Any())
            {
                ButtonClass btn1 = trackOneNotes.First();
                ButtonClass btn2 = trackTwoNotes.First();
                if (btn1.btnClass.beat < btn2.btnClass.beat)
                {
                    if (Input.GetKeyDown(btn1.key) || Input.GetKeyDown(btn1.keyAlt))
                    {
                        HandlePressNote(btn1.btnClass);
                        return;
                    }
                    else
                    {
                        btn1.btnClass.Missed();
                        return;
                    }
                }
                else if (btn1.btnClass.beat > btn2.btnClass.beat)
                {
                    if (Input.GetKeyDown(btn2.key) || Input.GetKeyDown(btn2.keyAlt))
                    {
                        HandlePressNote(btn2.btnClass);
                        return;
                    }
                    else
                    {
                        btn2.btnClass.Missed();
                        return;
                    }
                }
            }

            bool buttonHit = false;
            foreach (ButtonClass button in trackOneNotes)
            {
                Button btnClass = button.btnClass;

                // If you hit both on the same frame, pretend like you didnt hit the first one.
                if (Input.GetKeyDown(button.key) && Input.GetKeyDown(button.keyAlt))
                {
                    btnClass.Hit(pos, bpm);
                    break;
                }
                else if (Input.GetKeyDown(button.key) || Input.GetKeyDown(button.keyAlt))
                {
                    buttonHit = true;
                    HandlePressNote(btnClass);
                    break;
                }
            }

            bool buttonHit2 = false;
            foreach (ButtonClass button in trackTwoNotes)
            {
                Button btnClass = button.btnClass;

                if (Input.GetKeyDown(button.key) || Input.GetKeyDown(button.keyAlt))
                {
                    buttonHit2 = true;
                    HandlePressNote(btnClass);
                    break;
                }
            }

            if (!buttonHit2 && !buttonHit)
            {
                if (trackOneNotes.Count > 0)
                {
                    trackOneNotes.First().btnClass.Missed();
                }
                else if (trackTwoNotes.Count > 0)
                {
                    trackTwoNotes.First().btnClass.Missed();
                }
            }
        }
    }

    /// <summary>
    /// Logic for pressing a note (only runs when note is not a sustain note)
    /// </summary>
    private void HandlePressNote(Button btn)
    {
        btn.Hit(musicHandler.SongPosInBeats, musicHandler.BPM);
    }

    /// <summary>
    /// Logic for holding down a sustain note
    /// </summary>
    private void HandleHoldNote(Button btn, KeyCode keyPressed)
    {

    }
}
