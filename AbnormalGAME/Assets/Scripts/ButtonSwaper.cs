using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSwaper : MonoBehaviour
{
    [Header("Menu Panelit")]
    public GameObject KEYBOARD_MENU;
    public GameObject CONTROLLER_MENU;
    public GameObject KEYBOARD_MENU2;
    public GameObject CONTROLLER_MENU2;

    [Header("Ensimmäiset painikkeet ohjaimelle")]
    public Button mainFirstSelectedButton;    
    public Button keyboardMenuFirstButton;      
    public Button controllerMenuFirstButton;    

    void OnEnable()
    {
        // Kun koko menu aukeaa ensimmäistä kertaa
        SetFocusToButton(mainFirstSelectedButton);
    }

    public void GetKeyboardMenu()
    {
        CONTROLLER_MENU2.SetActive(false);
        CONTROLLER_MENU.SetActive(false);
        KEYBOARD_MENU.SetActive(true);
        KEYBOARD_MENU2.SetActive(true);


        // Siirretään ohjaimen valinta heti uuden näkymän painikkeeseen
        SetFocusToButton(keyboardMenuFirstButton);
    }

    public void GetControllerMenu()
    {
        CONTROLLER_MENU.SetActive(true);
        KEYBOARD_MENU.SetActive(false);
        CONTROLLER_MENU2.SetActive(true);
        KEYBOARD_MENU2.SetActive(false);

        // Siirretään ohjaimen valinta takaisin ohjainnäkymän painikkeeseen
        SetFocusToButton(controllerMenuFirstButton);
    }

    // Apumetodi, joka nollaa ja asettaa uuden fokuksen luotettavasti
    private void SetFocusToButton(Button targetButton)
    {
        if (targetButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(targetButton.gameObject);
        }
    }
}
