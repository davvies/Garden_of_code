using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Class <c>LSystemHUDInteraction</c> Core interaction for controlling arguments within a plant preset
/// </summary>
public class LSystemHUDInteraction : MonoBehaviour
{
    [SerializeField] GameObject selectionCanvas = default; //previous screen

    [SerializeField] PlantVisualiser currentPlant = default; //reference the script directly

    [SerializeField] TextMeshProUGUI plantName = default; //var value for TXT of plant name

    [SerializeField] TextMeshProUGUI generation = default; //var value for TXT of current generation

    [SerializeField] TextMeshProUGUI theta = default; //var value for TXT of plant name

    [SerializeField] GameObject prefabLR = default; //reference to the turtle renderer and its properties

    [SerializeField] Material customBlack = default; //Color.Black constant was not used as it was arbitarly deemed to dark for the design 
    
    [SerializeField] Material customGreen = default; //Color.Green constant was not used as it was arbitarly deemed to dark for the design

    [SerializeField] Material customBrown = default; //a custom brown material used

    [SerializeField] Slider thetaSlider = default; //angular adjustment scale

    [SerializeField] RawImage leafChecker = default; //a tick box for the state of leaves

    [SerializeField] Texture IMGleafChecked = default; //texture for tickbox

    [SerializeField] Texture IMGleafUnchecked = default; //texture for unticked box

    const float startLRWidth = 0.3f; //default line width of turtle graphics

    const float maxBranchThickness = 0.9f; 
    
    const float branchThicknessScalar = 0.3f;
    
    const float upperThicknessBound = 3f;
    
    const float lowerThicknessBound = 1f;
    
    void Start() => DisplayStats(); //at default stats should show

    /* *NOTE* 
     * Unity has unpredictable threads of execution, the order of which start updates run is decided at runtime by the complier 
     * to account for the edge case of a config loading before the params are sent, when the plant is enabled execution of growth
     * is halted till the end of the frame, this ensures that the plant data is never not loaded before the visuliser exectures its
     * 'Start()' function
    */
    void OnEnable() {
        prefabLR.GetComponent<LineRenderer>().startWidth = startLRWidth; //data is not accounted for in the edge case as only the prefab is being edited
        prefabLR.GetComponent<LineRenderer>().endWidth = startLRWidth; 
        prefabLR.GetComponent<LineRenderer>().material = customBlack; //default material black (design due to white-ish background)
        StartCoroutine(WaitAFrameToUpdateName()); //async from main thread to stop halting the main thread
        }
    
    IEnumerator WaitAFrameToUpdateName() {
        yield return new WaitWhile(() => plantName.text == string.Empty); //this wait until ensures data has been passed (edge case mentioned prior) 
        DisplayStats(); //since data is loaded this can be updated on the GUI
        currentPlant.branchLengthScalar = 1; //scalar value for length defaulted to one
        leafChecker.texture = IMGleafUnchecked; //by default plants will not have leaves

    }

    /// <summary>method <c>DisplayStats</c> Update on-screen GUI with information on states</summary>
    void DisplayStats(){
        plantName.text = "plant preset: "+currentPlant.plantName; 
        generation.text = "Generation: "+currentPlant.currentIteration;
        theta.text = "Theta: "+currentPlant.thetaRotationAngle+"°"; //degree symbol added for user usablity
        thetaSlider.value = currentPlant.thetaRotationAngle;
    }

    /// <summary>method <c>OnClickBackToMenu</c> functionality for exiting back to the menu</summary>
    public void OnClickBackToMenu() {
        selectionCanvas.SetActive(true);  
        currentPlant.ClearAll(); //plant state data is cleared    
        gameObject.SetActive(false);
    }

    /// <summary>method <c>OnClickBackToMenu</c> functionality for handling update of generation</summary>
    public void OnClickIncreaseGenerations(){
        if(currentPlant.currentIteration < currentPlant.maxIterations){ //clamp upper bounds to parsed max generation
            currentPlant.currentIteration += 1;
            currentPlant.onInstanceRecalcuateTreeStructure = true; //increasing generation needs a restructure of underlying data
            currentPlant.onInstanceGenerateListener = true; //this data is then generated via the generate function
            DisplayStats(); //on-screen stats are updated
        }
    }

