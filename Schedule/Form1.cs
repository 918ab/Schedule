using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Schedule
{
    public partial class Form1 : Form
    {
        private DateTime currentDate = DateTime.Now;
        int currentYear = DateTime.Now.Year;
        int currentMonth = DateTime.Now.Month;
        public Form1()
        {
            InitializeComponent();
            UpdateTitle();
            DrawCalendar();
        }
        private void UpdateTitle()
        {
            titleLabel.Text = $"{currentDate.Year}년 {currentDate.Month}월";
        }
       

        private void DrawCalendar()
        {
            // 1. 타이틀 표시
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";

            // 2. 기존 날짜 라벨 초기화
            tableLayoutPanel.Controls.Clear();

            // 3. 이번 달의 첫 날과 마지막 날 계산
            DateTime firstDay = new DateTime(currentYear, currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            int startDayOfWeek = (int)firstDay.DayOfWeek; // 일: 0 ~ 토: 6

            // 4. 날짜 채우기
            int day = 1;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    int cellIndex = row * 7 + col;

                    if (cellIndex >= startDayOfWeek && day <= daysInMonth)
                    {
                        Label dayLabel = new Label();
                        dayLabel.Text = day.ToString();
                        dayLabel.TextAlign = ContentAlignment.TopLeft;
                        dayLabel.Dock = DockStyle.Fill;
                        dayLabel.BorderStyle = BorderStyle.FixedSingle;

                        // 일요일은 빨간색
                        if (col == 0)
                            dayLabel.ForeColor = Color.Red;

                        tableLayoutPanel.Controls.Add(dayLabel, col, row);
                        day++;
                    }
                }
            }
        }
        private void leftMonth_Click(object sender, EventArgs e)
        {
            currentMonth--;
            if (currentMonth == 0)
            {
                currentMonth = 12;
                currentYear--;
            }
            DrawCalendar();
        }

        private void rightMonth_Click(object sender, EventArgs e)
        {
            currentMonth++;
            if (currentMonth == 13)
            {
                currentMonth = 1;
                currentYear++;
            }
            DrawCalendar();
        }
    }
}
