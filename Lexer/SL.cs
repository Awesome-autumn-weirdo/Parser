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
    public partial class SL: Form
    {
        public SL()
        {
            InitializeComponent();

            Text = "Список литературы";
            //webBrowser7.Navigate(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\Properties\\Список литературы.html");
            webBrowser7.DocumentText = Properties.Resources.Список_литературы;
        }
    }
}