    /// <summary>method <c>OnClickDecreaseGenerations</c> functionality for handling decrement of generation</summary>
    public void OnClickDecreaseGenerations(){
        if(currentPlant.currentIteration > 1){ //clamp for lower bounds
            currentPlant.currentIteration -= 1;
            currentPlant.onInstanceRecalcuateTreeStructure = true; //generation changes also require a restructure of instruction set
            currentPlant.onInstanceGenerateListener = true; //re-config with updated instructions
            DisplayStats(); //on-screen stats updated
        }
    }

    /// <summary>methods <c>OnClickChangeTreeTo{colour}</c> functionality for colour textures being clicked 
    /// since these methods are not live interaction and materials are cached optimisation are not needed (checking if material is already applied)</summary>
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

    /// <summary>method <c>OnDragChangeTheta</c> functionality for live editing of theta rotation</summary>
    public void OnDragChangeTheta()
    {
        float updatedSlideTheta = (float)Mathf.Round(thetaSlider.value * 10f) / 10; //parse new theta value to two decimal places
        currentPlant.thetaRotationAngle = updatedSlideTheta; //send updated value to visualiser
        theta.text = "Theta: "+updatedSlideTheta+ "°"; //update HUD theta value
        currentPlant.updateExisitngAngles = true; //regeneration or recalculation of string not needed, only angles and postions need changing
    }

    /// <summary>methods <c>OnClick{Increase/Decrease}BranchThickness</c> handles functionality of updating thickness</summary>
    public void OnClickIncreaseBranchThickness() {
        if (Mathf.Abs(prefabLR.GetComponent<LineRenderer>().startWidth) >= maxBranchThickness) 
            return;
       
        //unison width update
        prefabLR.GetComponent<LineRenderer>().startWidth += branchThicknessScalar; 
        prefabLR.GetComponent<LineRenderer>().endWidth += branchThicknessScalar;
        currentPlant.onInstanceGenerateListener = true; //regeneration is needed (prefabs need to be updated)
    }

    public void OnClickDecreaseBranchThickness()
    {
        if (Mathf.Abs(prefabLR.GetComponent<LineRenderer>().startWidth) <= branchThicknessScalar) 
            return;
            
        prefabLR.GetComponent<LineRenderer>().startWidth -= branchThicknessScalar;
        prefabLR.GetComponent<LineRenderer>().endWidth -= branchThicknessScalar;
        currentPlant.onInstanceGenerateListener = true;
    }

    /// <summary>methods <c>OnClickUncheckLeaves</c> handles functionality for complete clicking of leaves</summary>
    public void OnClickCheckUncheckLeaves()
    {
        currentPlant.hasLeaves = !currentPlant.hasLeaves; //simply boolean functionality does not require its on method, a state switch is used
        leafChecker.texture = currentPlant.hasLeaves == true ? IMGleafChecked : IMGleafUnchecked; //update image relative to state
        currentPlant.onInstanceGenerateListener = true; //tree needs to be re-generated
    }

    /// <summary>methods <c>OnClickMakePlantStochastic</c> handles trigger for applying randomess to ruleset</summary>
    public void OnClickMakePlantStochastic()
    {
        currentPlant.isStochastic = true; //trigger for updating random factor
        currentPlant.onInstanceRecalcuateTreeStructure = true;  //refactor of cache string is needed here as completely new strings are generated
        currentPlant.onInstanceGenerateListener = true; //regen tree from this structure
    }

    /// <summary>methods <c>OnClick{Increase/Decrease}BranchLength</c> handles functionality of updating length of branches</summary>
    public void OnClickIncreaseBranchlength()
    {
        if (currentPlant.branchLengthScalar < upperThicknessBound) 
            currentPlant.branchLengthScalar+=1; //upper bound check
            
        currentPlant.onInstanceGenerateListener = true; //rules are unchaning so only a generation is needed
    }
    
    public void OnClickDecreseBranchlength()
    {
        if (currentPlant.branchLengthScalar > lowerThicknessBound) 
            currentPlant.branchLengthScalar-=1; //lower bound check
            
        currentPlant.onInstanceGenerateListener = true; //rules are unchaning so only a generation is needed
    }

}
