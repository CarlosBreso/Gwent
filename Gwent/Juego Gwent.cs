using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Unity.VisualScripting;
using UnityEngine;

public class GameKey
{
    public readonly Dictionary<Jugador, Card[,]> field;
    public readonly Dictionary<Jugador, List<Card>> hand;
    public readonly Dictionary<Jugador, List<Card>> deck;
    public readonly Dictionary<Jugador, List<Card>> graveyard;
    public readonly Dictionary<Card, int> actualPower = new();
    public readonly Dictionary<Jugador, int> scoreTable;
    public readonly Dictionary<Jugador, bool> isGived;
    public readonly Dictionary<Jugador, bool> hasPlayed;

    public GameKey(Dictionary<Jugador, Card[,]> field, Dictionary<Jugador, List<Card>> hand, Dictionary<Jugador, List<Card>> deck, Dictionary<Jugador, List<Card>> graveyard, Dictionary<Card, int> actualPower, Dictionary<Jugador, int> scoreTable, Dictionary<Jugador, bool> isGived, Dictionary<Jugador, bool> hasPlayed)
    {
        this.field = field;
        this.hand = hand;
        this.deck = deck;
        this.graveyard = graveyard;
        this.actualPower = actualPower;
        this.scoreTable = scoreTable;
        this.isGived = isGived;
        this.hasPlayed = hasPlayed;
    }
}

public class JuegoGWENT : IEnumerable<Move>
{
    public Move ActualMove;
    private readonly Dictionary<Jugador, Card[,]> field;
    private readonly Dictionary<Jugador, List<Card>> hand;
    private readonly Dictionary<Jugador, List<Card>> deck;
    private readonly Dictionary<Jugador, List<Card>> graveyard;
    private readonly Dictionary<Card, int> actualPower = new();
    private readonly Dictionary<Jugador, int> scoreTable;
    private readonly Dictionary<Jugador, bool> isGived;
    private readonly Dictionary<Jugador, bool> hasPlayed;
    private readonly GameKey gameKey;


    public IReadOnlyDictionary<Jugador, Card[,]> Field
    {
        get
        {
            return new Dictionary<Jugador, Card[,]>
            {
                [Player1] = field[Player1],
                [Player2] = field[Player2],
            };
        }
    }
    public IReadOnlyDictionary<Jugador, IReadOnlyList<Card>> Hand
    {
        get
        {
            return new Dictionary<Jugador, IReadOnlyList<Card>>
            {
                [Player1] = hand[Player1],
                [Player2] = hand[Player2],
            };
        }
    }
    public IReadOnlyDictionary<Jugador, IReadOnlyList<Card>> Deck
    {
        get
        {
            return new Dictionary<Jugador, IReadOnlyList<Card>>
            {
                [Player1] = deck[Player1],
                [Player2] = deck[Player2],
            };
        }
    }
    public IReadOnlyDictionary<Jugador, IReadOnlyList<Card>> Graveyard
    {
        get
        {
            return new Dictionary<Jugador, IReadOnlyList<Card>>
            {
                [Player1] = graveyard[Player1],
                [Player2] = graveyard[Player2],
            };
        }
    }
    public Jugador Player1 { get; private set; }
    public Jugador Player2 { get; private set; }
    public IReadOnlyDictionary<Card, int> ActualPower { get => actualPower; }
    public IReadOnlyDictionary<Jugador, int> ScoreTable { get => scoreTable; }
    public int Turn { get; private set; }
    public Jugador PlayerInTurn { get; private set; }
    public Jugador PlayerWaiting => (PlayerInTurn == Player2) ? Player1 : Player2;
    public IReadOnlyDictionary<Jugador, bool> IsGived { get => isGived; }
    public IReadOnlyDictionary<Jugador, bool> HasPlayed { get => hasPlayed; }
    public bool IsFinished
    {
        get => scoreTable[Player1] == 2 || scoreTable[Player2] == 2;
        private set
        {
        }
    }
    public bool InvalidMoveCommitted { get; private set; }
    public Card EmptyCard = new(-1, "", 0, "", "", "", 0);
    public Dictionary<Jugador, int> HandCount
    {
        get
        {
            return new Dictionary<Jugador, int>
            {
                [Player1] = Hand[Player1].Count,
                [Player2] = Hand[Player2].Count
            };
        }
    }
    public JuegoGWENT(Jugador player1, Jugador player2)
    {


        field = new Dictionary<Jugador, Card[,]>
        {
            [player1] = new Card[2, 5],
            [player2] = new Card[2, 5]
        };
        hand = new Dictionary<Jugador, List<Card>>
        {
            [player1] = new List<Card>(),
            [player2] = new List<Card>()
        };
        graveyard = new Dictionary<Jugador, List<Card>>
        {
            [player1] = new List<Card>(),
            [player2] = new List<Card>()
        };
        deck = new Dictionary<Jugador, List<Card>>
        {
            [player1] = player1.Deck.ToList(),
            [player2] = player2.Deck.ToList()
        };
        isGived = new Dictionary<Jugador, bool>
        {
            [player1] = false,
            [player2] = false,
        };
        hasPlayed = new Dictionary<Jugador, bool>
        {
            [player1] = false,
            [player2] = false,
        };
        scoreTable = new Dictionary<Jugador, int>
        {
            [player1] = 0,
            [player2] = 0,
        };
        gameKey = new(field, hand, deck, graveyard, actualPower, scoreTable, isGived, hasPlayed);
        this.Player1 = player1;
        this.Player2 = player2;
        PlayerInTurn = ((new System.Random().Next(0, 1) == 0) ? Player1 : Player2);

    }
    public void StarGame()
    {
        Console.WriteLine("StarGame");
        ShufflingCards(Player1);
        ShufflingCards(Player2);
        DrawCard(Player1, 8);
        DrawCard(Player2, 8);

    }
    public void ShufflingCards(Jugador player)
    {
        Console.WriteLine("ShufflingCards");
        deck[player] = deck[player].OrderBy(x => new System.Random().Next(0, deck[player].Count)).ToList();
    }
    public void ExecuteMove(Move move)
    {
        ActualMove = move;
        Card card = move.Card;
        if (card.Name == "")
        {
            Debug.Log("pass detected");
            isGived[PlayerInTurn] = true;

            if (IsGived[Player1] && IsGived[Player2])
            {
                UpdateScoreTable();
                if (!IsFinished) NewRound();
            }
            return;
        }

        //if (card.Type == Card.CardType.Especial)
        //  Destroy(card);

        else
        {
            hand[PlayerInTurn].Remove(card);
            Field[PlayerInTurn][move.Position.Item1, move.Position.Item2] = card;
            actualPower[card] = card.Power;
        }
        Compila("{PowSelf(Get(lessP myField))}");
        DestroyDeathCards();
    }
    private void NewRound()
    {   actualPower.Clear();
        PlayerInTurn = ((new System.Random().Next(0, 1) == 0) ? Player1 : Player2);
        Turn = 0;
        foreach (Jugador player in new Jugador[] { Player1, Player2 })
        {
            isGived[player] = false;
            DrawCard(player, 3);
            foreach (Card card in field[player])
                Destroy(card);
        }
    }
    void DestroyDeathCards()
    {
        foreach (Jugador player in new Jugador[] { Player1, Player2 })
        {
            foreach (Card card in field[player])
                if (card != null && ActualPower[card] <= 0)
                    Destroy(card);
        }
    }
    private void Compila(string v)
    {
        Program.Interpretar(v, gameKey, this);
    }
    public int Score(Jugador player)
    {
        int score = 0;
        foreach (Card item in Field[player])
        {
            if (item != null) score += ActualPower[item];

        }
        return score;
    }
    public bool Destroy(Card card)
    {
        Console.WriteLine("Destroy");

        bool finded = false;
        foreach (Jugador player in new Jugador[] { Player1, Player2 })
        {
            //  finded = hand[player].Remove(card) || deck[player].Remove(card);
            for (int i = 0; i < Field[player].GetLength(0); i++)
                for (int j = 0; j < Field[player].GetLength(1); j++)
                    if (Field[player][i, j] == card)
                    {
                        Field[player][i, j] = null;
                        finded = true;
                    }

            if (finded)
                graveyard[player].Add(card);

        }
        return finded;
    }
    public bool DrawCard(Jugador player, int n)
    {
        Console.WriteLine("DrawCard");
        n = Math.Min(n, 8 - hand[player].Count);

        if (deck[player].Count < n) return false;

        else
        {
            hand[player].AddRange(deck[player].TakeLast(n));
            deck[player].RemoveRange(deck[player].Count - 1 - n, n);
            return true;
        }
    }
    public void RunNextMove()
    {

        foreach (Move move in this)
        {

            if (!IsAValidMove(move)) PenalizePlayer(PlayerInTurn);
            else
            {
                ExecuteMove(move);
                if (!IsGived[PlayerWaiting]) PlayerInTurn = PlayerWaiting;
                break;
            }

        }
    }
    public void RunToTheEnd()

