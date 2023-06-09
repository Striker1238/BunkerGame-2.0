using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public void NotificationFromServer(Vector3 endPos) => StartCoroutine(SetNotification(endPos));
    private IEnumerator SetNotification(Vector3 endPos)
    {
        var _timeToNewPos = 3;
        var _timeToLine = 2;
        var startPos = transform.position;

        for (float t = 0; t <= 1 * _timeToNewPos; t += Time.deltaTime)
        {
            //Движение до точки
            
            transform.position = Vector3.Lerp(startPos, endPos, t / _timeToNewPos);
            yield return new WaitForEndOfFrame();
        }
        for (float t = 0; t <= 1 * _timeToLine; t += Time.deltaTime)
        {
            //Линия
            //transform.position = Vector3.Lerp(startPos, endPos, t / _timeToLine);
            yield return new WaitForEndOfFrame();
        }
    }
}
