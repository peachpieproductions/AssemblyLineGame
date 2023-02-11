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


    int currentSongIndex;
    bool playingMusic;


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
            musicPlayer.Play();
            
            yield return new WaitForSecondsRealtime(musicPlayer.clip.length + Random.Range(20,40));

            currentSongIndex++;
            if (currentSongIndex == songs.Count) currentSongIndex = 0;

        }

    }



}
