using UnityEngine;

#pragma warning disable CA2235 // Mark all non-serializable fields

// A class that holds information about a button concisely, for use in Ispector and the button array
[System.Serializable]
public class ButtonClass
{
    public GameObject btn;
    public KeyCode key;
    public KeyCode keyAlt;
    public Button btnClass;

    public ButtonClass(GameObject button, KeyCode code, KeyCode codeAlt, Button buttonClass)
    {
        // Button gameobject reference
        this.btn = button;
        // Keycode to hit button according to the input array
        this.key = code;
        this.keyAlt = codeAlt;
        this.btnClass = buttonClass;
    }
}
