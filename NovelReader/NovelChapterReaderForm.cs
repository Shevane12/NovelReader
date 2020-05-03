﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NovelReader
{
    public partial class NovelChapterReaderForm : Form
    {
        private string _link;
        private string previouschapterlink = string.Empty, nextchapterlink = string.Empty;
        private string yourfont = "Segoe UI";

        private List<string> fontNames = FontFamily.Families.Select(f => f.Name).ToList();

        int sourcesite = Properties.Settings.Default.SourceSite;

        SpeechSynthesizer speech = new SpeechSynthesizer();
        public NovelChapterReaderForm(string link)
        {
            InitializeComponent();
            _link = link;

            speech.SelectVoice("Microsoft Zira Desktop");
            speech.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(SpeechProgress);
            speech.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(SpeechStarted);
            speech.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(SpeechCompleted);
        }

        private void NovelChapterReaderForm_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }
        private NovelReaderWebScrapper.Model.ChapterTextModel LoadChapterTextData(string url)
        {
            NovelReaderWebScrapper.Model.ChapterTextModel chapterText = SourcePickerMethod.GetChapterTextModel($"{url}", (SourcePickerMethod.Scrapper)sourcesite);
            return chapterText;
        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            foreach (var f in fontNames)
                guna2ComboBox1.Items.Add(f);

            await LoadChapterData(_link);

            btnNext.Enabled = (string.IsNullOrEmpty(nextchapterlink) ? false : true);
            btnPrev.Enabled = (string.IsNullOrEmpty(previouschapterlink) ? false : true);

        }

        private async Task LoadChapterData(string url)
        {
            _link = url;
            lbllink.Text = _link;
            NovelReaderWebScrapper.Model.ChapterTextModel chapterdata = LoadChapterTextData($"{url}");

            await Task.Run(() => LoadChapterTextData(chapterdata.ChapterText,
                chapterdata.PreviousChapterLink, chapterdata.NextChapterLink));
        }

        private void LoadChapterTextData(string chaptertext, string _previouschapterlink, string _nextchapterlink)
        {
            if (this.txtChapterText.InvokeRequired)
            {
                this.txtChapterText.Invoke(new MethodInvoker(delegate ()
                {
                    txtChapterText.Text = chaptertext.Trim();
                    previouschapterlink = _previouschapterlink;
                    nextchapterlink = _nextchapterlink;
                }));
            }
            else
            {
                txtChapterText.Text = chaptertext.Trim();
                previouschapterlink = _previouschapterlink;
                nextchapterlink = _nextchapterlink;
            }
        }
        private async void btnNext_Click(object sender, EventArgs e)
        {
            speech.SpeakAsyncCancelAll();

            if (!string.IsNullOrEmpty(nextchapterlink))
                await LoadChapterData($"{nextchapterlink}");
        }

        private async void btnPrev_Click(object sender, EventArgs e)
        {
            speech.SpeakAsyncCancelAll();

            if (!string.IsNullOrEmpty(previouschapterlink))
                await LoadChapterData($"{previouschapterlink}");
        }

        #region Text Settings..
        private void pictureBox1_Click(object sender, EventArgs e) => AlignText(txtChapterText, HorizontalAlignment.Center);
        private void pictureBox3_Click(object sender, EventArgs e) => AlignText(txtChapterText, HorizontalAlignment.Right);
        private void pictureBox4_Click(object sender, EventArgs e) => AlignText(txtChapterText, HorizontalAlignment.Left);

        private static void AlignText(RichTextBox txt, HorizontalAlignment alignment)
        {
            txt.SelectAll();
            txt.SelectionAlignment = alignment;
            txt.DeselectAll();
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            txtChapterText.Font = new Font(yourfont, guna2TrackBar1.Value);
            label4.Text = $"({guna2TrackBar1.Value})";
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            yourfont = guna2ComboBox1.SelectedItem.ToString();
            txtChapterText.Font = new Font(yourfont, txtChapterText.Font.Size);
        }
        #endregion

        private void SpeechCompleted(object sender, SpeakCompletedEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            btnRead.Enabled = true;
            btnStop.Enabled = false;
            btnContinue.Enabled = false;
            txtChapterText.SelectedText = "";
            txtChapterText.DeselectAll();
        }

        private void SpeechStarted(object sender, SpeakStartedEventArgs e)
        {
            Console.WriteLine("Speech Started");
        }
        private void SpeechProgress(object sender, SpeakProgressEventArgs e)
        {
            txtChapterText.HideSelection = false;
            int textposition = e.CharacterPosition;
            txtChapterText.Find(e.Text, textposition, RichTextBoxFinds.WholeWord);
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            speech.SpeakAsyncCancelAll();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (btnContinue.Text.Equals("Pause"))
            {
                speech.Pause();
                btnContinue.Text = "Continue";
            }
            else
            {
                speech.Resume();
                btnContinue.Text = "Pause";
            }
        }

        private async void btnReload_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await LoadChapterData(_link);
            Cursor.Current = Cursors.Default;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtChapterText.SelectedText))
            {
                speech.SpeakAsync(txtChapterText.SelectedText);
                btnRead.Enabled = false;
                btnStop.Enabled = true;
                btnContinue.Enabled = true;
            }
            else
            {
                MessageBox.Show("Select text to read or (CTRL + A) to select it all");
            }
        }
    }
}
