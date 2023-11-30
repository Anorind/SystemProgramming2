using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemProgramming2
{
    public partial class Form1 : Form
    {
        private string filePath;
        private XORCipher cipher = new XORCipher();
        private CancellationTokenSource cts;
        public Form1()
        {
            InitializeComponent();
            textBox2.PasswordChar = '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                textBox1.Text = filePath;
            }
        }


        private async Task WriteToFileAsync(string path, string text, CancellationToken cancellationToken)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteAsync(text[i].ToString());
                    progressBar1.Value = (int)((i + 1) / (double)text.Length * 100);
                    await Task.Delay(100);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(textBox2.Text) || (!radioButton1.Checked && !radioButton2.Checked))
            {
                MessageBox.Show("Будьласка, виберіть файл, введіть пароль и виберить режим операції.");
                return;
            }

            cts = new CancellationTokenSource();
            try
            {
                
                string fileText = File.ReadAllText(filePath, Encoding.UTF8);
                string resultText;

                if (radioButton1.Checked)
                {
                    if (fileText.StartsWith("ENCRYPTED")) 
                    {
                        MessageBox.Show("Файл вже зашифрован.");
                        return;
                    }
                    resultText = "ENCRYPTED" + cipher.Encrypt(fileText, textBox2.Text) + textBox2.Text;
                }
                else
                {
                    if (!fileText.StartsWith("ENCRYPTED")) 
                    {
                        MessageBox.Show("Файл не зашифрован.");
                        return;
                    }
                    resultText = cipher.Decrypt(fileText.Substring(9), textBox2.Text);
                    if (!fileText.EndsWith(textBox2.Text)) 
                    {
                        MessageBox.Show("Пароль не вірний.");
                        return;
                    }
                    resultText = cipher.Decrypt(fileText.Substring(9, fileText.Length - 9 - textBox2.Text.Length), textBox2.Text);
                }

                await WriteToFileAsync(filePath, resultText, cts.Token);
                if (progressBar1.Value == progressBar1.Maximum)
                { MessageBox.Show("Операція успішно завершена."); }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Операція була скасована.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Виникла помилка: {ex.Message}");
            }
        }

    }
}