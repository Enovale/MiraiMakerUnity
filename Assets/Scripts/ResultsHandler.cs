using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages the end results screen using the stats from the game handler
/// </summary>
public class ResultsHandler : MonoBehaviour
{
    private GameHandler gameHandler;
    public Text[] amountList = new Text[5];

    // Start is called before the first frame update
    private void Start()
    {
        // Get reference to the Game Handler
        gameHandler = FindObjectOfType<GameHandler>();

        var i = 0;
        // Fill in the rank amounts
        foreach (var text in amountList)
        {
            text.text = gameHandler.NoteHitCounter[i].ToString();
            i++;
        }
    }

    public static void EndGame()
    {
        Application.Quit();
    }
}