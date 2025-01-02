using UnityEngine;

public class SettingsSetter : MonoBehaviour
{
    [Header("Pots")]
    [SerializeField] private Animator potSound_fire;
    [SerializeField] private Animator potSound_pot;
    [SerializeField] private Animator potMusic_fire;
    [SerializeField] private Animator potMusic_pot;
    
    [Header("Triangles")]
    [SerializeField] private GameObject btn;

    [Header("Lists")]
    [SerializeField] private GameObject[] sugars;
    private int sugar_i = 0;
    [SerializeField] private GameObject[] corns;
    private int corn_i= 0;
    [SerializeField] private GameObject[] harinas;
    private int harina_i= 0;
    [SerializeField] private GameObject[] condiments;
    private bool condiment_b;

    
    void Start()
    {
        // TODO: tomar de los set ups de los usuarios los valores iniciales

        DeactivateAndActivateGameObjects(sugar_i, sugars);
        DeactivateAndActivateGameObjects(corn_i, corns);
        DeactivateAndActivateGameObjects(harina_i, harinas);
        DeactivateAndActivateGameObjects(1, condiments);
    }

    public void SoundValueModification(float value) {
        potSound_fire.SetFloat("Intensity", value);
        potSound_pot.SetFloat("Intensity", value);
    }

    public void MusicValueModification(float value) {
        potMusic_fire.SetFloat("Intensity", value);
        potMusic_pot.SetFloat("Intensity", value);
    }

    public void ChangeSugar(int i) {
        
        int j = sugar_i + i;
        if (j >= sugars.Length) {
            j = 0;
        } else if (j < 0) {
            j = sugars.Length - 1;
        }
        sugar_i = j;
        DeactivateAndActivateGameObjects(j, sugars);
    }

    public void ChangeCorn(int i) {
        
        int j = corn_i + i;
        if (j >= corns.Length) {
            j = 0;
        } else if (j < 0) {
            j = corns.Length - 1;
        }
        corn_i = j;
        DeactivateAndActivateGameObjects(j, corns);
    }

    public void ChangeHarina(int i) {
        
        int j = harina_i + i;
        if (j >= harinas.Length) {
            j = 0;
        } else if (j < 0) {
            j = harinas.Length - 1;
        }
        harina_i = j;
        DeactivateAndActivateGameObjects(j, harinas);
    }

    public void ToggleFullScreen(bool b) {
        if (b) {
            DeactivateAndActivateGameObjects(1, condiments);
        } else {
            DeactivateAndActivateGameObjects(0, condiments);
        }
    }

    private void DeactivateAndActivateGameObjects(int i, GameObject[] list){
        for (int j = 0; j < list.Length; j++) {
            if (j == i) list[j].SetActive(true); 
            else list[j].SetActive(false);
        }
    }

}
