using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tes3Json.Services
{
    public interface ITes3ConvService
    {
        Task<bool> ConvertJsonToEspAsync(FileInfo jsonFile, string espPath);
        Task<bool> ConvertJsonToEspAsync(string jsonString, string espPath);
        Task<string> GetJsonFromEsp(FileInfo espFile);
    }


}
