using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lexer
{
    public partial class Form1 : Form
    {
        private float currentEditorFontSize
        {
            get => Properties.Settings.Default.EditorFontSize;
            set => Properties.Settings.Default.EditorFontSize = value;
        }

        private float currentOutputFontSize
        {
            get => Properties.Settings.Default.OutputFontSize;
            set => Properties.Settings.Default.OutputFontSize = value;
        }

        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Timer statusTimer;

        public Form1()
        {
            InitializeComponent();

            // Создаем строку состояния
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();

            // Настройка внешнего вида
            statusStrip.BackColor = Color.PaleVioletRed;
            statusLabel.Spring = true; // Растягиваем на всю ширину
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Добавляем элементы
            statusStrip.Items.Add(statusLabel);

            // Добавляем строку состояния на форму
            this.Controls.Add(statusStrip);
            statusStrip.Dock = DockStyle.Bottom;
            

            // Таймер для обновления времени
            statusTimer = new Timer();
            statusTimer.Interval = 1000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            // Добавляем элемент для времени
            var timeLabel = new ToolStripStatusLabel();
            timeLabel.TextAlign = ContentAlignment.MiddleRight;
            statusStrip.Items.Add(timeLabel);

            Properties.Settings.Default.Reload();
            InitializeDataGridViewColumns(dataGridView1);
            dataGridView1.Font = new Font("Bookman Old Style", Properties.Settings.Default.OutputFontSize);
            InitializeErrorGrid(dataGridViewErrors);
            dataGridViewErrors.Font = new Font("Bookman Old Style", Properties.Settings.Default.OutputFontSize);

            this.FormClosing += Form1_FormClosing;

            CreateNewTab(null, "Новый документ", "a+b*(c-d)\r\n");
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (statusStrip.Items.Count > 1)
            {
                statusStrip.Items[1].Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        public void SetStatus(string message)
        {
            statusLabel.Text = message;
            statusStrip.Refresh();
        }

        public void ClearStatus()
        {
            statusLabel.Text = string.Empty;
            statusStrip.Refresh();
        }

        private void размерШрифтаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем форму для настройки размеров шрифта
            Form fontSizeForm = new Form()
            {
                Text = "Размер шрифта",
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.LavenderBlush // Задаем цвет фона формы
            };

            // Создаем элементы управления
            Label editorLabel = new Label()
            {
                Text = "Редактор:",
                Left = 20,
                Top = 20,
                Width = 100,
                Font = new Font("Bookman Old Style", 10),
                BackColor = Color.LavenderBlush // Устанавливаем цвет фона
            };

            NumericUpDown editorSize = new NumericUpDown()
            {
                Left = 170,
                Top = 20,
                Width = 60,
                Minimum = 6,
                Maximum = 36,
                Value = (decimal)currentEditorFontSize,
                Font = new Font("Bookman Old Style", 10),
                BackColor = Color.LavenderBlush
            };

            Label outputLabel = new Label()
            {
                Text = "Результаты:",
                Left = 20,
                Top = 60,
                Width = 150,
                Font = new Font("Bookman Old Style", 10),
                BackColor = Color.LavenderBlush
            };

            NumericUpDown outputSize = new NumericUpDown()
            {
                Left = 170,
                Top = 60,
                Width = 60,
                Minimum = 6,
                Maximum = 36,
                Value = (decimal)currentOutputFontSize,
                Font = new Font("Bookman Old Style", 10),
                BackColor = Color.LavenderBlush
            };

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button()
            {
                Text = "Применить",
                Left = 100,
                Top = 100,
                Width = 125,
                Height = 35,
                DialogResult = DialogResult.OK,
                Font = new Font("Bookman Old Style", 10),
                BackColor = Color.PaleVioletRed
            };

            // Добавляем элементы на форму
            fontSizeForm.Controls.Add(editorLabel);
            fontSizeForm.Controls.Add(editorSize);
            fontSizeForm.Controls.Add(outputLabel);
            fontSizeForm.Controls.Add(outputSize);
            fontSizeForm.Controls.Add(okButton);
            fontSizeForm.AcceptButton = okButton;

            // Показываем форму как диалоговое окно
            if (fontSizeForm.ShowDialog() == DialogResult.OK)
            {
                // Применяем новые размеры шрифтов
                currentEditorFontSize = (float)editorSize.Value;
                currentOutputFontSize = (float)outputSize.Value;

                // Сохраняем настройки сразу после изменения
                Properties.Settings.Default.Save();

                UpdateEditorFontSize();
                UpdateOutputFontSize();
            }
        }


        // Обновляем шрифт во всех редакторах
        private void UpdateEditorFontSize()
        {
            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                var splitContainer = tabPage.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer == null) continue;

                var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                if (richTextBox != null)
                {
                    richTextBox.Font = new Font(richTextBox.Font.FontFamily, currentEditorFontSize);

                    // Обновляем панель нумерации строк
                    var lineNumberPanel = splitContainer.Panel1.Controls.OfType<Panel>().FirstOrDefault();
                    if (lineNumberPanel != null)
                    {
                        lineNumberPanel.Invalidate();
                    }
                }
            }
        }


        private void UpdateOutputFontSize()
        {
            if (dataGridView1 != null)
            {
                // Создаем новый шрифт с текущими настройками
                Font newFont = new Font("Bookman Old Style", currentOutputFontSize);

                // Обновляем основной шрифт
                dataGridView1.Font = newFont;

                // Обновляем шрифт в ячейках
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.DefaultCellStyle.Font = newFont;
                }

                // Обновляем шрифт заголовков
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = newFont;

                // Обновляем высоту строк
                dataGridView1.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders);
            }

            // Обновляем таблицу ошибок
            if (dataGridViewErrors != null)
            {
                // Создаем новый шрифт с текущими настройками
                Font newFont = new Font("Bookman Old Style", currentOutputFontSize);

                dataGridViewErrors.Font = newFont;
                foreach (DataGridViewColumn column in dataGridViewErrors.Columns)
                {
                    column.DefaultCellStyle.Font = newFont;
                }
                dataGridViewErrors.ColumnHeadersDefaultCellStyle.Font = newFont;
                dataGridViewErrors.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders);
            }
        }

        private void пускToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStatus("Анализ...");

            if (tabControl1.SelectedTab == null)
            {
                SetStatus("Ошибка: не выбрана вкладка");
                return;
            }

            var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
            if (splitContainer == null)
            {
                SetStatus("Ошибка: не найден SplitContainer");
                return;
            }

            var editorRichTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
            if (editorRichTextBox == null)
            {
                SetStatus("Ошибка: не найден редактор кода");
                return;
            }

            try
            {
                // Очистка предыдущих результатов
                dataGridViewErrors.Rows.Clear();
                dataGridView1.Rows.Clear();
                InitializeErrorGrid(dataGridViewErrors);
                InitializeDataGridViewColumns(dataGridView1);

                // Синтаксический и лексический анализ
                string code = editorRichTextBox.Text;
                Scanner scanner = new Scanner(code);
                Parser parser = new Parser(scanner);

                // Построение тетрад
                List<Quad> quads;
                try
                {
                    quads = parser.Parse();
                }
                catch (ParserException ex)
                {
                    // Добавляем все ошибки в таблицу
                    foreach (var error in ex.Errors)
                    {
                        dataGridViewErrors.Rows.Add("Синтаксис", error.Message, error.Line, error.Column);
                    }

                    // Если есть какие-то тетрады, показываем их
                    if (parser.Errors.Count > 0 && ex.Errors.Count != parser.Errors.Count)
                    {
                        foreach (var error in parser.Errors)
                        {
                            dataGridViewErrors.Rows.Add("Синтаксис", error.Message, error.Line, error.Column);
                        }
                    }

                    SetStatus($"Анализ завершён с ошибками ({parser.Errors.Count})");
                    return;
                }

                // Отображение тетрад
                int lineNumber = 0;
                foreach (var quad in quads)
                {
                    dataGridView1.Rows.Add(lineNumber++, quad.Op, quad.Arg1, quad.Arg2, quad.Result);
                }


                SetStatus("Синтаксический анализ завершён успешно");
            }
            catch (Exception ex)
            {
                int line = 1;
                int pos = 1;

                try
                {
                    line = editorRichTextBox.GetLineFromCharIndex(editorRichTextBox.SelectionStart) + 1;
                    pos = editorRichTextBox.SelectionStart - editorRichTextBox.GetFirstCharIndexOfCurrentLine() + 1;
                }
                catch { /* игнорируем ошибки позиционирования */ }

                dataGridViewErrors.Rows.Add("Ошибка", ex.Message, line, pos);
                SetStatus("Ошибка анализа");
            }
        }



        private void InitializeErrorGrid(DataGridView dataGridView)
        {
            if (dataGridView != null)
            {
                Font currentFont = new Font("Bookman Old Style", currentOutputFontSize);
                DataGridViewCellStyle cellStyle = new DataGridViewCellStyle { Font = currentFont };

                dataGridView.Columns.Clear();

                dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ErrorType",
                    HeaderText = "Тип ошибки",
                    Width = 150,
                    DefaultCellStyle = cellStyle
                });

                dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Message",
                    HeaderText = "Сообщение",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    DefaultCellStyle = cellStyle
                });

                dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Line",
                    HeaderText = "Строка",
                    Width = 100,
                    DefaultCellStyle = cellStyle
                });

                dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Position",
                    HeaderText = "Позиция",
                    Width = 100,
                    DefaultCellStyle = cellStyle
                });

                dataGridView1.ColumnHeadersDefaultCellStyle.Font = currentFont;
                dataGridView1.Font = currentFont;
            }
        }


        private void InitializeDataGridViewColumns(DataGridView dataGridView1)
        {
            if (dataGridView1 != null)
            {
                Font currentFont = new Font("Bookman Old Style", currentOutputFontSize);
                DataGridViewCellStyle cellStyle = new DataGridViewCellStyle { Font = currentFont };

                dataGridView1.Columns.Clear();

                // Тип тетрады (опционально)
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "QuadType",
                    HeaderText = "Тип",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    Width = 70,
                    DefaultCellStyle = cellStyle
                });

                // Оператор (op)
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Op",
                    HeaderText = "Оператор",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    Width = 80,
                    DefaultCellStyle = cellStyle
                });

                // Аргумент 1 (arg1)
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Arg1",
                    HeaderText = "Аргумент 1",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    Width = 100,
                    DefaultCellStyle = cellStyle
                });

                // Аргумент 2 (arg2)
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Arg2",
                    HeaderText = "Аргумент 2",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    Width = 100,
                    DefaultCellStyle = cellStyle
                });

                // Результат (result)
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Result",
                    HeaderText = "Результат",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    Width = 100,
                    DefaultCellStyle = cellStyle
                });

                dataGridView1.ColumnHeadersDefaultCellStyle.Font = currentFont;
                dataGridView1.Font = currentFont;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Сохраняем настройки при закрытии формы
            Properties.Settings.Default.Save();

            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                var splitContainer = tabPage.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer == null) continue;

                var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                if (richTextBox == null) continue;

                if (richTextBox.Modified)
                {
                    DialogResult result = MessageBox.Show($"Сохранить изменения в {tabPage.Text}?",
                                                          "Предупреждение", MessageBoxButtons.YesNoCancel,
                                                          MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            // Сохраняем текущую вкладку
                            SaveTab(tabPage);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        // Метод для сохранения конкретной вкладки
        private void SaveTab(TabPage tabPage)
        {
            var splitContainer = tabPage.Controls.OfType<SplitContainer>().FirstOrDefault();
            if (splitContainer == null) return;

            var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
            if (richTextBox == null) return;

            string filePath = richTextBox.Tag as string;

            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllText(filePath, richTextBox.Text);
                richTextBox.Modified = false;
            }
            else
            {
                сохранитьКакToolStripMenuItem_Click(null, null);
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStatus("Сохранение файла...");
            try
            {
                Save();
                SetStatus("Файл успешно сохранен");
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Save()
        {
            if (tabControl1.SelectedTab == null) return;

            // Получаем текущий редактор
            var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
            if (splitContainer == null) return;

            var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
            if (richTextBox == null) return;

            string filePath = richTextBox.Tag as string;

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    File.WriteAllText(filePath, richTextBox.Text);
                    richTextBox.Modified = false;
                    MessageBox.Show("Файл сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла: " + ex.Message,
                                   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                сохранитьКакToolStripMenuItem_Click(null, null);
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null) return;

            var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
            if (splitContainer == null) return;

            var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
            if (richTextBox == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.Title = "Сохранить файл как";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, richTextBox.Text);
                    richTextBox.Tag = saveFileDialog.FileName;
                    tabControl1.SelectedTab.Text = Path.GetFileName(saveFileDialog.FileName);
                    richTextBox.Modified = false;
                    MessageBox.Show("Файл успешно сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла: " + ex.Message,
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Перебираем все вкладки в TabControl
            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                var splitContainer = tabPage.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer == null) continue;

                var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                if (richTextBox == null) continue;

                if (richTextBox.Modified)
                {
                    DialogResult result = MessageBox.Show($"Сохранить изменения в {tabPage.Text}?",
                                                          "Предупреждение",
                                                          MessageBoxButtons.YesNoCancel,
                                                          MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            SaveTab(tabPage); // Сохраняем вкладку
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Прерываем выход при ошибке
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return; // Прерываем выход, если пользователь нажал "Отмена"
                    }
                }
            }

            // Закрываем приложение
            Application.Exit();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewTab(null, "Новый документ", "");
        }

        private void OpenFileInNewTab(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);

            CreateNewTab(filePath, fileName, fileContent);
        }

        private void richTextBox_SelectionChanged(object sender, EventArgs e)
        {
            var richTextBox = sender as RichTextBox;
            if (richTextBox != null)
            {
                int line = richTextBox.GetLineFromCharIndex(richTextBox.SelectionStart) + 1;
                int column = richTextBox.SelectionStart - richTextBox.GetFirstCharIndexOfCurrentLine() + 1;
                SetStatus($"Строка: {line}, Колонка: {column}");
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer != null)
                {
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                    if (richTextBox != null)
                    {
                        string filePath = richTextBox.Tag as string;
                        string fileName = string.IsNullOrEmpty(filePath) ? "Новый документ" : Path.GetFileName(filePath);
                        int lineCount = richTextBox.Lines.Length;
                        SetStatus($"Файл: {fileName} | Строк: {lineCount}");
                    }
                }
            }
        }

        private void CreateNewTab(string filePath, string tabTitle, string fileContent)
        {
            // Создаём новую вкладку
            TabPage newTab = new TabPage(tabTitle)
            {
                Padding = new Padding(3),
                UseVisualStyleBackColor = true,
                BackColor = Color.FloralWhite
            };

            // Создаём SplitContainer для редактора с нумерацией строк
            SplitContainer editorSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                IsSplitterFixed = true,
                SplitterWidth = 1,
                FixedPanel = FixedPanel.Panel1
            };

            // Определяем ширину панели нумерации
            using (RichTextBox tempRichTextBox = new RichTextBox())
            {
                int requiredWidth = TextRenderer.MeasureText("999", tempRichTextBox.Font).Width + 10;
                editorSplitContainer.SplitterDistance = requiredWidth;
            }

            // Панель для нумерации строк
            Panel lineNumberPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LavenderBlush
            };

            // RichTextBox для редактирования кода
            RichTextBox editorRichTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LavenderBlush,
                Text = fileContent,
                Tag = filePath,
                Name = "editorRichTextBox",
                BorderStyle = BorderStyle.None,
                Font = new Font("Courier New", currentEditorFontSize)
            };

            // Инициализация нумерации строк
            InitializeLineNumbering(lineNumberPanel, editorRichTextBox);

            // Добавляем элементы в контейнеры
            editorSplitContainer.Panel1.Controls.Add(lineNumberPanel);
            editorSplitContainer.Panel2.Controls.Add(editorRichTextBox);

            // Добавляем редактор во вкладку
            newTab.Controls.Add(editorSplitContainer);

            // Добавляем вкладку в tabControl1, который находится в splitcontainer1.Panel1
            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;

            editorRichTextBox.SelectionChanged += richTextBox_SelectionChanged;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;

        }

        // Методы для нумерации строк остаются без изменений
        private void InitializeLineNumbering(Panel panel, RichTextBox richTextBox)
        {
            panel.Paint += (sender, e) => LineNumberPanel_Paint(sender, e, richTextBox);
            richTextBox.TextChanged += (s, e) => panel.Invalidate();
            richTextBox.VScroll += (s, e) => panel.Invalidate();
            richTextBox.SelectionChanged += (s, e) => panel.Invalidate();
            richTextBox.FontChanged += (s, e) => panel.Invalidate();
        }

        private void LineNumberPanel_Paint(object sender, PaintEventArgs e, RichTextBox richTextBox)
        {
            int firstIndex = richTextBox.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = richTextBox.GetLineFromCharIndex(firstIndex);

            int lineHeight = TextRenderer.MeasureText("0", richTextBox.Font).Height;
            int y = 0;

            for (int i = firstLine; y < richTextBox.Height; i++)
            {
                y = (i - firstLine) * lineHeight;
                e.Graphics.DrawString((i + 1).ToString(), richTextBox.Font, Brushes.Black, new PointF(5, y));
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetStatus("Открытие файла...");
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Открыть файл";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        SetStatus($"Загрузка файла: {Path.GetFileName(filePath)}");
                        OpenFileInNewTab(filePath);
                    }
                    SetStatus($"Загружено файлов: {openFileDialog.FileNames.Length}");
                }
                else
                {
                    SetStatus("Открытие файла отменено");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка: {ex.Message}");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            создатьToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            открытьToolStripMenuItem_Click(sender, e);
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer != null)
                {
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                    if (richTextBox != null && richTextBox.CanUndo)
                    {
                        richTextBox.Undo(); // Отменяем последнее действие
                        richTextBox.Focus(); // Возвращаем фокус для визуального отображения
                    }
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            отменитьToolStripMenuItem_Click(sender, e);
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (splitContainer != null)
                {
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();
                    if (richTextBox != null && richTextBox.CanRedo)
                    {
                        richTextBox.Redo(); // Повторяем последнее действие
                    }
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            повторитьToolStripMenuItem_Click(sender, e);
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // Безопасное получение SplitContainer
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();

                if (splitContainer != null)
                {
                    // Получаем RichTextBox из Panel2
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();

                    if (richTextBox != null && richTextBox.SelectionLength > 0)
                    {
                        richTextBox.Cut(); // Вырезаем выделенный текст
                    }
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            вырезатьToolStripMenuItem_Click(sender, e);
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // Безопасное получение SplitContainer
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();

                if (splitContainer != null)
                {
                    // Получаем RichTextBox из Panel2
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();

                    if (richTextBox != null && richTextBox.SelectionLength > 0)
                    {
                        richTextBox.Copy();
                    }
                }
            }
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            копироватьToolStripMenuItem_Click(sender, e);
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // Безопасное получение SplitContainer
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();

                if (splitContainer != null)
                {
                    // Получаем RichTextBox из Panel2
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();

                    if (richTextBox != null)
                    {
                        // Проверяем, есть ли текст в буфере обмена
                        if (Clipboard.ContainsText())
                        {
                            richTextBox.Paste();
                        }
                    }
                }
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            вставитьToolStripMenuItem_Click(sender, e);
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // Безопасное получение SplitContainer
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();

                if (splitContainer != null)
                {
                    // Получаем RichTextBox из Panel2
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();

                    if (richTextBox != null && richTextBox.SelectionLength > 0)
                    {
                        // Удаляем выделенный текст
                        richTextBox.SelectedText = string.Empty;
                    }
                }
            }
        }

        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                // Получаем SplitContainer из выбранной вкладки
                var splitContainer = tabControl1.SelectedTab.Controls.OfType<SplitContainer>().FirstOrDefault();

                // Проверяем, что SplitContainer не равен null
                if (splitContainer != null)
                {
                    // Получаем RichTextBox из Panel2 SplitContainer
                    var richTextBox = splitContainer.Panel2.Controls.OfType<RichTextBox>().FirstOrDefault();

                    // Проверяем, что richTextBox не равен null
                    if (richTextBox != null)
                    {
                        // Выделяем весь текст в RichTextBox
                        richTextBox.SelectAll();
                        richTextBox.Focus(); // Даем фокус для визуального выделения
                    }
                }
            }
        }
        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                Help helpForm = new Help();
                helpForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму "О программе"
                About aboutForm = new About();
                aboutForm.ShowDialog(); // Используем ShowDialog для модального окна
            }
            catch (Exception ex)
            {
                // Если возникла ошибка при создании формы, показываем MessageBox
                MessageBox.Show("Не удалось открыть информацию о программе: " + ex.Message,
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            вызовСправкиToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            оПрограммеToolStripMenuItem_Click(sender, e);
        }


        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            пускToolStripMenuItem_Click(sender, e);
        }

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                ПЗ пзForm = new ПЗ();
                пзForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void грамматикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                GR grForm = new GR();
                grForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                KL klForm = new KL();
                klForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                MetA maForm = new MetA();
                maForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                TP tpForm = new TP();
                tpForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                SL slForm = new SL();
                slForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private void исходныйКодПрограммаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Создаем и показываем форму справки
                IK ikForm = new IK();
                ikForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message,
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }
    }
}
