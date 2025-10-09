using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setBoard : MonoBehaviour
{
    public GameObject board;
    // Start is called before the first frame update
    void Start()
    {
        setNormalColors();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setNormalColors(){
        int count=0;
        bool side = true;
        foreach (Transform child in board.transform){
            child.gameObject.SetActive(true);
            
            if(side){//sa
                child.GetComponent<Renderer>().material.color = Color.black;
                
            }
            else{
                child.GetComponent<Renderer>().material.color = Color.white;
            }
            side = !side;
            count++;
            if(count % 8 == 0) side = !side;
        }


    }
}
