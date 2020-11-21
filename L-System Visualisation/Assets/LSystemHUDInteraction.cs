using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] Slider thetaSlider = default;

    [SerializeField] RawImage leafChecker = default;

    [SerializeField] Texture IMGleafChecked = default;

    [SerializeField] Texture IMGleafUnchecked = default;

    const float startLRWidth = 0.3f; 

    void Start() => DisplayStats();

    void OnEnable() { 
        StartCoroutine(WaitAFrameToUpdateName());
        }//plant name is static information, render it when information is parsed
    
    IEnumerator WaitAFrameToUpdateName() {
        yield return new WaitWhile(() => plantName.text == string.Empty);
        DisplayStats();
        
    }

    void DisplayStats(){
        plantName.text = "plant preset: "+currentPlant.plantName;
        generation.text = "Generation: "+currentPlant.currentIteration;
        theta.text = "Theta: "+currentPlant.thetaRotationAngle+"°";
        thetaSlider.value = currentPlant.thetaRotationAngle;
        prefabLR.GetComponent<LineRenderer>().startWidth = startLRWidth;
        prefabLR.GetComponent<LineRenderer>().endWidth = startLRWidth;
        leafChecker.texture = IMGleafUnchecked;
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

    public void OnDragChangeTheta()
    {
        float updatedSlideTheta = (float)Mathf.Round(thetaSlider.value * 10f) / 10;
        currentPlant.thetaRotationAngle = updatedSlideTheta;
        theta.text = "Theta: "+updatedSlideTheta+ "°";
        currentPlant.onInstanceGenerateListener = true;
    }

    public void OnClickIncreaseBranchThickness() {
        //prefabLR.GetComponent<LineRenderer>().startWidth = prefabLR.GetComponent<LineRenderer>().startWidth 
        if (Mathf.Abs(prefabLR.GetComponent<LineRenderer>().startWidth) >= 0.9f) return;
        Debug.Log("Running");
        prefabLR.GetComponent<LineRenderer>().startWidth += 0.3f;
        prefabLR.GetComponent<LineRenderer>().endWidth += 0.3f;
        currentPlant.onInstanceGenerateListener = true; 
    }

    public void OnClickDecreaseBranchThickness()
    {
        if (Mathf.Abs(prefabLR.GetComponent<LineRenderer>().startWidth) <= 0.3f) return;
        prefabLR.GetComponent<LineRenderer>().startWidth -= 0.3f;
        prefabLR.GetComponent<LineRenderer>().endWidth -= 0.3f;
        currentPlant.onInstanceGenerateListener = true;
    }

    public void OnClickCheckUncheckLeaves()
    {
        currentPlant.hasLeaves = !currentPlant.hasLeaves;
        leafChecker.texture = currentPlant.hasLeaves == true ? IMGleafChecked : IMGleafUnchecked;
        currentPlant.onInstanceGenerateListener = true;
    }
}
