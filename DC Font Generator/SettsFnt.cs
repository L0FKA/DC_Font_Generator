using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace DC_Font_Generator
{
    public partial class SettsFnt : Form
    {
        #region Свойства
        /*
         получаем свойства для передачи
         необходимо начертание и размер
         */
        internal float emSize { get; set; }
        internal FontStyle fsy { get; set; }
        internal System.Drawing.Text.PrivateFontCollection getFNT;
#endregion
        public SettsFnt()
        {
            InitializeComponent();
            this.Status.Text = String.Empty;
            this.fStyle.SelectedIndex = 0;
            this.fSize.SelectedIndex = 5;
            this.Preview.Text = "AaBbCc01";
            this.DialogResult = DialogResult.Cancel;
        }
        /// <summary>
        /// Для удобного выбора размра.(не используется)
        /// </summary>
        /// <param name="sender"></param>
        private void _RegExpFind(object sender)
        {
            /// Что сравнивают
            string Reg = ((Control)sender).Text.Trim();
            /// С чем сравнивают
            string s = fSize.Text;
            
            try
            {
                Regex regex = new Regex(Reg);
                Match match = regex.Match(s);

                //теперь нужно не выводить текст а выбирать элемент который совпадает.
                if (match.Success)
                {
                    string a = "";
                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        a += match.Groups[0].Value.ToString();
                    }
                }
                else
                {
                    Status.ForeColor = Color.Black;
                    Status.Text = "Нет совпадений !";
                }
            }
            catch (Exception E)
            {
                Status.ForeColor = Color.Red;
                Status.Text = "Неправильное выражение: " + E.Message;
            }
        }
        private void _updPreview()
        {
            if (getFNT != null)
                this.Preview.Font = new System.Drawing.Font(getFNT.Families[0], emSize, fsy, GraphicsUnit.Point);
        }
        private void fSize_IndexChange(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
            textBox1.Text = ((Control)sender).Text.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string ss = ((TextBox)sender).Text.ToString();
            try { float number = float.Parse(ss); emSize = number; _updPreview(); }
            catch { return; }
            finally { }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            FontStyle ffs = new FontStyle();

            switch (fStyle.SelectedIndex)
            {
                case 0://"обычный\n":
                    ffs = FontStyle.Regular;
                    break;
                case 1://"курсив\n":
                    ffs = FontStyle.Italic;
                    break;
                case 2://"полужирный\n":
                    ffs = FontStyle.Bold;
                    break;
                case 3://"полужирный курсив\n":
                    ffs = FontStyle.Italic | FontStyle.Bold;
                    break;
            }
            fsy = ffs;
            _updPreview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm mf = new MainForm();
                mf.emSize = emSize;
                mf.fsy = fsy;
                this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SettsFnt_Load(object sender, EventArgs e)
        {
            _updPreview();
        }
    }
}
