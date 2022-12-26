using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AST
{
    //not usefull
}

public class Empty : AST
{
    //pass
}
public class String : AST
{
    public string value;
    public String(string value)
    {
        this.value = value;
    }
}
public class Var : AST
{
    public string ID;

    public Var(string ID)
    {
        this.ID = ID;
    }
}
public class Integer : AST
{
    public int value;

    public Integer(int value)
    {
        this.value = value;
    }
}
public class Binary_op : AST
{
    public AST left;
    public Token op;
    public AST rigth;

    public Binary_op(AST left, Token op, AST rigth)
    {
        this.left = left;
        this.op = op;
        this.rigth = rigth;
    }
}
public class Unitary_op : AST
{
    public Token op;
    public AST rigth;

    public Unitary_op(Token op, AST rigth)
    {
        this.op = op;
        this.rigth = rigth;
    }
}
public class Bool : AST
{
    public bool value;

    public Bool(bool value)
    {
        this.value = value;
    }
}

public class If : AST
{
    public AST condition;
    public Block block;

    public If(AST condition, Block block)
    {
        this.condition = condition;
        this.block = block;
    }
}
public class While : AST
{
    public AST condition;
    public Block block;

    public While(AST condition, Block block)
    {
        this.condition = condition;
        this.block = block;
    }
}
public class Assign_statement : AST
{
    public string type;
    public Var var;
    public AST Expr;

    public Assign_statement(Var var, AST Expr)
    {
        this.type = "";
        this.var = var;
        this.Expr = Expr;
    }
}
public class Variable_D : AST
{
    public string type;
    public List<Var> IDs;

    public Variable_D(string type)
    {
        this.type = type;
        this.IDs = new List<Var>();
    }
}
public class Method_D : AST
{
    public string type;
    public string ID;
    public Parameters Parameters;
    public Block block;

    public Method_D(string type, string ID, Parameters Parameters, Block block)
    {
        this.type = type;
        this.ID = ID;
        this.Parameters = Parameters;
        this.block = block;
    }
}
public class Return : AST
{
    public AST retorno;

    public Return(AST retorno)
    {
        this.retorno = retorno;
    }
}
public class Method_call : AST
{
    public string ID;
    public List<AST> actual_parameters;
    public Method_symbol Method_reference;
    //esto es la hostia

    public Method_call(string ID, List<AST> actual_parameters)
    {
        this.ID = ID;
        this.actual_parameters = actual_parameters;
    }
}

public class Get : AST
{
    public string stat;
    public string place;
    public Card[,] field;

    public Get(string stat, string place)
    {
        this.stat = stat;
        this.place = place;
    }
}

public class RandomPower : AST
{
    public Card[,] Field;
    public string Place;
    public AST Change;
    public AST Targets;

    public RandomPower(string place, AST change, AST targets)
    {
        Place = place;
        Change = change;
        Targets = targets;
    }
}
public class Power : AST
{
    public AST chage;

    public Power(AST chage)
    {
        this.chage = chage;
    }
}

public class PowSelf : AST
{
    public AST change;

    public PowSelf(AST change)
    {
        this.change = change;
    }
}

public class Destroy : AST
{
    //pass
}

public class Invoke : AST
{
    //pass
}

public class Paliado : AST
{
    //pass
}

public class Displace : AST
{
    public string from;
    public int cardID;
    public string to;

    public Displace(string from, int cardID, string to)
    {
        this.from = from;
        this.cardID = cardID;
        this.to = to;
    }
}
public class Draw : AST
{
    public string place;
    public AST amount;

    public Draw(string place, AST amount)
    {
        this.place = place;
        this.amount = amount;
    }
}
public class Block : AST
{
    public List<AST> s_list;
    public Block(List<AST> s_list) => this.s_list = s_list;
}
public class Parameters : AST
{
    public List<(string, string)> parameters;
    // Type-Value
    public Parameters(List<(string, string)> parameters) => this.parameters = parameters;
}