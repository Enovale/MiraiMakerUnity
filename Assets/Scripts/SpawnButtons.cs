using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnButtons : MonoBehaviour
{

    public MotionPath path;
    public GameObject buttonPrefab;
    public float UV = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // Nothing yet
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject spawn(float uv, float type, float beat, int indexin)
    {
        GameObject button = Instantiate(buttonPrefab, new Vector3(path.PointOnNormalizedPath(uv).x, path.PointOnNormalizedPath(uv).y, buttonPrefab.transform.position.z), new Quaternion(0, 0, 0, 0));
        button.GetComponent<Button>().Init(Mathf.RoundToInt(type), beat, indexin);
        return button;
    }
}
