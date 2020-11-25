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

    /// <summary>method <c>OnClickVisualiser</c> Allows params to passed to a visualiser.</summary>
    public void OnClickVisualiser() {

        plantCanvas.gameObject.SetActive(true); //selection canvas must be deactivated.

        char axiom = transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text[gameObject.transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text.Length-1];
        string ruleOne = transform.Find("Rule").gameObject.GetComponent<TextMeshProUGUI>().text;
        int maxGenerations = int.Parse((Regex.Replace(transform.Find("MaxGenerations").GetComponent<TextMeshProUGUI>().text,@"[^\d]", "")));
        float theta = float.Parse(((Regex.Match(transform.Find("Theta").GetComponent<TextMeshProUGUI>().text, @"\d+.+\d").Value)));
        
       /* foreach(Transform t in transform)
        {
            if (t.CompareTag("Rule"))
            {

            }
        }*/

        if(transform.Find("Rule2")){
            string ruleTwo = transform.Find("Rule2").gameObject.GetComponent<TextMeshProUGUI>().text;
            plant.parcelableRules.Add(ruleTwo[1],ruleTwo.Substring(4).Replace(")",""));
        }

        if(transform.Find("Rule3")){
            string ruleThree = transform.Find("Rule3").gameObject.GetComponent<TextMeshProUGUI>().text;
            plant.parcelableRules.Add(ruleThree[1],ruleThree.Substring(4).Replace(")",""));
        }

        plant.axiom = axiom;
        plant.maxIterations = maxGenerations;
        plant.thetaRotationAngle = theta;
        plant.parcelableRules.Add(ruleOne[1],ruleOne.Substring(4).Replace(")",""));

        plant.plantName = gameObject.name;
        menuCanvas.gameObject.SetActive(false);
        plant.onInstanceGenerateListener = true;
    }

}
