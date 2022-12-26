using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;



public class Parser
{
    public Lexer lexer;
    public Token current_token;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
        this.current_token = lexer.Get_next_token();
    }

    public AST To_parse() => Block();

    public void Verify(string type)
    {
       
        //Match token type and passed type and continues the proccess 
        if (current_token.type == type) current_token = lexer.Get_next_token();
        else
        {
            Debug.Log(current_token.type);
            Debug.Log(type);
            throw new Exception("Sintaxis inválida");
        }
    }

    public Return Return()
    {
        Verify("return");
        return new Return(Bexpr());
    }
    public Integer Integer()
    {
        Token number = current_token;
        Verify("Number");
        return new Integer((int)number.value);
    }
    public String Cadena()
    {
        //"verify" quote pick every char to string until next quote
        Token cadena = current_token;
        Verify("Text");
        return new String((string)cadena.value);
    }
    public Var Variable()
    {
        Token token = current_token;
        Verify("ID");
        return new Var((string)token.value);
    }
    public Method_call Method_Call(string ID)
    {
        Verify("Lpar");

        List<AST> Actual_parameters = new();

        while (current_token.type != "Rpar")
        {
            Actual_parameters.Add(Bexpr());

            if (current_token.type != "Comma") break;
            Verify("Comma");
        }
        Verify("Rpar");
        return new Method_call(ID, Actual_parameters);
    }
    public AST Factor()
    {
        //factor: Get stat place
        //factor: Method call
        //factor: integer
        //factor: Lparen expr Rparen
        //factor: MINUS integer
        //factor: variable 
        switch (current_token.type)
        {
            case "Get":
                return Get();
            case "Number":
                return Integer();
            case "Minus":
                Verify("Minus");
                return new Unitary_op(current_token, Factor());
            case "Lpar":
                {
                    Verify("Lpar");
                    AST node = Expression();
                    Verify("Rpar");
                    return node;
                }

            case "ID":
                {
                    Var var = Variable();
                    if (current_token.type == "Lpar") return Method_Call(var.ID);
                    return var;
                }

            default:
                throw new Exception("Sintaxis inválida");
        }
    }

    private Get Get()
    {
        Verify("Get");
        Verify("Lpar");
        Token stat = current_token;
        Verify("stat");
        Token place = current_token;
        Verify("place");
        Verify("Rpar");
        return new((string)stat.value, (string)place.value);
    }

    public AST Term()
    {
        //term: factor[(*|/)factor]*
        AST node = Factor();

        while (current_token.type == "Mult" || current_token.type == "Div")
        {
            Token op = current_token;

            if (op.type == "Mult") Verify("Mult");
            else Verify("Div");

            node = new Binary_op(node, op, Factor());
        }
        return node;
    }
    public AST Expression()
    {
        //expr : Term [(+|-)term]*
        AST node = Term();

        while (current_token.type == "Plus" || current_token.type == "Minus")
        {
            Token op = current_token;

            if (op.type == "Plus") Verify("Plus");
            else if (op.type == "Minus") Verify("Minus");

            node = new Binary_op(node, op, Term());
        }
        return node;
    }

    public AST Bfactor()
    {
        //Bfactor: Quote string Quote
        //Bfactor: true/false |Expr (relation Expr)
        // relation: (< | > | == | != | <= | >=)
        switch (current_token.type)
        {
            case "true":
                Verify("true");
                return new Bool(true);
            case "false":
                Verify("false");
                return new Bool(false);
            case "Text":
                return Cadena();
            default:
                {
                    AST expr = Expression();
                    if (current_token.type == "Comparison")
                    {
                        Token op = current_token;
                        Verify("Comparison");
                        return new Binary_op(expr, op, Expression());
                    }
                    return expr;
                }
        }
    }
    public AST Bterm()
    {
        //Bterm: Bfactor (and Bfactor)*
        AST node = Bfactor();

        while (current_token.type == "and")
        {
            Token op = current_token;
            Verify("and");
            node = new Binary_op(node, op, Bfactor());
        }
        return node;
    }

    public AST Bexpr()
    {
        //Bterm (or Bterm)*
        AST node = Bterm();

        while (current_token.type == "or")
        {
            Token op = current_token;
            Verify("or");

            node = new Binary_op(node, op, Bterm());
        }
        return node;
    }

    public AST Statement()
    {
        //statement: Paliado()
        //statement Invoke()
        //Statement: Displace Lpar (Deck|Hand|Graveyard) Comma ID Rpar
        //Statement: Draw Lpar integer Comma (Deck|Graveyard|Field) Rpar
        //Statement: Power Lpar Expr Rpar
        //Statement: return Bexpr
        //Statement: if/while Lpar Bexpr Rpar Block
        //statement: Empty
        //Stamtement: Method Type ID Formal_Parameters Block
        //Statement: Type ID(comma ID)*
        //Statement: ID EQUAL Bexpr
        //Statement: ID Lpar Actual_parameters Rpar
        //Actual_parameters: Bexpr (comma Bexpr)*

        switch (current_token.type)
        {
            case "RandomPower":
                return RandomPower();
            case "TargetedPower":
                return PowSelf();
            case "SelectPower":
                return Paliado();
            case "PosPower":
                return Destroy();
            case "OrigPower":
                return Invoke();
            case "OrigSelectPower":
                return Displace();
            case "Draw":
                return Draw();
            case "return":
                return Return();
            case "Power":
                return Power();
            case "while":
                return While();
            case "if":
                return If();
            case "Type":
                return Variable_D();
            case "Method":
                return Method_D();
            case "Rscope":
                return new Empty();
            case "ID":
                {   // assign or method call
                    Token ID = current_token;
                    Verify("ID");
                    Var var = new((string)ID.value);
                    if (current_token.type == "Lpar") return Method_Call(var.ID);
                    else return Assign(var);
                }

            default:
                throw new Exception("Sintaxis inválida");
        }
    }
    
    public RandomPower RandomPower()
    {
        Verify("rPower");
        Verify("Lpar");
        Token field = current_token;
        Verify("place");
        AST change = Expression();
        Verify("Rpar");
        return new((string)field.value, change);
    }
    private PowSelf PowSelf()
    {
        Verify("PowSelf");
        Verify("Lpar");
        AST expr = Expression();
        Verify("Rpar");
        return new(expr);
    }

    public Paliado Paliado()
    {
        Verify("Paliado");
        Verify("Lpar");
        Verify("Rpar");
        return new();
    }
    private Destroy Destroy()
    {
        Verify("Destroy");
        Verify("Lpar");
        Verify("Rpar");
        return new();
    }

    //Statement: Summon Lpar (Deck|Hand|Graveyard) Comma ID Rpar
    //Statement: Draw Lpar integer Comma (Deck|Graveyard|Field) Rpar
    public Invoke Invoke()
    {
        Verify("Invoke");
        Verify("Lpar");
        Verify("Rpar");
        return new();
    }

    public Power Power()
    {
        Verify("Power");
        Verify("Lpar");
        AST change = Expression();
        Verify("Rpar");
        return new(change);
    }
    public Draw Draw()
    {
        Verify("Draw");
        Verify("Lpar");
        Token place = current_token;
        Verify("Place");
        Verify("Comma");
        AST amount = Expression();
        Verify("Rpar");
        return new Draw((string)place.value, amount);
    }
    public Displace Displace()
    {
        Verify("Displace");
        Verify("Lpar");
        Token from = current_token;
        Verify("Place");
        Verify("Comma");
        //cualquier manera de identificar la carta
        Integer cardID = Integer();
        Verify("Comma");
        Token to = current_token;
        Verify("Place");
        Verify("Rpar");
        Debug.Log(from.value);
        Debug.Log(to.value);
        return new Displace((string)from.value, cardID.value, (string)to.value);
    }
    public While While()
    {
        Verify("while");

        Verify("Lpar");
        AST condition = Bexpr();
        Verify("Rpar");

        return new While(condition, Block());
    }
    public If If()
    {
        Verify("If");

        Verify("Lpar");
        AST condition = Bexpr();
        Verify("Rpar");

        return new If(condition, Block());
    }
    public Variable_D Variable_D()
    {
        Token Type = current_token;
        Verify("Type");

        Variable_D D_statement = new ((string)Type.value);
        D_statement.IDs.Add(Variable());

        while (current_token.type == "Comma")
        {
            Verify("Comma");
            D_statement.IDs.Add(Variable());
        }
        return D_statement;
    }
    public Assign_statement Assign(Var var)
    {
        Verify("Equal");
        AST expr = Bexpr();
        return new Assign_statement(var, expr);
    }


    public Block Block()
    {
        //Block: Lscope statement (SEMI statement)* Rcope
        Verify("Lscope");

        List<AST> statements = new ();
        AST st = Statement();
        statements.Add(st);

        while (current_token.type != "Rscope")
        {
            Verify("Semi");
            statements.Add(Statement());
        }

        Verify("Rscope");
        return new Block(statements);
    }

    public Parameters Parameters()
    {
        //Parameters: Lpar (int | bool | sring) ID comma)* Rpar
        Verify("Lpar");
        List<(string, string)> parameters = new ();

        while (current_token.type != "Rpar")
        {
            Token type = current_token;
            Verify("Type");

            Token ID = current_token;
            Verify("ID");

            parameters.Add(((string)type.value, (string)ID.value));
            if (current_token.type == "Comma") Verify("Comma");
        }
        Verify("Rpar");
        return new Parameters(parameters);
    }

    public Method_D Method_D()
    {
        //Method Declaration: Method ID Parameters BLock 
        Verify("Method");
        Token type = current_token;
        Verify("Type");
        return new Method_D((string)type.value, Variable().ID, Parameters(), Block());
    }
}
