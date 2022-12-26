using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class Token
{
    public string type;
    public object value;

    public Token(string type, object value)
    {

        this.type = type;
        this.value = value;
    }
    public void verify(string type)
    {

        if (this.type == type) return;
        throw new("Invalid syntax");
    }
}
public class Lexer
    {
        public int pointer;
        public int Peek_pointer;
        public string text;
        public char current_char;
        // here goes the Keywords
        public static List<Token> Keywords = new()
        {
        //keywoeds del lenguaje
         new("Type", "int"),new("Type", "bool"), new("Type", "string"),
         new("Function", "Function"),new("Method", "Method"), new("if","if"),
         new("while", "while") , new("true", "true"), new("false", "false"),
         new("and", "and"), new("or", "or"), new("Type", "void"), 
         new("return", "return"),
         
         //acciones de poder
         new("RandomPower", "RandomPower"), new("TargetedPower", "TargetedPower"),
         new("SelfPower", "SelfPower"), new("PosPower","PosPower"),
         new("SelectPower", "SelectPower"), new ("OrigTargetedPower", "OrigTargetedPower"),
         new("OrigRandomPower", "OrigRandomPower"), new ("Destroy", "Destroy"),
         
         //encabezado para preguntar por una propiedad de las cartas en un lugar
         new("Get", "Get"),

         //Stats que poseen un conjunto de cartas
         new("stat", "majorP"), new ("stat", "minorP"),
         new("stat","cards"),

         //8 lugares donde pueden estar las cartas
         new ("place", "hand"), new ("place", "eHand"),
         new("place", "field"), new  ("place", "eField"),
         new ("place","graveyard"), new("place", "eGraveyard"),
         new ("place", "deck"), new ("place", "eDeck")
        };
        
        public Lexer (string text)
        {    
            this.text = text;
            pointer = 0;
            Peek_pointer = pointer;
            current_char = text[pointer];
        }

        public void Advance()
        {
            pointer++;
            if (pointer >= text.Length) {
                current_char = '#';
                return;
            }
            current_char = text[pointer];
        }

        public void Peek ()
        {
            //Mira los siguientes chars sin mover el puntero;
            Peek_pointer = pointer + 1;
            if(Peek_pointer >= text.Length-1) 
            {
                current_char = '#';
                return;
            }
            current_char = text[Peek_pointer];
        }
        public int Integer()
        {
            string result = "";

            while(Char.IsNumber(current_char))
            {   
                result += current_char;
                Advance();
            }
            return int.Parse(result);
        }
        public string Identifier()
        {
            // Identifier: pos, x, max_value
            string result = "";
            while (char.IsLetter(current_char)||char.IsDigit(current_char))
            {
                result += current_char;
                Advance();
            }
            return result;
        }
        public Token Get_next_token()
        {
            //identifies and returns next token
            while (current_char != '#')
            {
                if(current_char == ' ')
                    while (current_char == ' ') Advance();
                
                if (char.IsLetter(current_char))
                {
                    //checks if ID is a keyword
                    string symbol = Identifier();
                    
                    foreach (Token keyword in Keywords)   
                        if (symbol == (string)keyword.value) return keyword;
                    
                    return new("ID", symbol);  
                }
                if(char.IsNumber(current_char))
                {
                    return new("Number", Integer());
                }
                
                switch (current_char)
                {
                    case '>':
                        Advance();
                        if (current_char == '=')
                        {
                            Advance();
                            return new("Comparison", ">=");
                        }
                        return new("Comparison", ">");
                    case '<':
                        Advance();
                        if (current_char == '=')
                        {
                            Advance();
                            return new("Comparison", "<=");
                        }
                        return new("Comparison", "<");
                    case '!':
                        Advance();
                        if (current_char == '=')
                        {
                            Advance();
                            return new("Comparison", "!=");
                        }
                        return new("Not", "!");
                    case '+':
                        Advance();
                        return new("Plus", "+");
                    case '-':
                        Advance();
                        return new("Minus", "-");
                    case '*':
                        Advance();
                        return new("Mult", "*");
                    case '/':
                        Advance();
                        return new("Div", "/");
                    case '(':
                        Advance();
                        return new("Lpar", "(");
                    case ')':
                        Advance();
                        return new("Rpar", ")");
                    case '=':
                        Advance();
                        if (current_char == '=')
                        {
                            Advance();
                            return new("Comparison", "==");
                        }
                        return new("Equal", "=");
                    case '{':
                        Advance();
                        return new("Lscope", "{");
                    case '}':
                        Advance();
                        return new("Rscope", "}");
                    case ';':
                        Advance();
                        return new("Semi", ";");
                    case ',':
                        Advance();
                        return new("Comma", ",");
                    case '\'':
                        {
                            string value = "";
                            Advance();
                            while (current_char != '\'')
                            {

                                value += current_char;
                                Advance();
                            }
                            Advance();
                            return new("Text", value);
                        }
                    case '.':
                        Advance();
                        return new("Dot", ".");

                    default:
                        throw new("Invalid syntax");
                }
            }
            return new("EOL", "");
        }
    }
   