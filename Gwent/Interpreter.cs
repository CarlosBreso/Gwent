using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using Unity.VisualScripting;


public class Memory
{
    public List<Dictionary<string, object>> memory_stack;
    public Memory()
    {
        memory_stack = new List<Dictionary<string, object>>();
    }
    public void Pop() => memory_stack.RemoveAt(memory_stack.Count - 1);
    public Dictionary<string, object> Peek() => memory_stack.ElementAt(memory_stack.Count - 1);
}

public class Interpreter
{
    public Dictionary<string, object> current_stack;
    public Memory Memory;
    public Parser parser;
    public GameKey GameKey;
    public JuegoGWENT juego;
    public AST tree;

    public Interpreter(AST tree, Parser parser, GameKey gameKey, JuegoGWENT juego)
    {
        this.parser = parser;
        Memory = new Memory();
        Memory.memory_stack.Add((new Dictionary<string, object>()));
        current_stack = Memory.Peek();
        this.GameKey = gameKey;
        this.juego = juego;
        this.tree = tree;
    }
    public void To_interpret() => Visit_Block((Block)tree);
    public void Visit_Block(Block node)
    {
        foreach (AST statement in node.s_list)
            Visit_statement(statement);
    }

    public void Visit_statement(AST node)
    {
        switch (node)
        {
            //visitador generico de statements
            case rPower rPower:
                Visit_rPower(rPower);
                break;
            case Paliado paliado:
                Visit_Paliado(paliado);
                break;
            case Destroy destroy:
                Visit_Destroy(destroy);
                break;
            case Invoke invoke:
                Visit_Invoke(invoke);
                break;
            case Displace displace:
                Visit_Displace(displace);
                break;
            case PowSelf pow:
                Visit_PowSelf(pow);
                break;
            case Power power:
                Visit_Power(power);
                break;
            case Assign_statement A_statement:
                Visit_Assign_statement(A_statement);
                break;
            case While @while:
                Visit_while(@while);
                break;
            case If IF:
                Visit_if(IF);
                break;
            case Method_call method:
                Visit_void_Method_Call(method);
                break;
            case Variable_D:
            case Method_D:
            case Empty:
                return;
        }
    }

    //Declaraciones del juego de cartas
    private void Visit_rPower(rPower rPower)
    {
        GameKey.actualPower[GetRandomCard(rPower.field)] += Visit_expr(rPower.change);
    }
    private void Visit_PowSelf(PowSelf pow)
    {
        int pos1 = juego.ActualMove.Position.Item1;
        int pos2 = juego.ActualMove.Position.Item2;
        GameKey.actualPower[juego.ActualMove.Card] += Visit_expr(pow.change);
    }
    public void Visit_Paliado(Paliado paliado)
    {
        List<Card> cards = GetCards(IsPotencied, juego.Field[juego.PlayerInTurn]);
        GameKey.actualPower[juego.ActualMove.Card] += cards.Count;
    }
    private void Visit_Destroy(Destroy destroy)
    {
        int pos1 = juego.ActualMove.Target.Item1;
        int pos2 = juego.ActualMove.Target.Item2;
        switch (juego.ActualMove.Card.Efecttype)
        {
            case EfectType.TargetAllie:
                if (juego.Field[juego.PlayerInTurn][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerInTurn][pos1, pos2]] = 0;
                break;
            case EfectType.TargetEnemy:
                if (juego.Field[juego.PlayerWaiting][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerWaiting][pos1, pos2]] = 0;
                break;
            default: return;
        }
    }
    public void Visit_Invoke(Invoke node)
    {
        foreach (Card card in GameKey.deck[juego.PlayerInTurn])
        {
            if (card.Name == juego.ActualMove.Card.Name)
            {
                GameKey.deck[juego.PlayerInTurn].Remove(card);
                (int, int) pos = JugadorIa.GenerateRandomPosition(juego);
                GameKey.field[juego.PlayerInTurn][pos.Item1, pos.Item2] = card;
                GameKey.actualPower[card] = card.Power;
            }
        }
    }
    public void Visit_Displace(Displace node)
    {
        switch (node.from)
        {   //Remover la carta del from y ponerla en to
            case "deck1":
                foreach (Card card in GameKey.deck[juego.Player1])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            case "deck2":
                foreach (Card card in GameKey.deck[juego.Player2])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            case "hand1":
                foreach (Card card in GameKey.hand[juego.Player1])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            case "hand2":
                foreach (Card card in GameKey.hand[juego.Player2])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            case "field1":
                foreach (Card card in GameKey.field[juego.Player1])
                    if (card != null)
                        if (card.Id == node.cardID)
                        {
                            GameKey.field[juego.Player1] = null;
                            Put_Card(card, node.to);
                        }
                break;
            case "field2":
                foreach (Card card in GameKey.field[juego.Player2])
                    if (card != null)
                        if (card.Id == node.cardID)
                        {
                            GameKey.field[juego.Player2] = null;
                            Put_Card(card, node.to);
                        }
                break;
            case "graveyard1":
                foreach (Card card in GameKey.graveyard[juego.Player1])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            case "graveyard2":
                foreach (Card card in GameKey.graveyard[juego.Player2])
                    if (card.Id == node.cardID)
                    {
                        GameKey.deck[juego.Player1].Remove(card);
                        Put_Card(card, node.to);
                    }
                break;
            default: throw new Exception("Lugar inexistente");
        }
    }
    public void Put_Card(Card card, string to)
    {
        switch (to)
        {
            case "deck1":
                GameKey.deck[juego.Player1].Add(card);
                break;
            case "deck2":
                GameKey.deck[juego.Player2].Add(card);
                return;
            case "hand1":
                GameKey.hand[juego.Player1].Add(card);
                break;
            case "hand2":
                GameKey.hand[juego.Player2].Add(card);
                break;
            case "field1":
                (int, int) position = JugadorIa.GenerateRandomPosition(juego);
                GameKey.field[juego.Player1][position.Item1, position.Item2] = card;
                break;
            case "field2":
                (int, int) pos = JugadorIa.GenerateRandomPosition(juego);
                GameKey.field[juego.Player2][pos.Item1, pos.Item2] = card;
                break;
            case "graveyard1":
                GameKey.graveyard[juego.Player1].Add(card);
                break;
            case "graveyard2":
                GameKey.graveyard[juego.Player2].Add(card);
                break;
            default: throw new Exception("Lugar inexistente");
        }
    }
    public void Visit_Power(Power node)
    {
        //effecttype enemy or ally
        //(,)target
        //node.chage
        int pos1 = juego.ActualMove.Target.Item1;
        int pos2 = juego.ActualMove.Target.Item2;
        switch (juego.ActualMove.Card.Efecttype)
        {
            case EfectType.TargetAllie:
                if (juego.Field[juego.PlayerInTurn][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerInTurn][pos1, pos2]] += Visit_expr(node.chage);
                break;
            case EfectType.TargetEnemy:
                if (juego.Field[juego.PlayerWaiting][pos1, pos2] != null)
                    GameKey.actualPower[juego.Field[juego.PlayerWaiting][pos1, pos2]] += Visit_expr(node.chage);
                break;
            default: return;
        }
    }
    
