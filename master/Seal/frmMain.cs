using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Seal
{
    public partial class frmMain : Form
    {
        public int pix = 600;

        private Color tr_color = Color.Transparent;
        private bool b_start = false;
        bool[] b_visible = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            BeginSet();
            Setting();
            EndSet();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "选择公章";
            openFileDialog1.Filter = "Image Files(*.PNG;*.JPG;)|*.PNG;*.JPG|All files(*.*)|*.*";
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.ShowDialog();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "保存公章";
            saveFileDialog1.Filter = "png图片(*.png)|*.png";
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.FileName = openFileDialog1.FileName.ToLower().Replace(".jpg", ".png");
            saveFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = openFileDialog1.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var image = Image.FromFile(fileName);

            picSeal.Image = image;

            ConvertImage(image);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            picSeal.Image.Save(saveFileDialog1.FileName);
        }

        private void numPix_KeyUp(object sender, KeyEventArgs e)
        {
            InitForm();
        }

        public void InitForm()
        {
            pix = Convert.ToInt32(numPix.Value);
            this.Text = string.Format("公章透明处理，尺寸({0},{0})", pix);
            this.Width = pix + 40;
            this.Height = pix + 90;
            this.picSeal.Width = pix;
            this.picSeal.Height = pix;

            SetBackgroundImageTransparent();
        }

        public void ConvertImage(Image image)
        {
            Task t = new Task(() =>
            {
                Thread.Sleep(2000);

                image = KnockOutGzf(image);

                image = ChangeImgSize(image, pix, pix);

                picSeal.Image = image;
            });
            t.Start();
        }

        #region 图片加工

        public static Bitmap KnockOutGzf(Image image)
        {
            var bitmapProxy = new System.Drawing.Bitmap(image);
            image.Dispose();
            for (int i = 0; i < bitmapProxy.Width; i++)
            {
                for (int j = 0; j < bitmapProxy.Height; j++)
                {
                    System.Drawing.Color c = bitmapProxy.GetPixel(i, j);
                    if (!(c.R < 240 || c.G < 240 || c.B < 240))
                    {
                        bitmapProxy.SetPixel(i, j, System.Drawing.Color.Transparent);
                    }
                }
            }
            return bitmapProxy;
        }

        public static Bitmap ChangeImgSize(Image bit, int width, int height)
        {
            var newBitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(newBitmap);
            g.Clear(Color.Transparent);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(bit, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), 0, 0, bit.Width, bit.Height, GraphicsUnit.Pixel);
            g.Dispose();
            return newBitmap;
        }

        #endregion

        #region 设置透明背景

        private void frmMain_Resize(object sender, EventArgs e)
        {
            Setting();
        }

        private void frmMain_ResizeBegin(object sender, EventArgs e)
        {
            BeginSet();
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            EndSet();
        }

        private void frmMain_Move(object sender, EventArgs e)
        {
            Setting();
        }

        private void SetBackgroundImageTransparent()
        {
            Point pt = this.PointToScreen(this.picSeal.Location);
            var b = new Bitmap(this.picSeal.Width, this.picSeal.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(pt, new Point(), new Size(this.picSeal.Width, this.picSeal.Height));
            }

            this.picSeal.BackgroundImage = b;
        }

        private void BeginSet()
        {
            tr_color = this.TransparencyKey;
            b_start = true;
        }

        private void Setting()
        {
            if (b_start)
            {
                b_visible = new bool[Controls.Count];
                for (int i = 0; i < Controls.Count; i++)
                {
                    b_visible[i] = Controls[i].Visible;
                    Controls[i].Visible = false;
                }
                BackgroundImage = null;
                BackColor = Color.White;
                b_start = false;
                this.TransparencyKey = Color.White;
            }
        }

        private void EndSet()
        {
            SetBackgroundImageTransparent();
            this.TransparencyKey = tr_color;
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Visible = b_visible[i];
            }
            b_start = false;
        }

        #endregion
    }
}
