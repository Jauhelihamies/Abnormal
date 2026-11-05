using System.Collections;
using UnityEngine;

public class AudioViiveella : MonoBehaviour
{
    [Header("Asetukset")]
    [Tooltip("Äänilähde, jota käytetään toistamiseen.")]
    public AudioSource audioSource;

    [Tooltip("Äänitiedosto (Clip), joka halutaan soittaa.")]
    public AudioClip audioClip;

    [Tooltip("Viive sekunteina ennen kuin ääni soitetaan.")]
    public float viiveSekunteina = 3.0f;

    [Tooltip("Soitetaanko ääni automaattisesti, kun peli käynnistyy?")]
    public bool soitakaynnistyksessa = true;

    void Start()
    {
        if (soitakaynnistyksessa)
        {
            KäynnistäÄänenToisto();
        }
    }

    /// <summary>
    /// Kutsu tätä funktiota (esim. toisesta skriptistä tai UI-napista), jos haluat laukaista äänen manuaalisesti.
    /// </summary>
    public void KäynnistäÄänenToisto()
    {
        if (audioSource != null && audioClip != null)
        {
            StartCoroutine(SoitaAudioViiveella());
        }
        else
        {
            Debug.LogWarning("AudioSource tai AudioClip puuttuu skriptistä!", this);
        }
    }

    private IEnumerator SoitaAudioViiveella()
    {
        // Odotetaan haluttu aika sekunteina
        yield return new WaitForSeconds(viiveSekunteina);

        // Soitetaan ääni tasan kerran (PlayOneShot varmistaa, ettei se jää looppaamaan)
        audioSource.PlayOneShot(audioClip);

        Debug.Log($"Ääni '{audioClip.name}' soitettu viiveen jälkeen.");
    }
}
