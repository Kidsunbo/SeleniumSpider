using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Text.RegularExpressions;

namespace SeleniumSpider
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();

            this.dataGridView1.AutoResizeColumns();

            this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            this.dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            this.dataGridView1.RowPostPaint += new DataGridViewRowPostPaintEventHandler(this.dataGridView1_RowPostPaint);

            this.dataGridView1.CellContentClick += new DataGridViewCellEventHandler(openNewWeb);

        }

        private void openNewWeb(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 3)
            {
                System.Diagnostics.Process.Start("chrome.exe", this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {

                this.textBox1.Enabled = true;
                var folderDialog = new FolderBrowserDialog();
                folderDialog.ShowDialog();
                this.textBox1.Text = folderDialog.SelectedPath;
                this.webDriverAddress = folderDialog.SelectedPath;
                this.textBox1.Enabled = false;

                new Thread(() =>
                {
                    try
                    {
                        using (var driver = new ChromeDriver(this.webDriverAddress))
                        {
                            driver.Navigate().GoToUrl(this.webAddress);
                            /*
                            var options = driver.FindElementById(id: "state").FindElements(by:By.TagName("option"));
                            foreach(var o in options)
                            {
                                Console.WriteLine(o.Text);
                            }
                            var countries = from option in options
                                            where option.GetAttribute("value") != ""
                                            select new { value = option.GetAttribute("value"), option.Text };
                            dict = countries.ToDictionary(p => p.value, p => p.Text);

                            foreach (var i in dict)
                            {
                                Console.WriteLine(i.Value);
                                this.addCountryToUI(i.Key);
                            }
                            */
                            // The following snippet of code just kicks the selenium's ass. An annoying bug.
                            var contentOfPage = driver.PageSource;
                            Regex regex = new Regex("<select id=\"state\" name=\"state\">([\\s\\S]*)</select>");
                            var tempStr = regex.Match(contentOfPage).Groups[0];
                            Regex regex2 = new Regex("<option value=\"([^>]{2})\">([^<]*)</option>");
                            var matches = regex2.Matches(tempStr.ToString());
                            foreach (Match match in matches)
                            {
                                dict[match.Groups[2].ToString()] = match.Groups[1].ToString();

                            }
                            this.addCountryToUI(dict);

                        }

                    }
                    catch (DriverServiceNotFoundException exception)
                    {
                        MessageBox.Show(exception.Message, "未找到Driver");
                        this.clearTextBox1();
                    }
                    catch (ArgumentException)
                    {
                        this.clearTextBox1();
                    }
                }).Start();

        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                dataGridView1.RowHeadersWidth - 4,
                e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView1.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            var stateCode = dict[this.comboBox2.SelectedItem.ToString()];
            var webAddressForDetail = $"https://stores.ashleyfurniture.com/Locations?country=US&lat=&lng=&miles=50&search-by=State&address=&state={stateCode}&zip-code=";
            this.Hide();
            try
            {
                using (ChromeDriver driver = new ChromeDriver(this.webDriverAddress))
                {
                    driver.Navigate().GoToUrl(webAddressForDetail);
                    var addresses = driver.FindElementsByClassName("address");
                    var postalCodes = driver.FindElementsByClassName("city-postal-code");
                    var openTimes = driver.FindElementsByClassName("open-hours");
                    var directions = driver.FindElementsByClassName("directions");
                    var phoneNums = driver.FindElementsByClassName("phone-number");

                    var max = getMin(addresses.Count, postalCodes.Count, openTimes.Count, directions.Count, phoneNums.Count);
                    //Console.WriteLine(max);
                    for (int i = 0; i < max; i++)
                    {
                        //Console.WriteLine($"{addresses[i].Text},{postalCodes[i].Text},{openTimes[i].Text},{directions[i].GetAttribute("href").ToString()},{phoneNums[i].Text}");
                        DataGridViewRow row = new DataGridViewRow();

                        DataGridViewTextBoxCell addrCell = new DataGridViewTextBoxCell();
                        addrCell.Value = addresses[i].Text;
                        row.Cells.Add(addrCell);

                        DataGridViewTextBoxCell postCell = new DataGridViewTextBoxCell();
                        postCell.Value = postalCodes[i].Text;
                        row.Cells.Add(postCell);

                        DataGridViewTextBoxCell openCell = new DataGridViewTextBoxCell();
                        openCell.Value = openTimes[i].Text;
                        row.Cells.Add(openCell);

                        DataGridViewLinkCell direCell = new DataGridViewLinkCell();
                        direCell.LinkColor = Color.Blue;
                        direCell.Value = directions[i].GetAttribute("href").ToString();

                        row.Cells.Add(direCell);

                        DataGridViewTextBoxCell phoneCell = new DataGridViewTextBoxCell();
                        phoneCell.Value = phoneNums[i].Text;
                        row.Cells.Add(phoneCell);

                        this.dataGridView1.Rows.Add(row);
                    }
                }
            }
            catch (OpenQA.Selenium.WebDriverException ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
            this.Show();
        }
        #region Function for synchronizing UI in different threads.
        private void addCountryToUI(Dictionary<string,string> dict)
        {
            if (this.comboBox2.InvokeRequired)
            {
                this.comboBox2.BeginInvoke(new Action<Dictionary<string,string>>(addCountryToUI), dict);
            }
            else
            {
                foreach(var key in dict.Keys)
                {
                    this.comboBox2.Items.Add(key);
                }
            }
        }

        private void clearTextBox1() 
        {
            if (this.textBox1.InvokeRequired)
            {
                this.textBox1.BeginInvoke(new Action(clearTextBox1));
            }
            else
            {
                this.textBox1.Clear();
                this.textBox1.Enabled = true;
            }
        }




        #endregion

    }
}
