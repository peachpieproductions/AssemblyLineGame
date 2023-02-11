using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    [Header("Music")]
    public bool musicOn;
    public bool dontShuffleFirstSong = true;
    public AudioClip currentSong;
    public List<AudioClip> songs;

    [Header("Refs")]
    public AudioSource musicPlayer;


    public int currentSongIndex;
    bool playingMusic;
    float songWaitTimer;


    private void Start() {
        StartCoroutine(Music());
        StartCoroutine(SlowUpdate());
    }

    IEnumerator SlowUpdate() {
        while (true) {

            if (!musicOn && playingMusic) {
                StopCoroutine(Music());
                musicPlayer.Stop();
                playingMusic = false;
            }
            if (musicOn && !playingMusic) StartCoroutine(Music());

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    IEnumerator Music() {

        //Shuffle songs, except first song
        List<AudioClip> tempSongList = new List<AudioClip>(songs);
        songs.Clear();
        int listLength = tempSongList.Count;
        for (var i = 0; i < listLength; i++) {
            if (i == 0  && dontShuffleFirstSong) {
                songs.Add(tempSongList[0]);
                tempSongList.RemoveAt(0);
            } else {
                var index = Random.Range(0, tempSongList.Count);
                songs.Add(tempSongList[index]);
                tempSongList.RemoveAt(index);
            }
        }

        while (musicOn) {
            playingMusic = true;

            musicPlayer.clip = songs[currentSongIndex];
            currentSong = musicPlayer.clip;

            yield return StartCoroutine(PlaySong());

            currentSongIndex++;
            if (currentSongIndex == songs.Count) currentSongIndex = 0;
        }
    }

    IEnumerator PlaySong() {
        musicPlayer.Play();
        songWaitTimer = musicPlayer.clip.length + Random.Range(20, 40);
        while (songWaitTimer > 0) {
            songWaitTimer -= Time.deltaTime;
            yield return null;
        }
        musicPlayer.Stop();
    }

    public void SkipSong() {
        songWaitTimer = 0;
    }



}
