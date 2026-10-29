using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Taustapalat järjestyksessä")]
    public List<GameObject> taustaPrefabit;

    [Header("Asetukset")]
    public Transform pelaaja;
    public float palanLeveys = 12.8f;
    public float palanKorkeus = 6f;
    public int palojaKerrallaan = 2;

    private Queue<GameObject> aktiivisetPalat = new Queue<GameObject>();
    private float seuraavanPalanX = 0f;
    private int nykyinenTeemaIndeksi = 0;

    void Start()
    {
        for (int i = 0; i < palojaKerrallaan; i++)
        {
            SynnytaSeuraavaPala();
        }
    }

    void Update()
    {
        if (pelaaja != null && pelaaja.position.x > seuraavanPalanX - (palanLeveys * palojaKerrallaan))
        {
            SynnytaSeuraavaPala();
            PoistaVanhaPala();
        }
    }

    void SynnytaSeuraavaPala()
    {
        GameObject valittuPrefab = taustaPrefabit[nykyinenTeemaIndeksi];
        Vector3 uusiSijainti = new Vector3(seuraavanPalanX, palanKorkeus, 0f);
        GameObject uusiPala = Instantiate(valittuPrefab, uusiSijainti, Quaternion.identity);

        aktiivisetPalat.Enqueue(uusiPala);
        seuraavanPalanX += palanLeveys;

        // Teeman vaihto (esimerkki):
        if (aktiivisetPalat.Count % 5 == 0 && nykyinenTeemaIndeksi < taustaPrefabit.Count - 1)
        {
            nykyinenTeemaIndeksi++;
        }
    }

    void PoistaVanhaPala()
    {
        if (aktiivisetPalat.Count > palojaKerrallaan + 1)
        {
            GameObject vanha = aktiivisetPalat.Dequeue();
            Destroy(vanha);
        }
    }

    public void VaihdaSeuraavaanTeemaan()
    {
        if (nykyinenTeemaIndeksi < taustaPrefabit.Count - 1)
        {
            nykyinenTeemaIndeksi++;
        }
    }
}
