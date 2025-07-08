using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Schedule
{
    public partial class MyPopupForm : Form
    {
        private string _hwpFilePath;
        private Dictionary<string, string> _replacements;
        public MyPopupForm(string hwpFilePath, Dictionary<string, string> replacements)
        {
            InitializeComponent();

            string path = System.IO.Directory.GetCurrentDirectory() + @"\FilePathCheckerModuleExample.dll";
            string HNCRoot = @"HKEY_Current_User\Software\HNC\HwpCtrl\Modules";
            Microsoft.Win32.Registry.SetValue(HNCRoot, "FilePathCheckerModuleExample", Environment.CurrentDirectory + "\\FilePathCheckerModuleExample.dll");
            axHwpCtrl1.RegisterModule("FilePathCheckDLL", "FilePathCheckerModuleExample");
            _hwpFilePath = hwpFilePath;
            _replacements = replacements;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
                Color.Black, 5, ButtonBorderStyle.Solid,
                Color.Black, 5, ButtonBorderStyle.Solid,
                Color.Black, 5, ButtonBorderStyle.Solid,
                Color.Black, 5, ButtonBorderStyle.Solid);
        }


        private void HwpFindReplace(string findString, string replaceString)
        {
            HWPCONTROLLib.DHwpAction act = (HWPCONTROLLib.DHwpAction)axHwpCtrl1.CreateAction("AllReplace");
            HWPCONTROLLib.DHwpParameterSet pset = (HWPCONTROLLib.DHwpParameterSet)act.CreateSet();
            act.GetDefault(pset);
            pset.SetItem("FindString", findString);
            pset.SetItem("ReplaceString", replaceString);
            pset.SetItem("Direction", 2);
            pset.SetItem("MatchCase", false);
            pset.SetItem("AllWordForms", false);
            pset.SetItem("SeveralWords", true);
            pset.SetItem("UseWildCards", false);
            pset.SetItem("WholeWordOnly", false);
            pset.SetItem("AutoSpell", false);
            pset.SetItem("ReplaceMode", true);
            pset.SetItem("IgnoreMessage", true);
            act.Execute(pset);
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            HwpFindReplace("{어르신}", "인연자");
            HwpFindReplace("{YYYY}", "2025");
            HwpFindReplace("{MM}", "07");

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "한글 파일 (*.hwp)|*.hwp|모든 파일 (*.*)|*.*"; 
            saveFileDialog.Title = "다른 이름으로 저장"; 
            saveFileDialog.DefaultExt = "hwp"; 
            saveFileDialog.FileName = $"{_replacements["{어르신}"]} {_replacements["{MM}"]}월 인지활동 프로그램 계획표";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    axHwpCtrl1.SaveAs(saveFileDialog.FileName, "HWP", "HWP");
                    MessageBox.Show("파일이 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("파일 저장 중 오류가 발생했습니다.\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MyPopupForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_hwpFilePath) && File.Exists(_hwpFilePath))
                {
                    axHwpCtrl1.Open(_hwpFilePath);
                    foreach (var pair in _replacements)
                    {
                        HwpFindReplace(pair.Key, pair.Value);
                    }
                }
                else
                {
                    MessageBox.Show("파일 경로가 잘못되었거나 존재하지 않습니다.");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }


    }
}
