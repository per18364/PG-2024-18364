using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogoFIFO : MonoBehaviour
{
    [SerializeField] TMP_Text text_;
    [SerializeField] GameObject logo_;
    public float duration = 1.0f; // Duration of one fade cycle (from 0 to 1 and back)

    private float timer;

    void Start()
    {
        StartCoroutine(LogoEffect());
    }

    void Update()
    {
        // Calculate a value that oscillates between 0 and 1
        float t = Mathf.PingPong(Time.time / duration, 1.0f);

        // Set the alpha value of the image
        Color color = text_.color;
        color.a = Mathf.Lerp(0.0f, 1.0f, t);
        text_.color = color;
    }

    IEnumerator LogoEffect()
    {
        logo_.LeanRotateZ(-7f, 2f).setEaseOutBounce();
        while(true)
        {
            logo_.LeanRotateZ(7f, 4f).setEaseOutBounce();
            yield return new WaitForSeconds(4.1f);
            logo_.LeanRotateZ(-7f, 4f).setEaseOutBounce();
            yield return new WaitForSeconds(4.1f);
        }
    }

}
