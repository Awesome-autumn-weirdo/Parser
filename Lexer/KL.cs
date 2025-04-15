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
    public partial class KL: Form
    {
        public KL()
        {
            InitializeComponent();

            Text = "Классификация";
            //webBrowser4.Navigate(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\Properties\\Классификация.html");
            webBrowser4.DocumentText = Properties.Resources.Классификация;
        }
    }
}
