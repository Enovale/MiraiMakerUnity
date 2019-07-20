using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{

    public Sprite[] types;
    public List<GameObject> buttons;
    public KeyCode[] inputs;
    private MusicHandler musicHandler;
    private string keyDown = "";

    // Start is called before the first frame update
    void Start()
    {
        musicHandler = FindObjectsOfType<MusicHandler>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        // Handles pressing buttons
        if(Input.anyKeyDown)
        {
            int i = 0;
            foreach(KeyCode key in inputs)
            {
                keyDown = key.ToString();
                GameObject buttonObj = buttons[musicHandler.nextIndex - 1];
                Button button = buttonObj.GetComponent<Button>();
                foreach(GameObject btn in buttons)
                {
                    print(btn);
                }
                if (button.type == i)
                {
                    if (keyDown == key.ToString())
                    {
                        button.Hit(musicHandler.songPosInBeats, musicHandler.bpm);
                    }
                }
                i++;
            }
        }
        foreach(KeyCode key in inputs)
        {
            if(Input.GetKeyUp(key))
            {
                keyDown = "";
            }
        }
    }
}
