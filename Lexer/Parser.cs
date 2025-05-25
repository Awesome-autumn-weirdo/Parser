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

        // Пропускает один Unknown токен и сразу фиксирует ошибку
        private bool SkipOneUnknownToken()
        {
            if (GetCurrentToken().Type == TokenType.Unknown)
            {
                var current = GetCurrentToken();
                errorList.Add(($"Недопустимый символ: '{current.Value}'", current.Pos, current.Value.Length));
                GoNext();
                return true;
            }
            return false;
        }

        // Пропускает Unknown токены по одному, фиксируя ошибку для каждого
        private void SkipUnknownTokens()
        {
            while (SkipOneUnknownToken())
            {
                // Цикл пропускает по одному Unknown токену
            }
        }

        // Модифицированный MatchOrError с выводом трассировки токена
        private bool MatchOrError(TokenType type, string errorMessage, string traceName = null)
        {
            SkipUnknownTokens();

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
            var current = GetCurrentToken();

            // Не добавляем ошибку для Unknown, т.к. она уже добавлена в SkipOneUnknownToken
            if (current.Type == TokenType.Unknown)
                return;

            int length = 1;
            if (current.Value != null)
            {
                length = current.Value.Length;
            }
            errorList.Add((message, current.Pos, length));
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
            SkipUnknownTokens();
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

            // Проверка на точку с запятой перед EndOfInput
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
            SkipUnknownTokens();
            AddTrace("Вход в <Operand>");
            indentLevel++;

            bool operandFound = false;

            while (true)
            {
                var current = GetCurrentToken();

                if (current.Type == TokenType.Identifier || current.Type == TokenType.Number)
                {
                    AddTrace($"Токен: {(current.Type == TokenType.Identifier ? "var" : "const")} = {current.Value}");
                    GoNext();
                    operandFound = true;
                }
                else if (current.Type == TokenType.Unknown)
                {
                    // Собираем подряд идущие Unknown токены
                    int unknownStartPos = current.Pos;
                    int unknownLength = 0;
                    while (GetCurrentToken().Type == TokenType.Unknown)
                    {
                        unknownLength += GetCurrentToken().Value.Length;
                        GoNext();
                    }
                    errorList.Add(($"Недопустимые символы внутри операнда", unknownStartPos, unknownLength));
                }
                else
                {
                    // Токен не входит в операнд — выходим
                    break;
                }
            }

            if (!operandFound)
            {
                AddError("Ожидался операнд (переменная или число)");
                GoNext();
            }

            indentLevel--;
            AddTrace("Выход из <Operand>");
        }

        private void ParseStmt()
        {
            SkipUnknownTokens();
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
            SkipUnknownTokens();

            if (GetCurrentToken().Type == type)
            {
                GoNext();
                return true;
            }
            return false;
        }

        private void ParseArithExpr()
        {
            SkipUnknownTokens();
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
