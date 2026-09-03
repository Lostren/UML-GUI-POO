using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Shoot_Out_Game_MOO_ICT
{
    internal enum WeaponType
    {
        Empty,
        Knife,
        Pistol
    }

    public partial class Form1 : Form
    {
        bool goLeft, goRight, goUp, goDown, gameOver;
        string facing = "up";
        double playerHealth = 100;
        int speed = 10;
        int ammo = 10;
        int zombieSpeed = 3;
        Random randNum = new Random();
        int score;
        static int normalBestScore;
        static int survivalBestScore;
        int playerImmunityRemainingMs;
        int difficultyElapsedMs;
        int targetZombieCount = 3;
        int knifeEffectRemainingMs;
        int ammoDropRemainingMs;
        int healthDropRemainingMs;
        int contactDamageRemainingMs;
        bool canTakeContactDamage = true;
        bool isPaused;
        readonly GameMode gameMode;

        List<PictureBox> zombiesList = new List<PictureBox>();
        Dictionary<PictureBox, int> zombieKnifeHits = new Dictionary<PictureBox, int>();
        List<Bullet> bulletsList = new List<Bullet>();
        List<PictureBox> ammoList = new List<PictureBox>();
        List<PictureBox> healthList = new List<PictureBox>();

        readonly Panel pausePanel = new Panel();
        readonly Panel deathPanel = new Panel();
        readonly Panel weaponPanel = new Panel();
        readonly Panel knifeHitEffect = new Panel();
        readonly Label deathScoreLabel = new Label();

        const int CollisionCellSize = 128;
        readonly Dictionary<(int X, int Y), List<PictureBox>> zombieGrid = new();
        readonly Dictionary<(int X, int Y), PictureBox> occupiedZombieCells = new();

        readonly WeaponType[] weaponSlots =
        {
            WeaponType.Knife,
            WeaponType.Pistol,
            WeaponType.Empty,
            WeaponType.Empty,
            WeaponType.Empty,
            WeaponType.Empty,
            WeaponType.Empty
        };

        WeaponType selectedWeapon = WeaponType.Knife;
        readonly Button[] weaponButtons = new Button[7];

        // =========================================================
        // SOLUÇÃO DO ERRO 1: CACHE DE IMAGENS
        // As imagens são carregadas na memória apenas 1 vez aqui
        // =========================================================
        private readonly Image imgPlayerUp = Properties.Resources.up;
        private readonly Image imgPlayerDown = Properties.Resources.down;
        private readonly Image imgPlayerLeft = Properties.Resources.left;
        private readonly Image imgPlayerRight = Properties.Resources.right;
        private readonly Image imgPlayerDead = Properties.Resources.dead;

        private readonly Image imgZombieUp = Properties.Resources.zup;
        private readonly Image imgZombieDown = Properties.Resources.zdown;
        private readonly Image imgZombieLeft = Properties.Resources.zleft;
        private readonly Image imgZombieRight = Properties.Resources.zright;

        private readonly Image imgAmmo = Properties.Resources.ammo_Image;
        private readonly Image imgHealth = Properties.Resources.icone_vida;
        // =========================================================

        public Form1(GameMode gameMode)
        {
            InitializeComponent();
            this.gameMode = gameMode;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            FormClosed += FormClosedEvent;

            CreateWeaponPanel();
            CreateKnifeHitEffect();
            CreatePausePanel();
            CreateDeathPanel();

            RestartGame();
        }

        private void MainTimerEvent(object sender, EventArgs e)
        {
            if (isPaused)
            {
                return;
            }

            playerImmunityRemainingMs = Math.Max(0, playerImmunityRemainingMs - GameTimer.Interval);
            UpdateCentralTimers();

            if (playerHealth <= 0)
            {
                gameOver = true;
                // USANDO A IMAGEM EM CACHE
                player.Image = imgPlayerDead;
                GameTimer.Stop();
                ShowDeathMenu();
                return;
            }

            healthBar.Value = Math.Clamp((int)Math.Ceiling(playerHealth), healthBar.Minimum, healthBar.Maximum);

            txtAmmo.Text = "Ammo: " + ammo;
            txtScore.Text = "Kills: " + score;

            int difficultyIntervalMs = gameMode == GameMode.Survival ? 15000 : 30000;
            difficultyElapsedMs += GameTimer.Interval;

            while (difficultyElapsedMs >= difficultyIntervalMs)
            {
                difficultyElapsedMs -= difficultyIntervalMs;
                targetZombieCount++;
            }

            while (zombiesList.Count < targetZombieCount)
            {
                int zombieCount = zombiesList.Count;
                MakeZombies();

                if (zombiesList.Count == zombieCount)
                {
                    break;
                }
            }

            MovePlayer();
            UpdateBullets();

            for (int ammoIndex = ammoList.Count - 1; ammoIndex >= 0; ammoIndex--)
            {
                PictureBox ammoBox = ammoList[ammoIndex];

                if (player.Bounds.IntersectsWith(ammoBox.Bounds))
                {
                    RemoveAmmoAt(ammoIndex);
                    ammo += 5;
                }
            }

            for (int healthIndex = healthList.Count - 1; healthIndex >= 0; healthIndex--)
            {
                PictureBox healthBox = healthList[healthIndex];

                if (player.Bounds.IntersectsWith(healthBox.Bounds))
                {
                    RemoveHealthAt(healthIndex);
                    double healthRecovery = gameMode == GameMode.Survival ? 5 : 10;
                    playerHealth = Math.Min(100, playerHealth + healthRecovery);
                }
            }

            for (int zombieIndex = zombiesList.Count - 1; zombieIndex >= 0; zombieIndex--)
            {
                PictureBox zombie = zombiesList[zombieIndex];

                MoveZombie(zombie);
            }

            BuildZombieGrid();

            for (int bulletIndex = bulletsList.Count - 1; bulletIndex >= 0; bulletIndex--)
            {
                Bullet bullet = bulletsList[bulletIndex];
                (int X, int Y) bulletCell = GetCell(bullet.Bounds);

                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        if (!zombieGrid.TryGetValue((bulletCell.X + offsetX, bulletCell.Y + offsetY), out List<PictureBox>? candidates))
                        {
                            continue;
                        }

                        for (int candidateIndex = candidates.Count - 1; candidateIndex >= 0; candidateIndex--)
                        {
                            PictureBox zombie = candidates[candidateIndex];
                            if (!zombiesList.Contains(zombie) || !zombie.Bounds.IntersectsWith(bullet.Bounds))
                            {
                                continue;
                            }

                            RemoveBulletAt(bulletIndex);
                            RemoveZombie(zombie);
                            score++;
                            UpdateBestScore();
                            MakeZombies();
                            offsetX = 2;
                            break;
                        }

                        if (bulletIndex >= bulletsList.Count || bulletsList[bulletIndex] != bullet)
                        {
                            break;
                        }
                    }

                    if (bulletIndex >= bulletsList.Count || bulletsList[bulletIndex] != bullet)
                    {
                        break;
                    }
                }
            }
        }

        private void BuildZombieGrid()
        {
            zombieGrid.Clear();

            foreach (PictureBox zombie in zombiesList)
            {
                int firstCellX = zombie.Left / CollisionCellSize;
                int lastCellX = zombie.Right / CollisionCellSize;
                int firstCellY = zombie.Top / CollisionCellSize;
                int lastCellY = zombie.Bottom / CollisionCellSize;

                for (int cellX = firstCellX; cellX <= lastCellX; cellX++)
                {
                    for (int cellY = firstCellY; cellY <= lastCellY; cellY++)
                    {
                        if (!zombieGrid.TryGetValue((cellX, cellY), out List<PictureBox>? cell))
                        {
                            cell = new List<PictureBox>();
                            zombieGrid[(cellX, cellY)] = cell;
                        }

                        cell.Add(zombie);
                    }
                }
            }
        }

        private void UpdateCentralTimers()
        {
            if (knifeEffectRemainingMs > 0)
            {
                knifeEffectRemainingMs = Math.Max(0, knifeEffectRemainingMs - GameTimer.Interval);

                if (knifeEffectRemainingMs == 0)
                {
                    knifeHitEffect.Visible = false;
                }
            }

            if (!canTakeContactDamage)
            {
                contactDamageRemainingMs = Math.Max(0, contactDamageRemainingMs - GameTimer.Interval);

                if (contactDamageRemainingMs == 0)
                {
                    canTakeContactDamage = true;
                }
            }

            ammoDropRemainingMs -= GameTimer.Interval;

            healthDropRemainingMs -= GameTimer.Interval;

            if (ammoDropRemainingMs <= 0)
            {
                DropAmmo();
                ScheduleNextAmmoDrop();
            }

            if (healthDropRemainingMs <= 0)
            {
                DropHealth();
                ScheduleNextHealthDrop();
            }
        }

        private static (int X, int Y) GetCell(Rectangle bounds)
        {
            return ((bounds.Left + bounds.Width / 2) / CollisionCellSize,
                (bounds.Top + bounds.Height / 2) / CollisionCellSize);
        }

        private void RemoveBulletAt(int index)
        {
            Bullet bullet = bulletsList[index];
            bulletsList.RemoveAt(index);
            bullet.Dispose();
        }

        private void RemoveZombieAt(int index)
        {
            PictureBox zombie = zombiesList[index];
            RemoveZombie(zombie);
        }

        private void RemoveZombie(PictureBox zombie)
        {
            (int X, int Y) occupiedCell = GetCell(zombie.Bounds);

            if (occupiedZombieCells.TryGetValue(occupiedCell, out PictureBox? occupant) && occupant == zombie)
            {
                occupiedZombieCells.Remove(occupiedCell);
            }

            zombiesList.Remove(zombie);
            zombieKnifeHits.Remove(zombie);
            Controls.Remove(zombie);
            zombie.Dispose();
        }

        private void RemoveAmmoAt(int index)
        {
            PictureBox ammoBox = ammoList[index];
            ammoList.RemoveAt(index);
            Controls.Remove(ammoBox);
            ammoBox.Dispose();
        }

        private void RemoveHealthAt(int index)
        {
            PictureBox healthBox = healthList[index];
            healthList.RemoveAt(index);
            Controls.Remove(healthBox);
            healthBox.Dispose();
        }

        private void MovePlayer()
        {
            int horizontalMovement = goLeft ? -speed : goRight ? speed : 0;
            int verticalMovement = goUp ? -speed : goDown ? speed : 0;
            Point nextLocation = new Point(player.Left + horizontalMovement, player.Top + verticalMovement);
            Rectangle nextBounds = new Rectangle(nextLocation, player.Size);

            if (nextBounds.Left < 0 || nextBounds.Right > ClientSize.Width ||
                nextBounds.Top < 45 || nextBounds.Bottom > ClientSize.Height - weaponPanel.Height)
            {
                return;
            }

            if (zombiesList.Any(zombie => nextBounds.IntersectsWith(zombie.Bounds)))
            {
                return;
            }

            player.Location = nextLocation;
        }

        private void MoveZombie(PictureBox zombie)
        {
            int horizontalMovement = zombie.Left > player.Left ? -zombieSpeed : zombie.Left < player.Left ? zombieSpeed : 0;
            int verticalMovement = zombie.Top > player.Top ? -zombieSpeed : zombie.Top < player.Top ? zombieSpeed : 0;
            Point nextLocation = new Point(zombie.Left + horizontalMovement, zombie.Top + verticalMovement);
            Rectangle nextBounds = new Rectangle(nextLocation, zombie.Size);
            (int X, int Y) currentCell = GetCell(zombie.Bounds);
            (int X, int Y) nextCell = GetCell(nextBounds);

            bool isTouchingPlayer = zombie.Bounds.IntersectsWith(player.Bounds) ||
                nextBounds.IntersectsWith(player.Bounds) || IsPlayerCell(nextCell);

            if (isTouchingPlayer && playerImmunityRemainingMs == 0 && canTakeContactDamage)
            {
                playerHealth -= 5;
                canTakeContactDamage = false;
                contactDamageRemainingMs = 500;
            }

            if (isTouchingPlayer)
            {
                return;
            }

            if (horizontalMovement < 0)
            {
                zombie.Image = imgZombieLeft;
            }
            else if (horizontalMovement > 0)
            {
                zombie.Image = imgZombieRight;
            }
            else if (verticalMovement < 0)
            {
                zombie.Image = imgZombieUp;
            }
            else if (verticalMovement > 0)
            {
                zombie.Image = imgZombieDown;
            }

            occupiedZombieCells.Remove(currentCell);

            Point movementLocation = nextLocation;
            bool canMove = CanMoveZombie(zombie, movementLocation, out nextCell);

            if (!canMove && horizontalMovement != 0)
            {
                movementLocation = new Point(zombie.Left, zombie.Top + zombieSpeed);
                canMove = CanMoveZombie(zombie, movementLocation, out nextCell);

                if (!canMove)
                {
                    movementLocation = new Point(zombie.Left, zombie.Top - zombieSpeed);
                    canMove = CanMoveZombie(zombie, movementLocation, out nextCell);
                }
            }

            if (!canMove && verticalMovement != 0)
            {
                movementLocation = new Point(zombie.Left + zombieSpeed, zombie.Top);
                canMove = CanMoveZombie(zombie, movementLocation, out nextCell);

                if (!canMove)
                {
                    movementLocation = new Point(zombie.Left - zombieSpeed, zombie.Top);
                    canMove = CanMoveZombie(zombie, movementLocation, out nextCell);
                }
            }

            if (canMove)
            {
                zombie.Location = movementLocation;
                occupiedZombieCells[nextCell] = zombie;
            }
            else
            {
                occupiedZombieCells[currentCell] = zombie;
            }
        }

        private bool CanMoveZombie(PictureBox zombie, Point location, out (int X, int Y) cell)
        {
            Rectangle bounds = new Rectangle(location, zombie.Size);
            cell = GetCell(bounds);

            return bounds.Left >= 0 && bounds.Right <= ClientSize.Width &&
                bounds.Top >= 45 && bounds.Bottom <= ClientSize.Height - weaponPanel.Height &&
                !IsPlayerCell(cell) &&
                !bounds.IntersectsWith(player.Bounds) &&
                (!occupiedZombieCells.TryGetValue(cell, out PictureBox? occupant) || occupant == zombie);
        }

        private void BuildOccupiedZombieCells()
        {
            occupiedZombieCells.Clear();
            zombieGrid.Clear();

            foreach (PictureBox zombie in zombiesList)
            {
                occupiedZombieCells[GetCell(zombie.Bounds)] = zombie;
            }
        }

        private bool IsZombieCell((int X, int Y) cell)
        {
            return occupiedZombieCells.ContainsKey(cell);
        }

        private bool IsPlayerCell((int X, int Y) cell)
        {
            return GetCell(player.Bounds) == cell;
        }

        private void UpdateBullets()
        {
            Rectangle gameArea = new Rectangle(Point.Empty, ClientSize);

            for (int index = bulletsList.Count - 1; index >= 0; index--)
            {
                Bullet bullet = bulletsList[index];
                bullet.Move();

                if (bullet.IsOutside(gameArea))
                {
                    RemoveBulletAt(index);
                }
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                TogglePause();
                return;
            }

            int weaponSlot = GetWeaponSlot(e.KeyCode);
            if (weaponSlot >= 0)
            {
                SelectWeapon(weaponSlot);
                return;
            }

            if (gameOver == true)
            {
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                goLeft = true;
                facing = "left";
                player.Image = imgPlayerLeft; // USANDO CACHE
            }

            if (e.KeyCode == Keys.Right)
            {
                goRight = true;
                facing = "right";
                player.Image = imgPlayerRight; // USANDO CACHE
            }

            if (e.KeyCode == Keys.Up)
            {
                goUp = true;
                facing = "up";
                player.Image = imgPlayerUp; // USANDO CACHE
            }

            if (e.KeyCode == Keys.Down)
            {
                goDown = true;
                facing = "down";
                player.Image = imgPlayerDown; // USANDO CACHE
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                goLeft = false;
            }

            if (e.KeyCode == Keys.Right)
            {
                goRight = false;
            }

            if (e.KeyCode == Keys.Up)
            {
                goUp = false;
            }

            if (e.KeyCode == Keys.Down)
            {
                goDown = false;
            }

            if (e.KeyCode == Keys.Space && gameOver == false && isPaused == false)
            {
                AttackWithSelectedWeapon();
            }

            if (e.KeyCode == Keys.Enter && gameOver == true)
            {
                RestartGame();
            }
        }

        private void ShootBullet(string direction)
        {
            Bullet shootBullet = new Bullet();
            shootBullet.direction = direction;
            shootBullet.bulletLeft = player.Left + (player.Width / 2);
            shootBullet.bulletTop = player.Top + (player.Height / 2);
            bulletsList.Add(shootBullet);
            shootBullet.MakeBullet(this);
        }

        private void AttackWithSelectedWeapon()
        {
            if (selectedWeapon == WeaponType.Pistol)
            {
                if (ammo <= 0)
                {
                    return;
                }

                ammo--;
                ShootBullet(facing);
                return;
            }

            if (selectedWeapon == WeaponType.Knife)
            {
                ShowKnifeAttack();
                Rectangle attackBounds = GetKnifeAttackBounds();

                for (int zombieIndex = zombiesList.Count - 1; zombieIndex >= 0; zombieIndex--)
                {
                    PictureBox zombie = zombiesList[zombieIndex];

                    if (!attackBounds.IntersectsWith(zombie.Bounds))
                    {
                        continue;
                    }

                    zombieKnifeHits[zombie]--;

                    if (zombieKnifeHits[zombie] <= 0)
                    {
                        score++;
                        UpdateBestScore();
                        RemoveZombieAt(zombieIndex);
                        MakeZombies();
                    }
                }
            }
        }

        private Rectangle GetKnifeAttackBounds()
        {
            int attackWidth = player.Width / 2;
            int attackHeight = player.Height / 2;

            return facing switch
            {
                "left" => new Rectangle(
                    player.Left - attackWidth,
                    player.Top + (player.Height - attackHeight) / 2,
                    attackWidth,
                    attackHeight),
                "right" => new Rectangle(
                    player.Right,
                    player.Top + (player.Height - attackHeight) / 2,
                    attackWidth,
                    attackHeight),
                "down" => new Rectangle(
                    player.Left + (player.Width - attackWidth) / 2,
                    player.Bottom,
                    attackWidth,
                    attackHeight),
                _ => new Rectangle(
                    player.Left + (player.Width - attackWidth) / 2,
                    player.Top - attackHeight,
                    attackWidth,
                    attackHeight)
            };
        }

        private void CreateKnifeHitEffect()
        {
            knifeHitEffect.Visible = false;
            knifeHitEffect.BackColor = Color.OrangeRed;
            knifeHitEffect.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(knifeHitEffect);
        }

        private void ShowKnifeAttack()
        {
            knifeHitEffect.Bounds = GetKnifeAttackBounds();
            knifeHitEffect.Visible = true;
            knifeHitEffect.BringToFront();

            knifeEffectRemainingMs = 120;
        }

        private void MakeZombies()
        {
            Size zombieSize = imgZombieDown.Size;
            int maxLeft = Math.Max(0, ClientSize.Width - zombieSize.Width);
            int maxTop = Math.Max(45, ClientSize.Height - weaponPanel.Height - zombieSize.Height);
            Point spawnLocation = Point.Empty;
            bool validLocationFound = false;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                Point candidate = new Point(
                    randNum.Next(0, maxLeft + 1),
                    randNum.Next(45, maxTop + 1));
                (int X, int Y) cell = GetCell(new Rectangle(candidate, zombieSize));

                if (IsPlayerCell(cell) || occupiedZombieCells.ContainsKey(cell))
                {
                    continue;
                }

                spawnLocation = candidate;
                validLocationFound = true;
                break;
            }

            if (!validLocationFound)
            {
                return;
            }

            PictureBox zombie = new PictureBox();
            zombie.Tag = "zombie";
            // USANDO A IMAGEM EM CACHE
            zombie.Image = imgZombieDown;
            zombie.SizeMode = PictureBoxSizeMode.AutoSize;
            zombie.Location = spawnLocation;

            zombiesList.Add(zombie);
            zombieKnifeHits[zombie] = 3;
            occupiedZombieCells[GetCell(zombie.Bounds)] = zombie;

            this.Controls.Add(zombie);
            player.BringToFront();
        }

        private void ScheduleNextHealthDrop()
        {
            healthDropRemainingMs = randNum.Next(15000, 30001);
        }

        private void DropHealth()
        {
            PictureBox health = new PictureBox
            {
                Image = imgHealth,
                Size = imgAmmo.Size,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Tag = "health"
            };

            int maxLeft = Math.Max(10, ClientSize.Width - health.Width - 10);
            int maxTop = Math.Max(60, ClientSize.Height - weaponPanel.Height - health.Height);
            health.Location = new Point(
                randNum.Next(10, maxLeft + 1),
                randNum.Next(60, maxTop + 1));

            healthList.Add(health);
            Controls.Add(health);
            health.BringToFront();
            player.BringToFront();
        }

        private void ScheduleNextAmmoDrop()
        {
            ammoDropRemainingMs = randNum.Next(10000, 30001);
        }

        private void DropAmmo()
        {
            PictureBox ammo = new PictureBox();
            // USANDO A IMAGEM EM CACHE
            ammo.Image = imgAmmo;
            ammo.SizeMode = PictureBoxSizeMode.AutoSize;
            ammo.Left = randNum.Next(10, this.ClientSize.Width - ammo.Width);
            ammo.Top = randNum.Next(60, this.ClientSize.Height - weaponPanel.Height - ammo.Height + 1);
            ammo.Tag = "ammo";
            ammoList.Add(ammo);
            this.Controls.Add(ammo);
            ammo.BringToFront();
            player.BringToFront();
        }

        private void RestartGame()
        {
            UpdateBestScore();

            // USANDO A IMAGEM EM CACHE
            player.Image = imgPlayerUp;
            player.Location = new Point(
                (ClientSize.Width - player.Width) / 2,
                ClientSize.Height - weaponPanel.Height - player.Height - 20);

            pausePanel.Visible = false;
            deathPanel.Visible = false;
            pausePanel.Enabled = false;
            deathPanel.Enabled = false;
            isPaused = false;
            knifeHitEffect.Visible = false;
            knifeEffectRemainingMs = 0;
            contactDamageRemainingMs = 0;
            ammoDropRemainingMs = 0;
            healthDropRemainingMs = 0;
            playerImmunityRemainingMs = 1500;
            canTakeContactDamage = true;

            for (int zombieIndex = zombiesList.Count - 1; zombieIndex >= 0; zombieIndex--)
            {
                RemoveZombieAt(zombieIndex);
            }

            for (int healthIndex = healthList.Count - 1; healthIndex >= 0; healthIndex--)
            {
                RemoveHealthAt(healthIndex);
            }

            occupiedZombieCells.Clear();

            for (int ammoIndex = ammoList.Count - 1; ammoIndex >= 0; ammoIndex--)
            {
                RemoveAmmoAt(ammoIndex);
            }

            for (int bulletIndex = bulletsList.Count - 1; bulletIndex >= 0; bulletIndex--)
            {
                RemoveBulletAt(bulletIndex);
            }

            for (int i = 0; i < 3; i++)
            {
                MakeZombies();
            }

            goUp = false;
            goDown = false;
            goLeft = false;
            goRight = false;
            gameOver = false;
            difficultyElapsedMs = 0;
            targetZombieCount = 3;
            facing = "up";
            selectedWeapon = WeaponType.Pistol;

            UpdateWeaponPanel();

            playerHealth = 100;
            healthBar.Value = healthBar.Maximum;
            score = 0;
            ammo = 10;

            ScheduleNextAmmoDrop();
            ScheduleNextHealthDrop();
            GameTimer.Start();

            ActiveControl = null;
            Focus();
        }

        private void CreatePausePanel()
        {
            pausePanel.Dock = DockStyle.Fill;
            pausePanel.BringToFront();
            pausePanel.BackColor = Color.FromArgb(230, 35, 35, 35);
            pausePanel.Visible = false;
            pausePanel.Enabled = false;

            Label title = new Label
            {
                Text = "JOGO PAUSADO",
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold)
            };

            deathScoreLabel.TextAlign = ContentAlignment.MiddleCenter;
            deathScoreLabel.Dock = DockStyle.Top;
            deathScoreLabel.Height = 80;
            deathScoreLabel.ForeColor = Color.White;
            deathScoreLabel.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);

            TableLayoutPanel buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(300, 120, 300, 150),
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            buttons.Controls.Add(CreatePauseButton("Continuar", ContinueGame), 0, 0);
            buttons.Controls.Add(CreatePauseButton("Reiniciar o Jogo", RestartGameFromMenu), 0, 1);
            buttons.Controls.Add(CreatePauseButton("Sair", ExitApplication), 0, 2);

            pausePanel.Controls.Add(buttons);
            pausePanel.Controls.Add(title);

            Controls.Add(pausePanel);
            pausePanel.BringToFront();
        }

        private void CreateDeathPanel()
        {
            deathPanel.Dock = DockStyle.Fill;
            deathPanel.BackColor = Color.FromArgb(230, 35, 35, 35);
            deathPanel.Visible = false;
            deathPanel.Enabled = false;

            Label title = new Label
            {
                Text = "VOCÊ MORREU",
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold)
            };

            TableLayoutPanel buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(280, 100, 280, 130),
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            buttons.Controls.Add(CreatePauseButton("Reiniciar o Jogo", RestartGameFromMenu), 0, 0);
            buttons.Controls.Add(CreatePauseButton("Voltar ao Menu Inicial", ReturnToMainMenu), 0, 1);
            buttons.Controls.Add(CreatePauseButton("Sair", ExitApplication), 0, 2);

            deathPanel.Controls.Add(buttons);
            deathPanel.Controls.Add(deathScoreLabel);
            deathPanel.Controls.Add(title);

            Controls.Add(deathPanel);
            deathPanel.BringToFront();
        }

        private void ShowDeathMenu()
        {
            UpdateDeathMenuScore();
            pausePanel.Visible = false;
            pausePanel.Enabled = false;

            deathPanel.Visible = true;
            deathPanel.Enabled = true;
            deathPanel.BringToFront();
        }

        private void UpdateDeathMenuScore()
        {
            string modeName = gameMode == GameMode.Normal ? "Normal" : "Sobrevivência";
            deathScoreLabel.Text = $"Modo: {modeName}\nKills na sessão: {score}\nMelhor score no modo: {GetBestScore()}";
        }

        private int GetBestScore()
        {
            return gameMode == GameMode.Normal ? normalBestScore : survivalBestScore;
        }

        private void UpdateBestScore()
        {
            if (gameMode == GameMode.Normal)
            {
                normalBestScore = Math.Max(normalBestScore, score);
            }
            else
            {
                survivalBestScore = Math.Max(survivalBestScore, score);
            }
        }

        private void CreateWeaponPanel()
        {
            weaponPanel.Dock = DockStyle.Bottom;
            weaponPanel.Height = 70;
            weaponPanel.BackColor = Color.FromArgb(45, 45, 45);

            TableLayoutPanel slots = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = weaponSlots.Length,
                RowCount = 1,
                Padding = new Padding(4)
            };

            for (int index = 0; index < weaponSlots.Length; index++)
            {
                int slot = index;
                Button button = new MenuButton
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(3),
                    Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                    TabStop = false,
                    UseVisualStyleBackColor = true
                };

                button.Click += (_, _) => SelectWeapon(slot);
                weaponButtons[index] = button;
                slots.Controls.Add(button, index, 0);
            }

            weaponPanel.Controls.Add(slots);
            Controls.Add(weaponPanel);

            UpdateWeaponPanel();
        }

        private void UpdateWeaponPanel()
        {
            for (int index = 0; index < weaponButtons.Length; index++)
            {
                WeaponType weapon = weaponSlots[index];

                weaponButtons[index].Text = weapon switch
                {
                    WeaponType.Knife => $"{index + 1}\nFaca",
                    WeaponType.Pistol => $"{index + 1}\nPistola",
                    _ => $"{index + 1}\nVazio"
                };

                weaponButtons[index].BackColor = weapon == selectedWeapon
                    ? Color.Gold
                    : Color.White;
            }
        }

        private void SelectWeapon(int slot)
        {
            if (slot < 0 || slot >= weaponSlots.Length || weaponSlots[slot] == WeaponType.Empty)
            {
                return;
            }

            selectedWeapon = weaponSlots[slot];
            UpdateWeaponPanel();
            Focus();
        }

        private static int GetWeaponSlot(Keys key)
        {
            return key switch
            {
                Keys.D1 or Keys.NumPad1 => 0,
                Keys.D2 or Keys.NumPad2 => 1,
                Keys.D3 or Keys.NumPad3 => 2,
                Keys.D4 or Keys.NumPad4 => 3,
                Keys.D5 or Keys.NumPad5 => 4,
                Keys.D6 or Keys.NumPad6 => 5,
                Keys.D7 or Keys.NumPad7 => 6,
                _ => -1
            };
        }

        private static Button CreatePauseButton(string text, EventHandler click)
        {
            Button button = new MenuButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = true,
                TabStop = false
            };

            button.Click += click;
            return button;
        }

        private sealed class MenuButton : Button
        {
            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Space)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                base.OnKeyDown(e);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Space)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                base.OnKeyUp(e);
            }
        }

        private void TogglePause()
        {
            if (gameOver)
            {
                return;
            }

            isPaused = !isPaused;

            pausePanel.Visible = isPaused;
            pausePanel.Enabled = isPaused;

            if (isPaused)
            {
                pausePanel.BringToFront();
                GameTimer.Stop();
            }
            else
            {
                GameTimer.Start();
            }
        }

        private void ContinueGame(object? sender, EventArgs e)
        {
            TogglePause();
            Focus();
        }

        private void RestartGameFromMenu(object? sender, EventArgs e)
        {
            RestartGame();
        }

        private void ReturnToMainMenu(object? sender, EventArgs e)
        {
            Close();
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FormClosedEvent(object? sender, FormClosedEventArgs e)
        {
            GameTimer.Stop();

            for (int bulletIndex = bulletsList.Count - 1; bulletIndex >= 0; bulletIndex--)
            {
                RemoveBulletAt(bulletIndex);
            }

            for (int index = zombiesList.Count - 1; index >= 0; index--)
            {
                RemoveZombieAt(index);
            }

            for (int index = ammoList.Count - 1; index >= 0; index--)
            {
                RemoveAmmoAt(index);
            }

            for (int index = healthList.Count - 1; index >= 0; index--)
            {
                RemoveHealthAt(index);
            }
        }
    }
}