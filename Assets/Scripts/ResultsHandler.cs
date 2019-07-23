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
    void Start()
    {
        // Get reference to the Game Handler
        GameHandler[] objects = FindObjectsOfType<GameHandler>();
        gameHandler = objects[0];

        int i = 0;
        // Fill in the rank amounts
        foreach(Text text in amountList)
        {
            text.text = gameHandler.hits[i].ToString();
            i++;
        }
    }

    public void EndGame()
    {
        Application.Quit();
    }
}
