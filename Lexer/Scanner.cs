using System.Collections.Generic;

namespace Lexer
{
    public enum TokenType
    {
        For, To, Do, Assign, Plus, Minus, Semicolon,
        Identifier, Number, Equal, Variable,
        EndOfInput, Unknown
    }

    public class Token
    {
        public TokenType Type;
        public string Value;
        public int Pos;

        public Token(TokenType t, string v, int p)
        {
            Type = t;
            Value = v;
            Pos = p;
        }
    }

    public class Lexer
    {
        string txt;
        int i;
        int len;

        static Dictionary<string, TokenType> words = new Dictionary<string, TokenType>()
        {
            { "for", TokenType.For },
            { "to", TokenType.To },
            { "do", TokenType.Do }
        };

        public Lexer(string text)
        {
            txt = text;
            len = text.Length;
            i = 0;
        }

        char Curr
        {
            get
            {
                if (i < len) return txt[i];
                return '\0';
            }
        }

        void GoNext()
        {
            i++;
        }

        void SkipSpaces()
        {
            while (char.IsWhiteSpace(Curr)) GoNext();
        }

        public Token NextToken()
        {
            SkipSpaces();

            int start = i;

            if (Curr == '\0')
                return new Token(TokenType.EndOfInput, "", i);

            if (Curr == ':')
            {
                GoNext();
                if (Curr == '=')
                {
                    GoNext();
                    return new Token(TokenType.Assign, ":=", start);
                }
            }
            else if (Curr == '=')
            {
                GoNext();
                return new Token(TokenType.Equal, "=", start);
            }
            else if (Curr == '+')
            {
                GoNext();
                return new Token(TokenType.Plus, "+", start);
            }
            else if (Curr == '-')
            {
                GoNext();
                return new Token(TokenType.Minus, "-", start);
            }
            else if (Curr == ';')
            {
                GoNext();
                return new Token(TokenType.Semicolon, ";", start);
            }
            else if (char.IsLetter(Curr))
            {
                string id = "";
                while (char.IsLetterOrDigit(Curr))
                {
                    id += Curr;
                    GoNext();
                }

                if (words.ContainsKey(id))
                    return new Token(words[id], id, start);
                else
                    return new Token(TokenType.Identifier, id, start);
            }
            else if (char.IsDigit(Curr))
            {
                string n = "";
                while (char.IsDigit(Curr))
                {
                    n += Curr;
                    GoNext();
                }
                return new Token(TokenType.Number, n, start);
            }

            // неизвестный символ
            string unk = Curr.ToString();
            GoNext();
            return new Token(TokenType.Unknown, unk, start);
        }
    }
}
