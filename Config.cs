using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Config: MonoBehaviour
{/// <summary>
 /// direcciones de los mazos
 /// </summary>
   
    public static string PathCardBackIMage = "s06-cardback_thumb";
    public static string[] DeckPaths = new string[4] { "/Resources/MazoElfos.json", "/Resources/MazoEnanos.json", "/Resources/MazoMonstruos.json", "/Resources/MazoHumanos.json" };
    public static void OpacityUP(GameObject place)//metodo para subir la opacidad de un objeto
    {
        Image image = place.GetComponent<Image>();
        var tempcolor = image.color;
        tempcolor.a = 255;
        place.GetComponent<Image>().color = tempcolor;

    }
    public static void OpacityDown(GameObject place)//metodo para subir la opacidad de un objeto
    {
        Image image = place.GetComponent<Image>();
        var tempcolor = image.color;
        tempcolor.a = 0.09f;
        place.GetComponent<Image>().color = tempcolor;

    }

    public static Sprite LoadImage(string path)
    {
        Sprite image = Resources.Load<Sprite>(path);
        return image;
    }//con este metodo cargo las imagenes de la carta a partir de su path
    public static List<Card> LoadDeck(string path)//este metodo carga los decks a partir de su path
    {
        CardList mazo;
        string jsontext = File.ReadAllText(Application.dataPath + path);//se lee el json
        mazo = JsonUtility.FromJson<CardList>(jsontext);               //utilizando la clase json utility lleno la instancia
        return mazo.Deck;                                              //de cardlist con todos los objetos cartas y los devuelvo
    }
}

public class FieldPosition
{
    public FieldPosition(int x, int y)
    {
        X=x;
        Y=y;
    }
    public  int X { get; set; }
    public  int Y { get; set; }

    
}
