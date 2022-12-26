using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class Menusricpt : MonoBehaviour
{
    public bool P1alreadyselect;
    public static bool twoplayers;
    public static bool twoIA;
    [SerializeField]
    private TMP_Text CardName;
    [SerializeField]
    private TMP_Text CardDescription;
    public Image CardImage;
    [SerializeField]
    private List<Sprite> AllDeckCards;
    [SerializeField]
    private TMP_Text CardPower;
    public GameObject Select;
    public GameObject PanelCartas;
    public GameObject[] Decks;
    public GameObject Anuncio;

    public void ShowCard(string Card, string Descripcion, Image Img, int Power)//metodo de la escena VerCartas, muestra la informacion de una carta
    {
        CardName.text = Card;
        CardDescription.text = Descripcion;
        CardImage = Img;
        CardPower.text = Power.ToString();

    }
    //  public void SceneMazodeCartas() => SceneManager.LoadScene(1);
    public void SceneMenuPrincipal() => SceneManager.LoadScene(0);
    // public void SceneVerCartas() => SceneManager.LoadScene(2);
    public void CloseGame() => Application.Quit();
    public void Jugar()
    {
        SceneManager.LoadScene(1);
    }
    public void IvsI()
    {   
        twoIA = true; 
        Gamemanager.Deckselected1 = Config.DeckPaths[new System.Random().Next(0, 2)];
        Gamemanager.Deckselected2 = Config.DeckPaths[new System.Random().Next(0, 2)];
        SceneManager.LoadScene(3);
    }
    public void PvsI()
    {
        Select.SetActive(false);
        PanelCartas.SetActive(true);
        Anuncio.GetComponent<TextMeshProUGUI>().text = "Selecciona un Deck P1";

    }
    public void PvsP()
    {
        Select.SetActive(false);
        PanelCartas.SetActive(true);
        twoplayers = true;
        Anuncio.GetComponent<TextMeshProUGUI>().text = "Selecciona un Deck P1";
    }
    public void AssingDeck(GameObject selected)
    {
        if (!twoplayers)
        {
            for (int i = 0; i < Decks.Length; i++)
            {
                if (Decks[i] == selected)
                {
                    Gamemanager.Deckselected1 = Config.DeckPaths[i];
                    Gamemanager.Deckselected2 = Config.DeckPaths[new System.Random().Next(0, 2)];
                    break;
                }
            }
            SceneManager.LoadScene(3);  
        }

        else
        {
            if (P1alreadyselect)
            {
                for (int i = 0; i < Decks.Length; i++)
                {
                    if (Decks[i] == selected)
                    {
                        Gamemanager.Deckselected2 = Config.DeckPaths[i];
                        break;
                    }
                }
                SceneManager.LoadScene(3);
            }
            else
            {
                for (int i = 0; i < Decks.Length; i++)
                {
                    if (Decks[i] == selected)
                    {
                        Gamemanager.Deckselected1 = Config.DeckPaths[i];
                        P1alreadyselect = true;
                        Anuncio.GetComponent<TextMeshProUGUI>().text = "Selecciona un Deck P2";
                        break;
                    }
                }
            }
        }
    }
}