    //Declaraciones del lenguaje
    public void Visit_Assign_statement(Assign_statement node)
    {
        string ID = node.var.ID;
        if (node.type == "bool")
        {
            bool value = Visit_Bexpr(node.Expr);

            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
            return;
        }
        if (node.type == "int")
        {
            int value = Visit_expr(node.Expr);
            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
            return;
        }
        if (node.type == "string")
        {
            string value = Visit_string((String)node.Expr);
            if (current_stack.ContainsKey(ID))
                current_stack[ID] = value;

            else current_stack.Add(ID, value);
        }
    }
    public void Visit_if(If node)
    {
        if (Visit_Bexpr(node.condition))
            Visit_Block(node.block);
    }
    public void Visit_while(While node)
    {
        while (Visit_Bexpr(node.condition))
            Visit_Block(node.block);
    }
    public void Assign_Parameters(Method_call node)
    {
        List<(string, string)> formal_parameters = node.Method_reference.formal_parameters.parameters;
        List<(string, object)> arguments = new();

        for (int i = 0; i < formal_parameters.Count; i++)
        {
            string ID = formal_parameters.ElementAt(i).Item2;
            object value;
            switch (formal_parameters.ElementAt(i).Item1)
            {
                case "int":
                    value = Visit_expr(node.actual_parameters[i]);
                    break;
                case "bool":
                    value = Visit_Bexpr(node.actual_parameters[i]);
                    break;
                default:
                    value = Visit_string((String)node.actual_parameters[i]);
                    break;
            }
            arguments.Add((ID, value));
        }

        Memory.memory_stack.Add((new Dictionary<string, object>()));
        current_stack = Memory.Peek();

        foreach ((string, object) item in arguments)
            current_stack.Add(item.Item1, item.Item2);

    }
    public void Visit_void_Method_Call(Method_call node)
    {
        //CW
        if (node.ID == "print")
        {
            if (node.actual_parameters.ElementAt(0) is Var var)
                Console.WriteLine(current_stack[var.ID]);
            else if (node.actual_parameters.ElementAt(0) is String str)
                Console.WriteLine(str.value);
            return;
        }
        // asignando los parametros reales a los parametros formales
        Assign_Parameters(node);
        Visit_Block(node.Method_reference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
    }

    // Different Visit methods working inside an expression, can only return int
    private int Visit_Get(Get get)
    {
        Card[,] place = GameKey.field[juego.PlayerInTurn];

        switch (get.stat)
        {
            case "lessP":
                return GameKey.actualPower[GetCards(Menor, get.field)];
            case "mostP":
                return GameKey.actualPower[GetCards(Mayor, get.field)];
            default:
                throw new Exception("nada");
        }
    }
    
    

    //Seccion de expresiones aritmeticas
    public int Visit_expr(AST node)
    {
        switch (node)
        {
            // Identifies wich AST type is
            case Integer integer:
                return Visit_integer(integer);
            case Var var:
                return Visit_var(var);
            case Unitary_op unitaryOp:
                return Visit_Uni_op(unitaryOp);
            case Binary_op binary:
                return Visit_Bin_op(binary);
            case Method_call method:
                return Visit_intMethod(method);
            case Get get:
                return Visit_Get(get);
            default:
                throw new Exception("Wrong expression");
        }
    }
    public int Visit_intMethod(Method_call node)
    {
        Assign_Parameters(node);
        int value = Visit_intBlock(node.Method_reference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
        return value;
    }
    public int Visit_intBlock(Block block)
    {
        foreach (AST statement in block.s_list)
        {
            if (statement is Return re) return Visit_expr(re.retorno);
            Visit_statement(statement);
        }
        throw new Exception("Sin retorno");
    }
    public int Visit_integer(Integer node) => node.value;
    public int Visit_var(Var node) => (int)current_stack[node.ID];
    public int Visit_Uni_op(Unitary_op node) => -1 * Visit_expr(node.rigth);
    public int Visit_Bin_op(Binary_op node)
    {
        //left op rigth
        if (node.op.type == "Plus") return Visit_expr(node.left) + Visit_expr(node.rigth);
        if (node.op.type == "Minus") return Visit_expr(node.left) - Visit_expr(node.rigth);
        if (node.op.type == "Mult") return Visit_expr(node.left) * Visit_expr(node.rigth);
        if (node.op.type == "Div") return Visit_expr(node.left) / Visit_expr(node.rigth);

        throw new Exception("Invalid Sintax");
    }
    
    //Seccion de expresiones boleanas
    public bool Visit_Bblock(Block block)
    {
        foreach (AST statement in block.s_list)
        {
            if (statement is Return re) return Visit_Bexpr(re.retorno);
            Visit_statement(statement);
        }
        throw new Exception("Sin retorno");
    }
    public bool Visit_Bmethod(Method_call node)
    {
        Assign_Parameters(node);
        bool value = Visit_Bblock(node.Method_reference.block);

        Memory.Pop();
        current_stack = Memory.Peek();
        return value;
    }
    public bool Visit_Bexpr(AST node)
    {
        if (node is Binary_op binary_op) return Visit_Bexpr(binary_op);
        if (node is Bool boolean) return Visit_Bool(boolean);
        if (node is Var var) return Visit_Bvar(var);
        if (node is Method_call method) return Visit_Bmethod(method);
        else throw new Exception("Wrong boolean expression");
    }
    public bool Visit_Bexpr(Binary_op node)
    {
        switch ((string)node.op.value)
        {
            case ">": return Visit_expr(node.left) > Visit_expr(node.rigth);
            case "<": return Visit_expr(node.left) < Visit_expr(node.rigth);
            case ">=": return Visit_expr(node.left) >= Visit_expr(node.rigth);
            case "<=": return Visit_expr(node.left) <= Visit_expr(node.rigth);
            case "==": return Visit_expr(node.left) == Visit_expr(node.rigth);
            case "!=": return Visit_expr(node.left) != Visit_expr(node.rigth);
            case "or": return Visit_Bexpr(node.left) || Visit_Bexpr(node.rigth);
            case "and": return Visit_Bexpr(node.left) && Visit_Bexpr(node.rigth);

            default: throw new Exception("Comparador inexistente en el lenguaje");
        }
    }
    public bool Visit_Bool(Bool node) => node.value;
    public bool Visit_Bvar(Var var) => (bool)current_stack[var.ID];
    
    //Metodo para visitar expresion de tipo string
    public string Visit_string(String node) => node.value;
    
    
    //Metodos de recorrido del campo
    public Card GetRandomCard(Card[,] field)
    {
        bool finded = false;
        foreach (var card in field)
        {
            if (card != null)
            {
                finded = true;
                break;
            }
        }
        if (!finded) return null;
        else
        {
            (int, int) position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
            while (field[position.Item1, position.Item2] == null)
            {
                position = (new System.Random().Next(0, 2), new System.Random().Next(0, 5));
            }
            return field[position.Item1, position.Item2];
        }
    }
    public List<Card> GetCards(Func<Card, bool> function, Card[,] field)
    {
        List<Card> cards = new();
        foreach (var card in field)
        {
            if (card != null)
                if (function(card)) cards.Add(card);
        }
        return cards;
    }
    public Card GetCards(Func<Card, Card, Card> function, Card[,] place)
    {
        Card result = null;
        foreach (var card in place)
        {
            if (card != null)
            {
                result = card;
                break;
            }
        }
        if (result == null) return null;
        foreach (Card card in place)
        {
            if (card != null)
                result = function(result, card);
        }
        return result;
    }
    
    //Functions
    Card Mayor(Card x, Card y) => x.Power > y.Power ? x : y;
    Card Menor(Card x, Card y) => x.Power < y.Power ? x : y;
    bool IsPotencied(Card x) => juego.ActualPower[x] > x.Power ? true : false;
    bool AllCards(Card x) => true;
    bool IsInjured(Card x) => juego.ActualPower[x] < x.Power ? true : false;
}