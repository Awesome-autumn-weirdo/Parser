using Lexer.Properties;
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
    public partial class ПЗ: Form
    {
        public ПЗ()
        {
            InitializeComponent();

            Text = "Постановка задачи";
            //webBrowser2.Navigate(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\Properties\\Постановка задачи.html");
            webBrowser2.DocumentText = Properties.Resources.Постановка_задачи;
        }
    }
}
