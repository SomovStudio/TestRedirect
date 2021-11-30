using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

// Редирект - узнать URL https://gist.github.com/Delaire/3f1283dc6365d705dfd6ba24498d4993

namespace TestRedirect
{
    public partial class Form1 : Form
    {
        private bool processRun = false;
        private Thread thread;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            thread = new Thread(TestUrl);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен", "Сообщение");
                return;
            }
            richTextBox4.Clear();
            thread = new Thread(TestUrl);
            thread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TestEnd();
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@e.LinkText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox1.ReadOnly = true;
                textBox1.Text = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
            }
            else
            {
                textBox1.ReadOnly = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            richTextBox3.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (processRun == true)
            {
                MessageBox.Show("Процесс уже запущен", "Сообщение");
                return;
            }
            richTextBox4.Clear();
            thread = new Thread(TestUrl2);
            thread.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TestEnd();
            try
            {
                thread.Abort();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Ошибка");
            }
        }

        private void сохранитьОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveReport();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            saveReport();
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TestUrl()
        {
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

                int amount = richTextBox1.Lines.Length;
                for (int i = 0; i < amount; i++)
                {
                    string url = richTextBox1.Lines.GetValue(i).ToString();

                    client = new HttpClient(handler);
                    if (checkBox1.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBox1.Text);
                    client.BaseAddress = new Uri(url);
                    response = client.GetAsync(url).Result;
                    int statusCode = (int)response.StatusCode;

                    string redirectedUrl = "";
                    HttpResponseHeaders headers = response.Headers;
                    if (headers != null && headers.Location != null) redirectedUrl = headers.Location.AbsoluteUri;

                    item = new ListViewItem();
                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = url; //subitem.Text = response.RequestMessage.RequestUri.ToString();
                    item.SubItems.Add(subitem);
                    subitem = new ListViewItem.ListViewSubItem();
                    if (statusCode == 301) subitem.Text = statusCode.ToString() + " - редирект";
                    else subitem.Text = statusCode.ToString();
                    item.SubItems.Add(subitem);
                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = redirectedUrl;
                    item.SubItems.Add(subitem);

                    bool test = false;
                    if (checkBox2.Checked == true && checkBox3.Checked == true)
                    {
                        if (redirectedUrl != "" && (statusCode == 301 || statusCode == 302)) test = true;
                    }
                    else if (checkBox2.Checked == true && checkBox3.Checked == false)
                    {
                        if (redirectedUrl != "" && statusCode == 301) test = true;
                    }
                    else if (checkBox2.Checked == false && checkBox3.Checked == true)
                    {
                        if (redirectedUrl != "" && statusCode == 302) test = true;
                    }
                    else if (checkBox2.Checked == false && checkBox3.Checked == false)
                    {
                        if (redirectedUrl != "") test = true;
                    }
                    
                    subitem = new ListViewItem.ListViewSubItem();
                    if (test == true) subitem.Text = "PASSED";
                    else subitem.Text = "FAILED";
                    item.SubItems.Add(subitem);


                    if (test == true) item.ImageIndex = 0;
                    else item.ImageIndex = 1;
                    listView1.Items.Add(item);

                    if(test == true) richTextBox4.Text = richTextBox4.Text + "PASSED - cтатус [" + statusCode + "] - откуда [" + url + "] куда [" + redirectedUrl + "]" + Environment.NewLine;
                    else richTextBox4.Text = richTextBox4.Text + "FAILED - cтатус [" + statusCode + "] - откуда [" + url + "] куда [" + redirectedUrl + "]" + Environment.NewLine;
                }

            }
            catch (Exception error)
            {
                //MessageBox.Show(error.ToString());
                MessageBox.Show(error.Message.ToString(), "Ошибка");
            }
            finally
            {
                TestEnd();
                thread.Abort();
            }

            TestEnd();
            thread.Abort();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAbout();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            showAbout();
        }
        private void TestUrl2()
        {
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

                int amount = richTextBox2.Lines.Length;
                for (int i = 0; i < amount; i++)
                {
                    string originalUrl = richTextBox2.Lines.GetValue(i).ToString();
                    string targetUrl = richTextBox3.Lines.GetValue(i).ToString();


                    client = new HttpClient(handler);
                    if (checkBox1.Checked == false) client.DefaultRequestHeaders.UserAgent.ParseAdd(textBox1.Text);
                    client.BaseAddress = new Uri(originalUrl);
                    response = client.GetAsync(originalUrl).Result;
                    int statusCode = (int)response.StatusCode;

                    string redirectedUrl = "";
                    HttpResponseHeaders headers = response.Headers;
                    if (headers != null && headers.Location != null) redirectedUrl = headers.Location.AbsoluteUri;

                    item = new ListViewItem();
                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = originalUrl; //subitem.Text = response.RequestMessage.RequestUri.ToString();
                    item.SubItems.Add(subitem);
                    subitem = new ListViewItem.ListViewSubItem();
                    if (statusCode == 301) subitem.Text = statusCode.ToString() + " - редирект";
                    else subitem.Text = statusCode.ToString();
                    item.SubItems.Add(subitem);
                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = redirectedUrl;
                    item.SubItems.Add(subitem);

                    bool test = false;
                    if (checkBox2.Checked == true && checkBox3.Checked == true)
                    {
                        if (redirectedUrl == targetUrl && (statusCode == 301 || statusCode == 302)) test = true;
                    }
                    else if (checkBox2.Checked == true && checkBox3.Checked == false)
                    {
                        if (redirectedUrl == targetUrl && statusCode == 301) test = true;
                    }
                    else if (checkBox2.Checked == false && checkBox3.Checked == true)
                    {
                        if (redirectedUrl == targetUrl && statusCode == 302) test = true;
                    }
                    else if (checkBox2.Checked == false && checkBox3.Checked == false)
                    {
                        if (redirectedUrl == targetUrl) test = true;
                    }

                    subitem = new ListViewItem.ListViewSubItem();
                    if (test) subitem.Text = "PASSED";
                    else subitem.Text = "FAILED";
                    item.SubItems.Add(subitem);


                    if (test) item.ImageIndex = 0;
                    else item.ImageIndex = 1;
                    listView1.Items.Add(item);

                    if (test == true) richTextBox4.Text = richTextBox4.Text + "PASSED - cтатус [" + statusCode + "] - откуда [" + originalUrl + "] куда [" + targetUrl + "] по факту [" + redirectedUrl + "]" + Environment.NewLine;
                    else richTextBox4.Text = richTextBox4.Text + "FAILED - cтатус [" + statusCode + "] - откуда [" + originalUrl + "] куда [" + targetUrl + "] по факту [" + redirectedUrl + "]" + Environment.NewLine;
                }

            }
            catch (Exception error)
            {
                //MessageBox.Show(error.ToString());
                MessageBox.Show(error.Message.ToString(), "Ошибка");
            }
            finally
            {
                TestEnd();
                thread.Abort();
            }

            TestEnd();
            thread.Abort();
        }

        private void TestEnd()
        {
            MessageBox.Show("Процесс проверки - завершен!", "Сообщение");
            processRun = false;
        }

        private void saveReport()
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox4.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                if(File.Exists(saveFileDialog1.FileName)) MessageBox.Show("Файл успешно сохранён", "Сообщение");
                else MessageBox.Show("Неудалось сохранить файл", "Ошибка");
            }
        }

        private void showAbout()
        {
            Form2 about = new Form2();
            about.ShowDialog();
        }

        
    }
}
