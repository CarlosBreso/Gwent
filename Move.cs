using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Move
{
    public Card Card { get; }
    public (int, int) Position { get; }
    public (int, int) Target { get; }

    public Move(Card card, (int, int) position)
    {
        Card = card;
        Position = position;
    }
    public Move(Card card, (int, int) position, (int, int) target)
    {
        Card = card;
        Position = position;
        Target = target;
    }
    public Move()
    {
        Card = new(-1, "", 0, "", "", "", 0);
    }

}
