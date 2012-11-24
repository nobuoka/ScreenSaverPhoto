using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// DllImport を使うために必要
using System.Runtime.InteropServices;
// リンクのターゲット先取得のため (参照で Windows Script Host Object Model が必要)
using IWshRuntimeLibrary;

namespace ScreenSaverPhoto
{
    public partial class Form1 : Form
    {
        #region Preview API's

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        #endregion

        private bool IsPreviewMode = false;
        private string[] imageFilePaths = null;
        private Timer timer;
        private Random rand = null;
        
        public Form1()
            : this(false)
        {
        }

        //It is used when in normal mode
        public Form1(bool debugmode)
        {
            this.timer = new Timer();
            this.rand = new Random();

            InitializeComponent();
            SetupScreenSaver();
            /*
            this.Bounds = bounds;
            //hide the cursor
            Cursor.Hide();
            */

            InitializeImageFilePaths();

            timer.Interval = 650;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {
            if (imageFilePaths.Length == 0) return;

            var imageFilePath = imageFilePaths[rand.Next(imageFilePaths.Length)];
            var size = new Size(rand.Next(700), rand.Next(700));
            var pos = new Point(rand.Next(Bounds.Width-size.Width), rand.Next(Bounds.Height-size.Height));
            AddImage(imageFilePath, size, pos);
            // Move text to new location
            //textLabel.Left = rand.Next(Math.Max(1, Bounds.Width - textLabel.Width));
            //textLabel.Top = rand.Next(Math.Max(1, Bounds.Height - textLabel.Height));
        }

        private void InitializeImageFilePaths() {
            var pathOfMyPictures = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            //"C:\My Documents"以下のファイルをすべて取得
            //ワイルドカード"*"は、すべてのファイルを意味する
            imageFilePaths = GetAllFilesInDir(System.IO.Path.Combine(pathOfMyPictures, "screensaver"), true);
            //ListBox1に結果を表示する
            /*
            foreach (var path in filePaths)
            {
                Console.WriteLine(path);
            }
            */
        }

        public void AddImage(string imageFilePath, Size size, Point position)
        {
            var roundCornerBox = new RoundCornerS();
            //TextBox myText = new TextBox();
            var pictBox = new PictureBox();
            roundCornerBox.Controls.Add(pictBox);
            pictBox.Dock = DockStyle.Fill;
            //pictBox.Location = position;// new Point(25, 25);

            // Sets up an image object to be displayed.
            /*
            if (MyImage != null)
            {
                MyImage.Dispose();
            }
            */
            var path = imageFilePath;// "D:\\nobuoka\\Pictures\\127e16d38f9dc29c71cee6b01f067cf6.jpeg";
            // Stretches the image to fit the pictureBox.
            pictBox.SizeMode = PictureBoxSizeMode.Zoom;
            try
            {
                var maxSize = rand.Next(500, 800);
                var image = new Bitmap(path);
                var rat = (double)Math.Max(1, Math.Max((double)image.Width / maxSize, (double)image.Height / maxSize));
                var imageSize = new Size((int)(image.Width / rat), (int)(image.Height / rat));
                pictBox.Image = (Image)image;

                var pos = new Point(
                    rand.Next(-imageSize.Width / 3, Bounds.Width - imageSize.Width + imageSize.Width / 3),
                    rand.Next(-imageSize.Height / 3, Bounds.Height - imageSize.Height + imageSize.Height / 3));
                roundCornerBox.ClientSize = imageSize;// new Size(250, 250);
                roundCornerBox.Location = pos;// new Point(25, 25);

                if (this.Controls.Count > 40)
                {
                    var cont = this.Controls[this.Controls.Count - 1];  //  GetChildIndex(this.Controls.Count - 1);
                    this.Controls.RemoveAt(this.Controls.Count - 1);
                    cont.Dispose();
                }
                this.Controls.Add(roundCornerBox);
                roundCornerBox.BringToFront();
            }
            catch (Exception err)
            {
                // do nothing
                // 画像じゃないファイルをあれしようとしてエラーが発生することがある
            }
        }
        
        //This constructor is the handle to the select 
        //screensaver dialog preview window
        //It is used when in preview mode (/p)
        public Form1(IntPtr PreviewWndHandle)
        {
            InitializeComponent();
 
            // Set the preview window as the parent of this window
            SetParent(this.Handle, PreviewWndHandle);
 
            // Make this a child window so it will close when the parent dialog closes
            // GWL_STYLE = -16, WS_CHILD = 0x40000000
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));
 
            // Place our window inside the parent
            Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            Size = ParentRect.Size;
            Location = new Point(0, 0);
 
            // Make text smaller
            //textLabel.Font = new System.Drawing.Font("Arial", 6);
 
            IsPreviewMode = true;
        }

        //This constructor is the handle to the select 
        //screensaver dialog preview window
        //It is used when in preview mode (/p)
        /*
        public Form1(IntPtr PreviewHandle)
        {
            InitializeComponent();

            //set the preview window as the parent of this window
            SetParent(this.Handle, PreviewHandle);

            //make this a child window, so when the select 
            //screensaver dialog closes, this will also close
            SetWindowLong(this.Handle, -16,
              new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            //set our window's size to the size of our window's new parent
            Rectangle ParentRect;
            GetClientRect(PreviewHandle, out ParentRect);
            this.Size = ParentRect.Size;

            //set our location at (0, 0)
            this.Location = new Point(0, 0);

            IsPreviewMode = true;
        }
         */

