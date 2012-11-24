using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenSaverPhoto
{
    static class Program
    {

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
 
                if (args.Length > 0)
                {
                    string firstArgument = args[0].ToLower().Trim();
                    string secondArgument = null;

                    // Handle cases where arguments are separated by colon.
                    // Examples: /c:1234567 or /P:1234567
                    if (firstArgument.Length > 2)
                    {
                        secondArgument = firstArgument.Substring(3).Trim();
                        firstArgument = firstArgument.Substring(0, 2);
                    }
                    else if (args.Length > 1)
                    {
                        secondArgument = args[1];
                    }

                    // Get the 2 character command line argument
                    //string arg = args[0].ToLowerInvariant().Trim().Substring(0, 2);
                    switch (firstArgument)
                    {
                        case "/c":
                            // Show the options dialog
                            ShowOptions();
                            break;
                        case "/p":
                            if (secondArgument == null) {
                                MessageBox.Show(
                                    "Sorry, but the expected window handle was not provided.",
                                    "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return;
                            }
                            IntPtr previewWndHandle = new IntPtr(long.Parse(secondArgument));
                            Application.Run(new Form1(previewWndHandle));
                            break;
                        case "/s":
                            // Show screensaver form
                            ShowScreenSaver();
                            break;
                        case "/d":
                            // Show screensver in debug mode
                            ShowScreenSaver(true);
                            break;
                        default:
                            //MessageBox.Show("Invalid command line argument: " + arg, "Invalid Command Line Argument", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            MessageBox.Show(
                                "Sorry, but the command line argument \"" + firstArgument + "\" is not valid.", "ScreenSaver",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }
                else
                {
                    // If no arguments were passed in, show the screensaver
                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault(false);
                    //ShowScreenSaver(); //this is the good stuff
                    //Application.Run(new Form1());
                    ShowScreenSaver();
                }
        }

        static void ShowOptions()
        {
            OptionsForm optionsForm = new OptionsForm();
            Application.Run(optionsForm);
        }

        static void ShowScreenSaver()
        {
            ShowScreenSaver(false);
        }

        static void ShowScreenSaver(bool debugmode)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                //Form1 screensaver = new Form1(screen.Bounds);
                //screensaver.Show();
            }
            Form1 screenSaver = new Form1(debugmode);
            Application.Run(screenSaver);
        }
    }
}