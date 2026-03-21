using System;
using UnityEngine;
using System.Collections;
using NUnit.Framework.Constraints;

public class vendingport : MonoBehaviour
{
    private Vector3 OpenPosition = new Vector3(-0.89f, 1.066f, -0.528f);
    private Vector3 ClosedPosition = new Vector3(-0.9477118f, 1.06632f, 1.899954f);
    private bool isAnimating = false;
    public float slideSpeed = 1f;
    
    // slide to open then start timer to close
    public void Open()
    {
        StartCoroutine(SlideTo(OpenPosition));
        StartCoroutine(timer(5f));
    }
    
    // close the receptacle
    public void Close()
    {
        StartCoroutine(SlideTo(ClosedPosition));
    }

    // timer which then closes
    IEnumerator timer(float duration)
    {
        yield return new WaitForSeconds(duration);
        Close();
    }
    
    // code to slide to a position
    private IEnumerator SlideTo(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPos = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        isAnimating = false;
    }
    
}
