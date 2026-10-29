using UnityEngine;
using TMPro;

public class TMPSmeltingEffect : MonoBehaviour
{
    private TMP_Text textComponent;
    private bool isMelting = false;

    [Header("Aallon asetukset")]
    [Tooltip("Kuinka nopeasti aalto leviää keskeltä reunoille")]
    public float waveSpeed = 8f;

    [Tooltip("Aallon tiheys / kehien määrä tekstissä")]
    public float waveFrequency = 0.05f;

    [Tooltip("Kuinka voimakkaasti aalto pullistaa/skaalaa kirjaimia")]
    public float waveMagnitude = 0.3f;

    private float effectTimer = 0f;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!isMelting)
        {
            if (effectTimer > 0)
            {
                effectTimer = 0;
                textComponent.ForceMeshUpdate();
            }
            return;
        }

        effectTimer += Time.deltaTime;
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // Lasketaan tekstikentän todellinen keskipiste maailmantilassa (tai paikallisessa tilassa)
        // Tämä varmistaa, että aalto lähtee ympyränä tismalleen tekstin keskeltä
        Vector3 textCenter = Vector3.zero;
        int validChars = 0;

        for (int i = 0; i < characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                textCenter += textInfo.characterInfo[i].bottomLeft;
                textCenter += textInfo.characterInfo[i].topRight;
                validChars += 2;
            }
        }
        if (validChars > 0) textCenter /= validChars;

        // Käydään läpi kirjaimet ja luodaan ympyräaalto
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Lasketaan merkin keskipiste
            Vector3 charCenter = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) * 0.5f;

            // Lasketaan reaalinen etäisyys (ympyrän säde) tekstin keskipisteestä tähän kirjaimeen
            float distanceFromCenter = Vector3.Distance(textCenter, charCenter);

            // Luodaan aaltoefektikerroin, joka liikkuu ajan myötä ulospäin
            float waveFactor = (effectTimer * waveSpeed) - (distanceFromCenter * waveFrequency);

            // Käytetään sini-aaltoa luomaan pehmeä ylös-alas pullistus (pisara-aalto)
            float scaleMultiplier = 1f + Mathf.Sin(waveFactor) * waveMagnitude;

            // Muokataan kirjaimen neljää kärkeä sen oman keskipisteen suhteen (skaalaus)
            // Tämä pullistaa kirjainta ulospäin/sisäänpäin rikkomatta tekstin rivitystä tai UI-laatikkoa
            for (int j = 0; j < 4; j++)
            {
                Vector3 origVertex = vertices[vertexIndex + j];
                Vector3 toVertex = origVertex - charCenter;

                // Skaataan sekä X- että Y-suunnassa, jolloin syntyy pyöreä "pulssi"
                vertices[vertexIndex + j] = charCenter + toVertex * scaleMultiplier;
            }
        }

        // Piirretään muutokset
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    public void StartMelting()
    {
        isMelting = true;
    }

    public void StopMelting()
    {
        isMelting = false;
    }
}
