using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestRedirect;

namespace QASupport.TestRedirect
{
    public partial class FormTestRedirect : Form
    {
        public FormTestRedirect()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private bool processRun = false;
        private Thread thread;

        private void showProgressTest(double totalPages, double onePercent, double step)
        {
            toolStripProgressBar1.Maximum = Convert.ToInt32(totalPages);
            toolStripProgressBar1.Value = Convert.ToInt32(step);
            double progressPercent = 0;
            if (totalPages < 100 && onePercent > 0) progressPercent = (step * onePercent);
            if (totalPages >= 100) progressPercent = (step / onePercent);
            progressPercent = Math.Round(progressPercent, 0);
            if (progressPercent < 100) toolStripStatusLabel2.Text = Convert.ToString(progressPercent) + "%";
            else toolStripStatusLabel2.Text = "99%";

            double dSec = (totalPages - step) * 1;

            int sec = Convert.ToInt32(dSec);
            int minutes = sec / 60;
            int newSec = sec - minutes * 60;
            int hour = minutes / 60;
            int newMinnutes = minutes - hour * 60;
            TimeSpan time = new TimeSpan(hour, newMinnutes, newSec);
            toolStripStatusLabel5.Text = time.ToString();
        }

        private void FormTestRedirect_Load(object sender, EventArgs e)
        {
            checkedListBox1.SetItemChecked(1, true);
            thread = new Thread(TestUrl);
        }

        private void FormTestRedirect_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { thread.Abort(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Предупреждение"); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = checkedListBox1.Items.Count;
            for (int i = 0; i < count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int count = checkedListBox1.Items.Count;
            for (int i = 0; i < count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = true;
        }

        private void checkBoxUserAgent_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUserAgent.Checked == true)
            {
                checkBoxUserAgent.Text = "Включен User-Agent по умолчанию";
                textBoxUserAgent.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
                textBoxUserAgent.Enabled = false;
            }
            else
            {
                checkBoxUserAgent.Text = "Отключен User-Agent по умолчанию";
                textBoxUserAgent.Enabled = true;
            }
        }

        private void запуститьПроверкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestBegin();
        }

        private void остановитьПроверкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestStop();
        }

