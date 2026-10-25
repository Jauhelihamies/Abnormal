using UnityEngine;

public class KameranSeuranta : MonoBehaviour
{
    public Transform pelaaja;

    [Header("Seurannan pehmeys")]
    public float xPehmeys = 5f;
    public float yPehmeys = 3f;

    [Header("Etäisyys pelaajasta")]
    public float xEtäisyys = 3f;
    public float yEtäisyys = 1f;
    public float zEtäisyys = -10f;

    [Header("Kameran pystysuuntaiset rajat")]
    public float minY = 0f;    // Alin piste, johon kamera voi mennä
    public float maxY = 5f;    // Ylin piste, johon kamera voi mennä

    void LateUpdate()
    {
        if (pelaaja != null)
        {
            float kohdeX = pelaaja.position.x + xEtäisyys;
            float kohdeY = pelaaja.position.y + yEtäisyys;

            // Rajoitetaan kohdeY pysymään annettujen rajojen sisällä
            kohdeY = Mathf.Clamp(kohdeY, minY, maxY);

            float uusiX = Mathf.Lerp(transform.position.x, kohdeX, xPehmeys * Time.deltaTime);
            float uusiY = Mathf.Lerp(transform.position.y, kohdeY, yPehmeys * Time.deltaTime);

            transform.position = new Vector3(uusiX, uusiY, zEtäisyys);
        }
    }
}
