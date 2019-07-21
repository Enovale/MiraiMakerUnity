using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actually spawns the buttons when the Music Handler demands it
/// </summary>
public class SpawnButtons : MonoBehaviour
{
    // Needs a reference to some game state vars
    public MotionPath path;
    public GameObject buttonPrefab;
    // Prototype var, not needed?
    //public float UV = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // Nothing yet
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Spawns the button using a prefab and initiates it with the variables you give it. Also returns the button as a gameObject
    /// </summary>
    /// <param name="uv"></param>
    /// <param name="type"></param>
    /// <param name="beat"></param>
    /// <param name="indexin"></param>
    /// <returns></returns>
    public GameObject spawn(float uv, float type, float beat, int indexin)
    {
        GameObject button = Instantiate(buttonPrefab, new Vector3(path.PointOnNormalizedPath(uv).x, path.PointOnNormalizedPath(uv).y, buttonPrefab.transform.position.z), new Quaternion(0, 0, 0, 0));
        button.GetComponent<Button>().Init(Mathf.RoundToInt(type), beat, indexin);
        return button;
    }
}
