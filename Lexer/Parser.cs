using System;
using System.Collections.Generic;

namespace Lexer
{
    public class Quad
    {
        public string Op { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Result { get; set; }

        public Quad(string op, string arg1, string arg2, string result)
        {
            Op = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }

        public override string ToString()
        {
            return $"({Op}, {Arg1}, {Arg2}, {Result})";
        }
    }

    public class Parser
    {
        private readonly Scanner _scanner;
        private Token _currentToken;
        private readonly List<Quad> _quads = new List<Quad>();
        private int _tempCounter = 1;
        private readonly List<ParserError> _errors = new List<ParserError>();

        public Parser(Scanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _currentToken = _scanner.GetNextToken();
        }

        public IReadOnlyList<ParserError> Errors => _errors;

        private void Eat(TokenType type)
        {
            try
            {
                if (_currentToken.Type == type)
                {
                    _currentToken = _scanner.GetNextToken();
                }
                else
                {
                    var pos = _scanner.GetCurrentPosition();
                    _errors.Add(new ParserError(
                        $"Ожидался {type}, получено {_currentToken.Type}",
                        pos.line,
                        pos.column));
                    _currentToken = _scanner.GetNextToken();
                }
            }
            catch (LexerException ex)
            {
                _errors.Add(new ParserError(ex.Message, ex.Line, ex.Column));
                throw;
            }
        }

        private string NewTemp()
        {
            return $"t{_tempCounter++}";
        }

        public List<Quad> Parse()
        {
            try
            {
                while (_currentToken.Type != TokenType.EOF)
                {
                    if (_currentToken.Type == TokenType.Identifier ||
                        _currentToken.Type == TokenType.LParen ||
                        _currentToken.Type == TokenType.Minus)
                    {
                        ParseExpression();
                    }
                    else
                    {
                        var pos = _scanner.GetCurrentPosition();
                        _errors.Add(new ParserError(
                            $"Неверный токен {_currentToken.Type} в начале выражения",
                            pos.line,
                            pos.column));
                        Eat(_currentToken.Type);
                    }
                }

                if (_errors.Count > 0)
                {
                    throw new ParserException("Обнаружены ошибки при разборе", _errors);
                }

                return _quads;
            }
            catch (Exception ex)
            {
                if (!(ex is ParserException))
                {
                    throw new ParserException("Ошибка при разборе", _errors, ex);
                }
                throw;
            }
        }

        private string ParseExpression()
        {
            string left = ParseTerm();
            return ParseAdditiveExpr(left);
        }

        private string ParseAdditiveExpr(string left)
        {
            while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
            {
                string op = _currentToken.Value;
                Eat(_currentToken.Type);

                try
                {
                    string right = ParseTerm();
                    string result = NewTemp();
                    _quads.Add(new Quad(op, left, right, result));
                    left = result;
                }
                catch (ParserException)
                {
                    if (_currentToken.Type != TokenType.Plus &&
                        _currentToken.Type != TokenType.Minus)
                    {
                        break;
                    }
                }
            }
            return left;
        }

        private string ParseTerm()
        {
            string left = ParseFactor();
            return ParseMultiplicativeExpr(left);
        }

        private string ParseMultiplicativeExpr(string left)
        {
            while (_currentToken.Type == TokenType.Mul || _currentToken.Type == TokenType.Div)
            {
                string op = _currentToken.Value;
                Eat(_currentToken.Type);

                try
                {
                    string right = ParseFactor();
                    string result = NewTemp();
                    _quads.Add(new Quad(op, left, right, result));
                    left = result;
                }
                catch (ParserException)
                {
                    if (_currentToken.Type != TokenType.Mul &&
                        _currentToken.Type != TokenType.Div)
                    {
                        break;
                    }
                }
            }
            return left;
        }

        private string ParseFactor()
        {
            try
            {
                if (_currentToken.Type == TokenType.Identifier)
                {
                    string val = _currentToken.Value;
                    Eat(TokenType.Identifier);
                    return val;
                }
                else if (_currentToken.Type == TokenType.LParen)
                {
                    Eat(TokenType.LParen);
                    string val = ParseExpression();
                    Eat(TokenType.RParen);
                    return val;
                }
                else if (_currentToken.Type == TokenType.Minus)
                {
                    Eat(TokenType.Minus);
                    string val = ParseFactor();
                    string result = NewTemp();
                    _quads.Add(new Quad("minus", val, null, result));
                    return result;
                }

                var pos = _scanner.GetCurrentPosition();
                throw new ParserException(
                    $"Ожидался идентификатор или выражение в скобках, получено {_currentToken.Type}",
                    pos.line,
                    pos.column);
            }
            catch (ParserException ex)
            {
                _errors.Add(new ParserError(ex.Message, ex.Line, ex.Column));
                throw;
            }
        }
    }

    public class ParserError
    {
        public string Message { get; }
        public int Line { get; }
        public int Column { get; }

        public ParserError(string message, int line, int column)
        {
            Message = message;
            Line = line;
            Column = column;
        }
    }

    public class ParserException : Exception
    {
        public int Line { get; }
        public int Column { get; }
        public IReadOnlyList<ParserError> Errors { get; }

        public ParserException(string message, int line, int column)
            : base(message)
        {
            Line = line;
            Column = column;
            Errors = new List<ParserError> { new ParserError(message, line, column) };
        }

        public ParserException(string message, IEnumerable<ParserError> errors, Exception innerException = null)
            : base(message, innerException)
        {
            Errors = new List<ParserError>(errors);
        }
    }
}