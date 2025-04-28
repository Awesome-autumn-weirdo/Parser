using System;

namespace Lexer
{
    public enum TokenType
    {
        Identifier, Plus, Minus, Mul, Div,
        LParen, RParen, EOF
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
        private int _line = 1;
        private int _column = 1;

        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        public Scanner(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _pos = 0;
        }

        public (int line, int column) GetCurrentPosition() => (_line, _column);

        private void AdvancePosition()
        {
            if (Current == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _pos++;
        }

        public Token GetNextToken()
        {
            while (char.IsWhiteSpace(Current))
            {
                AdvancePosition();
            }

            if (char.IsLetter(Current))
            {
                string id = "";
                while (char.IsLetter(Current))
                {
                    id += Current;
                    AdvancePosition();
                }
                return new Token(TokenType.Identifier, id);
            }

            var currentChar = Current;
            TokenType tokenType;

            switch (currentChar)
            {
                case '+':
                    tokenType = TokenType.Plus;
                    break;
                case '-':
                    tokenType = TokenType.Minus;
                    break;
                case '*':
                    tokenType = TokenType.Mul;
                    break;
                case '/':
                    tokenType = TokenType.Div;
                    break;
                case '(':
                    tokenType = TokenType.LParen;
                    break;
                case ')':
                    tokenType = TokenType.RParen;
                    break;
                case '\0':
                    tokenType = TokenType.EOF;
                    break;
                default:
                    throw new LexerException($"Неизвестный символ: '{currentChar}'", _line, _column);
            }

            AdvancePosition();
            return new Token(tokenType, currentChar.ToString());
        }

        public Token PeekNextToken()
        {
            int savedPos = _pos;
            int savedLine = _line;
            int savedColumn = _column;

            try
            {
                return GetNextToken();
            }
            finally
            {
                _pos = savedPos;
                _line = savedLine;
                _column = savedColumn;
            }
        }
    }

    public class LexerException : Exception
    {
        public int Line { get; }
        public int Column { get; }

        public LexerException(string message, int line, int column)
            : base($"{message} (строка {line}, позиция {column})")
        {
            Line = line;
            Column = column;
        }
    }
}