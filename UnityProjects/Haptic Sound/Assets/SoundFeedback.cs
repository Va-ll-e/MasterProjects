using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFeedback : MonoBehaviour
{
    public AudioSource audioSource;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 0.01f)
        {

        }
        else
        {
            audioSource.pitch = 0.7f + (collision.relativeVelocity.magnitude / 10f);
            var relativeVelocity = collision.relativeVelocity.magnitude;
            audioSource.Play();
        }
    }
}
