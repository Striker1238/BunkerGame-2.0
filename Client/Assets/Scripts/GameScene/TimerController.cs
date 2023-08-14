using System.Collections;
using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [Header("Timer objects")]
    public TextMeshProUGUI Timer_TMP;
    public GameObject TimerObject;
    private int time;

    ///Визуализация таймера на экране
    protected internal IEnumerator TimerControll()
    {
        time = 60;
        while (true)
        {
            Timer_TMP.text = $"{time}";
            if (time <= 0) break;

            yield return new WaitForSeconds(1);
            time--;
        }
        Debug.Log("End step");
    }
}
