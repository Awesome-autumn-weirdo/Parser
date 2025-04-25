using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        public override string ToString() => $"({Op}, {Arg1}, {Arg2}, {Result})";
    }

    public class Parser
    {
        private readonly Scanner _scanner;
        private Token _currentToken;
        private readonly List<Quad> _quads = new List<Quad>();
        private int _tempCounter = 1;

        public Parser(Scanner scanner)
        {
            _scanner = scanner;
            _currentToken = _scanner.GetNextToken();
        }

        private void Eat(TokenType type)
        {
            if (_currentToken.Type == type)
                _currentToken = _scanner.GetNextToken();
            else
                throw new Exception($"Ожидался {type}, получено {_currentToken.Type}");
        }

        private string NewTemp() => $"t{_tempCounter++}";

        public List<Quad> Parse()
        {
            while (_currentToken.Type != TokenType.EOF)
            {
                if (_currentToken.Type == TokenType.Identifier &&
                    _scanner.PeekNextToken().Type == TokenType.Assign)
                {
                    ParseAssignment();
                }
                else
                {
                    ParseExpression();
                }
            }
            return _quads;
        }

        private void ParseAssignment()
        {
            string left = _currentToken.Value;
            Eat(TokenType.Identifier);
            Eat(TokenType.Assign);
            string right = ParseExpression();
            _quads.Add(new Quad("=", right, null, left));
            Eat(TokenType.Semicolon);
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
                string right = ParseTerm();
                string result = NewTemp();
                _quads.Add(new Quad(op, left, right, result));
                left = result;
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
                string right = ParseFactor();
                string result = NewTemp();
                _quads.Add(new Quad(op, left, right, result));
                left = result;
            }
            return left;
        }

        private string ParseFactor()
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                string val = _currentToken.Value;
                Eat(TokenType.Identifier);
                return val;
            }
            else if (_currentToken.Type == TokenType.Number)
            {
                string val = _currentToken.Value;
                Eat(TokenType.Number);
                return val;
            }
            else if (_currentToken.Type == TokenType.LParen)
            {
                Eat(TokenType.LParen);
                string val = ParseExpression();
                Eat(TokenType.RParen);
                return val;
            }
            else if (_currentToken.Type == TokenType.Minus) // Унарный минус
            {
                Eat(TokenType.Minus);
                string val = ParseFactor();
                string result = NewTemp();
                _quads.Add(new Quad("minus", val, null, result));
                return result;
            }

            throw new Exception($"Ожидался идентификатор, число или выражение в скобках, получено {_currentToken.Type}");
        }
    }

}
