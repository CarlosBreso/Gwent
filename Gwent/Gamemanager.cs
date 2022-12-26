using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;





public class Gamemanager : MonoBehaviour
{
    public bool Played;
    public bool ShowingCard;
    public static bool Iscardselected;
    public static FieldPosition Position;
    public static FieldPosition Target;
    public static Card SelectedCard;
    public static bool PassTurn= false;


    /// <summary>
    /// GameObjects de La interfaz
    /// </summary>  
    public GameObject InformationPanel;
    public TMP_Text NameINf;
    public TMP_Text PowerInf;
    public TMP_Text DescrpcionInf;
    public Image ImageInf;
    public GameObject Anuncements;
    public GameObject PanelPausa;
    public TMP_Text PuntuacionTotal1;
    public TMP_Text PuntuacionTotal2;
    public GameObject Mazo1Count;
    public GameObject Mazo2Count;
    public GameObject[] Hand1 = new GameObject[8];
    public GameObject[] Hand2 = new GameObject[8];
    public GameObject field1;
    public GameObject field2;
    public GameObject[] CC1 = new GameObject[5];
    public GameObject[] CC2 = new GameObject[5];
    public GameObject[] D1 = new GameObject[5];
    public GameObject[] D2 = new GameObject[5];
    public GameObject[] RoundsPlayer1;
    public GameObject[] RoundsPlayer2;


    /// <summary>
    /// Mazos
    /// </summary>

    private List<Card> Player1Deck;
    private List<Card> Player2Deck;
    public JuegoGWENT Game;
    public Jugador Player1;
    public Jugador Player2;
    public static string Deckselected1;
    public static string Deckselected2;



