using AzureApp.Helper;
using MongoDB.Driver;
using Syncfusion.Dashboard.Service.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace AzureWebApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        public JsonResult FormOne(Models.MongoDBConnectionDetails mongodbConnection)
        {
            try
            {
                MongoServer mongoServer = GetMongoServer(mongodbConnection);
                return Json(mongoServer.GetDatabaseNames().ToList(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(string.Concat(string.IsNullOrEmpty(e.Message) ? "":e.Message,"**********",e.InnerException==null?"":e.InnerException.Message, JsonRequestBehavior.AllowGet));
            }
        }
        
        [HttpPost]
        public string FileUpload(string filePath)
        {
            return FilePathHelper.WriteStreamToFile(new MemoryStream(System.IO.File.ReadAllBytes(filePath.ToString())));
        }

        [HttpPost]
        public ActionResult UploadFiles()
        {
            if (Request.Files.Count > 0)
            {
                try
                {  
                    HttpFileCollectionBase files = Request.Files;
                    string fname = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string filename = Path.Combine(Server.MapPath("~/Files/certificates/"), fname);
                        string directoryPath = Path.GetDirectoryName(filename);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        file.SaveAs(filename);
                    }
                    return Json(fname);
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile)
        {
            string path = Server.MapPath("~/Uploads/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (postedFile != null)
            {
                string fileName = Path.GetFileName(postedFile.FileName);
                postedFile.SaveAs(path + fileName);
                ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
            }

            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        private MongoServer GetMongoServer(Models.MongoDBConnectionDetails connectionParameters)
        {
            MongoClient mongoClient = PrepareMongoClient(connectionParameters);
            try
            {
                MongoServer mongoServer = mongoClient.GetServer();
                mongoServer.Connect();
                return mongoServer;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    
        private MongoClient PrepareMongoClient(Models.MongoDBConnectionDetails connectionParameters)
        {
            return new MongoClient(GetMongoSettings(connectionParameters));
        }

        private MongoClientSettings GetMongoSettings(Models.MongoDBConnectionDetails connectionParameters)
        {
            MongoClientSettings settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(connectionParameters.HostName, connectionParameters.Port),
                UseSsl = connectionParameters.IsSslEnabled,
                VerifySslCertificate = !connectionParameters.IsSelfSignedEnabled,
                RetryWrites = true
            };
            if (connectionParameters.IsSslEnabled &&
                !connectionParameters.AuthenticationMechanism.Equals(Models.MongoAuthentication.X509))
            {
                connectionParameters.SslCertificateData = FilePathHelper.ReadFile(Server, connectionParameters.SslClientCertificate);
                if (connectionParameters.SslCertificateData != null)
                {
                    var certificate = string.IsNullOrEmpty(connectionParameters.SslCertificatePassword) ? new X509Certificate2(connectionParameters.SslCertificateData) : new X509Certificate2(connectionParameters.SslCertificateData, connectionParameters.SslCertificatePassword);
                    settings.SslSettings = new SslSettings()
                    {
                        ClientCertificates = new[] { certificate }
                    };
                }
            }
            switch (connectionParameters.AuthenticationMechanism)
            {
                case Models.MongoAuthentication.SCRAM:
                    break;
                case Models.MongoAuthentication.X509:
                    settings.UseSsl = true;
                    connectionParameters.SslCertificateData = FilePathHelper.ReadFile(Server, connectionParameters.SslClientCertificate);
                    var certificate = string.IsNullOrEmpty(connectionParameters.SslCertificatePassword) ? new X509Certificate2(connectionParameters.SslCertificateData) : new X509Certificate2(connectionParameters.SslCertificateData, connectionParameters.SslCertificatePassword);
                    settings.SslSettings = new SslSettings()
                    {
                        ClientCertificates = new[] { certificate }
                    };
                    settings.Credential = MongoCredential.CreateMongoX509Credential(certificate.Subject.ToString().Replace("S=", "ST=").Replace(", ", ","));
                    break;
                default:
                    break;
            }
            return settings;
        }
    }
}