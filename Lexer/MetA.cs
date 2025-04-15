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
    public partial class MetA : Form
    {
        public MetA()
        {
            InitializeComponent();
            Text = "Методы анализа";

            // Получаем HTML из ресурсов
            string htmlTemplate = Properties.Resources.Методы_анализа;

            // Получаем изображение из ресурсов и превращаем в base64
            using (MemoryStream ms = new MemoryStream())
            {
                Properties.Resources.ТфякСхема.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                string base64Image = Convert.ToBase64String(ms.ToArray());

                // Вставляем в HTML вместо имени файла
                string htmlWithImage = htmlTemplate.Replace("ТфякСхема.png", $"data:image/png;base64,{base64Image}");

                // Загружаем в WebBrowser
                webBrowser5.DocumentText = htmlWithImage;
            }
        }
    }

}
