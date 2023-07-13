using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    bool ButtonState = false;
    public void ChangeOfReadiness(TextMeshProUGUI Tmp)
    {
        ButtonState = !ButtonState;
        Tmp.text = (ButtonState)? "NOT READY":"READY";
        FindObjectOfType<GameController>().isChangedReadiness(ButtonState);
    }
}
