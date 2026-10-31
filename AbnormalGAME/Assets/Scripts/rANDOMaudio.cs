using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBackgroundAudio : MonoBehaviour
{
    [Header("Äänitiedostot")]
    [Tooltip("Lisää tähän kaikki taustaäänet, joita haluat soittaa satunnaisesti.")]
    public List<AudioClip> audioClips;

    [Header("Aika-asetukset (sekunteina)")]
    [Tooltip("Minimiaika äänien välillä.")]
    public float minDelay = 5f;

    [Tooltip("Maksimiaika äänien välillä.")]
    public float maxDelay = 15f;

    private AudioSource audioSource;

    void Start()
    {
        // Haetaan AudioSource-komponentti
        audioSource = GetComponent<AudioSource>();

        // Varmistetaan, että listassa on ääniä ennen kuin aloitetaan
        if (audioClips != null && audioClips.Count > 0)
        {
            StartCoroutine(PlayAudioWithRandomDelay());
        }
        else
        {
            Debug.LogWarning("RandomBackgroundAudio: Äänilista on tyhjä! Lisää AudioClip-tiedostoja Inspectorissa.");
        }
    }

    IEnumerator PlayAudioWithRandomDelay()
    {
        // Pyöritetään koodia ikuisesti taustalla
        while (true)
        {
            // Odotetaan satunnainen aika (minDelay ja maxDelay väliltä) ennen seuraavaa ääntä
            float randomDelay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(randomDelay);

            // Valitaan satunnainen ääni listasta
            int randomIndex = Random.Range(0, audioClips.Count);
            AudioClip randomClip = audioClips[randomIndex];

            // Soitetaan valittu ääni kerran (PlayOneShot sallii useamman äänen soida päällekkäin, jos uusi alkaa ennen kuin vanha loppuu)
            if (randomClip != null)
            {
                audioSource.PlayOneShot(randomClip);
            }
        }
    }
}
