using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LSystemHUDInteraction : MonoBehaviour
{
    [SerializeField] GameObject selectionCanvas = default;

    [SerializeField] PlantVisualiser currentPlant = default; 

    [SerializeField] TextMeshProUGUI plantName = default;

    [SerializeField] TextMeshProUGUI generation = default;
    
    [SerializeField] TextMeshProUGUI theta = default;

    [SerializeField] GameObject prefabLR = default; 

    [SerializeField] Material customBlack = default; 
    
     [SerializeField] Material customGreen = default; 

     [SerializeField] Material customBrown = default;

    void Start() => DisplayStats();

    void OnEnable() { 
        StartCoroutine(WaitAFrameToUpdateName());
        }//plant name is static information, render it when information is parsed
    
    IEnumerator WaitAFrameToUpdateName() { //by waiting for the nex
        yield return new WaitWhile(() => plantName.text == string.Empty);
        DisplayStats();
        
    }

    void DisplayStats(){
        plantName.text = "plant preset: "+currentPlant.plantName;
        generation.text = "Generation: "+currentPlant.currentIteration;
        theta.text = "Theta: "+currentPlant.thetaRotationAngle+"°";
    }

    public void OnClickBackToMenu() {
        selectionCanvas.SetActive(true);  
        currentPlant.ClearAll();    
        gameObject.SetActive(false);
    } 

    public void OnClickIncreaseGenerations(){
        if(currentPlant.currentIteration < currentPlant.maxIterations){
            currentPlant.currentIteration += 1; 
            currentPlant.onInstanceGenerateListener = true;
            DisplayStats();
        }
    }

    public void OnClickDecreaseGenerations(){
        Debug.Log("clicked");
        if(currentPlant.currentIteration > 1){
            currentPlant.currentIteration -= 1; 
            DisplayStats();
            currentPlant.onInstanceGenerateListener = true;
        }
    }

    public void OnClickChangeTreeToBlack(){
        prefabLR.GetComponent<LineRenderer>().material = customBlack;
        currentPlant.onInstanceGenerateListener = true;  
    }

    public void OnClickChangeTreeToGreen(){
        prefabLR.GetComponent<LineRenderer>().material = customGreen;
        currentPlant.onInstanceGenerateListener = true;
    }

    public void OnClickChangeTreeToBrown(){
        prefabLR.GetComponent<LineRenderer>().material = customBrown;
        currentPlant.onInstanceGenerateListener = true;
    }
}
