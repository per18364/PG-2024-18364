using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarProgress : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Image progressBarForeground;
    [SerializeField] Image progressBarSecondForeground;

    public void SetProgress(float progress) {
        progressBarForeground.fillAmount = progress;
        StartCoroutine(YellowBar(progress));
    }

    private IEnumerator YellowBar(float progress)
    {
        yield return new WaitForSeconds(0.2f);
        progressBarSecondForeground.fillAmount = progress;

    }

}
