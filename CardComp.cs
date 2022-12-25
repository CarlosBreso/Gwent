using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class CardComp : MonoBehaviour
{
    public Card Card { get; set; }
    public bool Hide;
    public bool Infield;
    public int ActualPower;
    public Sprite CardBack;

    /// <summary>
    /// en este metodo uptade conecto la carta de la logica con lo visual
    /// </summary>
    public void Awake()
    {
        CardBack = Config.LoadImage(Config.PathCardBackIMage);
    }
    public void Update()
    {
        if (this.Card != null)
        {
            if (Hide) { HideCard(); }
            else
            {
                this.gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite = Config.LoadImage(this.Card.ImageUrl);
                Config.OpacityUP(this.gameObject);
                if (Infield)
                {
                    ColorChange();
                    this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = ActualPower.ToString();
                }
                else this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = this.Card.Power.ToString();
            }
        }
        else
        {
            this.gameObject.GetComponent<Image>().sprite = null;
            this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
            this.Infield = false;
            Config.OpacityDown(this.gameObject);

        }

        //igualo los componentes imagenes y texto del game object a la imagen de la carta y su poder
    }
    public void HideCard()
    {
        if (this.Card != null)
        { 
            this.gameObject.GetComponent<Image>().sprite = CardBack; 
            this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
            Config.OpacityUP(this.gameObject);

        }

    }
    public void ColorChange()
    {
        if (ActualPower < this.Card.Power) this.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        else if (ActualPower > this.Card.Power) this.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
        else this.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
    }



}
