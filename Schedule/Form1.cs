using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

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


        private async void DrawCalendar()
        {
            // 타이틀 업데이트
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";
            tableLayoutPanel.Controls.Clear();

            DateTime firstDay = new DateTime(currentYear, currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            int startDayOfWeek = (int)firstDay.DayOfWeek;

            // ✅ 공휴일 정보 가져오기
            var holidays = await GetHolidaysAsync(currentYear, currentMonth);

            int day = 1;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    int cellIndex = row * 7 + col;

                    if (cellIndex >= startDayOfWeek && day <= daysInMonth)
                    {
                        DateTime thisDate = new DateTime(currentYear, currentMonth, day);

                        Label dayLabel = new Label();
                        dayLabel.Text = day.ToString();
                        dayLabel.TextAlign = ContentAlignment.TopLeft;
                        dayLabel.Dock = DockStyle.Fill;
                        dayLabel.Font = new Font("맑은 고딕", 11, FontStyle.Bold);
                        dayLabel.Padding = new Padding(5);
                        dayLabel.Margin = new Padding(2);
                        dayLabel.BackColor = Color.White;
                        dayLabel.BorderStyle = BorderStyle.FixedSingle;

                        // 🔴 일요일
                        if (col == 0)
                            dayLabel.ForeColor = Color.Red;
                        // 🔵 토요일
                        else if (col == 6)
                            dayLabel.ForeColor = Color.Blue;
                        else
                            dayLabel.ForeColor = Color.Black;

                        if (holidays.ContainsKey(thisDate))
                        {
                            dayLabel.ForeColor = Color.Red;
                            dayLabel.Text += $"\n{holidays[thisDate]}";

                        }

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
        public async Task<Dictionary<DateTime, string>> GetHolidaysAsync(int year, int month)
        {
            var apiKey = "coORf%2Fadr5Q4QdTlwmt4Uv9EjJNT3hioOnbntLqqpTWtCxDYgSc%2BoYkZR%2BbSCwzeaBsaWmXgZfYvyuZi2gOpAw%3D%3D";
            var url = $"https://apis.data.go.kr/B090041/openapi/service/SpcdeInfoService/getHoliDeInfo" +
                      $"?solYear={year}&solMonth={month.ToString("D2")}&ServiceKey={apiKey}&_type=json";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);

                var holidays = new Dictionary<DateTime, string>();

                var json = JObject.Parse(response);
                var itemsToken = json["response"]?["body"]?["items"];

                if (itemsToken == null || itemsToken.Type == JTokenType.Null)
                {
                    Console.WriteLine("해당 월에는 공휴일이 없습니다.");
                    return holidays;
                }

                if (itemsToken.Type == JTokenType.Object)
                {
                    var itemToken = itemsToken["item"];
                    if (itemToken == null || itemToken.Type == JTokenType.Null)
                    {
                        return holidays;
                    }

                    var items = itemToken.Type == JTokenType.Array ? itemToken : new JArray(itemToken);

                    foreach (var item in items)
                    {
                        var dateStr = item["locdate"]?.ToString();
                        var name = item["dateName"]?.ToString();
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date))
                        {
                            holidays[date] = name;
                        }
                    }
                }
                else if (itemsToken.Type == JTokenType.Array)
                {
                    foreach (var itemToken in itemsToken)
                    {
                        var dateStr = itemToken["locdate"]?.ToString();
                        var name = itemToken["dateName"]?.ToString();
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date))
                        {
                            holidays[date] = name;
                        }
                    }
                }

                return holidays;
            }
            
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            currentYear = DateTime.Now.Year;
            currentMonth = DateTime.Now.Month;
            DrawCalendar();
        }
    }
}
