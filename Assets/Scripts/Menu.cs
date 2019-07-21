using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu of the game
/// </summary>
public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Load a given scene; for the main menu start button
    /// </summary>
    /// <param name="scene"></param>
    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
