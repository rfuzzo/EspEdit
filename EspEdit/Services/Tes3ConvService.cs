using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspEdit.Services
{
    internal interface ITes3ConvService
    {
        Task<bool> ConvertJsonToEspAsync(FileInfo jsonFile, string espPath);
        Task<bool> ConvertJsonToEspAsync(string jsonString, string espPath);
        Task<string> GetJsonFromEsp(FileInfo espFile);
    }

    internal class Tes3ConvService : ITes3ConvService
    {
        private readonly string Tes3ConvPath = Path.Combine(FileSystem.Current.AppDataDirectory, Constants.Tes3Conv);

        private readonly IDialogService _dialogService;

        public Tes3ConvService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        private async Task<bool> EnsureTes3ConvExistsAsync()
        {
            if (!await FileSystem.Current.AppPackageFileExistsAsync("lib/tes3conv.exe"))
            {
                // todo logging
                return false;
            }

            // todo hash the exe

            if (!File.Exists(Tes3ConvPath))
            {
                using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync("lib/tes3conv.exe");

                // Write the file content to the app data directory
                using FileStream outputStream = System.IO.File.OpenWrite(Tes3ConvPath);
                await fileStream.CopyToAsync(outputStream);
            }

            if (!File.Exists(Tes3ConvPath))
            {
                // todo logging
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <param name="espPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<bool> ConvertJsonToEspAsync(FileInfo jsonFile, string espPath)
        {
            if (!jsonFile.Exists)
            {
                // logging
                throw new FileNotFoundException("File not found", nameof(jsonFile));
            }

            if (File.Exists(espPath))
            {
                // todo if the esp already exists, override for now? or ask
            }

            //tes3conv "test.json" "test.esp"
            string arg = $"\"{jsonFile.FullName}\" \"{espPath}\"";
            ProcessStartInfo si = new(Tes3ConvPath, arg)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            string error = "";
            Process p = new();
            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                error += e.Data;
            });

            p.StartInfo = si;
            p.Start();

            p.BeginErrorReadLine();
            string output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();

            if (string.IsNullOrEmpty(error))
            {
                return true;
            }
            else
            {
                await _dialogService.DisplayAlert("Error", error, "OK");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="espPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> ConvertJsonToEspAsync(string jsonString, string espPath)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                // logging
                throw new ArgumentException("Input json is null or empty", nameof(jsonString));
            }

            if (File.Exists(espPath))
            {
                // todo if the esp already exists, override for now? or ask
            }

            //tes3conv - "test.esp"
            string arg = $"- \"{espPath}\"";
            ProcessStartInfo si = new(Tes3ConvPath, arg)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            string error = "";
            Process p = new();
            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                error += e.Data;
            });

            p.StartInfo = si;
            p.Start();


            StreamWriter myStreamWriter = p.StandardInput;
            myStreamWriter.Write(jsonString);
            myStreamWriter.Close();


            p.BeginErrorReadLine();
            string output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();

            if (string.IsNullOrEmpty(error))
            {
                return true;
            }
            else
            {
                await _dialogService.DisplayAlert("Error", error, "OK");
                return false;
            }

            ////tes3conv - "test.esp"
            //using (Process myProcess = new())
            //{
            //    myProcess.StartInfo.FileName = Tes3ConvPath;
            //    myProcess.StartInfo.UseShellExecute = false;
            //    myProcess.StartInfo.RedirectStandardInput = true;

            //    myProcess.Start();

            //    StreamWriter myStreamWriter = myProcess.StandardInput;

            //    myStreamWriter.Write(jsonString);

            //    myStreamWriter.Close();

            //    // Wait for the sort process to write the sorted text lines.
            //    myProcess.WaitForExit();
            //}
            // return true;
        }

        /// <summary>
        /// Convert esp file to json string
        /// </summary>
        /// <param name="espFile"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<string> GetJsonFromEsp(FileInfo espFile)
        {
            if (!espFile.Exists)
            {
                // logging
                throw new FileNotFoundException();
            }

            if (!await EnsureTes3ConvExistsAsync())
            {
                return null;
            }

            //tes3conv "test.esp" to stdout
            string arg = $"\"{espFile.FullName}\"";
            ProcessStartInfo si = new(Tes3ConvPath, arg)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,

            };

            string error = "";
            Process p = new();
            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                error += e.Data;
            });

            p.StartInfo = si;
            p.Start();

            p.BeginErrorReadLine();
            string output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();

            if (string.IsNullOrEmpty(error))
            {
                return output;
            }
            else
            {
                await _dialogService.DisplayAlert("Error", error, "OK");
                return null;
            }

        }

    }
}
