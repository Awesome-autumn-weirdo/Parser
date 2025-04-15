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
    public partial class IK: Form
    {
        public IK()
        {
            InitializeComponent();

            Text = "Исходный код";
            //webBrowser8.Navigate(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\Properties\\Исходный код.html");
            webBrowser8.DocumentText = Properties.Resources.Исходный_код;
        }
    }
}
