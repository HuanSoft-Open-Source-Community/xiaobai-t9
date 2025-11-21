using System;
using System.Drawing;
using System.Windows.Forms;

public class ImageTooltipForm : Form
{
    private PictureBox pictureBox;

    public ImageTooltipForm(Image img)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.BackColor = Color.White;
        this.Padding = new Padding(1);

        pictureBox = new PictureBox
        {
            Image = img,
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill
        };

        this.Controls.Add(pictureBox);

        // 自动适配图片大小（可以调整）
        this.ClientSize = new Size(150, 240);
    }
}
