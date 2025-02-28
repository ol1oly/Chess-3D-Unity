using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class change_weapons : MonoBehaviour
{
    private Camera mainCamera; 
    string gun = "gunDefault";
    public TextMeshProUGUI useGun;

    public GameObject currentGun;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        changer();
        useGun.text ="";

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
        playAnimation();
        }
        
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // Create a RaycastHit variable to store information about the collision
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the collider belongs to a specific object, for example, with a specific tag
            if (hit.collider.tag.Contains("gun") && Mathf.Abs((hit.collider.transform.position-mainCamera.transform.position).magnitude) <5)
            {
                
                useGun.text = "Press 'F' to select this gun";
                if(Input.GetKeyDown("f")){
                    gun = hit.collider.tag;
                    changer();

                }

            }
            else{
                useGun.text ="";
            }
             
            
            
            
        }
    }
    void changer(){
        foreach (Transform child in mainCamera.transform)
            {
                // Vérifier si l'enfant a le tag "gun"
                if (child.CompareTag(gun))
                {
                    // Activer l'objet si le tag "gun" est trouvé
                    child.gameObject.SetActive(true);
                    currentGun = child.gameObject;                
                }
                else
                {
                    // Désactiver l'objet s'il ne comporte pas le tag "gun"
                    child.gameObject.SetActive(false);
                }
            }

    }
    public void playAnimation(){
        
            

            // Check if the GameObject exists and has an Animation component
            if (currentGun != null)
            {
                Animation gunAnimation = currentGun.GetComponent<Animation>();
                if (gunAnimation != null)
                {
                    // Play the "gunfiring" animation
                        gunAnimation.Play("animFiring");
                    
                    
                }
                else
                {
                    Debug.LogWarning("The GameObject does not have an Animation component.");
                }
            }
            else
            {
                Debug.LogWarning("The gunObject reference in the OtherScript is not set.");
            }
    
    }
}
