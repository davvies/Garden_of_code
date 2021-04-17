using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

/// <summary>Class <c>InitGivenPlant</c> Handles functionality for parcing preset data to the plant visualiser
/// </summary>
public class InitGivenPlant : MonoBehaviour
{
    [SerializeField] Canvas plantCanvas = default; //Demonstration canvas to enable

    [SerializeField] Canvas menuCanvas = default; //Menu canvas to disable

    [SerializeField] PlantVisualiser plant; //Reference to pass parced data

    const int ruleIndex = 1; //The standard index of any rule
    
    const int maxRuleIndexLen = 4; //Typical end of rule
    
    /// <summary>method <c>OnClickVisualiser</c> Allows params to passed to a visualiser.</summary>
    public void OnClickVisualiser() {

        plantCanvas.gameObject.SetActive(true); //selection canvas must be deactivated.

        char axiom = transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text[gameObject.transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text.Length-1]; //parce single char for axiom
        int maxGenerations = int.Parse((Regex.Replace(transform.Find("MaxGenerations").GetComponent<TextMeshProUGUI>().text,@"[^\d]", ""))); //parce max generations as a number rather than full text
        float theta = float.Parse(((Regex.Match(transform.Find("Theta").GetComponent<TextMeshProUGUI>().text, @"\d+.+\d").Value))); //this regex allows a simply fraction to be left behind
        
        foreach(Transform plantProperty in transform) //O(n) linear search is not a problem as we assume that only basic fields such as axiom exist
        {
            if (plantProperty.CompareTag("Rule")) //this design allows any amount of rules to be added, assuming the rules are marked as such with a tag
            {
                string rule = plantProperty.GetComponent<TextMeshProUGUI>().text; //taking a reference to the full GUI display of the rule..
                plant.parcelableRules.Add(rule[ruleIndex], rule.Substring(maxIndexLen).Replace(")", "")); //we pass the character for that rule, along with the rest of the rule
            }
        }

        //plant properties set
        plant.axiom = axiom;
        plant.maxIterations = maxGenerations;
        plant.thetaRotationAngle = theta;
        plant.plantName = gameObject.name;

        menuCanvas.gameObject.SetActive(false); //disable the menu completely to stop background rendering (increased performance)
        plant.onInstanceGenerateListener = true; //enough arguments are used to create a tree
    }

}