    public void Awake()
    {
        Player1Deck = Config.LoadDeck(Deckselected1);
        Player2Deck = Config.LoadDeck(Deckselected2);

    }
    public void Start()
    {
        PlayerAsignement(Menusricpt.twoplayers, Menusricpt.twoIA);
        Game = new JuegoGWENT(Player1, Player2);
        Game.StarGame();
        

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!ShowingCard) Pausa();
            else NotShowCard();

        }//si se presiona la tecla esc se pausa el juego o se deselecciona la carta
        ActualizarPuntuacion();
        UptadeHands();
        UpdateField();
        UptadeWinRounds();
        UpdateDecknumber();
        AreWinner();
        Play();

        if (Input.GetKeyDown(KeyCode.T))
        {

        }



    }
    private void Pausa()
    {
        PanelPausa.SetActive(true);

    }
    public void Reanudar()
    {
        PanelPausa.SetActive(false);
    }/// <summary>
    public void Salir()
    {
        SceneManager.LoadScene(0);
    }   ///metodos de control 
        /// </summary>
    public void Select(GameObject card)
    {
        if ((Game.PlayerInTurn == Player1 && Hand1.Contains(card)) || (Game.PlayerInTurn == Player2 && Hand2.Contains(card)))
        {
            SelectedCard = card.GetComponent<CardComp>().Card;
            Iscardselected = true;
            Target = null;
        }

    }
    public static void Deselect()
    {
        SelectedCard = null;
        Iscardselected = false;
    }

    /// <summary>
    /// metodos de la logica del juego
    /// </summary>

    public void ShowCard(GameObject carta)
    {
        if (carta.GetComponent<CardComp>().Hide) { }
        else
        {
            ShowingCard = true;
            InformationPanel.SetActive(true);
            ImageInf.GetComponent<Image>().sprite = Config.LoadImage(carta.GetComponent<CardComp>().Card.ImageUrl);
            NameINf.text = carta.GetComponent<CardComp>().Card.Name;
            PowerInf.text = carta.GetComponent<CardComp>().Card.Power.ToString();
            DescrpcionInf.text = carta.GetComponent<CardComp>().Card.Description;
        }



    }//este metodo muestra en pantalla la carta seleccionada con sus datos
    public void NotShowCard()
    {
        NameINf.text = null;
        PowerInf.text = null;
        ImageInf.sprite = null;
        DescrpcionInf.text = null;
        InformationPanel.SetActive(false);
        ShowingCard = false;
        Deselect();
    }
    public void ActualizarPuntuacion()
    {
        PuntuacionTotal1.text = Game.Score(Player1).ToString();
        PuntuacionTotal2.text = Game.Score(Player2).ToString();
    }
    public void UptadeHands()
    {
        Card[] han1 = Game.Hand[Player1].ToArray();
        Card[] han2 = Game.Hand[Player2].ToArray();


        for (int i = 0; i < 8; i++)
        {
            if (i >= han1.Length)
            {
                Hand1[i].GetComponent<CardComp>().Card = null;
                Hand1[i].SetActive(false);
            }
            else
            {
                Hand1[i].GetComponent<CardComp>().Card = han1[i];
                Hand1[i].SetActive(true);
            }
        }
        for (int i = 0; i < 8; i++)
        {
            if (i >= han2.Length)
            {
                Hand2[i].GetComponent<CardComp>().Card = null;
                Hand2[i].SetActive(false);
            }
            else
            {
                Hand2[i].GetComponent<CardComp>().Card = han2[i];
                Hand2[i].SetActive(true);
            }
        }
    }//conecta el hand de la logica con el hand visual
    public void UpdateField()
    {
        Card[,] field1 = Game.Field[Player1];
        Card[,] field2 = Game.Field[Player2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == 0)
                {
                    if (field1[i, j] != null)
                    {
                        CC1[j].GetComponent<CardComp>().Card = field1[i, j];
                        CC1[j].GetComponent<CardComp>().ActualPower = Game.ActualPower[CC1[j].GetComponent<CardComp>().Card];
                        CC1[j].GetComponent<CardComp>().Infield = true;
                        CC1[j].GetComponent<CardComp>().Hide = false;
                    }
                    else
                    {
                        CC1[j].GetComponent<CardComp>().Card = null;
                    }
                    if (field2[i, j] != null)
                    {
                        D2[j].GetComponent<CardComp>().Card = field2[i, j];
                        D2[j].GetComponent<CardComp>().ActualPower = Game.ActualPower[D2[j].GetComponent<CardComp>().Card];
                        D2[j].GetComponent<CardComp>().Infield = true;
                        D2[j].GetComponent<CardComp>().Hide = false;
                    }
                    else
                    {
                        D2[j].GetComponent<CardComp>().Card = null;
                    }

                }
                else
                {
                    if (field1[i, j] != null)
                    {
                        D1[j].GetComponent<CardComp>().Card = field1[i, j];
                        D1[j].GetComponent<CardComp>().ActualPower = Game.ActualPower[D1[j].GetComponent<CardComp>().Card];
                        D1[j].GetComponent<CardComp>().Infield = true;
                        D1[j].GetComponent<CardComp>().Hide = false;
                    }
                    else
                    {
                        D1[j].GetComponent<CardComp>().Card = null;
                    }
                    if (field2[i, j] != null)
                    {
                        CC2[j].GetComponent<CardComp>().Card = field2[i, j];
                        CC2[j].GetComponent<CardComp>().ActualPower = Game.ActualPower[CC2[j].GetComponent<CardComp>().Card];
                        CC2[j].GetComponent<CardComp>().Infield = true;
                        CC2[j].GetComponent<CardComp>().Hide = false;
                    }
                    else
                    {
                        CC2[j].GetComponent<CardComp>().Card = null;
                    }
                }
            }
        }
    }//Conecta el campo de la logica con el campo visual
    public void DecideField(GameObject place)
    {
        if (place.GetComponent<CardComp>().Card != null)
        {
            ShowCard(place);
            TargetPlace(place);
        }
        else {PositionInputValid(place);}
    }
    public void PositionInputValid(GameObject Position)
    {
        if (Game.PlayerInTurn == Player1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (CC1[i] == Position) { Gamemanager.Position = new FieldPosition(0, i); break; }
                if (D1[i] == Position) { Gamemanager.Position = new FieldPosition(1, i); break; }
            }
        }
        if (Game.PlayerInTurn == Player2)
        {
            for (int i = 0; i < 5; i++)
            {
                if (D2[i] == Position) { Gamemanager.Position = new FieldPosition(0, i); break; }
                if (CC2[i] == Position) { Gamemanager.Position = new FieldPosition(1, i); break; }
            }
        }
    }
    public void Pass()
    {
        PassTurn = true;
    }
    public void Annuncement(string Anuncio)
    {
        Anuncements.SetActive(true);
        Anuncements.GetComponent<TextMeshProUGUI>().text = Anuncio;


    }
    public void HideHand(GameObject[] Hand)
    {
        foreach (GameObject hand in Hand)
        {
            hand.GetComponent<CardComp>().Hide = true;
        }
    }
    public void ShowHand(GameObject[] Hand)
    {
        foreach (GameObject hand in Hand)
        {
            hand.GetComponent<CardComp>().Hide = false;
        }
    }
    public void PlayerAsignement(bool twoplayers, bool twoIas)
    {
        if (twoplayers)
        {
            Player1 = new JugadorHumano("humano1", Player1Deck.ToArray());
            Player2 = new JugadorHumano("humano2", Player2Deck.ToArray());
        }

        else if (twoIas)
        {
            Player1 = new JugadorIa("Ia1", Player1Deck.ToArray());
            Player2 = new JugadorIa("Ia2", Player2Deck.ToArray());
        }
        else
        {
            Player1 = new JugadorHumano("humano1", Player1Deck.ToArray());
            Player2 = new JugadorIa("Ia2", Player2Deck.ToArray());
        }
    }
    public void UptadeWinRounds()
    {
        for (int i = 0; i < Game.ScoreTable[Player1]; i++)
        {
            RoundsPlayer1[i].SetActive(true);
        }

        for (int i = 0; i < Game.ScoreTable[Player2]; i++)
        {
            RoundsPlayer2[i].SetActive(true);
        }

    }
    public void UpdateDecknumber()
    {
        Mazo1Count.GetComponent<TextMeshProUGUI>().text = Game.Deck[Player1].Count.ToString();
        Mazo2Count.GetComponent<TextMeshProUGUI>().text = Game.Deck[Player2].Count.ToString();
    }
    public void AreWinner()
    {
        if (Game.IsFinished)
        {
            foreach (var a in Game.ScoreTable)
            {
                if (a.Value == 2)
                {
                    Annuncement("El GANADOR ES " + a.Key.Name);
                }
            }

        }
    }
    public static void Reload()
    {
        PassTurn = false;
        Deselect();
        Position = null;
        Target=null;


    }
    public void TargetPlace(GameObject place)
    {
        if (SelectedCard != null)
        {
            if (Game.PlayerInTurn == Player1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (SelectedCard.Efecttype == EfectType.TargetAllie)
                    {
                        if (CC1[i] == place) Target = new FieldPosition(0, i);
                        if (D1[i] == place) Target = new FieldPosition(1, i);

                    }

                    if(SelectedCard.Efecttype==EfectType.TargetEnemy)
                    {
                        if (CC2[i] == place) Target = new FieldPosition(1, i);
                        if (D2[i] == place) Target = new FieldPosition(0, i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (SelectedCard.Efecttype == EfectType.TargetEnemy)
                    {
                        if (CC1[i] == place) Target = new FieldPosition(0, i);
                        if (D1[i] == place) Target = new FieldPosition(1, i);

                    }

                    if (SelectedCard.Efecttype == EfectType.TargetAllie)
                    {
                        if (CC2[i] == place) Target = new FieldPosition(1, i);
                        if (D2[i] == place) Target = new FieldPosition(0, i);
                    }
                }

            }
           
        }
    }
    public bool VerifyField()
    {
        if (SelectedCard.Efecttype == EfectType.TargetAllie)
        {
            foreach (var VARIABLE in Game.Field[Game.PlayerInTurn])
            {
                if (VARIABLE != null) return true;
            }
        }

        if (SelectedCard.Efecttype == EfectType.TargetEnemy)
        {
            foreach (var VARIABLE in Game.Field[Game.PlayerWaiting])
            {
                if (VARIABLE != null) return true;
            }
        }

        return false;
    }
    public void Play()
    {
        if (Menusricpt.twoIA)
        {//implementacion del modo dos Ia
            //Annuncement("Pulsa la Tecla D para continuar");
            if (Input.GetKeyDown(KeyCode.D)) Game.RunNextMove();
        }
        else if (Menusricpt.twoplayers)
        {
            //implementacion del modo dos jugadores
            if (Game.PlayerWaiting == Player1) { HideHand(Hand1); ShowHand(Hand2); }
            else { HideHand(Hand2); ShowHand(Hand1); }



            if ((Position != null && SelectedCard != null) || PassTurn)
            {
                if (SelectedCard.Efecttype == EfectType.NoTarget)
                {
                    Target = null;
                    Game.RunNextMove();
                    Played = true;

                }
                else
                {
                    if (!VerifyField() && !Played)
                    {
                        Target = null;
                        Game.RunNextMove();
                        Played = false;

                    }
                    if (Target != null && !Played)
                    {
                        Game.RunNextMove();
                    }
                    Played = false;
                }

            }





        }
        else
        {
            //implementacion del modo jugador vs Ia
            HideHand(Hand2);
            if (Game.PlayerInTurn.GetType() == typeof(JugadorHumano))
            {
              
                if (Game.IsGived[Game.PlayerInTurn] && !Game.IsFinished) Game.RunNextMove();
                else if ((Position != null && SelectedCard != null) || PassTurn)
                {
                    if (PassTurn)
                    {
                        Game.RunNextMove();
                    }
                    else
                    {
                        if (SelectedCard.Efecttype == EfectType.NoTarget)
                        {
                            Target = null;
                            Game.RunNextMove();
                            Played = true;

                        }
                        else
                        {
                            if (!VerifyField() && !Played)
                            {
                                Target = null;
                                Game.RunNextMove();
                                Played = false;

                            }


                            if (Target != null && !Played)
                            {
                                Game.RunNextMove();
                            }
                            Played = false;
                        }
                    }
                   

                }

            }
            else if(!Game.IsFinished)Game.RunNextMove();
        }

    }

}
