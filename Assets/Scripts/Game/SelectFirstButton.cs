using UnityEngine; //Connect to Unity Engine
using UnityEngine.EventSystems; //Use UnityEngine.EventSystems to allow us to set First button as enabled

public class SelectFirstButton : MonoBehaviour
{
    void OnEnable()
    {
        //Store the FirstButton object of the activated Canvas
        GameObject selectedButton = GameObject.Find("FirstButton");
        //Create a reference to the current EventSystem
        EventSystem eventSystem = EventSystem.current;
        //Set the FirstButton as the selected object
        eventSystem.SetSelectedGameObject(selectedButton, new BaseEventData(eventSystem));
    }
}
