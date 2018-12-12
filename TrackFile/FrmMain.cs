using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TrackFile
{
    public partial class FrmMain : Form
    {
        private Timer _timer;
        private List<string> _lstFile;
        public FrmMain()
        {
            InitializeComponent();
            Load += Frm_Load;
        }

        protected void Frm_Load(object sender, EventArgs e)
        {
            try
            {
                _timer = new Timer
                {
                    Interval = 3*1000
                };
                _timer.Tick += TimerTrackFile;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtFileDir.Text.Trim()))
                    throw new ApplicationException("图片目录不能为空");
               // if (string.IsNullOrEmpty(txtUrl.Text.Trim()))
                 //   throw new ApplicationException("上传图片URL不能为空");
                var folderResult = Directory.Exists(txtFileDir.Text.Trim());
                if (!folderResult)
                    throw new ApplicationException("图片目录不正确");

                LoadFolderFile();
                  _timer.Start();
               // TimerTrackFile(null, null);
                StartStatus();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void LoadFolderFile()
        {
            _lstFile = new List<string>();
            var lst = Directory.GetFiles(txtFileDir.Text.Trim());
            foreach (var s in lst)
            {
                try
                {
                    if (!File.Exists(s))
                        continue;
                    _lstFile.Add(s);
                    AddLog(string.Format("当前文件：{0}",s));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            AddLog(string.Format("当前文件一共：{0}", _lstFile.Count));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            StopStatus();
        }

        private void StartStatus()
        {
            btnStart.Enabled = false;
            txtFileDir.Enabled = false;
            txtUrl.Enabled = false;
            txtDevice.Enabled = false;
            btnFile.Enabled = false;
            AddLog("开始监听");
        }
        private void StopStatus()
        {
            btnStart.Enabled = true;
            txtFileDir.Enabled = true;
            txtUrl.Enabled = true;
            txtDevice.Enabled = true;
            btnFile.Enabled = true;
            AddLog("停止监听");
        }

        private void TimerTrackFile(object sender, EventArgs e)
        {
            try
            {
                var lst = Directory.GetFiles(txtFileDir.Text.Trim());
                for (int i = lst.Length-1; i >0; i--)
                {
                    var filename = lst[i];
                    if (!File.Exists(filename))
                        continue;
                    if (_lstFile.Contains(filename))
                        continue;
                    //监听到新文件
                    TrackNewFile(filename);
                }

            }
            catch (Exception err)
            {
                AddLog(err.Message);
            }
        }

        private void TrackNewFile(string filename)
        {
            //移到子目录
            var newfileFullname = MoveNewFile(filename);
            //上传到URL
            var msg = string.Empty;
            var uploadResult=UploadNewFile(newfileFullname,out msg);
            //写日志
            var file=new FileInfo(newfileFullname);
            if (!uploadResult)
            {
                AddLog(string.Format("文件：{0} 的上传失败：{1} 并移到 {2} 子目录", file.Name, msg,file.Directory));
            }
            else
            {
                AddLog(string.Format("文件：{0} 的上传成功并移到 {1} 子目录", file.Name, file.Directory));
            }
        }

        private bool UploadNewFile(string filename,out string msg)
        {
            msg = string.Empty;
             if (string.IsNullOrEmpty(txtUrl.Text.Trim()))
                return false;

            var file = new FileInfo(filename);
            //todo
            var url = txtUrl.Text.Trim();
            var files = new string[1];
            files[0] = filename;
            var strFileName = Path.GetFileNameWithoutExtension(filename);
            var iFileName = 0;
            if (!int.TryParse(strFileName, out iFileName))
            {
                //paperId只能是整型
                msg=string.Format("{0} 文件名不是整型，上传失败", filename);
                return false;
            }
            var nvc = new NameValueCollection
            {
                {"paperId", strFileName},
                { "DealerId", txtDevice.Text.Trim()}
            };
            return UploadHelper.UploadFiles(url,files,nvc,out msg);
        }

        private string MoveNewFile(string filename)
        {
            var file = new FileInfo(filename);
            var folder = DateTime.Now.ToString("yyyy-MM-dd");
            var fileFolder=Path.Combine(file.Directory.FullName, folder);
            //检查目录是否已存在
            if (!Directory.Exists(fileFolder))
                Directory.CreateDirectory(fileFolder);

            var newfileFullname = Path.Combine(fileFolder, file.Name);
            //检查文件是否已存在
            if(File.Exists(newfileFullname))
                File.Delete(newfileFullname);

            File.Move(filename, newfileFullname);
            return newfileFullname;
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            try
            {
                var folder=new FolderBrowserDialog();
                var result=folder.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtFileDir.Text = folder.SelectedPath;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private int logNum = 0;
        private void AddLog(string log)
        {
            if (logNum == 1000)
                txtLog.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtLog.Text))
            {
                txtLog.Text =string.Format("{0} {1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"),log);
                return;
            }
            txtLog.Text = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"), log) 
                + "\r\n" + txtLog.Text;

            logNum++;
        }
    }
}
