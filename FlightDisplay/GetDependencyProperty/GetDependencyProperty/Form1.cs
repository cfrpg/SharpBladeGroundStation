using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDependencyProperty
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ctrl = ctrltext.Text;
            string type = typetext.Text;
            string name = nametext.Text;

            string res = "";
            string a = System.Environment.NewLine;
            res += string.Format("public static readonly DependencyProperty {0}Property =", name);
            res += a;
            res += string.Format("\tDependencyProperty.Register(\"{0}\", typeof({1}), typeof({2}), new PropertyMetadata(0, {2}.On{0}PropertyChanged));", name, type, ctrl);
            res += a;
            res += string.Format("public {0} {1}", type, name);
            res += a + "{" + a;
            res += string.Format("\tget {{ return ({0})GetValue({1}Property); }}", type, name);
            res += a;
            res += string.Format("\tset {{ SetValue({0}Property, value); }}", name);
            res += a + "}" + a;
            res += string.Format("public static void On{0}PropertyChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)", name);
            res += a + "{" + a;
            res += string.Format("\t{0} s = sender as {0};",ctrl);
            res += a;
            res += string.Format("\ts.On{0}Changed(e);", name);
            res += a + "}" + a;
            res += string.Format("public void On{0}Changed(DependencyPropertyChangedEventArgs e)", name);
            res += a + "{" + a + a;
            res += a + "}" + a;
            restext.Text = res;
        }
    }
}
