
namespace SyncPractice
{
    public class MainForm : Form
    {
        public PictureBox Player { get; set; }

        public MainForm()
        {
            InitData();

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

        private void InitData()
        {
            string file = "init-data.json";

            if (File.Exists(file) == true)
            {
                using (StreamWriter sw = new StreamWriter(file))
                {

                }
            }

            using (StreamReader sr = new StreamReader(file))
            {

            }
        }

        private class InitDataJson
        {

        }
    }
}
