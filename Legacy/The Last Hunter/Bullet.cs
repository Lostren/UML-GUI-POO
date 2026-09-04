using System.Drawing;

namespace Shoot_Out_Game_MOO_ICT
{
    internal class Bullet
    {

        public string direction = string.Empty;
        public int bulletLeft;
        public int bulletTop;

        private const int Speed = 20;
        private readonly PictureBox bullet = new PictureBox();
        private bool isDisposed;


        public void MakeBullet(Form form)
        {
            bullet.BackColor = Color.White;
            bullet.Size = new Size(5, 5);
            bullet.Tag = "bullet";
            bullet.Left = bulletLeft;
            bullet.Top = bulletTop;
            bullet.BringToFront();

            form.Controls.Add(bullet);
        }

        public void Move()
        {
            if (isDisposed)
            {
                return;
            }

            if (direction == "left")
            {
                bullet.Left -= Speed;
            }

            if (direction == "right")
            {
                bullet.Left += Speed;
            }

            if (direction == "up")
            {
                bullet.Top -= Speed;
            }

            if (direction == "down")
            {
                bullet.Top += Speed;
            }
        }

        public Rectangle Bounds => bullet.Bounds;

        public bool IsOutside(Rectangle area)
        {
            return bullet.Right < area.Left || bullet.Left > area.Right ||
                   bullet.Bottom < area.Top || bullet.Top > area.Bottom;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            if (bullet.Parent is not null)
            {
                bullet.Parent.Controls.Remove(bullet);
            }

            bullet.Dispose();
        }



    }
}