        private void TestBegin()
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен", "Сообщение");
                return;
            }

            richTextBoxReport.Clear();
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel2.Text = "0%";
            toolStripStatusLabel5.Text = "0:00";

            if (radioButton1.Checked == true)
            {
                thread = new Thread(TestUrl);
                thread.Start();
            }

            if (radioButton2.Checked == true)
            {
                thread = new Thread(TestUrl2);
                thread.Start();
            }
        }

        private void TestEnd()
        {
            processRun = false;
            this.Update();
            MessageBox.Show("Процесс проверки - завершен!");
        }

        private void TestStop()
        {
            try
            {
                processRun = false;
                thread.Abort();
                MessageBox.Show("Процесс проверки - остановлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Предупреждение");
            }
        }

        private void TestUrl()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            bool response300 = checkedListBox1.GetItemChecked(0); // 300 Multiple Choices («множество выборов»);
            bool response301 = checkedListBox1.GetItemChecked(1); // 301 Moved Permanently («перемещено навсегда»);
            bool response302 = checkedListBox1.GetItemChecked(2); // 302 Moved Temporarily («перемещено временно»), Found («найдено»);
            bool response303 = checkedListBox1.GetItemChecked(3); // 303 See Other («смотреть другое»);
            bool response304 = checkedListBox1.GetItemChecked(4); // 304 Not Modified («не изменялось»);
            bool response305 = checkedListBox1.GetItemChecked(5); // 305 Use Proxy («использовать прокси»);
            bool response306 = checkedListBox1.GetItemChecked(6); // 306 Зарезервировано (код использовался только в ранних спецификациях);
            bool response307 = checkedListBox1.GetItemChecked(7); // 307 Temporary Redirect («временное перенаправление»);
            bool response308 = checkedListBox1.GetItemChecked(8); // 308 Permanent Redirect («постоянное перенаправление»).

            int count = richTextBoxFrom.Lines.Length;
            int index = 0;
            double totalPages = count;
            double onePercent = 0;
            if (totalPages < 100) onePercent = (100 / totalPages);
            else onePercent = (totalPages / 100);

            processRun = true;
            try
            {
                listView1.Items.Clear();
                ListViewItem item;
                ListViewItem.ListViewSubItem subitem;

                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;

                bool test = false;
                string url = "";
                int amount = richTextBoxFrom.Lines.Length;
                for (int i = 0; i < amount; i++)
                {
                    try
                    {
                        index++;
                        url = richTextBoxFrom.Lines.GetValue(i).ToString();

                        client = new HttpClient(handler);
                        if (checkBoxUserAgent.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBoxUserAgent.Text);
                        client.BaseAddress = new Uri(url);
                        response = client.GetAsync(url).Result;
                        int statusCode = (int)response.StatusCode;

                        string redirectedUrl = "";
                        HttpResponseHeaders headers = response.Headers;
                        if (headers.Location == null)
                        {
                            if ((int)response.StatusCode == 200)
                            {
                                item = new ListViewItem();
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = url;
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "200";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "редирект не сработал";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "FAILED";
                                item.SubItems.Add(subitem);
                                item.ImageIndex = 1;
                                listView1.Items.Add(item);
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [200] - редирект не сработал " + url + Environment.NewLine;
                            }
                            else
                            {
                                item = new ListViewItem();
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = url;
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "ошибка";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "произошла ошибка при чтении страницы (статус: " + ((int)response.StatusCode).ToString() + ")";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "ERROR";
                                item.SubItems.Add(subitem);
                                item.ImageIndex = 1;
                                listView1.Items.Add(item);
                                richTextBoxReport.Text = richTextBoxReport.Text + "ERROR - cтатус [ошибка] - ошибка на странице " + url + Environment.NewLine;
                            }
                            continue;
                        }

                        if (headers.Location.IsAbsoluteUri)
                        {
                            if (headers != null && headers.Location != null) redirectedUrl = headers.Location.AbsoluteUri;
                        }
                        else
                        {
                            if (headers != null && headers.Location != null) redirectedUrl = headers.Location.OriginalString;
                        }

                        item = new ListViewItem();
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = url; //subitem.Text = response.RequestMessage.RequestUri.ToString();
                        item.SubItems.Add(subitem);
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = statusCode.ToString();
                        item.SubItems.Add(subitem);
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = redirectedUrl;
                        item.SubItems.Add(subitem);

                        test = false;
                        if (redirectedUrl != "")
                        {
                            if (response300 == true && statusCode == 300) test = true;
                            if (response301 == true && statusCode == 301) test = true;
                            if (response302 == true && statusCode == 302) test = true;
                            if (response303 == true && statusCode == 303) test = true;
                            if (response304 == true && statusCode == 304) test = true;
                            if (response305 == true && statusCode == 305) test = true;
                            if (response306 == true && statusCode == 306) test = true;
                            if (response307 == true && statusCode == 307) test = true;
                            if (response308 == true && statusCode == 308) test = true;
                        }

                        subitem = new ListViewItem.ListViewSubItem();
                        if (test == true) subitem.Text = "PASSED";
                        else subitem.Text = "FAILED";
                        item.SubItems.Add(subitem);


                        if (test == true) item.ImageIndex = 0;
                        else item.ImageIndex = 1;
                        listView1.Items.Add(item);

                        listView1.Items[listView1.Items.Count - 1].Selected = true;
                        listView1.Items[listView1.Items.Count - 1].EnsureVisible();

                        if (checkBoxReportFaildOnly.Checked == true) // отчет
                        {
                            if (test == false)
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда: " + url + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда  : " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                        }
                        else
                        {
                            if (test == true)
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "PASSED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда: " + url + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда  : " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                            else
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда: " + url + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда  : " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                    }

                    showProgressTest(totalPages, onePercent, index);
                    toolStripStatusLabel2.Text = toolStripStatusLabel2.Text + " ["+ index + "/" + amount +"]";
                }

                toolStripStatusLabel2.Text = "100%";
                toolStripStatusLabel5.Text = "0:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }

            TestEnd();
        }

        private void TestUrl2()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            bool response300 = checkedListBox1.GetItemChecked(0); // 300 Multiple Choices («множество выборов»);
            bool response301 = checkedListBox1.GetItemChecked(1); // 301 Moved Permanently («перемещено навсегда»);
            bool response302 = checkedListBox1.GetItemChecked(2); // 302 Moved Temporarily («перемещено временно»), Found («найдено»);
            bool response303 = checkedListBox1.GetItemChecked(3); // 303 See Other («смотреть другое»);
            bool response304 = checkedListBox1.GetItemChecked(4); // 304 Not Modified («не изменялось»);
            bool response305 = checkedListBox1.GetItemChecked(5); // 305 Use Proxy («использовать прокси»);
            bool response306 = checkedListBox1.GetItemChecked(6); // 306 Зарезервировано (код использовался только в ранних спецификациях);
            bool response307 = checkedListBox1.GetItemChecked(7); // 307 Temporary Redirect («временное перенаправление»);
            bool response308 = checkedListBox1.GetItemChecked(8); // 308 Permanent Redirect («постоянное перенаправление»).

            int count = richTextBoxFrom.Lines.Length;
            int index = 0;
            double totalPages = count;
            double onePercent = 0;
            if (totalPages < 100) onePercent = (100 / totalPages);
            else onePercent = (totalPages / 100);

            processRun = true;
            try
            {
                listView1.Items.Clear();
                ListViewItem item;
                ListViewItem.ListViewSubItem subitem;

                HttpClient client;
                HttpResponseMessage response;
                HttpClientHandler handler = new HttpClientHandler();
                handler.AllowAutoRedirect = false;

                bool test = false;
                string originalUrl = "";
                string targetUrl = "";
                int amount = richTextBoxFrom.Lines.Length;
                for (int i = 0; i < amount; i++)
                {
                    try
                    {
                        index++;
                        originalUrl = richTextBoxFrom.Lines.GetValue(i).ToString();
                        targetUrl = richTextBoxTo.Lines.GetValue(i).ToString();

                        client = new HttpClient(handler);
                        if (checkBoxUserAgent.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBoxUserAgent.Text);
                        client.BaseAddress = new Uri(originalUrl);
                        response = client.GetAsync(originalUrl).Result;
                        int statusCode = (int)response.StatusCode;

                        string redirectedUrl = "";
                        HttpResponseHeaders headers = response.Headers;
                        if (headers.Location == null)
                        {
                            if ((int)response.StatusCode == 200)
                            {
                                item = new ListViewItem();
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = originalUrl;
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "200";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "редирект не сработал";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "FAILED";
                                item.SubItems.Add(subitem);
                                item.ImageIndex = 1;
                                listView1.Items.Add(item);
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [200] - редирект не сработал " + originalUrl + Environment.NewLine;
                            }
                            else
                            {
                                item = new ListViewItem();
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = originalUrl;
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "ошибка";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "произошла ошибка при чтении страницы (статус: " + ((int)response.StatusCode).ToString() + ")";
                                item.SubItems.Add(subitem);
                                subitem = new ListViewItem.ListViewSubItem();
                                subitem.Text = "ERROR";
                                item.SubItems.Add(subitem);
                                item.ImageIndex = 1;
                                listView1.Items.Add(item);
                                richTextBoxReport.Text = richTextBoxReport.Text + "ERROR - cтатус [ошибка] - ошибка на странице " + originalUrl + Environment.NewLine;
                            }
                            continue;
                        }

                        if (headers.Location.IsAbsoluteUri)
                        {
                            if (headers != null && headers.Location != null) redirectedUrl = headers.Location.AbsoluteUri;
                        }
                        else
                        {
                            if (headers != null && headers.Location != null) redirectedUrl = headers.Location.OriginalString;
                        }

                        item = new ListViewItem();
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = originalUrl; //subitem.Text = response.RequestMessage.RequestUri.ToString();
                        item.SubItems.Add(subitem);
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = statusCode.ToString();
                        item.SubItems.Add(subitem);
                        subitem = new ListViewItem.ListViewSubItem();
                        subitem.Text = redirectedUrl;
                        item.SubItems.Add(subitem);

                        test = false;
                        if (redirectedUrl == targetUrl)
                        {
                            if (response300 == true && statusCode == 300) test = true;
                            if (response301 == true && statusCode == 301) test = true;
                            if (response302 == true && statusCode == 302) test = true;
                            if (response303 == true && statusCode == 303) test = true;
                            if (response304 == true && statusCode == 304) test = true;
                            if (response305 == true && statusCode == 305) test = true;
                            if (response306 == true && statusCode == 306) test = true;
                            if (response307 == true && statusCode == 307) test = true;
                            if (response308 == true && statusCode == 308) test = true;
                        }
                        else if (redirectedUrl.Contains(targetUrl) == true)
                        {
                            if (response300 == true && statusCode == 300) test = true;
                            if (response301 == true && statusCode == 301) test = true;
                            if (response302 == true && statusCode == 302) test = true;
                            if (response303 == true && statusCode == 303) test = true;
                            if (response304 == true && statusCode == 304) test = true;
                            if (response305 == true && statusCode == 305) test = true;
                            if (response306 == true && statusCode == 306) test = true;
                            if (response307 == true && statusCode == 307) test = true;
                            if (response308 == true && statusCode == 308) test = true;
                        }

                        subitem = new ListViewItem.ListViewSubItem();
                        if (test == true) subitem.Text = "PASSED";
                        else subitem.Text = "FAILED";
                        item.SubItems.Add(subitem);


                        if (test == true) item.ImageIndex = 0;
                        else item.ImageIndex = 1;
                        listView1.Items.Add(item);

                        listView1.Items[listView1.Items.Count - 1].Selected = true;
                        listView1.Items[listView1.Items.Count - 1].EnsureVisible();

                        if (checkBoxReportFaildOnly.Checked == true) // отчет
                        {
                            if (test == false)
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда  : " + originalUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда    : " + targetUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- по факту: " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                        }
                        else
                        {
                            if (test == true)
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "PASSED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда  : " + originalUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда    : " + targetUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- по факту: " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                            else
                            {
                                richTextBoxReport.Text = richTextBoxReport.Text + "FAILED - cтатус [" + statusCode + "]" + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- откуда  : " + originalUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- куда    : " + targetUrl + Environment.NewLine;
                                richTextBoxReport.Text = richTextBoxReport.Text + "- по факту: " + redirectedUrl + Environment.NewLine + Environment.NewLine;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                    }

                    showProgressTest(totalPages, onePercent, index);
                    toolStripStatusLabel2.Text = toolStripStatusLabel2.Text + " [" + index + "/" + amount + "]";
                }

                toolStripStatusLabel2.Text = "100%";
                toolStripStatusLabel5.Text = "0:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }

            TestEnd();
        }

        private void сохранитьОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "report.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter SW = new StreamWriter(new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write));
                    SW.Write(richTextBoxReport.Text);
                    SW.Close();
                    MessageBox.Show("Отчет сохранён", "Сообщение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void загрузитьДанныеВСписокСсылокоткудаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.FileName = "";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    richTextBoxFrom.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            
        }

        private void загрузитьДанныеВСписокСсылоккудаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.FileName = "";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    richTextBoxTo.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
            
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            richTextBoxFrom.Cut();
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            richTextBoxFrom.Copy();
        }

        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            richTextBoxFrom.Paste();
        }

        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(" ");
            richTextBoxFrom.Paste();
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            richTextBoxFrom.SelectAll();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBoxTo.Cut();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            richTextBoxTo.Copy();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            richTextBoxTo.Paste();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(" ");
            richTextBoxTo.Paste();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            richTextBoxTo.SelectAll();
        }

        private void richTextBoxFrom_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void richTextBoxTo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
        }

        private void openStep()
        {
            try
            {
                if (listView1.Items.Count <= 0) return;
                FormTestRedirectResult result = new FormTestRedirectResult();
                result.textBox1.Text = listView1.SelectedItems[0].SubItems[1].Text;
                result.textBox2.Text = listView1.SelectedItems[0].SubItems[2].Text;
                result.textBox3.Text = listView1.SelectedItems[0].SubItems[3].Text;
                result.textBox4.Text = listView1.SelectedItems[0].SubItems[4].Text;
                result.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            openStep();
        }

        private void resultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openStep();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout about = new FormAbout();
            about.ShowDialog();
        }
    }
}
