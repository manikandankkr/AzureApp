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