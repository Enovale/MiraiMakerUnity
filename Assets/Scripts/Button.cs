using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ButtonClass
{
    public GameObject btn;
    public KeyCode key;

    public ButtonClass(GameObject button, KeyCode code)
    {
        this.btn = button;
        this.key = code;
    }
}

public class Button : MonoBehaviour
{

    public int type;
    public float beat;
    public int index;
    public ButtonClass btn;
    public GameObject rankPrefab;
    private GameHandler gameHandler;
    private MusicHandler musicHandler;
    private new SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Hit(float songPosInBeats, float bpm, int rate)
    {
        
        print("Did hit " + rate);
        GameObject rankText = Instantiate(rankPrefab, this.gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(rate);
        Destroy(this.gameObject);
    }

    public void Missed()
    {
        print("Fucking missed idiot");
        GameObject rankText = Instantiate(rankPrefab, this.gameObject.transform.position, new Quaternion(0, 0, 0, 0));
        rankText.GetComponent<RankText>().Init(3);
        Destroy(this.gameObject);
    }

    public void Init(int typein, float curbeat, int indexin)
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
        btn = new ButtonClass(this.gameObject, gameHandler.inputs[type]);
    }

    // Update is called once per frame
    void Update()
    {
        if((beat / musicHandler.lengthInBeats) <= musicHandler.fadeEnd)
        {
            Missed();
        }
        if(Input.GetKeyDown(btn.key))
        {
            float pos = musicHandler.songPosInBeats;
            float bpm = musicHandler.bpm;
            if ((pos <= beat && pos > beat - ((bpm / 60) / 4)) || (pos >= beat && pos < beat + ((bpm / 60) / 4)))
            {
                Hit(pos, bpm, 0);
            }
            else if ((pos <= beat && pos > beat - ((bpm / 60) / 3)) || (pos >= beat && pos < beat + ((bpm / 60) / 3)))
            {
                Hit(pos, bpm, 1);
            }
            else if ((pos <= beat && pos > beat - ((bpm / 60) / 2)) || (pos >= beat && pos < beat + ((bpm / 60) / 2)))
            {
                Hit(pos, bpm, 2);
            }
            else if ((pos <= beat && pos > beat - ((bpm / 60))) || (pos >= beat && pos < beat + ((bpm / 60))))
            {
                Hit(pos, bpm, 3);
            }
        }
    }
}
