using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using IWshRuntimeLibrary;
using System.IO;
using Newtonsoft.Json;
using System.Management;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;

namespace invokeai_starter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;


        private string strExeFolderPath = "";
        private string strSettingsPath = "";

        private StarterSettings starterSettings = new StarterSettings();

        private const string c_strExeName = "invokeai_starter.exe";
        private const string c_strSettingsName = "starter_settings.json";

        private bool bNsfwFilter = true;
        private bool bShareAccess = false;

        private string strInternalAddress = "";
        private string strInternetAddress = "";

        private Process processInvokeAi;

        private CancellationTokenSource cancelSource;

        private class StarterSettings
        {
            public bool bFirstStart = true;
            public bool bAcceptedLicense = false;
        }


        public MainWindow()
        {
            instance = this;

            InitializeComponent();

            strExeFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            strExeFolderPath = strExeFolderPath.Replace('\\', '/');
            //strExeFolderPath = "E:\\invokeai_2_3_0_standalone";
            strSettingsPath = System.IO.Path.Combine(strExeFolderPath, c_strSettingsName);

            LoadSettings();

            // if first start: create desktop shortcut
            if (starterSettings.bFirstStart)
                CreateDesktopShortcut();

            starterSettings.bFirstStart = false;

            strCheckForIssues();

            // get settings
            GetParameters();

            checkNsfw.IsChecked = bNsfwFilter;
            checkShareAccess.IsChecked = bShareAccess;

            // get ip links
            GenerateIPLinks();

            textInternalAddress.Text = strInternalAddress;
            textInternetAddress.Text = strInternetAddress;

            // edit python file
            SetPathInPythonFile();
  
            // check that license is accepted
            // -> remove license text + change button text + button action
            if (starterSettings.bAcceptedLicense)
                OnLicenseAccepted();
        }

        private void OnLicenseAccepted()
        {
            textLicense.Text = "";

            starterSettings.bAcceptedLicense = true;

            buttonStart.Content = "Start InvokeAI";
            textFeedback.Text = strCheckForIssues();
        }

        private void StartInvokeAI()
        {
            // apply settings
            SetParameters();

            // run bat file that starts invokeai
            processInvokeAi = Process.Start(System.IO.Path.Combine(strExeFolderPath, "helper.bat"));

            buttonStart.Content = "Loading...";

            Task.Run(WaitForInvokeToLoad);
        }

        private async void WaitForInvokeToLoad()
        {
            // wait until port is open
            cancelSource = new CancellationTokenSource();
            await CheckForOpenPort(new TimeSpan(1), cancelSource.Token);

            this.Dispatcher.Invoke(() =>
            {
                buttonStart.Content = "Running";
            });

            // open browser window
            Process.Start("http://localhost:9090");
        }

        private void SetPathInPythonFile()
        {
            string strPythonFilePath = System.IO.Path.Combine(strExeFolderPath, "env\\Lib\\site-packages\\ldm\\invoke\\globals.py");
            string strInjectedPath = System.IO.Path.Combine(strExeFolderPath, "invokeai");

            try
            {
                string[] arInitFileLines = System.IO.File.ReadAllLines(strPythonFilePath);
                for (int i = 0; i < arInitFileLines.Length; i++)
                {
                    string strLine = arInitFileLines[i];
                    if (strLine.StartsWith("    Globals.root = osp.abspath(osp.expanduser("))
                    {
                        arInitFileLines[i] = $"    Globals.root = osp.abspath(osp.expanduser(\'{strInjectedPath}\'))";
                        break;
                    }
                }
                System.IO.File.WriteAllLines(strPythonFilePath, arInitFileLines);
            }
            catch
            {
                Console.WriteLine($"Could not override {strPythonFilePath}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                string strFileContent = JsonConvert.SerializeObject(starterSettings);
                System.IO.File.WriteAllText(strSettingsPath, strFileContent);
            }
            catch (Exception _exception)
            {
                Console.WriteLine("Could not save!");
                // Could not save settings _exception.Message
            }
        }

        private void LoadSettings()
        {
            starterSettings = new StarterSettings();

            if (System.IO.File.Exists(strSettingsPath))
            {
                try
                {
                    string strFileContent = System.IO.File.ReadAllText(strSettingsPath);
                    starterSettings = JsonConvert.DeserializeObject<StarterSettings>(strFileContent);
                }
                catch (Exception _exception)
                {
                    Console.WriteLine("Could not load!");
                    // Could not load settings _exception.Message
                }
            }
        }

        private void SetParameters()
        {
            string strIniFilePath = System.IO.Path.Combine(strExeFolderPath, "invokeai/invokeai.init");

            if (!System.IO.File.Exists(strIniFilePath))
                return;

            string[] arInitFileLines = System.IO.File.ReadAllLines(strIniFilePath);

            bNsfwFilter = (bool)checkNsfw.IsChecked;
            bShareAccess = (bool)checkShareAccess.IsChecked;

            for (int i = 0; i < arInitFileLines.Length; i++)
            {
                string strLine = arInitFileLines[i];

                // set outdir
                if (strLine.StartsWith("--outdir="))
                    arInitFileLines[i] = $"--outdir=\"{strExeFolderPath}/outputs/\"";

                // set nsfw checker and access share
                if (strLine.StartsWith("# generation arguments"))
                {
                    arInitFileLines[i + 1] = bNsfwFilter ? "" : "--no-nsfw_checker";
                    arInitFileLines[i + 2] = bShareAccess ? "--host=0.0.0.0" : "";
                }
            }

            System.IO.File.WriteAllLines(strIniFilePath, arInitFileLines);
        }

        private void GetParameters()
        {
            try
            {
                string strIniFilePath = System.IO.Path.Combine(strExeFolderPath, "invokeai/invokeai.init");
                string strFileContent = System.IO.File.ReadAllText(strIniFilePath);

                bNsfwFilter = !strFileContent.Contains("--no-nsfw_checker");
                bShareAccess = strFileContent.Contains("--host=0.0.0.0");
            }
            catch
            {
                Console.WriteLine("Could not find invokeai.init");
            }
        }



        // based on https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
        private void CreateDesktopShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string strShortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\InvokeAI Starter.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(strShortcutAddress);
            shortcut.Description = "Shortcut for InvokeAI Starter.";
            shortcut.Hotkey = "Ctrl+Shift+N";
            shortcut.TargetPath = System.IO.Path.Combine(strExeFolderPath, c_strExeName);
            shortcut.Save();
        }

        private string strCheckForIssues()
        {
            string strOutput = "";
            int iWorks = 2; // yes, maybe, no
            string strProblem = "";

            string strGPUName = "";
            float fGPUMemory = 0f;
            float fRAMInGB = 0f;

            // get gpu info
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                strGPUName = obj["Name"].ToString();
            }

            NvAPIWrapper.GPU.PhysicalGPU[] arGPUs = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
            foreach (NvAPIWrapper.GPU.PhysicalGPU gpu in arGPUs)
            {
                fGPUMemory = gpu.MemoryInformation.DedicatedVideoMemoryInkB;
                fGPUMemory = fGPUMemory / 1024f / 1024f;
            }

            // get ram info
            searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                long memory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                fRAMInGB = memory / 1024f / 1024f / 1024f;
            }

            strOutput = $"Your graphics card: {strGPUName} ({fGPUMemory} GB)\n";

            if (fGPUMemory < 3.9f)
            {
                iWorks = 0;
                strProblem += "\nNot enough GPU memory. Needs at least 4 GB.";
            }

            if (fRAMInGB < 11f)
            {
                iWorks = 1;
                strProblem += "\nAI images needs at least 12 GB of memory (RAM). It might work, but the first loading time will be reaaaaally long (~10 min).";
            }

            if (strExeFolderPath.Contains(" "))
            {
                iWorks = 0;
                strProblem += $"\nPath contains space! Please rename all folders, so they don't contain spaces." +
                    $"\nBAD EXAMPLE:  D:/my folder/invokeai/" +
                    $"\nGOOD EXAMPLE: D:/myfolder/invokeai/" +
                    $"\n\nYour path: {strExeFolderPath}";
            }

            if (strExeFolderPath.Contains("[") || strExeFolderPath.Contains("("))
            {
                iWorks = 0;
                strProblem += $"\nPath contains special characters! Please rename all folders, so they don't contain special characters." +
                    $"\nProblematic characters: [], (), cyrillic alphabet, other non-latin characters, etc." +
                    $"\n\nYour path: {strExeFolderPath}";
            }

            if (strGPUName.ToLower().Contains("amd")
                || strGPUName.ToLower().Contains("ati")
                || strGPUName.ToLower().Contains("radeon"))
            {
                iWorks = 0;
                strProblem += "\nAMD graphic cards won't work. :( They will only work on Linux with a proper installation of InvokeAI.";
            }
            else if (strGPUName.ToLower().Contains("intel"))
            {
                iWorks = 1;
                strProblem += "\nYour PC appears to not have a dedicated graphics card. InvokeAI might be reeeeeally slow. :(";
            }
            else if (!strGPUName.ToLower().Contains("rtx 50")
                && !strGPUName.ToLower().Contains("rtx 40")
                && !strGPUName.ToLower().Contains("rtx 30")
                && !strGPUName.ToLower().Contains("rtx 20")
                && !strGPUName.ToLower().Contains("gtx 1"))
            {
                iWorks = Math.Min(iWorks, 1);
                strProblem += "\nGPU is not the newest.";
            }

            if (iWorks == 2)
                strOutput += "Should work! <3";
            else if (iWorks == 1)
                strOutput += "Might work! <3" + strProblem;
            else if (iWorks == 0)
                strOutput += "Won't work, most likely. :(" + strProblem;

            return strOutput;
        }

        public void GenerateIPLinks()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    strInternalAddress = $"http://{ip}:9090";
            }

            strInternetAddress = $"http://{new WebClient().DownloadString("https://api.ipify.org")}:9090";
        }

        private void MainButtonClick(object sender, RoutedEventArgs e)
        {
            if (!starterSettings.bAcceptedLicense)
                OnLicenseAccepted();
            else
                StartInvokeAI();
        }

        public bool PortInUse(int port)
        {
            bool inUse = false;

            System.Net.NetworkInformation.IPGlobalProperties ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        public async Task CheckForOpenPort(TimeSpan interval, System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested || PortInUse(9090))
                    return;
                else
                    await Task.Delay(interval, cancellationToken);     
            }
        }

        private void textInternalAddressClick(object sender, MouseButtonEventArgs e)
        {
            CopyInternalAddress(null, null);
        }

        private void textInternetAddressClick(object sender, MouseButtonEventArgs e)
        {
            CopyInternetAddress(null, null);
        }

        private void CopyInternalAddress(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(strInternalAddress);
        }

        private void CopyInternetAddress(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(strInternetAddress);
        }

        private void textCloseX(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cancelSource != null)
                cancelSource.Cancel();
            SetParameters();
            SaveSettings();

            if (processInvokeAi != null)
                processInvokeAi.Close();
        }

        private void OnConfigurator(object sender, RoutedEventArgs e)
        {
            string strPath = System.IO.Path.Combine(strExeFolderPath, "env/Scripts/invoke.exe"); //invokeai-configure.exe
            if (System.IO.File.Exists(strPath))
                Process.Start(System.IO.Path.Combine(strExeFolderPath, "install_models.bat"));
        }
    }
}
