using System;
using System.Collections;
using System.Collections.Generic;
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
        foreach(KeyCode codeFirst in gameHandler.inputs)
        {
            int index = Array.IndexOf(gameHandler.inputs, codeFirst);
            KeyCode codeSecond = gameHandler.inputs[index];
            // Was the first button key pressed?
            bool firstDown = Input.GetKeyDown(codeFirst);
            // Was the second  button key pressed?
            bool secondDown = Input.GetKeyDown(codeSecond);

            // The closest notes aren't on the same beat
            if(!musicHandler.ClosestNotesAreTheSame())
            {
                if(firstDown || secondDown)
                {
                    Button note = musicHandler.GetClosestNote();
                    if (note == null) return;
                    if (codeFirst == note.btn.key || codeSecond == note.btn.keyAlt)
                    {
                        HandlePressNote(note);
                    }
                    else
                    {
                        Button altNote = null;
                        if (note.track == 0)
                        {
                            altNote = musicHandler.SecondLineNextButton;
                        } else
                        {
                            altNote = musicHandler.FirstLineNextButton;
                        }
                        if (altNote == null) return;
                        if (codeFirst == altNote.btn.key || codeSecond == altNote.btn.keyAlt)
                        {
                            HandlePressNote(altNote);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Logic for pressing a note (only runs when note is not a sustain note)
    /// </summary>
    private void HandlePressNote(Button btn)
    {
        btn.Hit(musicHandler.songPosInBeats, musicHandler.bpm, btn.GetRank(musicHandler.songPosInBeats, musicHandler.bpm, btn.beat));
    }

    /// <summary>
    /// Logic for holding down a sustain note
    /// </summary>
    private void HandleHoldNote(Button btn, KeyCode keyPressed)
    {

    }
}