    {
        foreach (Move move in this)
        {
            if (!IsAValidMove(move))
            {
                PenalizePlayer(PlayerInTurn);
                break;
            }
            else
            {
                ExecuteMove(move);
                if (!IsGived[PlayerWaiting]) PlayerInTurn = PlayerWaiting;
            }


        }
    }
    private void UpdateScoreTable()
    {
        int ply1Score = Score(Player1);
        int ply2Score = Score(Player2);
        if (ply1Score > ply2Score) scoreTable[Player1]++;
        else if (ply1Score < ply2Score) scoreTable[Player2]++;
        else
        {
            scoreTable[Player1]++;
            scoreTable[Player2]++;
        }
    }
    private void PenalizePlayer(Jugador player)
    {
        InvalidMoveCommitted = true;
        scoreTable[(player == Player2) ? Player1 : Player2]++;
    }
    private bool IsAValidMove(Move move)
    {
        if (move.Card.Name == "")
        {
            return true;
        }
        if (!hand[PlayerInTurn].Contains(move.Card))
        {
            Console.WriteLine("This is not a valid move");
            return false;
        }
        if (field[PlayerInTurn][move.Position.Item1, move.Position.Item2] != null)
        {
            Console.WriteLine("This is not a valid move");
            return false;
        }
        if (move.Card.Efecttype == EfectType.TargetEnemy || move.Card.Efecttype == EfectType.TargetAllie)
        {
            Jugador player = (move.Card.Efecttype == EfectType.TargetAllie) ? PlayerInTurn : PlayerWaiting;
            if (field[player][move.Target.Item1, move.Target.Item2] == null && !IsFieldEmpty(field[player])) return false;
        }
        return true;
    }
    private bool IsFieldEmpty(Card[,] field)
    {
        foreach (Card card in field)
        {
            if (card != null) return false;
        }

        return true;
    }
    public IEnumerator<Move> GetEnumerator()
    {

        if (IsFinished || InvalidMoveCommitted) throw new InvalidOperationException("Este juego ya se ha terminado");
        while (!IsFinished)
        {
            Turn++;
            yield return PlayerInTurn.Play(this, hand[PlayerInTurn]);

        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
