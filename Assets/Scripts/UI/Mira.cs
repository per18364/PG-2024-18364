using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mira : MonoBehaviour
{
    // Start is called before the first frame update

    public Color[] miraColors;
    public bool isActive = false;
    [SerializeField] private float expandTime = 0.3f; // Duration of the animation
    [SerializeField] private float shrinkTime = 0.2f; // Duration to shrink back
    private Vector3 originalScale; // Original size of the bullseye
    private Image bullseyeImage; // The Image component of the bullseye
    [SerializeField] private Transform raycastOrigin; // Point from which the raycast is fired
    [SerializeField] private float raycastDistance = 50f; // Distance of the raycast
    private int currentAmmo = 0;
    private bool theresEnemy = false;

    void Start()
    {
        
        originalScale = gameObject.transform.localScale;
        gameObject.transform.localScale = Vector3.zero;
        gameObject.SetActive(false);

        bullseyeImage = GetComponent<Image>();
    }

    public void Show() {
        gameObject.SetActive(true);
        isActive= true;
        StartCoroutine(Expand());
    }

    private IEnumerator Expand() {
        LeanTween.scale(gameObject, originalScale, expandTime).setEaseOutBack();
        yield return new WaitForSeconds(expandTime); 
    }

    public void Hide() {
        StartCoroutine(Shrink());
    }

    private IEnumerator Shrink() {
        LeanTween.scale(gameObject, Vector3.zero, expandTime).setEasePunch();
        yield return new WaitForSeconds(expandTime); 

        isActive = false;
        gameObject.SetActive(false);
    }

    public void UpdateColor(int ammoType)
    {
        if (!theresEnemy) {
            if (bullseyeImage.color != miraColors[ammoType]) bullseyeImage.color = miraColors[ammoType]; // Update color based on ammo type
            if (ammoType != currentAmmo) currentAmmo = ammoType;
        }        
    }

    public void SetShootCursorRed()
    {
        bullseyeImage.color = Color.red;                
        theresEnemy = true;
    }

    public void ReturnColorToOriginal()
    {
        theresEnemy = false;
        UpdateColor(currentAmmo);
    }
}

