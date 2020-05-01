using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using System;

public class Counter : MonoBehaviour
{

    public void StartCount(Action callback)
    {
        StartCoroutine(StartCountRoutine(callback));
    }

    private IEnumerator StartCountRoutine(Action callback)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<UIView>().Show();
            yield return new WaitForSeconds(1f);
            transform.GetChild(i).GetComponent<UIView>().Hide();
        }

        callback?.Invoke();
    }
}
