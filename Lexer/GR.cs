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
    public partial class GR: Form
    {
        public GR()
        {
            InitializeComponent();


            Text = "Грамматика";
            //webBrowser3.Navigate(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\Properties\\Грамматика.html");
            webBrowser3.DocumentText = Properties.Resources.Грамматика;
        }
    }
}
