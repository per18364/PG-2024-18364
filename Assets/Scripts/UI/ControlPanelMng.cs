using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanelMng : MonoBehaviour
{
    [SerializeField] GameObject PanelPC;
    [SerializeField] GameObject PanelPS;
    [SerializeField] GameObject PanelXBox;


    void Start()
    {
        // todo, determinar control
        ChangeSelection(0);
    }

    // Update is called once per frame
    public void ChangeSelection(int selection) {
        if (selection == 0) {
            PanelPC.SetActive(true);
            PanelPS.SetActive(false);
            PanelXBox.SetActive(false);
        } else if (selection == 1) {
            PanelPC.SetActive(false);
            PanelPS.SetActive(true);
            PanelXBox.SetActive(false);
        } else if (selection == 2) {
            PanelPC.SetActive(false);
            PanelPS.SetActive(false);
            PanelXBox.SetActive(true);
        }
    }
}
