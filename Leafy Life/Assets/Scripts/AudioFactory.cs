using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFactory : MonoBehaviour
{
    public AudioSource musicTreehouse;
    public AudioSource musicGarden;
    public AudioSource voiceHungry;
    public AudioSource voiceDelicious;
    public AudioSource voiceTired;
    public AudioSource voiceNightshift;

    private AudioSource currentPlaying;
    private AudioSource laterPlaying;

    void OnEnable() {
        SceneManager.MapChangedEvent += OnMapChanged;
    }

    void OnDisable() {
        SceneManager.MapChangedEvent -= OnMapChanged;
    }

    void Start()
    {
        
    }

    private void changeMusicTrack(AudioSource newTrack) {
        if (currentPlaying == null) {
            currentPlaying = newTrack;
            currentPlaying.Play();
        } else {
            StartCoroutine(crossFade(newTrack));
        }
    }

    public IEnumerator crossFade(AudioSource nextTrack) {
        float time = 0;
        float CrossFadeDuration = 2f;

        laterPlaying = nextTrack;
        laterPlaying.Play();

        while (time <= CrossFadeDuration) {
            time += Time.deltaTime;
            float progress = time / CrossFadeDuration;
            currentPlaying.volume = Mathf.Lerp(1.0f, 0.0f, progress);
            laterPlaying.volume = Mathf.Lerp(0.0f, 1.0f, progress);
            yield return null;
        }

        currentPlaying.Stop();
        currentPlaying = laterPlaying;
    }

    public void OnMapChanged(MapController.MapType mapType) {
        switch (mapType) {
            case MapController.MapType.TREEHOUSE:
                changeMusicTrack(musicTreehouse);
                break;
            case MapController.MapType.GARDEN:
                changeMusicTrack(musicGarden);
                break;
        }
    }

    public void playMusicTreehouse() {
        currentPlaying?.Stop();
        musicTreehouse.Play();
        currentPlaying = musicTreehouse;
    }

    public void playMusicGarden() {
        currentPlaying?.Stop();
        musicGarden.Play();
        currentPlaying = musicGarden;
    }
}
