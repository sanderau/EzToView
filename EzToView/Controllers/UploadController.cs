using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using EzToView.Models;
using Amazon;
using Amazon.Runtime;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Net.Mail;
using System.Net;


namespace EzToView.Controllers
{
    public class UploadController : Controller
    {
        // data needed to access the s3 bucket.


        

        string _awsAccessKeyId = "";
        private static readonly string _awsprofilename = "";
        private static readonly string _awssecretkey = "";
        private static readonly string _bucketname = "";
        private static readonly RegionEndpoint _bucketRegion = RegionEndpoint.USWest2;

        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadFile()
        {
            ViewBag.emailError = "";
            ViewBag.fileError = "";

            return View();
        }

        public IActionResult Success()
        {
            /* This path is followed after successfully uploading a file */

            return View();
        }

        public IActionResult Error()
        {
            /* This route is followed on an error uploading a file */
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(Upload up)
        {
            ViewBag.emailError = "";
            ViewBag.fileError = "";

            if (ModelState.IsValid)
            {
                var uniqueFileName = Guid.NewGuid() + up.file.FileName; // uniquely identify files
                string presignedURL = ""; // the presigned url to send to user
                var client = new AmazonS3Client(_awsAccessKeyId, _awssecretkey, _bucketRegion); // client session used to talk to s3 bucket.
                /* The function that uploads a file, and then emails it to the user.*/
                try
                {

                    using (var memstr = new MemoryStream())
                    {

                        up.file.CopyTo(memstr);

                        var uploadReq = new TransferUtilityUploadRequest
                        {
                            InputStream = memstr,
                            Key = uniqueFileName,
                            BucketName = _bucketname,
                            CannedACL = S3CannedACL.PublicRead
                        };

                        var fileTransferUtility = new TransferUtility(client);
                        fileTransferUtility.Upload(uploadReq);
                    }

                }
                catch (Exception e)
                {
                    /* There was an error uploading the file */
                    ViewBag.gError = e;
                    return View();
                }

                try
                {
                    GetPreSignedUrlRequest preUrlReq = new GetPreSignedUrlRequest
                    {
                        BucketName = _bucketname,
                        Key = uniqueFileName,
                        Expires = DateTime.Now.AddDays(7) // The max length for a presigned url to exist is 7 days, so I will use that.
                    };

                    presignedURL = client.GetPreSignedURL(preUrlReq);
                }
                catch (Exception e)
                {
                    /*There was an error getting the presigned url */
                    ViewBag.gError = e;
                    return View();
                }

                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("");
                        mail.To.Add(up.email);
                        mail.Subject = "Here is your presigned URL!";
                        mail.Body = presignedURL + "\n\nThis URL will only persist for 7 days.";

                        using (SmtpClient sc = new SmtpClient("smtp.gmail.com", 587))
                        {
                           
                            sc.Credentials = new NetworkCredential("ez2viewbot@gmail.com", "");
                            sc.EnableSsl = true;
                            sc.Send(mail);
                        }
                    }

                }
                catch (Exception e)
                {
                    /* There was an error sending the email */
                    
                    ViewBag.gError = e;
                    return View();
                }

                return RedirectToAction("Success"); // success is assumed if it has reached this point
            }

            else
            {
                if (up.email == null)
                {
                    ViewBag.emailError = "Email is required!";
                }

                if(up.file == null)
                {
                    ViewBag.fileError = "A valid filepath is required!";
                }
            }

            return View(); // if execution reaches here. there was an error.
        }

    }

}

