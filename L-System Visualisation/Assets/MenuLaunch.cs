using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuLaunch : MonoBehaviour
{
    public void OpenFirstScene() => SceneManager.LoadScene(1);
}
