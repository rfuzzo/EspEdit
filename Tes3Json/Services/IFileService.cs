using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tes3Json.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Open a filepicker with 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<string> OpenFileAsync(string title, string[] filetypes);
    }
}
