using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
public class InitGivenPlant : MonoBehaviour
{
    [SerializeField] Canvas plantCanvas = default;  

    [SerializeField] Canvas menuCanvas = default;

    [SerializeField] PlantVisualiser plant;
    //cacheVisualiser when on click mention not having to read memory each time

    public void OnClickVisualiser() {
        plantCanvas.gameObject.SetActive(true);

        char axiom = transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text[gameObject.transform.Find("Axiom").GetComponent<TextMeshProUGUI>().text.Length-1];
        string ruleOne = transform.Find("Rule").gameObject.GetComponent<TextMeshProUGUI>().text;
        int maxGenerations = int.Parse((Regex.Replace(transform.Find("MaxGenerations").GetComponent<TextMeshProUGUI>().text,@"[^\d]", "")));
        float theta = float.Parse(((Regex.Match(transform.Find("Theta").GetComponent<TextMeshProUGUI>().text, @"\d+.+\d").Value)));
        
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
