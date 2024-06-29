namespace SyncPractice
{
    public class MainForm : Form
    {
        public PictureBox Player { get; set; }

        public MainForm()
        {
            ClientSize = new Size(800, 600);

            int size = 50;
            int x = ClientSize.Width / 2 - size / 2;
            int y = ClientSize.Height / 2 - size / 2;

            Player = new PictureBox()
            {
                Parent = this,
                Visible = true,
                Location = new Point(x, y),
                Size = new Size(size, size),
                BackColor = Color.Black,
            };
        }
    }
}
