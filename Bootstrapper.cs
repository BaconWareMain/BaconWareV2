namespace Bootstrapper
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Remoting.Contexts;
    using System.Threading.Tasks;
    using static System.Net.WebRequestMethods;

    public static class Program
    {
        #region Private Properties

        private static string CurrentDirectory { get; } 
            = Environment.CurrentDirectory;

        private static string ZipLocation { get; }
            = $"{CurrentDirectory}\\Name.zip";

        private static string DirectoryLocaction { get; }
            = $"{CurrentDirectory}\\Folder Name";

        #endregion

        #region Private Helpers

        private static void DeleteDirectories(bool DeleteZip = true, bool DeleteDirectory = true) {
            if (DeleteZip && System.IO.File.Exists(ZipLocation))
                System.IO.File.Delete(ZipLocation);

            if (DeleteDirectory && Directory.Exists(DirectoryLocaction))
                Directory.Delete(DirectoryLocaction, true);
        }

        #endregion

        #region Events

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            Console.Write("\rDownloading... ToReceive: {0}, Received {1}, Total {2}%",
                e.TotalBytesToReceive, e.BytesReceived, e.ProgressPercentage);
        }

        private static void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {

            if (e.Cancelled && e.Error != null && e.Error is Exception ex) {
                Logger.Log(ex.Message, Logger.LogType.Error);
            }

            if (System.IO.File.Exists(ZipLocation)) {
                ZipFile.ExtractToDirectory(ZipLocation, CurrentDirectory);
                DeleteDirectories(true, false);
            }
            else {
                Logger.Log("Cant find downloaded zip file in current directory, please try again....");
            }

            return; // Back to Console.ReadLine() ;)
        }

        #endregion

        static bool IsLatestVerion = true;

        static string OldLink = "https://drive.google.com/file/d/1NGJLkyxgwadX_u3GJ4gb0W5sLuiwD70Q";
        static void DownloadFile1(string FileLink,string FileName)
        {
            using (var wc = new WebClient() { Proxy = null })
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                string filename = FileName; // file name [include .exe, .txt, etc.]
                string link = FileLink; // the link
                wc.DownloadFile(FileLink, filename);
                Logger.Log("Completed!");
            }
        }

        private static void DownloadFileV2(string fileLink, string fileName, string directoryPath)
        {
            using (WebClient webClient = new WebClient
            {
                Proxy = null
            })
            {
                webClient.DownloadProgressChanged += Program.Wc_DownloadProgressChanged;
                webClient.DownloadFileCompleted += Program.Wc_DownloadFileCompleted;
                string fileName2 = Path.Combine(directoryPath, fileName);
                webClient.DownloadFile(fileLink, fileName2);
                Logger.Log(fileName + " Download completed!", Logger.LogType.Info);
            }
        }




        private static void DownloadFile(string fileLink, string fileName, string directoryPath,bool ShowConsole)
        {
            using (WebClient webClient = new WebClient { Proxy = null })
            {
                webClient.DownloadProgressChanged += Wc_DownloadProgressChanged;
                webClient.DownloadFileCompleted += Wc_DownloadFileCompleted;
                string filePath = Path.Combine(directoryPath, fileName);
                webClient.DownloadFile(fileLink, filePath);
                if (ShowConsole)
                {
                    Logger.Log($"{fileName} download completed!");

                }
            }
        }
        private static async Task<string> GetPastebinContent(string pastebinRawLink)
        {
            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(pastebinRawLink);
            }
        }
        private static async Task CheckVersion()
        {
            string currentDirectory = Environment.CurrentDirectory;
            DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);


            string versionFilePath = Path.Combine(currentDirectory, "Auth/Version.txt");
            string pastebinLink = "https://pastebin.com/raw/AvxyT7Mq"; // Replace with your Pastebin link

            if (!System.IO.File.Exists(versionFilePath))
            {
                //Logger.Log("version.txt file does not exist,Please Open Bacon Ware Launcher.exe");
                return;
            }

            string localVersion =  System.IO.File.ReadAllText(versionFilePath);
            string pastebinVersion = await GetPastebinContent(pastebinLink);

            if (localVersion != pastebinVersion)
            {
                IsLatestVerion = false;
                Logger.Log("Updating To The Latest Version!");
            }
        }
        private static async Task Main(string[] args)
        {
            Console.Title = "Bacon Ware Bootstrapper";

            FilesCheck();

            await CheckVersion();
            Update();

            DownloadDlls();
            
            SetupDownload();
            CreateShortcut();



        }

        private static void FilesCheck()
        {
            Logger.Log("Checking directories and files...");
            CreateFolder("Auth", CurrentDirectory);
            CreateFolder("Bin", CurrentDirectory);
            CreateFolder("Dlls", Path.Combine(CurrentDirectory, "Bin"));
        }

        private static bool IsFile(string fileName)
        {
            return System.IO.File.Exists(Path.Combine(CurrentDirectory, "Bin/Dlls", fileName));
        }

        static bool IsDownloading = false;
        private static void DownloadDlls()
        {
            

            string[,] dlls = {
        {"BioShock-Infinite.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Dlls/BioShock-Infinite.dll"},
        {"BlackOps2.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Dlls/BlackOps2.dll"},
        {"Idle-Slayer.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Dlls/Idle-Slayer.dll"},

        // Add more DLLs if needed: {"DllName", "DownloadLink"}
    };

            // Get the number of rows (DLLs) in the table
            int rowCount = dlls.GetLength(0);

            // Iterate through each DLL in the table
            for (int i = 0; i < rowCount; i++)
            {
                string dllName = dlls[i, 0];
                string dllLink = dlls[i, 1];

                if (!IsFile(dllName) & !IsDownloading)
                {
                    Logger.Log("Download Dlls...");
                    IsDownloading = true;
                }

                if (!IsFile(dllName))
                {
                    //Logger.Log($"Downloading {dllName}...");
                    string directoryPath = Path.Combine(CurrentDirectory, "Bin/Dlls");
                    DownloadFile(dllLink, dllName, directoryPath, false);
                }
            }



            //if (!IsFile("BioShock-Infinite.dll"))
           // {
            //    string fileLink = "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Dlls/BioShock-Infinite.dll";
            //   string directoryPath = Path.Combine(CurrentDirectory, "Bin/Dlls");
            //    DownloadFile(fileLink, "BioShock-Infinite.dll", directoryPath,false);
           // }
        }

        static bool IsDownloadingFiles = false;

        static bool IsUpdating = false;
        private static void Update()
        {
            if (!IsLatestVerion)
            {

            string[,] fileLinks = {
                {"BaconWare.exe", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/BaconWare.exe"},
                {"Siticone.UI.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Siticone.UI.dll"},
                {"Siticone.Desktop.UI.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Siticone.Desktop.UI.dll"},
            };
            // string directoryPath = Path.Combine(CurrentDirectory, "Bin");
            //foreach (var link in fileLinks)
            // {
            //    DownloadFile(link, Path.GetFileName(link), directoryPath, true);
            // }

            // Get the number of rows (DLLs) in the table
            int rowCount = fileLinks.GetLength(0);

            // Iterate through each DLL in the table
            for (int i = 0; i < rowCount; i++)
            {
                string dllName = fileLinks[i, 0];
                string dllLink = fileLinks[i, 1];

                if (!IsUpdating)
                {
                    Logger.Log("Downloading Files...");
                        IsUpdating = true;
                }


                    Logger.Log($"Downloading {dllName}...");
                    string directoryPath = Path.Combine(CurrentDirectory, "Bin");
                    DownloadFile(dllLink, dllName, directoryPath, false);
            }
                DownloadDlls();
            }


        }
        private static void SetupDownload()
        {
            string directoryPathtest = Path.Combine(CurrentDirectory, "Bin");

            if (!IsFile(directoryPathtest + "/BaconWare.exe"))
            { 
            string[,] fileLinks = {
                {"BaconWare.exe", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/BaconWare.exe"},
                {"Siticone.UI.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Siticone.UI.dll"},
                {"Siticone.Desktop.UI.dll", "https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Siticone.Desktop.UI.dll"},
            };
           // string directoryPath = Path.Combine(CurrentDirectory, "Bin");
            //foreach (var link in fileLinks)
           // {
            //    DownloadFile(link, Path.GetFileName(link), directoryPath, true);
           // }

            // Get the number of rows (DLLs) in the table
            int rowCount = fileLinks.GetLength(0);

            // Iterate through each DLL in the table
            for (int i = 0; i < rowCount; i++)
            {
                string dllName = fileLinks[i, 0];
                string dllLink = fileLinks[i, 1];

                if (!IsFile(dllName) & !IsDownloadingFiles)
                {
                    Logger.Log("Downloading Files...");
                    IsDownloadingFiles = true;
                }

                if (!IsFile(dllName))
                {
                    Logger.Log($"Downloading {dllName}...");
                    string directoryPath = Path.Combine(CurrentDirectory, "Bin");
                    DownloadFile(dllLink, dllName, directoryPath, false);
                }
            }

        }
        }

        private static void CreateShortcut()
        {
            DownloadFile("https://raw.githubusercontent.com/BaconWareMain/BaconWareV2/main/Bacon%20Ware%20Launcher.exe", "Bacon Ware Launcher.exe", CurrentDirectory,true);
        }

        private static void CreateFolder(string folderName, string newPath)
        {
            Directory.CreateDirectory(Path.Combine(newPath, folderName));
        }
    }
}
