using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private GameHandler gameHandler;
    private MusicHandler musicHandler;

    private int? _trackOneHold;
    private int? _trackTwoHold;

    private void Awake()
    {
        gameHandler = FindObjectsOfType<GameHandler>()[0];
        musicHandler = FindObjectsOfType<MusicHandler>()[0];
    }

    private void Update()
    {
        if (musicHandler == null)
            return;

        HandleInput();
    }

    /*
    * Handle note registration.
    *
    * Here's how it works:
    * "Next" refers to the earliest note in the timing window.
    * On Key press:
    * - Is the next note a double note? (two Notes on the same Beat for each Track)
    *   - No: Handle normally, end method
    * - Get the next note on Track one
    *   - Is there one that matches the Key we pressed?
    *     - Yes: Hit it
    *     - Hit both inputs at once: Hit the first, then go to No
    *     - No: Is there one on the second Track?
    *       - No: Miss the next note on Track 1, Track 2 if there are none
    *       - Yes: Hit it
    */
    private void HandleInput()
    {
        // If a note is being held on a track, run holding logic
        // instead of normal press logic
        if (_trackOneHold != null)
        {
            HandleHoldNote(0);
            return;
        }

        if (_trackTwoHold != null)
        {
            HandleHoldNote(1);
            return;
        }

        if (Input.anyKeyDown)
        {
            var trackOneNotes = musicHandler.GetButtonsInTimingWindow(true);
            var trackTwoNotes = musicHandler.GetButtonsInTimingWindow(false);

            var pos = musicHandler.SongPosInBeats;
            var bpm = musicHandler.Level.BPM;
            if (trackOneNotes.Any() && trackTwoNotes.Any())
            {
                var btn1 = trackOneNotes.First();
                var btn2 = trackTwoNotes.First();
                if (btn1.Button.Beat < btn2.Button.Beat)
                {
                    if (Input.GetKeyDown(btn1.Key) || Input.GetKeyDown(btn1.KeyAlt))
                    {
                        HandlePressNote(btn1.Button);
                        return;
                    }
                    else
                    {
                        btn1.Button.Missed();
                        return;
                    }
                }
                else if (btn1.Button.Beat > btn2.Button.Beat)
                {
                    if (Input.GetKeyDown(btn2.Key) || Input.GetKeyDown(btn2.KeyAlt))
                    {
                        HandlePressNote(btn2.Button);
                        return;
                    }
                    else
                    {
                        btn2.Button.Missed();
                        return;
                    }
                }
            }
            else
            {
                // Hit the next possible note if you try to hit earlier than the timing window
                var spawned1 = musicHandler.GetSpawnedButtons(true).FirstOrDefault();
                var spawned2 = musicHandler.GetSpawnedButtons(false).FirstOrDefault();

                if (spawned1 != null && spawned2 != null)
                    HandlePressNote(
                        Math.Abs(Math.Min(spawned1.Button.Beat, spawned2.Button.Beat) - spawned1.Button.Beat) < 0.05
                            ? spawned1.Button
                            : spawned2.Button);
                else if (spawned1 != null || spawned2 != null)
                    HandlePressNote(spawned1 != null ? spawned1.Button : spawned2.Button);
            }

            var buttonHit = false;
            foreach (var button in trackOneNotes)
            {
                var btnClass = button.Button;

                // If you hit both on the same frame, pretend like you didnt hit the first one.
                // because the second input will hit the first one
                if (Input.GetKeyDown(button.Key) && Input.GetKeyDown(button.KeyAlt))
                {
                    HandlePressNote(btnClass);
                    break;
                }
                else if (Input.GetKeyDown(button.Key) || Input.GetKeyDown(button.KeyAlt))
                {
                    buttonHit = true;
                    HandlePressNote(btnClass);
                    break;
                }
            }

            var buttonHit2 = false;
            foreach (var button in trackTwoNotes)
            {
                var btnClass = button.Button;

                if (Input.GetKeyDown(button.Key) || Input.GetKeyDown(button.KeyAlt))
                {
                    buttonHit2 = true;
                    HandlePressNote(btnClass);
                    break;
                }
            }

            if (!buttonHit2 && !buttonHit)
            {
                if (trackOneNotes.Count > 0)
                    trackOneNotes.First().Button.Missed();
                else if (trackTwoNotes.Count > 0)
                    trackTwoNotes.First().Button.Missed();
            }
        }
    }

    private void HandlePressNote(Button btn)
    {
        if (btn.Sustain)
            if (btn.Track == 0)
                _trackOneHold = btn.NoteType;
            else
                _trackTwoHold = btn.NoteType;
        btn.Hit(musicHandler.SongPosInBeats, musicHandler.Level.BPM);
    }

    private void HandleHoldNote(int track)
    {
        // Stub
    }
}