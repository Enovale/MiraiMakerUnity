using UnityEngine;

#pragma warning disable CA2235 // Mark all non-serializable fields

// A class that holds information about a button concisely, for use in Ispector and the button array
[System.Serializable]
public class SerializedButton
{
    public GameObject ButtonObj;
    public KeyCode Key;
    public KeyCode KeyAlt;
    public Button Button;

    public SerializedButton(GameObject button, KeyCode code, KeyCode codeAlt, Button buttonClass)
    {
        // Button gameobject reference
        ButtonObj = button;
        // Keycode to hit button according to the input array
        Key = code;
        KeyAlt = codeAlt;
        Button = buttonClass;
    }
}