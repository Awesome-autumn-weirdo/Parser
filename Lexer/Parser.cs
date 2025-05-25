using System.Collections.Generic;
using System.Windows.Forms;

namespace Lexer
{
    public class Parser
    {
        private List<Token> tokenList;
        private int index;
        private List<(string, int, int)> errorList;
        private List<string> traceLog;
        private int indentLevel;

        private Token GetCurrentToken()
        {
            if (index < tokenList.Count)
            {
                return tokenList[index];
            }
            else
            {
                return tokenList[tokenList.Count - 1]; // возвращаем последний если что
            }
        }

        private void GoNext()
        {
            index++;
        }

        private bool MatchToken(TokenType type)
        {
            if (GetCurrentToken().Type == type)
            {
                GoNext();
                return true;
            }
            return false;
        }

        private void AddError(string message)
        {
            int length = 1;
            if (GetCurrentToken().Value != null)
            {
                length = GetCurrentToken().Value.Length;
            }
            errorList.Add((message, GetCurrentToken().Pos, length));
        }

        private void AddTrace(string message)
        {
            string spaces = new string(' ', indentLevel * 2);
            traceLog.Add(spaces + message);
        }

        public (List<(string, int, int)>, List<string>) Parse(string input, RichTextBox editor)
        {
            tokenList = new List<Token>();
            index = 0;
            errorList = new List<(string, int, int)>();
            traceLog = new List<string>();

            var lexer = new Lexer(input);
            Token token;
            do
            {
                token = lexer.NextToken();
                tokenList.Add(token);
            } while (token.Type != TokenType.EndOfInput);

            ParseFor();

            return (errorList, traceLog);
        }

        private bool MatchOrError(TokenType type, string errorMessage)
        {
            if (MatchToken(type))
                return true;
            AddError(errorMessage);
            GoNext();
            return false;
        }

        private void ParseFor()
        {
            AddTrace("Вход в <For>");
            indentLevel++;

            MatchOrError(TokenType.For, "Ожидался 'for'");
            MatchOrError(TokenType.Identifier, "Ожидался идентификатор");
            MatchOrError(TokenType.Assign, "Ожидалось ':='");

            ParseOperand();

            MatchOrError(TokenType.To, "Ожидался 'to'");
            ParseOperand();

            MatchOrError(TokenType.Do, "Ожидался 'do'");

            ParseStmt();

            if (tokenList.Count >= 2)
            {
                var last = tokenList[tokenList.Count - 2];
                if (last.Type != TokenType.Semicolon)
                {
                    AddError("Ожидалась точка с запятой в конце");
                }
            }

            indentLevel--;
            AddTrace("Выход из <For>");
        }

        private void ParseOperand()
        {
            AddTrace("Вход в <Operand>");
            indentLevel++;

            var current = GetCurrentToken();
            if (current.Type == TokenType.Identifier || current.Type == TokenType.Number)
            {
                GoNext();
            }
            else
            {
                AddError("Ожидался операнд (переменная или число)");
                GoNext();
            }

            indentLevel--;
            AddTrace("Выход из <Operand>");
        }

        private void ParseStmt()
        {
            AddTrace("Вход в <Stmt>");
            indentLevel++;

            if (!MatchToken(TokenType.Identifier))
            {
                AddError("Ожидалась переменная");
            }

            if (!MatchToken(TokenType.Equal))
            {
                AddError("Ожидался '='");
            }

            ParseArithExpr();

            indentLevel--;
            AddTrace("Выход из <Stmt>");
        }

        private void ParseArithExpr()
        {
            AddTrace("Вход в <ArithExpr>");
            indentLevel++;

            ParseOperand();

            var current = GetCurrentToken();
            while (current.Type == TokenType.Plus || current.Type == TokenType.Minus)
            {
                GoNext();
                ParseOperand();
                current = GetCurrentToken(); // обновляем после перехода
            }

            indentLevel--;
            AddTrace("Выход из <ArithExpr>");
        }
    }
}
