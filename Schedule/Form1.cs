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
            InitActivityList(); // 활동 미리 넣기
            UpdateTitle();
            DrawCalendar();
        }

        private void InitActivityList()
        {
            listBoxActivities.Items.AddRange(new string[]
            {
                "사랑의 편지 쓰기", "퍼즐 맞추기", "신문읽기", "미술활동", "체조", "노래 부르기",
                "종이접기", "건강체조", "동화 듣기", "회상 퀴즈"
            });
        }

        private void UpdateTitle()
        {
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";
        }

        private async void DrawCalendar()
        {
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";
            tableLayoutPanel.Controls.Clear();

            DateTime firstDay = new DateTime(currentYear, currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            int startDayOfWeek = (int)firstDay.DayOfWeek;

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

                        Panel dayPanel = new Panel();
                        dayPanel.Dock = DockStyle.Fill;
                        dayPanel.Margin = new Padding(2);
                        dayPanel.BackColor = Color.White;
                        dayPanel.BorderStyle = BorderStyle.FixedSingle;

                        Label dayLabel = new Label();
                        dayLabel.Text = day.ToString();
                        dayLabel.Font = new Font("맑은 고딕", 11, FontStyle.Bold);
                        dayLabel.AutoSize = true;
                        dayLabel.Location = new Point(5, 5);

                        Label holidayLabel = new Label();
                        holidayLabel.Text = holidays.ContainsKey(thisDate) ? holidays[thisDate] : "";
                        holidayLabel.Font = new Font("맑은 고딕", 8, FontStyle.Regular);
                        holidayLabel.AutoSize = true;
                        holidayLabel.ForeColor = Color.Red;
                        holidayLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                        holidayLabel.Location = new Point(dayPanel.Width - 60, 8);

                        dayPanel.Resize += (s, e) =>
                        {
                            holidayLabel.Location = new Point(dayPanel.Width - holidayLabel.Width - 5, 8);
                        };

                        if (col == 0 || holidays.ContainsKey(thisDate))
                            dayLabel.ForeColor = Color.Red;
                        else if (col == 6)
                            dayLabel.ForeColor = Color.Blue;
                        else
                            dayLabel.ForeColor = Color.Black;

                        // ✅ 클릭 이벤트 등록
                        dayPanel.Click += (s, e) => AddRandomActivitiesToPanel(dayPanel);
                        dayLabel.Click += (s, e) => AddRandomActivitiesToPanel(dayPanel);

                        dayPanel.Controls.Add(dayLabel);
                        dayPanel.Controls.Add(holidayLabel);
                        tableLayoutPanel.Controls.Add(dayPanel, col, row);
                        day++;
                    }
                }
            }
        }

        private void AddRandomActivitiesToPanel(Panel panel)
        {
            var rand = new Random();
            var allItems = listBoxActivities.Items.Cast<string>().ToList();
            if (allItems.Count < 3) return;

            // 이미 등록된 활동 수만큼 아래에 추가
            int existing = panel.Controls.Count;
            int topOffset = 25 + (existing - 2) * 18; // 2는 dayLabel, holidayLabel 제외

            var selected = allItems.OrderBy(x => rand.Next()).Take(3).ToList();

            foreach (var activity in selected)
            {
                Label actLabel = new Label();
                actLabel.Text = activity;
                actLabel.Font = new Font("맑은 고딕", 8);
                actLabel.AutoSize = false;
                actLabel.Width = panel.Width - 10;
                actLabel.Height = 16;
                actLabel.Location = new Point(5, topOffset);
                actLabel.ForeColor = Color.Black;

                topOffset += 16;
                panel.Controls.Add(actLabel);
            }
        }

        private async Task<Dictionary<DateTime, string>> GetHolidaysAsync(int year, int month)
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
                    return holidays;

                if (itemsToken.Type == JTokenType.Object)
                {
                    var itemToken = itemsToken["item"];
                    if (itemToken == null || itemToken.Type == JTokenType.Null)
                        return holidays;

                    var items = itemToken.Type == JTokenType.Array ? itemToken : new JArray(itemToken);

                    foreach (var item in items)
                    {
                        var dateStr = item["locdate"]?.ToString();
                        var name = item["dateName"]?.ToString();
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date))
                            holidays[date] = name;
                    }
                }
                else if (itemsToken.Type == JTokenType.Array)
                {
                    foreach (var itemToken in itemsToken)
                    {
                        var dateStr = itemToken["locdate"]?.ToString();
                        var name = itemToken["dateName"]?.ToString();
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date))
                            holidays[date] = name;
                    }
                }

                return holidays;
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

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            currentYear = DateTime.Now.Year;
            currentMonth = DateTime.Now.Month;
            DrawCalendar();
        }
    }
}
