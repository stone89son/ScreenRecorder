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
using ScreenRecorder.Properties;
using ScreenRecorderLib;
using Timer = System.Windows.Forms.Timer;

namespace ScreenRecorder
{
    public partial class Form1 : Form
    {
        private Timer _progressTimer;
        private bool _isRecording;
        private int _secondsElapsed;
        Recorder _rec;
        private string _videoPath;
        public Form1()
        {
            InitializeComponent();
            try
            {
                _videoPath=Settings.Default.VideoPath;
                this.FormBorderStyle= FormBorderStyle.Sizable;
                this.ShowInTaskbar = false;
            }
            catch
            {

            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW
                return cp;
            }
        }

        private void Init()
        {
            try
            {
                if (Settings.Default.AutoRecord)
                {
                    timer1.Interval=Convert.ToInt32(TimeSpan.FromMinutes(Settings.Default.VideoLength).TotalMilliseconds);
                    timer1.Start();
                    //Start Record
                    StartStopRecord();
                    RemoveFileMaxStorage();
                }
            }
            catch
            {

            }
        }
        private void PauseButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_rec.Status == RecorderStatus.Paused)
                {
                    _rec.Resume();
                    return;
                }
                _rec.Pause();
            }
            catch
            {

            }
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {

        }
        private void Record()
        {
            try
            {
                if (_isRecording)
                {
                    _rec.Stop();
                    _progressTimer?.Stop();
                    _progressTimer = null;
                    _secondsElapsed = 0;
                    RecordButton.Enabled = false;
                    return;
                }
                textBoxResult.Text = "";
                UpdateProgress();


                DateTime dateTime = DateTime.Now;
                string fileName = dateTime.ToString("HH-mm-ss.fff");
                string videoPath = Path.Combine(_videoPath, dateTime.ToString("yyyy-MM-dd"), fileName + ".mp4");


                _progressTimer = new Timer();
                _progressTimer.Tick += _progressTimer_Tick;
                _progressTimer.Interval = 1000;
                _progressTimer.Start();

                if (_rec == null)
                {
                    _rec = Recorder.CreateRecorder();
                    _rec.OnRecordingComplete += Rec_OnRecordingComplete;
                    _rec.OnRecordingFailed += Rec_OnRecordingFailed;
                    _rec.OnStatusChanged += _rec_OnStatusChanged;
                    _rec.OnSnapshotSaved += _rec_OnSnapshotSaved;
                }

                _rec.Record(videoPath);

                _secondsElapsed = 0;
                _isRecording = true;
            }
            catch
            {

            }
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            try
            {
                BeginInvoke(((Action)(() =>
                {
                    string filePath = e.FilePath;

                    textBoxResult.Text = filePath;
                    PauseButton.Visible = false;
                    RecordButton.Text = "Record";
                    RecordButton.Enabled = true;
                    this.labelStatus.Text = "Completed";
                    _isRecording = false;
                    CleanupResources();
                    StartStopRecord();
                })));
            }
            catch
            {

            }
        }

        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            try
            {
                BeginInvoke(((Action)(() =>
                {
                    PauseButton.Visible = false;
                    RecordButton.Text = "Record";
                    RecordButton.Enabled = true;
                    labelStatus.Text = "Error:";
                    labelError.Visible = true;
                    labelError.Text = e.Error;
                    _isRecording = false;
                    CleanupResources();
                    StartStopRecord();
                })));
            }
            catch
            {

            }
        }

        private void _rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            try
            {
                BeginInvoke(((Action)(() =>
                {
                    labelError.Visible = false;
                    switch (e.Status)
                    {
                        case RecorderStatus.Idle:
                            this.labelStatus.Text = "Idle";
                            break;
                        case RecorderStatus.Recording:
                            PauseButton.Visible = true;
                            if (_progressTimer != null)
                                _progressTimer.Enabled = true;
                            RecordButton.Text = "Stop";
                            PauseButton.Text = "Pause";
                            this.labelStatus.Text = "Recording";
                            break;
                        case RecorderStatus.Paused:
                            if (_progressTimer != null)
                                _progressTimer.Enabled = false;
                            PauseButton.Text = "Resume";
                            this.labelStatus.Text = "Paused";
                            break;
                        case RecorderStatus.Finishing:
                            PauseButton.Visible = false;
                            this.labelStatus.Text = "Finalizing video";
                            break;
                        default:
                            break;
                    }
                })));
            }
            catch
            {

            }
        }

        private void _rec_OnSnapshotSaved(object sender, SnapshotSavedEventArgs e)
        {

        }

        private void _progressTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _secondsElapsed++;
                UpdateProgress();
            }
            catch
            {

            }
        }

        private void UpdateProgress()
        {
            labelTimestamp.Text = TimeSpan.FromSeconds(_secondsElapsed).ToString();
        }

        private void CleanupResources()
        {
            try
            {
                _progressTimer?.Stop();
                _progressTimer = null;
                _secondsElapsed = 0;
                _rec?.Dispose();
                _rec = null;
            }
            catch
            {

            }
        }

        private void buttonOpenDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                string directory = _videoPath;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                System.Diagnostics.Process.Start(directory);
            }
            catch
            {

            }
        }
        private void buttonDeleteRecordedVideos_Click(object sender, EventArgs e)
        {
            try
            {
                Directory.Delete(_videoPath, true);
                MessageBox.Show("Temp files deleted");
            }
            catch (Exception ex)
            {

            }
        }

        private void buttonCopyPath_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxResult.Text))
                {
                    Clipboard.SetText(textBoxResult.Text);
                }
            }
            catch
            {

            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                Init();
            }
            catch
            {

            }
            //this.Visible=false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                StartStopRecord();
            }
            catch
            {

            }
        }

        private void StartStopRecord()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                if (_isRecording)
                {
                    RemoveFileMaxStorage();
                    //Stop recording
                    Record();
                }
                else
                {
                    DayOfWeek dayOfWeek = currentDate.DayOfWeek;
                    switch (dayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            break;
                        default:
                            //Start record
                            int hour = currentDate.Hour;
                            if (hour>=8&&hour<=20)
                            {
                                Record();

                            }
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void RemoveFileMaxStorage()
        {
            try
            {
                double maxStorageGB = Settings.Default.MaxStorageGB;
                DirectoryInfo info = new DirectoryInfo(_videoPath);
                FileInfo[] files = info.GetFiles("*", SearchOption.AllDirectories);
                long currentVolume = files.Sum(t => t.Length);
                double currentStoreageGB = currentVolume>>30;
                if (currentStoreageGB>=maxStorageGB)
                {
                    FileInfo fileDelete = files.OrderBy(p => p.CreationTime).FirstOrDefault();
                    if (fileDelete!=null)
                    {
                        File.Delete(fileDelete.FullName);
                    }
                    RemoveFileMaxStorage();
                }
            }
            catch
            {

            }
        }
        private bool _isFormClose = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_isRecording)
                {
                    _isFormClose=true;
                    StartStopRecord();
                }
            }
            catch
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Location=new Point(Settings.Default.LocationX, Settings.Default.LocationY);
            }
            catch
            {

            }
        }
    }
}
