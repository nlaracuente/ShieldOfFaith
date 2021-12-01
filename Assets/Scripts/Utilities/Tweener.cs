using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tweener
{
    public static IEnumerator MoveToDestination(Transform target, Vector3 destination, float speed)
    {
        while (Vector3.Distance(target.localPosition, destination) > .01f)
        {
            var position = Vector3.MoveTowards(target.localPosition, destination, speed * Time.deltaTime);
            target.localPosition = position;
            yield return new WaitForEndOfFrame();
        }

        target.localPosition = destination;
    }

    public static IEnumerator ChangeScale(Transform xForm, Vector3 targetScale, float time)
    {
        float totalTime = Time.time + time;

        if (time < 0f)
            time = 1f;

        var distance = Vector3.Distance(xForm.localScale, targetScale);
        var speed = distance / time;

        while (Time.time < totalTime)
        {
            yield return null;
            xForm.localScale = Vector3.Lerp(xForm.localScale, targetScale, Time.deltaTime * speed);
        }

        xForm.localScale = targetScale;
    }

    public static IEnumerator Rotate(Transform xForm, float angle = 25f, float time = 1f)
    {
        if (time < 0f)
            time = 1f;

        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var distance = Vector3.Distance(xForm.eulerAngles, targetRotation.eulerAngles);
        var speed = distance / time;

        while (Vector3.Distance(xForm.eulerAngles, targetRotation.eulerAngles) > 0.01f)
        {
            xForm.rotation = Quaternion.Lerp(xForm.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }

        // Ensure the objects rotation is what we expect
        xForm.rotation = targetRotation;
    }    
}
