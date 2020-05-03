using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using System;
using TMPro;

public class DialogueView : MonoBehaviour
{
    [SerializeField] float startDelay = 0.2f;
    [SerializeField] float waitTime = 0.10f;

    [SerializeField] TextMeshProUGUI dialogueText;
    
    private void Awake()
    {
        dialogueText.text = "";
    }

    public IEnumerator StartRoutine(string dialogue, Action callback, float endDelay = 2f)
    {
        string actualText = "";

        GetComponentInChildren<UIView>(true).Show();
        yield return new WaitForSeconds(startDelay);
        for (int i = 0; i < dialogue.Length; i++)
        {
            actualText += dialogue[i];
            dialogueText.text = actualText;
            yield return new WaitForSeconds(waitTime);
        }

        yield return new WaitForSeconds(endDelay);

        //HideAll
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<UIView>().Hide();
        }
        yield return new WaitForSeconds(0.2f);
        dialogueText.text = "";

        callback?.Invoke();
    }
}
