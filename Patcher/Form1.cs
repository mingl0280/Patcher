using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using NAudio.Wave;

namespace Patcher
{
    public partial class Form1 : Form
    {
        Stream musStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Patcher.Resources.bgm.mp3");

        WaveOut wOut;
        WaveStream baStream;
        Image iPause;
        Image iPlay;
        Image iStop;
        Icon icoPrimary = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Patcher.Resources.Patcher_Icon.ico"));
        Boolean isStopBtnClicked = false;

        public Form1()
        {
            InitializeComponent();
            iPause = Image.FromStream(GetResMs("pause.png"));
            iPlay = Image.FromStream(GetResMs("play.png"));
            iStop = Image.FromStream(GetResMs("stop.png"));
            Icon = icoPrimary;
            btnStop.BackgroundImageLayout = ImageLayout.Stretch;
            btnPlay.BackgroundImageLayout = ImageLayout.Zoom;
            btnStop.BackgroundImage = iStop;
            btnPlay.BackgroundImage = iPause;
            baStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(musStream)));
            wOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            wOut.PlaybackStopped += WOut_PlaybackStopped;
            wOut.Init(baStream);
            wOut.Play();
        }

        private void WOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (isStopBtnClicked == false)
            {
                musStream.Seek(0, 0);
                wOut.Init(baStream);
                wOut.Play();
            }
            else
            {
                isStopBtnClicked = false;
            }
        }

        private MemoryStream GetResMs(string resName)
        {
            Stream st = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Patcher.Resources." + resName);
            MemoryStream ms = new MemoryStream();
            st.CopyTo(ms);
            st.Dispose();
            return ms;
        }
        string curDir = Environment.CurrentDirectory;
        string DstDir = Environment.CurrentDirectory + @"\ColdWaters_Data\StreamingAssets\default\";
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ZipFile.ExtractToDirectory("PatchData.dat", Path.GetTempPath() + @"\forValidate\");
                Directory.Delete(Path.GetTempPath() + @"\forValidate\", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("补丁已失效！错误：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (File.Exists("__MostRecentBackup.bak"))
            {
                var ret = MessageBox.Show("确定要覆盖掉最近的备份吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ret == DialogResult.No)
                {
                    return;
                }
                File.Delete("__MostRecentBackup.bak");
            }
            
            ZipFile.CreateFromDirectory(DstDir, "__MostRecentBackup.bak", CompressionLevel.Optimal, false);
            Directory.Delete(DstDir, true);
            Directory.CreateDirectory(DstDir);
            ZipFile.ExtractToDirectory("PatchData.dat", DstDir);
            MessageBox.Show("成功应用补丁文件", "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (wOut.PlaybackState == PlaybackState.Paused)
            {
                wOut.Resume();
                btnPlay.BackgroundImage = iPause;
                return;
            }
            if (wOut.PlaybackState == PlaybackState.Stopped)
            {
                musStream.Seek(0, 0);
                wOut.Init(baStream);
                wOut.Play();
                btnPlay.BackgroundImage = iPause;
                return;
            }
            if (wOut.PlaybackState == PlaybackState.Playing)
            {
                wOut.Pause();
                btnPlay.BackgroundImage = iPlay;

                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isStopBtnClicked = true;
            wOut.Stop();
            btnPlay.BackgroundImage = iPlay;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists("__MostRecentBackup.bak"))
            {
                try
                {
                    ZipFile.ExtractToDirectory("__MostRecentBackup.bak", Path.GetTempPath() + @"\forValidate\");
                    Directory.Delete(Path.GetTempPath() + @"\forValidate\", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("备份文件已失效！错误：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Directory.Delete(DstDir, true);
                ZipFile.ExtractToDirectory("__MostRecentBackup.bak", DstDir);
                MessageBox.Show("成功还原备份", "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("不存在备份文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
