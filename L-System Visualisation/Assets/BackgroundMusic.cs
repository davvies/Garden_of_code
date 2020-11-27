using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Class <c>BackgroundMusic</c> Handles functionality for autoplaying a background nature track
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    AudioSource audioPlayer; //reference the audio player attached

    void Start()
    {
        DontDestroyOnLoad(gameObject); //function is used allow for continous music 
        audioPlayer = GetComponent<AudioSource>();
        audioPlayer.Play(); //assume the audio player has such a soundtrack attached
        QualitySettings.vSyncCount = 0; //stop webgl vsync rendering to affect potential sound
    }

}
