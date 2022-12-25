using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary> Provee la clase abstracta base para jugadores de juegos de mesa. </summary>
public abstract class Jugador
{
    /// <summary> Representa el nombre del jugador. </summary>
    public string Name { get; }

    /// <summary> Constructor base de la clase abstracta Jugador. </summary>
    /// <param name="Name"> Representa el nombre del jugador. </param>
    protected Jugador(string Name, Card[] deck)
    {
        this.Name = Name;
        Deck = deck;
    }

    public Card[] Deck { get; }
    /// <summary> Devuelve la jugada seleccionada por el jugador. </summary>


    public abstract Move Play(JuegoGWENT game, IEnumerable<Card> hand);

    public override string ToString()
    {
        return Name;
    }
}

public class JugadorHumano : Jugador
{
    public JugadorHumano(string nombre, Card[] deck) : base(nombre, deck)
    {
    }

    /// <summary> Devuelve la jugada seleccionada por el jugador. </summary>
    public override Move Play(JuegoGWENT game, IEnumerable<Card> hand)
    {

        Card card = game.EmptyCard;
        (int, int) pos = (0, 0);
        (int, int) target = (0, 0);


        bool pass = Gamemanager.PassTurn;
        if (Gamemanager.SelectedCard != null) card = Gamemanager.SelectedCard;
        if (Gamemanager.Position != null) pos = (Gamemanager.Position.X, Gamemanager.Position.Y);
        if (Gamemanager.Target != null) target = (Gamemanager.Target.X, Gamemanager.Target.Y);
        Gamemanager.Reload();


        if (pass) return new Move(game.EmptyCard, pos);
        if (card.Efecttype == EfectType.NoTarget)
            return new Move(card, pos);
        else return new Move(card, pos,target);
    }
}

public class JugadorIa : Jugador
{
    public JugadorIa(string nombre, Card[] deck) : base(nombre, deck)
    {
    }

    public static(int, int)  GenerateRandomPosition(JuegoGWENT game)
    {
        (int, int) position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
        while (game.Field[game.PlayerInTurn][position.Item1, position.Item2] != null)
        {
            position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
        }
        return position;
    }

