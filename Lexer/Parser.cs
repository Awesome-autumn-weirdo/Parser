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
                return tokenList[tokenList.Count - 1];
            }
        }

        private void GoNext()
        {
            index++;
        }

        // Модифицированный MatchOrError с выводом трассировки токена
        private bool MatchOrError(TokenType type, string errorMessage, string traceName = null)
        {
            var current = GetCurrentToken();
            if (current.Type == type)
            {
                if (traceName != null)
                {
                    if (current.Value != null)
                        AddTrace($"Токен: {traceName} = {current.Value}");
                    else
                        AddTrace($"Токен: {traceName}");
                }
                GoNext();
                return true;
            }
            AddError(errorMessage);
            GoNext();
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

            if (errorList.Count == 0)
                AddTrace("Анализ завершен успешно. Ошибок не найдено.");
            else
                AddTrace($"Анализ завершен с {errorList.Count} ошибками.");

            return (errorList, traceLog);
        }

        private void ParseFor()
        {
            AddTrace("Вход в <For>");
            indentLevel++;

            MatchOrError(TokenType.For, "Ожидался 'for'", "for");
            MatchOrError(TokenType.Identifier, "Ожидался идентификатор", "var");
            MatchOrError(TokenType.Assign, "Ожидалось ':='", ":=");
            ParseOperand();
            MatchOrError(TokenType.To, "Ожидался 'to'", "to");
            ParseOperand();
            MatchOrError(TokenType.Do, "Ожидался 'do'", "do");
            ParseStmt();

            if (tokenList.Count >= 2)
            {
                var last = tokenList[tokenList.Count - 2];
                if (last.Type != TokenType.Semicolon)
                {
                    AddError("Ожидалась точка с запятой в конце");
                }
                else AddTrace("Токен: ;");
            }

            indentLevel--;
            AddTrace("Выход из <For>");
        }

        private void ParseOperand()
        {
            AddTrace("Вход в <Operand>");
            indentLevel++;

            var current = GetCurrentToken();
            if (current.Type == TokenType.Identifier)
            {
                AddTrace($"Токен: var = {current.Value}");
                GoNext();
            }
            else if (current.Type == TokenType.Number)
            {
                AddTrace($"Токен: const = {current.Value}");
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

            var current = GetCurrentToken();
            if (current.Type == TokenType.Identifier)
            {
                AddTrace($"Токен: var = {current.Value}");
                GoNext();
            }
            else
            {
                AddError("Ожидалась переменная");
                GoNext();
            }

            AddTrace("Вход в <Assign>");
            if (MatchToken(TokenType.Equal))
            {
                AddTrace("Токен: =");
            }
            else
            {
                AddError("Ожидался '='");
            }
            AddTrace("Выход из <Assign>");

            ParseArithExpr();

            indentLevel--;
            AddTrace("Выход из <Stmt>");
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

        private void ParseArithExpr()
        {
            AddTrace("Вход в <ArithExpr>");
            indentLevel++;

            ParseOperand();

            var current = GetCurrentToken();
            while (current.Type == TokenType.Plus || current.Type == TokenType.Minus)
            {
                if (current.Type == TokenType.Plus)
                {
                    AddTrace("Вход в <Plus>");
                    AddTrace("Токен: +");
                    GoNext();
                    ParseOperand();
                    AddTrace("Выход из <Plus>");
                }
                else if (current.Type == TokenType.Minus)
                {
                    AddTrace("Вход в <Minus>");
                    AddTrace("Токен: -");
                    GoNext();
                    ParseOperand();
                    AddTrace("Выход из <Minus>");
                }
                current = GetCurrentToken();
            }

            indentLevel--;
            AddTrace("Выход из <ArithExpr>");
        }
    }
}
