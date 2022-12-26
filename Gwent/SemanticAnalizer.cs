using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;




public class Symbol
{
    public string ID;
    public Symbol(string ID)
    {
        this.ID = ID;
    }
}

public class Type_symbol : Symbol
{
    public string type;
    public Type_symbol(string ID) : base(ID)
    {
        this.type = ID;
    }
}
public class Var_symbol : Symbol
{
    public string type;
    public Var_symbol(string ID, string type) : base(ID)
    {
        this.type = type;
    }
}

public class Method_symbol : Symbol
{
    public string type;
    public Parameters formal_parameters;
    public Block block;

    public Method_symbol(string type, string ID, Parameters parameters, Block block) : base(ID)
    {
        this.type = type;
        this.formal_parameters = parameters;
        this.block = block;
    }
}
public class Build_out : Symbol
{
    public Build_out(string ID) : base(ID) { }
}
public class Function_Symbol : Symbol
{
    //Functions does return a value
    public Token Return_type;
    public List<object> parameters;

    public Function_Symbol(string ID, Token Return_type, List<object> parameters) : base(ID)
    {
        this.Return_type = Return_type;
        this.parameters = parameters;
    }
}
public class Scope
{
    public Dictionary<string, Symbol> Symbol_tab;
    public Scope father;

    public Scope(Scope father, Dictionary<string, Symbol> parameters)
    {
        this.father = father;
        Symbol_tab = parameters;

        Symbol_tab.Add("int", new Type_symbol("int"));
        Symbol_tab.Add("string", new Type_symbol("string"));
        Symbol_tab.Add("bool", new Type_symbol("bool"));
        Symbol_tab.Add("print", new Build_out("print"));
    }
}

public class Semantic_analizer
{
    public GameKey gameKey;
    public JuegoGWENT Juego;
    public Scope Current_scope;
    public Parser parser;
    AST tree;

    public Semantic_analizer(AST tree, Parser parser, GameKey GameKey, JuegoGWENT Juego)
    {
        Current_scope = new Scope(null, new Dictionary<string, Symbol>());
        this.parser = parser;
        this.gameKey = GameKey;
        this.Juego = Juego;
        this.tree = tree;
    }
    public void Check() => Visit_void_Block((Block)tree);
    public void Visit_void_Block(Block node)
    {
        foreach (AST item in node.s_list) Visit_statement(item);
    }
    public void Visit_return_Block(Block block, string type, string ID)
    {
        bool doesReturn = false;
        foreach (AST statement in block.s_list)
        {
            if (statement is Return re)
            {
                if (type == "int") Visit_Expr(re.retorno);
                if (type == "bool") Visit_Bexpr(re.retorno);
                if (type == "string") Visit_string(re.retorno);
                doesReturn = true;
            }
            else Visit_statement(statement);
        }
        if (!doesReturn) throw new Exception("The method " + ID + " does not have a return value");
    }

    public void Visit_statement(AST node)
    {
        if (node is RandomPower      randomPower) Visit_rPower(randomPower);
        if (node is PowSelf          pow) Visit_Expr(pow.change);
        if (node is Power            power) Visit_power(power);
        if (node is Displace         displace) Visit_Displace(displace);
        if (node is While            nodewhile) Visit_while(nodewhile);
        if (node is If               nodeif) Visit_If(nodeif);
        if (node is Variable_D       variable_D) Visit_Var_D(variable_D);
        if (node is Method_D         method_D) Visit_Method_D(method_D);
        if (node is Method_call      method_Call) Visit_Method_Call(method_Call);
        if (node is Assign_statement assign_Statement) Visit_Asign_statement(assign_Statement);
        if (node is Empty) return;
        else throw new Exception("Declaracion invalida");
    }

    private void Visit_rPower(RandomPower node)
    {
        switch (node.place)
        {
            case "myField":
                node.field = Juego.Field[Juego.PlayerInTurn];
                break;
            case "eField":
                node.field = Juego.Field[Juego.PlayerInTurn];
                break;
            default: throw new Exception();
        }
    }

