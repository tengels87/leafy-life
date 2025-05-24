using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    private List<AudioSource> soundQueue = new List<AudioSource>();
    private AudioSource currentPlaying = null;

    void Start()
    {
        
    }

    void Update()
    {
        // play first sound in queue
        // than remove and proceed with next sound
        if (soundQueue.Count > 0) {
            if (currentPlaying == null) {
                currentPlaying = soundQueue[0];
                currentPlaying.Play();
            } else {
                if (!currentPlaying.isPlaying) {
                    soundQueue.RemoveAt(0);
                    currentPlaying = null;
                }
            }
        }
    }

    public void addSound(AudioSource audio) {
        soundQueue.Add(audio);
    }

    public void playImmediate(AudioSource audio) {
        audio.Play();
    }
}