        /// <summary>
        /// Set up the main form as a full screen screensaver.
        /// </summary>
        private void SetupScreenSaver()
        {
            // Use double buffering to improve drawing performance
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            // Capture the mouse
            this.Capture = true;

            // Set the application to full screen mode and hide the mouse
            //Cursor.Hide();
            //Bounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            //TopMost = true;

            //ShowInTaskbar = false;
            //DoubleBuffered = true;
            //BackgroundImageLayout = ImageLayout.Stretch;
        }

        // リンクが先祖を参照してると何度も繰り返してしまうので lookLinkTarget が true でも再起呼び出し時は false にする
        private static string[] GetAllFilesInDir(string dirPath, bool lookLinkTarget)
        {
            //"C:\My Documents"以下のファイルをすべて取得
            //ワイルドカード"*"は、すべてのファイルを意味する
            var filePaths = new List<string>(System.IO.Directory.GetFiles(dirPath, "*", System.IO.SearchOption.AllDirectories));
            if (lookLinkTarget)
            {
                var paths = new List<string>();
                foreach (var path in filePaths)
                {
                    if (System.IO.Path.GetExtension(path) == ".lnk")
                    {
                        var targetPath = GetShortcutTargetFile(path);
                        paths.AddRange(GetAllFilesInDir(targetPath, false));
                    }
                }
                filePaths.AddRange(paths);
            }
            return filePaths.ToArray();
        }

        private static string GetShortcutTargetFile(string shortcutFilePath)
        {
            IWshShell shell = null;         // シェルオブジェクト
            IWshShortcut shortcut = null;   // ショートカットオブジェクト
            try
            {
                //---------------------------------------------------------------------------------
                // オブジェクト作成
                //---------------------------------------------------------------------------------
                // シェルオブジェクト作成
                shell = new WshShell();
                // ショートカットオブジェクト作成
                //---------------------------------------------------------------------------------
                // ショートカット プロパティ設定
                //---------------------------------------------------------------------------------
                shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
                // ショートカットのリンク先を設定
                return shortcut.TargetPath;
            }
            finally
            {
                //---------------------------------------------------------------------------------
                // COMオブジェクト解放
                //---------------------------------------------------------------------------------
                // ショートカットオブジェクトの解放
                if (shortcut != null) Marshal.ReleaseComObject(shortcut);
                // シェルオブジェクトの解放
                if (shell != null) Marshal.ReleaseComObject(shell);
            }
        }


        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode)
            {
                //Close();
                //Application.Exit();
            }
            /*
            // Set IsActive and MouseLocation only the first time this event is called.
            if (!isActive)
            {
                mouseLocation = MousePosition;
                isActive = true;
            }
            else
            {
                // If the mouse has moved significantly since first call, close.
                if ((Math.Abs(MousePosition.X - mouseLocation.X) > 10) ||
                    (Math.Abs(MousePosition.Y - mouseLocation.Y) > 10))
                {
                    Close();
                }
            }
             */
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == (Keys.RButton | Keys.ShiftKey))
            { ;} // capture the Alt keypress
            else if (e.KeyCode == Keys.F && e.Alt)
                isFeedViewShown = !isFeedViewShown;
            else if (e.KeyCode == Keys.I && e.Alt)
                isItemsViewShown = !isItemsViewShown;
            else if (e.KeyCode == Keys.Down)
            {
                feedlist.MoveNext();
                rssDescriptionView.Reset();
            }
            else if (e.KeyCode == Keys.Up)
            {
                feedlist.MovePrevious();
                rssDescriptionView.Reset();
            }
            else //if(e.KeyCode == Keys.Escape)
                Close();

            this.Refresh();
             */
            if (!IsPreviewMode)
            {
                //Close();
                Application.Exit();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode)
            {
                //Close();
                Application.Exit();
            }
        }

        /*
        #region User Input

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //** take this if statement out if your not doing a preview
            if (!IsPreviewMode) //disable exit functions for preview
            {
                Application.Exit();
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            //** take this if statement out if your not doing a preview
            if (!IsPreviewMode) //disable exit functions for preview
            {
                Application.Exit();
            }
        }

        //start off OriginalLoction with an X and Y of int.MaxValue, because
        //it is impossible for the cursor to be at that position. That way, we
        //know if this variable has been set yet.
        Point OriginalLocation = new Point(int.MaxValue, int.MaxValue);

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //** take this if statement out if your not doing a preview
            if (!IsPreviewMode) //disable exit functions for preview
            {
                //see if originallocation has been set
                if (OriginalLocation.X == int.MaxValue &
                    OriginalLocation.Y == int.MaxValue)
                {
                    OriginalLocation = e.Location;
                }
                //see if the mouse has moved more than 20 pixels 
                //in any direction. If it has, close the application.
                if (Math.Abs(e.X - OriginalLocation.X) > 20 |
                    Math.Abs(e.Y - OriginalLocation.Y) > 20)
                {
                    Application.Exit();
                }
            }
        }
        #endregion
        */
    }
}
