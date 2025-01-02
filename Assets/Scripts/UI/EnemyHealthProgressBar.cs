using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthProgressBar : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Image progressBarForeground;
    [SerializeField] Image progressBarSecondForeground;
    private Camera cam;

    private void Start() {
        cam = FindObjectOfType<Camera>();
    }

    public void SetProgress(float progress) {
        progressBarForeground.fillAmount = progress;
        StartCoroutine(YellowBar(progress));
    }

    private IEnumerator YellowBar(float progress)
    {
        yield return new WaitForSeconds(0.2f);
        progressBarSecondForeground.fillAmount = progress;

    }

    private void LateUpdate() {
        transform.LookAt(
            transform.position +
            cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
    }
}
