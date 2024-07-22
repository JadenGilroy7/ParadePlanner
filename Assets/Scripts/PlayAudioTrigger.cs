using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioTrigger : MonoBehaviour
{
    public AudioSource audioSource;

    void OnTriggerEnter(Collider other)
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource is null.");
        }
    }
}