    public (int, int) SelectTarget(Card card, Card[,] field)
    {
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if (field[i, j] == card) return (i, j);
            }
        }

        return (0, 0);
    }

    public override Move Play(JuegoGWENT game, IEnumerable<Card> hand)
    {
        Card posiblemove = hand.FirstOrDefault();
        int Scoreself = game.Score(game.PlayerInTurn);
        int Scoreother = game.Score(game.PlayerWaiting);
        Card[,] selfField = game.Field[game.PlayerInTurn];
        Card[,] otherField = game.Field[game.PlayerWaiting];
       

        if (hand.Count() == 0)
        {
            return new Move(game.EmptyCard, (0, 0));
        }

        if (game.IsGived[game.PlayerWaiting])// si el contrario ha pasado
        {
            if (Scoreother < Scoreself) return new Move(game.EmptyCard, (0, 0));//si tiene menos puntos q tu pasa
            else//si no
            {
                foreach (Card card in hand)
                {
                    if (card.Power + Scoreself > Scoreother)//si con solo una carta es posible ganar la juega
                    {
                        //a partir de aqui se verifican los tipos de efectos de la carta

                        if (card.Efecttype == EfectType.NoTarget)
                            return new Move(card, GenerateRandomPosition(game));
                        else if (card.Efecttype == EfectType.TargetEnemy)
                        {
                            foreach (var field in otherField)
                            {
                                if (field != null) return new Move(card, GenerateRandomPosition(game), SelectTarget(card, otherField));


                            }
                        }
                        else
                        {
                            foreach (var card1 in selfField)
                            {
                                if (card1 != null) return new Move(card, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                            }
                        }
                        // finish
                    }
                }

                if (game.ScoreTable[game.PlayerWaiting] > 0)//si el contrario ya gano una ronda juega la carta de mayor poder
                {
                    foreach (Card card in hand)
                    {
                        if (card.Power > posiblemove.Power && card.Efecttype == EfectType.TargetEnemy)
                        {

                            posiblemove = card;
                        }
                    }
                    if (posiblemove.Efecttype == EfectType.NoTarget)
                        return new Move(posiblemove, GenerateRandomPosition(game));
                    else if (posiblemove.Efecttype == EfectType.TargetEnemy)
                    {
                        foreach (var field in otherField)
                        {
                            if (field != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(field, otherField));


                        }
                    }
                    else
                    {
                        foreach (var card1 in selfField)
                        {
                            if (card1 != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                        }
                    }



                }
                else //si no ha ganado ninguna ronda
                {
                    if (Scoreother - Scoreself > 12)//si la diferencia es isalvable pasa
                    {
                        return new Move(game.EmptyCard, (0, 0));
                    }
                    else//si no juega la carta mayor
                    {
                        foreach (Card card in hand)
                        {
                            if (card.Power > posiblemove.Power && card.Efecttype == EfectType.TargetEnemy)
                            {

                                posiblemove = card;
                            }
                        }
                        if (posiblemove.Efecttype == EfectType.NoTarget)
                            return new Move(posiblemove, GenerateRandomPosition(game));
                        else if (posiblemove.Efecttype == EfectType.TargetEnemy)
                        {
                            foreach (var field in otherField)
                            {
                                if (field != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(field, otherField));


                            }
                        }
                        else
                        {
                            foreach (var card1 in selfField)
                            {
                                if (card1 != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                            }
                        }


                    }
                }
            }


        }

        if (Scoreself - Scoreother > 12) //si tienes una ventaja aplastante
        {
            if (game.ScoreTable[game.PlayerWaiting] > 0)
            {
                if (game.HandCount[game.PlayerWaiting] < 3)// y tu contrario no tiene muchas cartas pasa
                {
                    return new Move(game.EmptyCard, (0, 0));
                }
                else// si no juega la menor carta
                {
                    foreach (Card card in hand)
                    {
                        if (card.Power < posiblemove.Power) posiblemove = card;
                    }
                    if (posiblemove.Efecttype == EfectType.NoTarget)
                        return new Move(posiblemove, GenerateRandomPosition(game));
                    else if (posiblemove.Efecttype == EfectType.TargetEnemy)
                    {
                        foreach (var field in otherField)
                        {
                            if (field != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(field, otherField));


                        }
                    }
                    else
                    {
                        foreach (var card1 in selfField)
                        {
                            if (card1 != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                        }
                    }



                }
            }
            else return new Move(game.EmptyCard, (0, 0));//si el rival no ha ganado ninguna ronda pasa
        }

        if (Scoreself > Scoreother)
        {
            foreach (Card card in hand)
            {
                if (card.Power < posiblemove.Power) posiblemove = card;
            }
            if (posiblemove.Efecttype == EfectType.NoTarget)
                return new Move(posiblemove, GenerateRandomPosition(game));
            else if (posiblemove.Efecttype == EfectType.TargetEnemy)
            {
                foreach (var field in otherField)
                {
                    if (field != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(field, otherField));


                }
            }
            else
            {
                foreach (var card1 in selfField)
                {
                    if (card1 != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                }
            }

        }
        else
        {
            if (hand.Count() == 1 && (Scoreself + hand.First().Power < Scoreother))
            {
                return new Move(game.EmptyCard, (0, 0));
            }

            foreach (Card card in hand)
            {
                if (Math.Abs(Scoreother - (Scoreself + card.Power)) < posiblemove.Power) posiblemove = card;
            }
            if (posiblemove.Efecttype == EfectType.NoTarget)
                return new Move(posiblemove, GenerateRandomPosition(game));
            else if (posiblemove.Efecttype == EfectType.TargetEnemy)
            {
                foreach (var field in otherField)
                {
                    if (field != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(field, otherField));


                }
            }
            else
            {
                foreach (var card1 in selfField)
                {
                    if (card1 != null) return new Move(posiblemove, GenerateRandomPosition(game), SelectTarget(card1, selfField));

                }
            }
        }

        return new Move(posiblemove,GenerateRandomPosition(game),(0,0));
    }

}