    public void Visit_while(While node)
    {
        Visit_Bexpr(node.condition);
        Visit_void_Block(node.block);
    }

    public void Visit_If(If node)
    {
        Visit_Bexpr(node.condition);
        Visit_void_Block(node.block);
    }

    public void Visit_Method_D(Method_D node)
    {
        //Type - ID
        //Añadir variables declaradas en los parametros
        Current_scope = new Scope(Current_scope, new Dictionary<string, Symbol>());

        Visit_Parameters(node.Parameters);

        if (node.type != "void") Visit_return_Block(node.block, node.type, node.ID);
        else Visit_void_Block(node.block);

        // Despues de visitar el metodo, vuelve al scope anterior y añade el metodo al symbol tab
        Current_scope = Current_scope.father;

        Current_scope.Symbol_tab.Add(node.ID, new Method_symbol(node.type, node.ID, node.Parameters, node.block));
    }

    public void Visit_Parameters(Parameters node)
    {
        //Type-ID
        foreach ((string, string) var in node.parameters)
        {
            Current_scope.Symbol_tab.Add(var.Item2, new Var_symbol(var.Item2, var.Item1));
        }
    }

    public void Visit_Asign_statement(Assign_statement node)
    {
        if (Current_scope.Symbol_tab.ContainsKey(node.var.ID))
        {
            Var_symbol var = (Var_symbol)Current_scope.Symbol_tab[node.var.ID];

            if (var.type == "bool")
            {
                node.type = "bool";
                Visit_Bexpr(node.Expr);
            }
            if (var.type == "int")
            {
                node.type = "int";
                Visit_Expr(node.Expr);
            }
            if (var.type == "string")
            {
                node.type = "string";
                Visit_string(node.Expr);
            }
            else throw new Exception("Variable de tipo inexistente");
        }
        else throw new Exception("Variable no delcarada");
    }

    public void Visit_Var_D(Variable_D node)
    {
        // checks type and addS new vaR Symbol
        if (!Current_scope.Symbol_tab.ContainsKey(node.type))
            throw new Exception("Tipo no existente");

        foreach (Var var in node.IDs)
        {
            if (!Current_scope.Symbol_tab.ContainsKey(var.ID))
                Current_scope.Symbol_tab.Add(var.ID, new Var_symbol(var.ID, node.type));

            else throw new Exception("Declaracion de una variable ya existente");
        }
    }

    public void Visit_Method_Call(Method_call node)
    {
        //check if name is in table
        //chek number and types of actual parameters and formal parameters
        if (!Current_scope.Symbol_tab.ContainsKey(node.ID))
            throw new Exception("The method " + node.ID + " does not exist in current context");

        Method_symbol Mref = (Method_symbol)Current_scope.Symbol_tab[node.ID];
        //check arguments

        int N = node.actual_parameters.Count;

        if (N != Mref.formal_parameters.parameters.Count)
            throw new Exception("Cantidad de parametros incorrecta");

        for (int i = 0; i < N; i++)
        {
            switch (Mref.formal_parameters.parameters.ElementAt(i).Item1)
            {
                case "int":
                    Visit_Expr(node.actual_parameters.ElementAt(i));
                    break;
                case "bool":
                    Visit_Bexpr(node.actual_parameters.ElementAt(i));
                    break;
                case "string":
                    Visit_string(node.actual_parameters.ElementAt(i));
                    break;
                default:
                    throw new Exception("Tipo no permitido");
            }
        }
        node.Method_reference = Mref;
    }
    public void Visit_Var(Var node, string type)
    {
        if (!Current_scope.Symbol_tab.ContainsKey(node.ID))
            throw new Exception("Variable no declarada");

        Var_symbol var = (Var_symbol)Current_scope.Symbol_tab[node.ID];
        // ID y typo intercambiados
        if (var.type != type) throw new Exception("Variable y tipo no coincidentes");

        foreach (Token keyword in Lexer.Keywords)
            if ((string)keyword.value == node.ID)
                throw new Exception("Nombre de variable no permitido");

    }
    public void Visit_string(AST node)
    {
        if (node is String cadena) return;

        if (node is Var var)
            if (Current_scope.Symbol_tab[var.ID] is Var_symbol var_Symbol)
                if (var_Symbol.type == "string") return;

        throw new Exception("Expresion incoherente en segun tipo");
    }

