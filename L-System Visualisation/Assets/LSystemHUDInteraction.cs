using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemHUDInteraction : MonoBehaviour
{
    [SerializeField] GameObject SelectionCanvas = default;

    [SerializeField] PlantVisualiser currentPlant = default; 

    public void OnClickBackToMenu() {
        SelectionCanvas.SetActive(true);
        currentPlant.ClearAll();
        gameObject.SetActive(false);
    } 
}
