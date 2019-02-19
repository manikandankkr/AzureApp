namespace AzureApp.Helper
{
    using System;
    using System.IO;
    using System.Web;

    public static class FilePathHelper
    {
        public static string WriteStreamToFile(Stream stream)
        {
            try
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                stream.Position = 0;
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "certificates", fileName);
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (FileStream writer = new FileStream(filePath, FileMode.Create))
                {
                    stream.CopyTo(writer);

                }
                return fileName;
            }
            catch (Exception ex)
            {
                return string.Concat(string.Concat(string.IsNullOrEmpty(ex.Message) ? "" : ex.Message, "**********", ex.InnerException == null ? "" : ex.InnerException.Message));
            }
        }

        public static byte[] ReadFile(HttpServerUtilityBase server,string fileName)
        {
            try
            {
                string filePath = Path.Combine(server.MapPath("~/Files/certificates/"), fileName);
                if (System.IO.File.Exists(filePath))
                {
                    Stream streamData = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    MemoryStream memoryStr = new MemoryStream();
                    streamData.CopyTo(memoryStr);
                    return memoryStr.ToArray();
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static string ReadAsFile(HttpServerUtilityBase server, string fileName)
        {
            try
            {
                string filePath = Path.Combine(server.MapPath("~/Files/certificates/"), fileName);
                string tempFilePath = Path.Combine(server.MapPath("~/Files/certificates/temp"), string.Concat(fileName,".pfx"));
                if (System.IO.File.Exists(filePath))
                {
                    Stream streamData = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    MemoryStream memoryStr = new MemoryStream();
                    streamData.CopyTo(memoryStr);
                    string directoryPath = Path.GetDirectoryName(tempFilePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    File.WriteAllBytes(tempFilePath, memoryStr.ToArray());
                    return tempFilePath;
                }
            }
            catch (Exception ex)
            {
            }
            return string.Empty;
        }
    }
}