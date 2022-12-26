//Lexer takes input and converts it into a stream of tokens
//Parser fedds off the stream of tokens provided by the lexer and makes an sinctactically correct structure
//Semer builds a Symbol table and aproves te smantic 
//Interpreter that generates results after  the parser has successfully parsed (recognized) a valid arithmetic expression


//print(candela)
//to implement:
//Bool and string assignment
//nested scopes (done)
//Actual symbol table that supports variables , and methods (done)
//grammar for methods, allow new methods (done)
//upgrade catch error system
//Upgrade Memory System (done)
//Cable predefined methods

/*
    Grammar: 
    Program: Block
    Block: Lscope statement (Semi statement)* Rscope
    Statement: Declaration statement | Assing Statement | Method call | If | While
    
    Declaration Statement: Method declaration| Variable Declaration | Function Declaration | method call
    Method declaration: Method ID Lpar Parameters Rpar Block
    Function declaration: Function Type ID Lpar parameters Rpar Block
    Variable Declaration: Type ID (Comma ID)*
    Method call: ID Lpar Parameters Rpar

    Assing Statement: ID Equal (Bexpr | Function call) | Reference Equal (Bexpr | Function call)
    Function call: ID Lpar (Expr| Bexpr| ID| Empty) Rpar
    Method call: ID Lpar (Parameters | Empty) Rpar

    If: If Lpar Bexpr Rpar block
    While: While Lpar Bexpr Rpar Block
    
    Reference: Reference.ID | ID
    Parameters: Bexpr| Expr| ID (Comma ID|Bexpr|Expr)*
    Var: ID
    ID: string
    Bexpr: Proposition (and|or Proposition)|
    Proposition: Expr comparison Expr | not Bexpr | true | false  | var
    
    
    bExpr: bterm (or bterm)*
    bterm: bfactor (and bfactor)*
    bfactor: true/false | variable | Function call | Expr (relation Expr)
    Expr: Term ((+|-)Term)* 
    Term: Factor ((*|/) Factor)
    Factor: Integer | Lpar Expr Rpar | Minus Factor | Var | Function call

    Symbol table storage:
    
    function: ID, Class
    Method: ID, Class
    Variable: ID, Class
    Type: ID, Class

    generic expression (done)
    atom: attribute
    generic assign (done)
    string type (done)
    semantic analisis: type and function*****
    semantic analisis: buildout methods 
    interpreter: get attribute and buildout methods
    interpreter: change attributes.

    finish return and var not null check
    program
    {
        Draw(player1, 3)
    }
    field[2,4].power = 3
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;




public class Program
{

    public static void Interpretar(string text, GameKey gameKey, JuegoGWENT juego)
    {
        Lexer lexer = new (text);

        Parser parser = new (lexer);
        AST tree = parser.To_parse();

        Semantic_analizer semantic = new(tree, parser, gameKey, juego);
        semantic.Check();

        Interpreter interpreter = new(tree, parser, gameKey, juego);
        interpreter.To_interpret();
    }
}
