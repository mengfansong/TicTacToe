using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject menuGO;
    public GameObject btnsGO;
    public GameObject roleGO;


    private void Awake()
    {
        Instance = this;
    }

    public void ChooseRole()
    {
        btnsGO.SetActive(false);
        roleGO.SetActive(true);
    }

    public void HideMenu()
    {
        menuGO.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