    public void Visit_Expr(AST node)
    {
        switch (node)
        {
            case Binary_op binary when !(binary.op.type == "comparison") && binary.op.type != "or" && binary.op.type != "and":
                Visit_Expr(binary.left);
                Visit_Expr(binary.rigth);
                return;
            case Binary_op binary:
                throw new Exception("Expresion incoherente en segun tipo");
            case Unitary_op unitary:
            {
                if (unitary.op.type == "Minus") Visit_Expr(unitary.rigth);
                else throw new Exception("Expresion incoherente en segun tipo");
                return;
            }
            case Var var:
                Visit_Var(var, "int");
                break;
            case Get get:
                Visit_Get(get);
                break;
            case Integer:
                return;
            case Method_call method:
                Visit_Method_Call(method);
                break;
            default:
                throw new Exception("Expresion incoherente en segun tipo");
        }
    }

    public void Visit_Get(Get node)
    {
        switch (node.place)
        {
            case "myField":
                node.field = gameKey.field[Juego.PlayerInTurn];
                break;
            case "eField":
                node.field = gameKey.field[Juego.PlayerWaiting];
                break;
            default:
                throw new Exception("Lugar inexistente");
        }

        switch (node.stat)
        {
            case "mostP":
                break;
            case "lessP":
                break;
            default:
                throw new Exception("stat equivocada");
        }
    }

    public void Visit_Bexpr(AST node)
    {
        if (node is Binary_op binary)
        {
            if ((binary.op.type == "Comparison"))
            {
                Visit_Expr(binary.left);
                Visit_Expr(binary.rigth);
                return;
            }
            else if (binary.op.type == "or" || binary.op.type == "and")
            {
                Visit_Bexpr(binary.left);
                Visit_Bexpr(binary.rigth);
                return;
            }
            else throw new Exception("Expresion incoherente en segun tipo");
        }
        else if (node is Bool) return;
        else if (node is Var var) Visit_Var(var, "bool");
        else if (node is Method_call method) Visit_Method_Call(method);
        else throw new Exception("Expresion incoherente en segun tipo");
    }
    public void Visit_power(Power node)
    {
        //effecttype enemy or ally
        //(,)target
        //node.chage
        return;

        throw new Exception("La carta no existe en el campo");
    }
    public bool Spot_Card(int ID, string place)
    {
        switch (place)
        {   //verificar si la carta existe y
            case "deck1":
                foreach (Card card in gameKey.deck[Juego.Player1])
                    if (card.Id == ID)
                        return true;
                return false;
            case "deck2":
                foreach (Card card in gameKey.deck[Juego.Player2])
                {
                    if (card.Id == ID)
                    {
                        Debug.Log("existe");
                        return true;
                    }
                }
                return false;
            case "hand1":
                foreach (Card card in gameKey.hand[Juego.Player1])
                    if (card.Id == ID)
                        return true;
                return false;
            case "hand2":
                foreach (Card card in gameKey.hand[Juego.Player2])
                    if (card.Id == ID)
                        return true;
                return false;
            case "graveyard1":
                foreach (Card card in gameKey.graveyard[Juego.Player1])
                    if (card.Id == ID)
                        return true;
                return false;
            case "graveyard2":
                foreach (Card card in gameKey.graveyard[Juego.Player2])
                    if (card.Id == ID)
                        return true;
                return false;
            default: throw new Exception("Lugar inexistente");
        }
    }
    public void Visit_Displace(Displace node)
    {
        if (Spot_Card(node.cardID, (string)node.from))
        {
            //verificar si la carta esta en el from      
            return;
        }
        throw new Exception("Carta no encontrada");
        //ir al lugar y verificar si la carta existe
    }
}
