using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using System;

public class WordByWordCounter : MonoBehaviour
{
    [SerializeField] float startDelay = 0.2f;
    [SerializeField] float waitTime = 1.0f;
    [SerializeField] float endDelay = 2.0f;

    public void StartCount(Action callback)
    {
        StartCoroutine(StartCountRoutine(callback));
    }

    public IEnumerator StartCountRoutine(Action callback)
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<UIView>().Show();
            yield return new WaitForSeconds(waitTime);
            //transform.GetChild(i).GetComponent<UIView>().Hide();
        }

        yield return new WaitForSeconds(endDelay);

        //HideAll
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<UIView>().Hide();
        }

        callback?.Invoke();
    }
}
