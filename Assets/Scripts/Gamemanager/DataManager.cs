using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (WeaponSaveManager.Instance.weaponCatalog!=null)
        {
            WeaponSaveManager.Instance.LoadDefaultWeapons();
            WeaponSaveManager.Instance.SaveWeapons();
            //WeaponSaveManager.Instance.LoadWeapons();
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
