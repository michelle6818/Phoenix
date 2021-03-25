using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Phoenix.Services
{
    public interface IBTFileService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string fileName);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes);

    }
}
