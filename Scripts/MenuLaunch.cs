using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Class <c>MenuLaunch</c> Handles a simple transition to open a scene </summary>
public class MenuLaunch : MonoBehaviour
{
    const indexOfFirstScene = 1;

    public void OpenFirstScene() => SceneManager.LoadScene(indexOfFirstScene); //onClick method used in execution of 'begin' button
}
