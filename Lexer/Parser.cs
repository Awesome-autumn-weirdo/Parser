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

                var validTypes = new HashSet<string> { "integer", "real", "char", "boolean", "string" };

                while (i < tokens.Count && tokens[i].Value != "end")
                {
                    var fieldNames = new List<string>();
                    bool awaitingComma = false;
                    bool colonExpected = false;

                    // Сбор имён полей
                    while (i < tokens.Count)
                    {
                        if (tokens[i].Value == ":")
                        {
                            colonExpected = true;
                            break;
                        }

                        // Если встретили тип или конец записи - выходим
                        if ((tokens[i].Type == TokenType.Keyword && validTypes.Contains(tokens[i].Value.ToLower())) ||
                            tokens[i].Value == "end" || tokens[i].Value == ";")
                        {
                            break;
                        }

                        if (tokens[i].Type == TokenType.Identifier && !Keywords.Contains(tokens[i].Value))
                        {
                            if (awaitingComma)
                            {
                                errors.Add(($"Пропущена запятая между именами полей перед '{tokens[i].Value}'",
                                            tokens[i].Position, tokens[i].Value.Length));
                            }

                            fieldNames.Add(tokens[i].Value);
                            i++;
                            awaitingComma = true;
                        }
                        else if (tokens[i].Value == ",")
                        {
                            if (!awaitingComma)
                            {
                                errors.Add(("Запятая без идентификатора перед ней", tokens[i].Position, 1));
                            }
                            i++;
                            awaitingComma = false;
                        }
                        else
                        {
                            // Неизвестный токен - предполагаем, что это начало типа
                            break;
                        }
                    }

                    // Обработка двоеточия
                    if (!colonExpected)
                    {
                        if (i < tokens.Count && tokens[i].Value != ":" && fieldNames.Count > 0)
                        {
                            errors.Add(("Ожидалось ':' после списка полей", tokens[i].Position, 1));
                            tokens.Insert(i, new Token(TokenType.Symbol, ":", tokens[i].Position));
                        }
                    }

                    if (i < tokens.Count && tokens[i].Value == ":")
                    {
                        i++; // пропустить двоеточие
                    }

                    // Проверка типа
                    if (i >= tokens.Count || tokens[i].Value == "end" || tokens[i].Value == ";")
                    {
                        int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                        errors.Add(("Ожидался тип поля", pos, 1));
                        tokens.Insert(i, new Token(TokenType.Keyword, "integer", pos));
                        i++;
                    }
                    else
                    {
                        string typeName = tokens[i].Value;
                        if (!validTypes.Contains(typeName.ToLower()))
                        {
                            errors.Add(($"Недопустимый тип поля: '{typeName}'", tokens[i].Position, typeName.Length));
                        }
                        i++;
                    }

                    // Завершение поля
                    if (i < tokens.Count && tokens[i].Value == ";")
                    {
                        i++;
                    }
                    else if (i < tokens.Count && tokens[i].Value == "end")
                    {
                        // Если встретили 'end' без точки с запятой - это нормально
                        continue;
                    }
                    else if (fieldNames.Count > 0)
                    {
                        // Если нет ни ';' ни 'end' - ожидаем 'end'
                        break;
                    }
                }

                // Проверка закрытия записи
                if (i >= tokens.Count || tokens[i].Value != "end")
                {
                    int pos = i < tokens.Count ? tokens[i].Position : input.Length;
                    errors.Add(("Ожидалось 'end' в конце объявления", pos, 3));

                    // Восстанавливаем: добавляем "end" токен
                    tokens.Insert(i, new Token(TokenType.Keyword, "end", pos));
                }
                else
                {
                    i++; // если "end" был — просто идём дальше
                }


                // Проверка на ; после end
                if (i < tokens.Count && tokens[i].Value == ";")
                {
                    i++;
                }
                else if (i < tokens.Count)
                {
                    errors.Add(("Ожидалась ';' после 'end'", tokens[i].Position, 1));
                }
                else
                {
                    // Конец ввода — но точка с запятой всё равно обязательна
                    errors.Add(("Ожидалась ';' после 'end'", input.Length, 1));
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

            var sb = new System.Text.StringBuilder();
            var cleanedInput = new System.Text.StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                if (char.IsLetterOrDigit(c) || c == '_' || c == ':' || c == ',' || c == ';' || c == '=' || char.IsWhiteSpace(c))
                {
                    // если в буфере были недопустимые символы — запишем их как ошибку
                    if (sb.Length > 0)
                    {
                        int errPos = i - sb.Length;
                        string invalidSeq = sb.ToString();
                        lexErrors.Add(($"Недопустимый символ: '{invalidSeq}'", errPos, invalidSeq.Length));
                        HighlightError(richTextBox, errPos, invalidSeq.Length);
                        sb.Clear();
                    }

                    cleanedInput.Append(c);
                }
                else
                {
                    sb.Append(c); // накапливаем недопустимый символ
                }

                i++;
            }

            // если что-то осталось в буфере после цикла
            if (sb.Length > 0)
            {
                int errPos = input.Length - sb.Length;
                string invalidSeq = sb.ToString();
                lexErrors.Add(($"Недопустимый символ: '{invalidSeq}'", errPos, invalidSeq.Length));
                HighlightError(richTextBox, errPos, invalidSeq.Length);
            }

            // Теперь токенизация
            var pattern = @"\w+|[:,;=]";
            var matches = Regex.Matches(cleanedInput.ToString(), pattern);

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
