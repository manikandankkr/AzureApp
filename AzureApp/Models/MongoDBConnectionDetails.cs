using AzureApp.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace AzureWebApp.Models
{
    public class FileModel
    {
        [Required(ErrorMessage = "Please select file.")]
        public HttpPostedFileBase PostedFile { get; set; }
    }
    public class MongoDBConnectionDetails
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool IsSslEnabled { get; set; }
        public bool IsSelfSignedEnabled { get; set; }
        public MongoAuthentication AuthenticationMechanism {get;set;}
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SslClientCertificate { get; set; }
        public string SslCertificatePassword { get; set; }
        public string AuthenticationDatabase { get; set; }
        public byte[] SslCertificateData
        {
            get;set;
        }
    }

    public enum MongoAuthentication
    {
        SCRAM,
        NONE,
        X509
    }
}