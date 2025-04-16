using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Lexer
{
    public class RecordParser
    {
        private static readonly HashSet<string> Keywords = new HashSet<string> { "type", "record", "end", "real", "integer", "string", "boolean", "char" };

        private enum TokenType { Keyword, Identifier, Symbol }

        private class Token
        {
            public TokenType Type;
            public string Value;
            public int Position;

            public Token(TokenType type, string value, int position)
            {
                Type = type;
                Value = value;
                Position = position;
            }

            public override string ToString() => $"{Type}: {Value} (at {Position})";
        }

        public List<(string message, int position, int length)> ParseRecord(string input, RichTextBox richTextBox)
        {
            var (tokens, lexErrors) = Tokenize(input, richTextBox);
            var errors = new List<(string message, int position, int length)>();
            errors.AddRange(lexErrors);

            int i = 0;

            void Expect(string expected, string context)
            {
                if (i >= tokens.Count)
                {
                    errors.Add(($"Ожидалось '{expected}' {context}, найдено 'EOF'", input.Length, 1));
                    return;
                }

                if (tokens[i].Value != expected)
                {
                    errors.Add(($"Ожидалось '{expected}' {context}, найдено '{tokens[i].Value}'", tokens[i].Position, tokens[i].Value.Length));
                }

                i++;
            }

            bool CheckIdentifier(string context)
            {
                if (i >= tokens.Count)
                {
                    errors.Add(($"Ожидался идентификатор {context}, найдено 'EOF'", input.Length, 1));
                    return false;
                }

                if (tokens[i].Type != TokenType.Identifier)
                {
                    errors.Add(($"Ожидался идентификатор {context}, найдено '{tokens[i].Value}'", tokens[i].Position, tokens[i].Value.Length));
                    i++;
                    return false;
                }

                i++;
                return true;
            }

            try
            {
                Expect("type", "в начале объявления");
                CheckIdentifier("после 'type'");

                if (i >= tokens.Count || tokens[i].Value != "=")
                {
                    int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                    errors.Add(($"Ожидалось '=' после имени типа, найдено '{(i < tokens.Count ? tokens[i].Value : "EOF")}'", pos, 1));
                    tokens.Insert(i, new Token(TokenType.Symbol, "=", pos));
                }
                i++;

                if (i >= tokens.Count || tokens[i].Value != "record")
                {
                    int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                    errors.Add(($"Ожидалось 'record' после '=', найдено '{(i < tokens.Count ? tokens[i].Value : "EOF")}'", pos, 1));
                    tokens.Insert(i, new Token(TokenType.Keyword, "record", pos));
                    i++;
                }
                i++;

                while (i < tokens.Count && tokens[i].Value != "end")
                {
                    var fieldNames = new List<string>();

                    // Первый идентификатор (или виртуальный)
                    if (tokens[i].Type == TokenType.Identifier)
                    {
                        fieldNames.Add(tokens[i].Value);
                        i++;
                    }
                    else
                    {
                        errors.Add(("Ожидался идентификатор в списке полей, найдено '" + tokens[i].Value + "'", tokens[i].Position, tokens[i].Value.Length));
                        // Вставляем виртуальный идентификатор
                        string virtualId = $"_virt_{i}";
                        tokens.Insert(i, new Token(TokenType.Identifier, virtualId, tokens[i].Position));
                        fieldNames.Add(virtualId);
                        i++;
                    }

                    // Дополнительные идентификаторы через запятую
                    while (i < tokens.Count && tokens[i].Value == ",")
                    {
                        i++; // пропускаем запятую

                        if (i < tokens.Count && tokens[i].Type == TokenType.Identifier)
                        {
                            fieldNames.Add(tokens[i].Value);
                            i++;
                        }
                        else
                        {
                            errors.Add(("Ожидался идентификатор после ',', найдено '" + (i < tokens.Count ? tokens[i].Value : "EOF") + "'", tokens[i].Position, 1));
                            string virtualId = $"_virt_{i}";
                            tokens.Insert(i, new Token(TokenType.Identifier, virtualId, tokens[i - 1].Position + 1));
                            fieldNames.Add(virtualId);
                            i++;
                        }
                    }

                    // Проверка на ':'
                    if (i >= tokens.Count || tokens[i].Value != ":")
                    {
                        int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                        errors.Add(("Ожидалось ':' после списка полей", pos, 1));
                        tokens.Insert(i, new Token(TokenType.Symbol, ":", pos));
                    }
                    i++;

                    // Тип поля
                    if (i >= tokens.Count || !(tokens[i].Type == TokenType.Keyword && Keywords.Contains(tokens[i].Value)))
                    {
                        int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                        errors.Add(("Ожидался тип поля, найдено '" + (i < tokens.Count ? tokens[i].Value : "EOF") + "'", pos, 1));
                        tokens.Insert(i, new Token(TokenType.Keyword, "real", pos));
                    }
                    i++;

                    // Завершение поля
                    if (i < tokens.Count && tokens[i].Value == ";")
                    {
                        i++;
                    }
                    else if (i < tokens.Count && tokens[i].Value != "end")
                    {
                        errors.Add(("Ожидалась ';' после поля", tokens[i].Position, 1));
                        tokens.Insert(i, new Token(TokenType.Symbol, ";", tokens[i].Position));
                        i++;
                    }
                }

                // Закрытие записи
                if (i >= tokens.Count || tokens[i].Value != "end")
                {
                    int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                    errors.Add(("Ожидалось 'end' в конце объявления", pos, 1));
                    tokens.Insert(i, new Token(TokenType.Keyword, "end", pos));
                }
                i++;

                if (i < tokens.Count && tokens[i].Value == ";")
                {
                    i++;
                }
                else
                {
                    int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                    errors.Add(("Ожидалась ';' после 'end'", pos, 1));
                    tokens.Insert(i, new Token(TokenType.Symbol, ";", pos));
                    i++;
                }


            }
            catch (Exception ex)
            {
                errors.Add(($"Ошибка при разборе: {ex.Message}", input.Length, 1));
            }

            return errors;
        }

        private (List<Token> tokens, List<(string message, int position, int length)> errors) Tokenize(string input, RichTextBox richTextBox)
        {
            var tokens = new List<Token>();
            var lexErrors = new List<(string message, int position, int length)>();

            var cleanedInput = Regex.Replace(input, @"[^a-zA-Z0-9_:\s,;=\n]", m =>
            {
                lexErrors.Add(($"Недопустимый символ '{m.Value}'", m.Index, 1));
                HighlightError(richTextBox, m.Index, 1);
                return "";
            });

            var pattern = @"\w+|[:,;=]";
            var matches = Regex.Matches(cleanedInput, pattern);

            foreach (Match match in matches)
            {
                string val = match.Value;
                int pos = match.Index;

                if (Regex.IsMatch(val, @"^[a-zA-Z_]\w*$"))
                {
                    if (Keywords.Contains(val))
                        tokens.Add(new Token(TokenType.Keyword, val, pos));
                    else
                        tokens.Add(new Token(TokenType.Identifier, val, pos));
                }
                else if (Regex.IsMatch(val, @"^[:,;=]$"))
                {
                    tokens.Add(new Token(TokenType.Symbol, val, pos));
                }
            }

            return (tokens, lexErrors);
        }

        private void HighlightError(RichTextBox richTextBox, int start, int length)
        {
            int originalSelectionStart = richTextBox.SelectionStart;
            int originalSelectionLength = richTextBox.SelectionLength;

            richTextBox.Select(start, length);
            richTextBox.SelectionBackColor = Color.Plum;

            richTextBox.Select(originalSelectionStart, originalSelectionLength);
        }
    }
}
