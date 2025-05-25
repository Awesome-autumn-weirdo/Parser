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

        // Новый метод синхронизации для error recovery
        private void Synchronize(HashSet<TokenType> syncSet)
        {
            while (index < tokenList.Count && !syncSet.Contains(GetCurrentToken().Type))
            {
                GoNext();
            }
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
            return false;
        }

        private void ParseFor()
        {
            AddTrace("Вход в <For>");
            indentLevel++;

            if (!MatchOrError(TokenType.For, $"Ожидался 'for', найдено '{GetCurrentToken().Value}'"))
            {
                Synchronize(new HashSet<TokenType> { TokenType.For, TokenType.EndOfInput });
                indentLevel--;
                AddTrace("Выход из <For>");
                return;
            }

            if (!MatchOrError(TokenType.Identifier, $"Ожидался идентификатор, найдено '{GetCurrentToken().Value}'"))
            {
                Synchronize(new HashSet<TokenType> { TokenType.Assign, TokenType.To, TokenType.Do, TokenType.EndOfInput });
            }

            if (!MatchOrError(TokenType.Assign, $"Ожидалось ':=', найдено '{GetCurrentToken().Value}'"))
            {
                Synchronize(new HashSet<TokenType> { TokenType.To, TokenType.Do, TokenType.EndOfInput });
                indentLevel--;
                AddTrace("Выход из <For>");
                return;
            }

            ParseOperand();

            if (!MatchOrError(TokenType.To, $"Ожидался 'to', найдено '{GetCurrentToken().Value}'"))
            {
                Synchronize(new HashSet<TokenType> { TokenType.Do, TokenType.EndOfInput });
                indentLevel--;
                AddTrace("Выход из <For>");
                return;
            }

            ParseOperand();

            if (!MatchOrError(TokenType.Do, $"Ожидался 'do', найдено '{GetCurrentToken().Value}'"))
            {
                Synchronize(new HashSet<TokenType> { TokenType.EndOfInput });
                indentLevel--;
                AddTrace("Выход из <For>");
                return;
            }

            ParseStmt();

            indentLevel--;
            AddTrace("Выход из <For>");
        }

        private void ParseOperand()
        {
            AddTrace($"Вход в <Operand>");
            indentLevel++;

            var current = GetCurrentToken();

            if (current.Type == TokenType.Identifier)
            {
                AddTrace($"  → '{current.Value}' (var)");
                GoNext();
            }
            else if (current.Type == TokenType.Number)
            {
                AddTrace($"  → '{current.Value}' (const)");
                GoNext();
            }
            else
            {
                AddError($"Ожидался операнд (переменная или число), найдено '{current.Value}'");
                Synchronize(new HashSet<TokenType> { TokenType.Plus, TokenType.Minus, TokenType.To, TokenType.Do, TokenType.EndOfInput });
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
                AddError($"Ожидалась переменная, найдено '{GetCurrentToken().Value}'");
                Synchronize(new HashSet<TokenType> { TokenType.Equal, TokenType.EndOfInput });
            }

            if (!MatchToken(TokenType.Equal))
            {
                AddError($"Ожидался '=', найдено '{GetCurrentToken().Value}'");
                Synchronize(new HashSet<TokenType> { TokenType.Identifier, TokenType.EndOfInput });
                indentLevel--;
                AddTrace("Выход из <Stmt>");
                return;
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
                current = GetCurrentToken();
            }

            if (current.Type == TokenType.Unknown)
            {
                AddError($"Недопустимый оператор '{current.Value}' (ожидалось '+' или '-')");
                GoNext();

                // Попытка продолжить разбор арифметического выражения
                ParseArithExpr();
            }

            indentLevel--;
            AddTrace("Выход из <ArithExpr>");
        }
    }
}
