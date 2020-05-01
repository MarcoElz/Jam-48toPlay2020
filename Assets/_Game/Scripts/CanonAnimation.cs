using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonAnimation : MonoBehaviour
{

    [SerializeField] float duration = 0.5f;

    [SerializeField] SpriteRenderer sprite;

    [SerializeField] AnimationCurve rotationCurve;

    bool rotateAnimation;
    bool alphaAnimation;

    private float rotationTime;
    private float alphaTime;

    float startAngle;
    float endAngle;

    float speedRatio;

    Vector3 euler;

    Color color;

    private IEnumerator Start()
    {
        //Set up values
        //transform.up = -(-transform.position + Vector3.zero).normalized; //Look to center
        color = sprite.color; //Cache color
        rotateAnimation = true; //Reset
        alphaAnimation = true; //Reset
        speedRatio = 1f / duration;

        //Alpha Animation
        alphaAnimation = false;
        alphaTime = 0f;
       
        yield return new WaitForSeconds(1.0f);//WaitFor rotation

        //RotationAnimation
        rotateAnimation = false;
        rotationTime = 0f;

        startAngle = transform.rotation.eulerAngles.z;
        endAngle = startAngle + 180;
        euler = transform.rotation.eulerAngles;    
    }

    private void Update()
    {
        if(!rotateAnimation)
        {
            rotationTime += Time.deltaTime * speedRatio;
            float curveValue = rotationCurve.Evaluate(rotationTime);
            euler.z = Mathf.Lerp(startAngle, endAngle, curveValue);
            color.a = Mathf.Lerp(0f, 1f, curveValue);

            transform.rotation = Quaternion.Euler(euler);

            if (rotationTime >= 1.0f)
                rotateAnimation = true;
        }

        if (!alphaAnimation)
        {
            alphaTime += Time.deltaTime * speedRatio;
            float curveValue = rotationCurve.Evaluate(alphaTime);
            color.a = Mathf.Lerp(0f, 1f, curveValue);
            sprite.color = color;

            if (alphaTime >= 1.0f)
                alphaAnimation = true;
        }
    }

}
