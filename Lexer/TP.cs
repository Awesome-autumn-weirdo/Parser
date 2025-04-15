using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lexer
{
    public partial class TP : Form
    {
        public TP()
        {
            InitializeComponent();
            Text = "Тестовый пример";

            // Загружаем HTML-шаблон из ресурсов
            string html = Properties.Resources.Тестовый_пример;

            // Подставляем изображения
            html = InsertImageIntoHtml(html, "test1.png", Properties.Resources.test1);
            html = InsertImageIntoHtml(html, "test2.png", Properties.Resources.test2);
            html = InsertImageIntoHtml(html, "test3.png", Properties.Resources.test3);

            // Отображаем в WebBrowser
            webBrowser6.DocumentText = html;
        }

        private string InsertImageIntoHtml(string html, string placeholder, Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                string base64 = Convert.ToBase64String(ms.ToArray());
                string dataUri = $"data:image/png;base64,{base64}";
                return html.Replace(placeholder, dataUri);
            }
        }
    }

}
