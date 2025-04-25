using System.Windows.Forms;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Lexer
{
    public enum TokenType
    {
        Identifier, Number, Plus, Minus, Mul, Div,
        LParen, RParen, Assign, Semicolon, EOF
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type}: {Value}";
    }

    public class Scanner
    {
        private readonly string _text;
        private int _pos;

        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        public Scanner(string text)
        {
            _text = text;
            _pos = 0;
        }

        private char Peek() => _pos + 1 < _text.Length ? _text[_pos + 1] : '\0';

        public Token GetNextToken()
        {
            while (char.IsWhiteSpace(Current)) _pos++;

            if (char.IsLetter(Current))
            {
                string id = "";
                while (char.IsLetterOrDigit(Current))
                {
                    id += Current;
                    _pos++;
                }
                return new Token(TokenType.Identifier, id);
            }

            if (char.IsDigit(Current))
            {
                string num = "";
                while (char.IsDigit(Current))
                {
                    num += Current;
                    _pos++;
                }
                return new Token(TokenType.Number, num);
            }

            switch (Current)
            {
                case '+': _pos++; return new Token(TokenType.Plus, "+");
                case '-': _pos++; return new Token(TokenType.Minus, "-");
                case '*': _pos++; return new Token(TokenType.Mul, "*");
                case '/': _pos++; return new Token(TokenType.Div, "/");
                case '(': _pos++; return new Token(TokenType.LParen, "(");
                case ')': _pos++; return new Token(TokenType.RParen, ")");
                case '=': _pos++; return new Token(TokenType.Assign, "=");
                case ';': _pos++; return new Token(TokenType.Semicolon, ";");
                case '\0': return new Token(TokenType.EOF);
                default: throw new Exception($"Неизвестный символ: '{Current}'");
            }
        }

        public Token PeekNextToken()
        {
            int savedPos = _pos;
            var token = GetNextToken();
            _pos = savedPos;
            return token;
        }
    }

}
