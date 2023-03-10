using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace invokeai_starter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WindowRequirements : Window
    {
        public WindowRequirements()
        {
            InitializeComponent();

            try
            {
                textFeedback.Text = MainWindow.strCheckForIssues();
            }
            catch
            {
                textFeedback.Text = "Couldn't run issue check.";
            }
        }

        private void textMinimize(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void textCloseX(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnLogoClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://invoke-ai.github.io/InvokeAI/");
        }

        private void OnStandaloneTextClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://sunija.itch.io/invokeai");
        }

        private void OnGoToDownloadPage(object sender, RoutedEventArgs e)
        {
            Process.Start("https://sunija.itch.io/invokeai");
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try // can crash when clicking buttons
                {
                    this.DragMove();
                }
                catch
                { 
                    
                }
            }
                
        }
    }
}
