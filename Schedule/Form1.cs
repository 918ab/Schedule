using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Schedule
{
    public partial class Form1 : Form
    {
        int currentYear = DateTime.Now.Year;
        int currentMonth = DateTime.Now.Month;
        private List<Panel> selectedPanels = new List<Panel>();

        public Form1()
        {
            InitializeComponent();
            InitActivityLists();
            UpdateTitle();
            DrawCalendar();

        }
        
        private void InitActivityLists()    
        {
            try
            {
                string folderPath = @"C:\Schedule";
                string normalFilePath = Path.Combine(folderPath, "인지활동.txt");
                string specialFilePath = Path.Combine(folderPath, "일상생활활동.txt");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }


                if (!File.Exists(normalFilePath))
                {
                    File.WriteAllLines(normalFilePath, new string[]
                    {
                "청소하기", "독서하기", "산책하기", "운동하기", "요리하기"
                    });
                }

                if (!File.Exists(specialFilePath))
                {
                    File.WriteAllLines(specialFilePath, new string[]
                    {
                "놀이공원 가기", "카페 탐방", "영화관 가기", "게임하기"
                    });
                }

                listBoxActivities.Items.Clear();
                listBoxActivities2.Items.Clear();

                foreach (var line in File.ReadAllLines(normalFilePath))
                {
                    listBoxActivities.Items.Add(line);
                }

                foreach (var line in File.ReadAllLines(specialFilePath))
                {
                    listBoxActivities2.Items.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("활동 목록 초기화 중 오류 발생: " + ex.Message);
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
                for (int col = 0; col < 8; col++)
                {
                    int cellIndex = row * 8 + col;
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

                        DayOfWeek dayOfWeek = thisDate.DayOfWeek;

                        if (dayOfWeek == DayOfWeek.Sunday || holidays.ContainsKey(thisDate))
                            dayLabel.ForeColor = Color.Red;
                        else if (dayOfWeek == DayOfWeek.Saturday)
                            dayLabel.ForeColor = Color.Blue;
                        else
                            dayLabel.ForeColor = Color.Black;

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

            var generalPool = listBoxActivities.Items.Cast<string>().ToList();
            var specialPool = listBoxActivities2.Items.Cast<string>().ToList();

            if (generalPool.Count < 2 || specialPool.Count < 1 || selectedPanels.Count == 0) return;

            var generalCopy = generalPool.OrderBy(x => rand.Next()).ToList();
            var specialCopy = specialPool.OrderBy(x => rand.Next()).ToList();

            var shuffledPanels = selectedPanels.OrderBy(x => rand.Next()).ToList();

            foreach (var panel in shuffledPanels)
            {
                var toRemove = panel.Controls.OfType<Label>().Where(l => l.Tag?.ToString() == "activity").ToList();
                foreach (var lbl in toRemove)
                    panel.Controls.Remove(lbl);

                if (generalCopy.Count < 2)
                    generalCopy = generalPool.OrderBy(x => rand.Next()).ToList();
                if (specialCopy.Count < 1)
                    specialCopy = specialPool.OrderBy(x => rand.Next()).ToList();

                var selectedGeneral = generalCopy.Take(2).ToList();
                var selectedSpecial = specialCopy.Take(1).ToList();

                generalCopy = generalCopy.Skip(2).ToList();
                specialCopy = specialCopy.Skip(1).ToList();

                var activities = selectedGeneral.Concat(selectedSpecial).ToList();
                int topOffset = 30;

                foreach (var activity in activities)
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
            guna2CheckBox1.Checked = false;
            guna2CheckBox2.Checked = false;
            DrawCalendar();
        }

        private void assignRandomButton_Click(object sender, EventArgs e)
        {
            AssignRandomActivities();
            guna2CheckBox1.Checked = false;
            guna2CheckBox2.Checked = false;
        }

        private void SelectDaysByWeekday(DayOfWeek day, bool isSelected)
        {
            foreach (Control ctrl in tableLayoutPanel.Controls)
            {
                if (ctrl is Panel panel && panel.Tag is DateTime date)
                {
                    if (date.DayOfWeek == day)
                    {
                        if (isSelected)
                        {
                            if (!selectedPanels.Contains(panel))
                            {
                                selectedPanels.Add(panel);
                                panel.BackColor = Color.LightGreen;
                            }
                        }
                        else
                        {
                            if (selectedPanels.Contains(panel))
                            {
                                selectedPanels.Remove(panel);
                                panel.BackColor = Color.White;
                            }
                        }
                    }
                }
            }
        }
        private bool IsAllSelected(DayOfWeek day)
        {
            foreach (Control ctrl in tableLayoutPanel.Controls)
            {
                if (ctrl is Panel panel && panel.Tag is DateTime date)
                {
                    if (date.DayOfWeek == day)
                    {
                        if (!selectedPanels.Contains(panel))
                            return false;
                    }
                }
            }
            return true;
        }
        private void labelMonday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Monday);
            SelectDaysByWeekday(DayOfWeek.Monday, selectAll);
        }

        private void labelTuesday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Tuesday);
            SelectDaysByWeekday(DayOfWeek.Tuesday, selectAll);
        }

        private void labelWednesday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Wednesday);
            SelectDaysByWeekday(DayOfWeek.Wednesday, selectAll);
        }

        private void labelThursday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Thursday);
            SelectDaysByWeekday(DayOfWeek.Thursday, selectAll);
        }

        private void labelFriday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Friday);
            SelectDaysByWeekday(DayOfWeek.Friday, selectAll);
        }

        private void labelSaturday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Saturday);
            SelectDaysByWeekday(DayOfWeek.Saturday, selectAll);
        }

        private void labelSunday_Click(object sender, EventArgs e)
        {
            bool selectAll = !IsAllSelected(DayOfWeek.Sunday);
            SelectDaysByWeekday(DayOfWeek.Sunday, selectAll);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = @"C:\Schedule";
                string filePath = Path.Combine(folderPath, "인지활동.txt");
                string[] lines = File.ReadAllLines(filePath);
                listBoxActivities.Items.Clear();
                listBoxActivities.Items.AddRange(lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("활동 목록을 다시 불러오는 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        
        {
            try
            {
                string folderPath = @"C:\Schedule";
                string filePath = Path.Combine(folderPath, "인지활동.txt");
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일을 여는 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpen2_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = @"C:\Schedule";
                string filePath = Path.Combine(folderPath, "일상생활활동.txt");

                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일을 여는 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReload2_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = @"C:\Schedule";
                string filePath = Path.Combine(folderPath, "일상생활활동.txt");
                string[] lines = File.ReadAllLines(filePath);
                listBoxActivities2.Items.Clear();
                listBoxActivities2.Items.AddRange(lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("활동 목록을 다시 불러오는 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnout_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("어르신 이름을 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox2.Text))
            {
                MessageBox.Show("담당자 이름을 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool hasActivity = tableLayoutPanel.Controls.OfType<Panel>()
                                   .Any(p => p.Controls.OfType<Label>().Any(l => l.Tag?.ToString() == "activity"));

            if (!hasActivity)
            {
                MessageBox.Show("달력에 활동이 하나 이상 설정되어야 합니다.", "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var replacements = new Dictionary<string, string>
            {
                { "{어르신}", guna2TextBox1.Text },
                { "{YYYY}", currentYear.ToString() },
                { "{MM}", currentMonth.ToString("D2") },
                { "{담당자}", guna2TextBox2.Text }
            };

            for (int i = 1; i <= 42; i++)
            {
                replacements[$"{{{i}d}}"] = "";
                replacements[$"{{{i}1}}"] = "";
                replacements[$"{{{i}2}}"] = "";
                replacements[$"{{{i}3}}"] = "";
            }

            for (int row = 0; row < tableLayoutPanel.RowCount; row++)
            {
                for (int col = 0; col < tableLayoutPanel.ColumnCount; col++)
                {
                    if (tableLayoutPanel.GetControlFromPosition(col, row) is Panel panel && panel.Tag is DateTime)
                    {
                        int placeholderIndex = (row * tableLayoutPanel.ColumnCount) + col + 1;
                        var dayLabel = panel.Controls.OfType<Label>()
                                              .FirstOrDefault(l => l.Tag?.ToString() != "activity");
                        if (dayLabel != null)
                        {
                            replacements[$"{{{placeholderIndex}d}}"] = dayLabel.Text;
                        }
                        var activityLabels = panel.Controls.OfType<Label>()
                                                      .Where(l => l.Tag?.ToString() == "activity")
                                                      .ToList();
                        for (int i = 0; i < 3; i++)
                        {
                            string placeholder = $"{{{placeholderIndex}{i + 1}}}";
                            string value = (i < activityLabels.Count) ? activityLabels[i].Text : "";
                            replacements[placeholder] = value;
                        }
                    }
                }
            }

            string hwpFilePath = @"C:\Schedule\인지활동계획표.hwp";
            MyPopupForm popup = new MyPopupForm(hwpFilePath, replacements);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.ShowDialog(this);
        }



        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Monday, guna2CheckBox1.Checked);
            SelectDaysByWeekday(DayOfWeek.Tuesday, guna2CheckBox1.Checked);
            SelectDaysByWeekday(DayOfWeek.Wednesday, guna2CheckBox1.Checked);
            SelectDaysByWeekday(DayOfWeek.Thursday, guna2CheckBox1.Checked);
            SelectDaysByWeekday(DayOfWeek.Friday, guna2CheckBox1.Checked);
        }

        private void guna2CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            SelectDaysByWeekday(DayOfWeek.Saturday, guna2CheckBox2.Checked);
            SelectDaysByWeekday(DayOfWeek.Sunday, guna2CheckBox2.Checked);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {

            foreach (Panel panel in selectedPanels)
            {

                var activityLabels = panel.Controls.OfType<Label>()
                                            .Where(label => label.Tag?.ToString() == "activity")
                                            .ToList();

                foreach (Label labelToRemove in activityLabels)
                {
                    panel.Controls.Remove(labelToRemove);
                }

                panel.BackColor = Color.White;
            }
            guna2CheckBox1.Checked = false;
            guna2CheckBox2.Checked = false;
            selectedPanels.Clear();
        }
    }

}
