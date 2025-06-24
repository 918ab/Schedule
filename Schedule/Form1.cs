using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
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

        private List<Panel> selectedPanels = new List<Panel>();

        public Form1()
        {
            InitializeComponent();
            InitActivityList();
            UpdateTitle();
            DrawCalendar();
        }
        private void InitActivityList()
        {
            string folderPath = @"C:\Schedule";
            string filePath = Path.Combine(folderPath, "list.txt");

            string[] defaultActivities = new string[]
            {
        "사랑의 편지 쓰기", "퍼즐 맞추기", "신문읽기", "미술활동", "체조", "노래 부르기",
        "종이접기", "건강체조", "동화 듣기", "회상 퀴즈"
            };

            try
            {
                // 파일이 없으면 생성하고 기본 활동 저장
                if (!File.Exists(filePath))
                {
                    File.WriteAllLines(filePath, defaultActivities);
                }

                // 파일에서 활동 목록 읽어오기
                string[] activities = File.ReadAllLines(filePath);

                listBoxActivities.Items.Clear();
                listBoxActivities.Items.AddRange(activities);
            }
            catch (Exception ex)
            {
                MessageBox.Show("활동 목록을 불러오는 중 오류가 발생했습니다.\n" + ex.Message,
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UpdateTitle()
        {
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";
        }

        private async void DrawCalendar()
        {
            titleLabel.Text = $"{currentYear}년 {currentMonth}월";
            tableLayoutPanel.Controls.Clear();
            selectedPanels.Clear();

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

                        Panel dayPanel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            Margin = new Padding(2),
                            BackColor = Color.White,
                            BorderStyle = BorderStyle.FixedSingle,
                            Tag = thisDate
                        };

                        Label dayLabel = new Label
                        {
                            Text = day.ToString(),
                            Font = new Font("맑은 고딕", 11, FontStyle.Bold),
                            AutoSize = true,
                            Location = new Point(5, 5)
                        };

                        Label holidayLabel = new Label
                        {
                            Text = holidays.ContainsKey(thisDate) ? holidays[thisDate] : "",
                            Font = new Font("맑은 고딕", 8),
                            AutoSize = true,
                            ForeColor = Color.Red,
                            Anchor = AnchorStyles.Top | AnchorStyles.Right,
                            Location = new Point(dayPanel.Width - 60, 8)
                        };

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
                        AddPanelClickEvents(dayPanel, dayLabel, holidayLabel);

                        dayPanel.Controls.Add(dayLabel);
                        dayPanel.Controls.Add(holidayLabel);
                        tableLayoutPanel.Controls.Add(dayPanel, col, row);
                        day++;
                    }
                }
            }
        }

        private void AddPanelClickEvents(Panel panel, params Control[] controls)
        {
            foreach (var ctrl in controls.Append(panel))
            {
                ctrl.Click += (s, e) =>
                {
                    var parentPanel = ctrl is Panel ? (Panel)ctrl : (Panel)((Control)s).Parent;

                    if (selectedPanels.Contains(parentPanel))
                    {
                        selectedPanels.Remove(parentPanel);
                        parentPanel.BackColor = Color.White;
                    }
                    else
                    {
                        selectedPanels.Add(parentPanel);
                        parentPanel.BackColor = Color.LightGreen;
                    }
                };
            }
        }

        private void AssignRandomActivities()
        {
            var rand = new Random();
            var allItems = listBoxActivities.Items.Cast<string>().ToList();
            if (allItems.Count < 3) return;

            foreach (var panel in selectedPanels)
            {
                var toRemove = panel.Controls.OfType<Label>().Where(l => l.Tag?.ToString() == "activity").ToList();
                foreach (var lbl in toRemove)
                    panel.Controls.Remove(lbl);

                var selected = allItems.OrderBy(x => rand.Next()).Take(3).ToList();
                int topOffset = 30;

                foreach (var activity in selected)
                {
                    Label actLabel = new Label
                    {
                        Text = activity,
                        Font = new Font("맑은 고딕", 8),
                        AutoSize = false,
                        Width = panel.Width - 10,
                        Height = 16,
                        Location = new Point(5, topOffset),
                        ForeColor = Color.Black,
                        Tag = "activity"
                    };
                    actLabel.Click += (s, e) =>
                    {
                        Panel parent = ((Control)s).Parent as Panel;
                        if (selectedPanels.Contains(parent))
                        {
                            selectedPanels.Remove(parent);
                            parent.BackColor = Color.White;
                        }
                        else
                        {
                            selectedPanels.Add(parent);
                            parent.BackColor = Color.LightGreen;
                        }
                    };

                    panel.Controls.Add(actLabel);
                    topOffset += 20;
                }

                panel.BackColor = Color.White;
            }

            selectedPanels.Clear();
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

        private void assignRandomButton_Click(object sender, EventArgs e)
        {
            AssignRandomActivities();
        }

        private void SelectDaysByWeekday(DayOfWeek day)
        {
            foreach (Control ctrl in tableLayoutPanel.Controls)
            {
                if (ctrl is Panel panel && panel.Tag is DateTime date)
                {
                    if (date.DayOfWeek == day)
                    {
                        if (selectedPanels.Contains(panel))
                        {
                            selectedPanels.Remove(panel);
                            panel.BackColor = Color.White;
                        }
                        else
                        {
                            selectedPanels.Add(panel);
                            panel.BackColor = Color.LightGreen;
                        }
                    }
                }
            }
        }
        private void TogglePanelSelection(Panel panel, bool? forceSelect = null)
        {
            bool isSelected = selectedPanels.Contains(panel);

            if (forceSelect == true && !isSelected)
            {
                selectedPanels.Add(panel);
                panel.BackColor = Color.LightBlue;
            }
            else if (forceSelect == false && isSelected)
            {
                selectedPanels.Remove(panel);
                panel.BackColor = Color.White;
            }
            else if (forceSelect == null)
            {
                if (isSelected)
                {
                    selectedPanels.Remove(panel);
                    panel.BackColor = Color.White;
                }
                else
                {
                    selectedPanels.Add(panel);
                    panel.BackColor = Color.LightBlue;
                }
            }
        }
        private void labelMonday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Monday);
        }

        private void labelTuesday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Tuesday);
        }

        private void labelWednesday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Wednesday);
        }

        private void labelThursday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Thursday);
        }

        private void labelFriday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Friday);
        }

        private void labelSaturday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Saturday);
        }

        private void labelSunday_Click(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Sunday);
        }
    }
}
