using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] GameObject flashlightLight;
    private bool active = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flashlightLight.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (active == false){
                flashlightLight.gameObject.SetActive(true);
                active = true;
            } else{
                flashlightLight.gameObject.SetActive(false);
                active = false;
            }
        }
    }
}
