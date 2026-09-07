using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace juggle_joy.Models
{
    public class GlobalMethods
    {

        //public static string webUrl = "http://www.jugglejoy.uptindia.com/";
        public static string webUrl = "http://www.jugglejoy.com/";
        //public static string webUrl = "http://localhost:60600/";


        /// <summary>
        /// Amruta test keys
        /// </summary>
        //public static string StripePublishableKey = "pk_test_51Oold2SILT1h5QjbUgNVCFc5gxGO4wUsLM11OF81BtgjLrsF0r8EL7vR82MfsHBMa5HVhpccUWCOP7uyX1gFC9Ij003LVbjbxM";
        //public static string StripeSecretKey = "sk_test_51Oold2SILT1h5QjbcTcA5eR30Lp3AVslBeph4CBpEm0OrlXSVZP6SaX6LJXuBYAxR6HPgJrQ1DTWuXAZ1ucRu84m00Ii561zQA";
        public static string StripePublishableKey = "";
        public static string StripeSecretKey = "";

        ////---Client's Stripe Live API key------//
        //public static string StripePublishableKey = "pk_live_51PH57NDli2Ue5yQvTaZMnSxmygaK1FVux7kLrYO9mGHl1zHMDjLEG7DJWGb3yTuzHeBP4FMx193Vab6e3wADqgDV00xFfGYx3v";
        //public static string StripeSecretKey = "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";

        //Google reCAPTCHA
        //for localhost
        //public static string Google_reCAPTCHA_SiteKey = "6LcUrgsqAAAAAMQ64X9kUVcnAy_KabvfHnc2Aynm";
        //public static string Google_reCAPTCHA_SecretKey = "6LcUrgsqAAAAAK7VkBCLcTp7NzteWfwI4Gp3TuYK";

        //for jugglejoy uptindia
        //public static string Google_reCAPTCHA_SiteKey = "6LdcvQsqAAAAAPCtoPllH5Y0VRWFxFlSmj7ExK0r";
        //public static string Google_reCAPTCHA_SecretKey = "6LdcvQsqAAAAAMgI_ooTUUijlN_KssyVQ1vye3dC";

        //for client jugglejoy
        //public static string Google_reCAPTCHA_SiteKey = "6Ld9jBYqAAAAAIpYhqpf3fA9V8Dvl0jsJ-Va2W3d";
        //public static string Google_reCAPTCHA_SecretKey = "6Ld9jBYqAAAAAOJ_nIsJwhs4Dwic9DdNS7irKv0P";
         public static string Google_reCAPTCHA_SiteKey = "";
        public static string Google_reCAPTCHA_SecretKey = "";

        public static async Task<bool> IsCaptchaValid(string response, string action, string UserHostAddress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        {"secret", GlobalMethods.Google_reCAPTCHA_SecretKey},
                        {"response", response},
                        {"remoteip", UserHostAddress}
                    };

                    var content = new FormUrlEncodedContent(values);
                    var verify = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    var captchaResponseJson = await verify.Content.ReadAsStringAsync();
                    var captchaResult = JsonConvert.DeserializeObject<CaptchaResponseViewModel>(captchaResponseJson);
                    return captchaResult.Success
                           && captchaResult.Action == action
                           && captchaResult.Score > 0.5;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string GenerateRandomNumber()
        {
            string st = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // 12 hours format
            //string st = DateTime.Now.ToString("yyyyMMddHHmmssfff"); /// 24 hours format

            Random rnd = new Random();
            st = st + rnd.Next(1000000, 9999999).ToString();
            //Thread.Sleep(300);
            return st;
        }
        public static string MailBody(string mailContent = "")
        {
            string strbody = "<html>" +
            "<head>" +
            "</head>" +
            "<body>" +
            "<table border='0' cellpadding='0' cellspacing='0' width='100%'>" +
            "<tr>" +
            "<td bgcolor='#f5f5f5' style='padding: 4%;'>" +
            "<div style='background-color: white;box-shadow: 5px 5px 5px #888888;'>" +
            "<div style='padding: 2%;'>" +
            "<a href='" + webUrl + "'><img style='width: 150px;' src='" + webUrl + "/assets/images/logo.png' alt='Juggle joy' /></a> " +
            "</div> " +
            "<div style='padding: 3%;'>" +
            mailContent +
            "<br />Thank you <br /> <br /> " +
            "Regards<br /> " +
            "<a href='" + webUrl + "'>Juggle joy</a><br /> " +
            "</div> " +
            "</div> " +
            "</td> " +
            "</tr> " +
            "<tr>" +
            "<td bgcolor='#FFFFFF' class='dotted_line'>&nbsp;</td>" +
            "</tr> " +
            "</table> " +
            "</body> " +
            "</html>";
            return strbody;

        }
        public static string HashSHA1(string value)
        {
            var sha1 = SHA1.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hash = sha1.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string Encrypt(string plainText, string password)
        {
            if (plainText == null)
            {
                return null;
            }

            if (password == null)
            {
                password = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);
        }

        public static string Decrypt(string encryptedText, string password)
        {
            if (encryptedText == null)
            {
                return null;
            }

            if (password == null)
            {
                password = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static string ConvertToDate(object value)
        {
            return string.Format("{0:dd/MMM/yyyy}", value);
        }

        public static string ConvertToYearDate(object value)
        {
            return string.Format("{0:MMMM yyyy}", value);
        }

        public static string Convert24HrsTime(object value)
        {
            return string.Format("{0:HH:mm:ss}", value);
        }
        public static string Convert12HrsTime(object value)
        {
            return string.Format("{0:hh:mm tt}", value);
        }
        public static DateTime ConvertToIST()
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            return indianTime;
        }

        public static string ConvertTo24hrFormatDate(object value)
        {
            return string.Format("{0:dd-MM-yyyy HH:mm:ss}", value);
        }

        public static string convert_special_characters(string input)
        {
            if (input != null && input.Length > 0)
            {
                input = input.Replace("Æ", "AE");
                input = input.Replace("æ", "ae");
                input = input.Replace("Å", "AA");
                input = input.Replace("å", "aa");
                input = input.Replace("Ø", "OE");
                input = input.Replace("ø", "oe");
                input = input.Replace(" ", "-");
                input = input.Replace("/", "");
                input = input.Replace("\\", "");
                input = input.Replace(":", "");
                input = input.Replace("*", "");
                input = input.Replace("?", "");
                input = input.Replace("\"", "");
                input = input.Replace("<", "");
                input = input.Replace(">", "");
                input = input.Replace("|", "");

                return input.ToLower();
            }
            else
            {
                return "";
            }
        }

        public static bool IsFileExists(string url)
        {
            bool result = false;
            if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(url)))
            {
                result = true;
            }
            return result;
        }

        public static string ConvertPriceToCommaSeperated(decimal value)
        {
            return Convert.ToInt64(value).ToString("###,###,###");
        }

        public static string DecimalFormatForRatingPrecision(decimal? rating)
        {
            return string.Format("{0:0.00}", rating);
        }

        public static string DecimalFormatForProductPrice(decimal? price)
        {
            return string.Format("{0:0}", price);
        }

        public static bool RemoteFileExistsUsingClient(string url)
        {
            bool result = false;
            using (WebClient client = new WebClient())
            {
                try
                {
                    Stream stream = client.OpenRead(url);
                    if (stream != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        public static bool RemoteFileExistsUsingHTTP(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";

                //Getting the Web Response
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                //Returns TURE if the Status code == 200
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public static System.Drawing.Image ResizeImageKeepAspectRatioHeight(System.Drawing.Image imgToResize, int Width)
        {
            float aspectRatio = (float)imgToResize.Width / (float)imgToResize.Height;
            float newHeight = (float)Width / (float)aspectRatio;
            //float newWidth = (float)Height * (float)aspectRatio;

            Bitmap b = new Bitmap(Width, (int)Math.Floor(newHeight));
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, Width, (int)Math.Floor(newHeight));
            g.Dispose();

            return (System.Drawing.Image)b;
        }

        public static System.Drawing.Image ResizeImageKeepAspectRatioWidth(System.Drawing.Image imgToResize, int Height)
        {
            float aspectRatio = (float)imgToResize.Width / (float)imgToResize.Height;
            //float newHeight = (float)Width / (float)aspectRatio;
            float newWidth = (float)Height * (float)aspectRatio;

            Bitmap b = new Bitmap((int)Math.Floor(newWidth), Height);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, (int)Math.Floor(newWidth), Height);
            g.Dispose();

            return (System.Drawing.Image)b;
        }

        public static System.Drawing.Image ImageCropAndResizeToFixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            //Resizing image to fixed size of width 775px/350px and constraint to the aspect ratio height of the image.
            System.Drawing.Image ResizedImg = ResizeImageKeepAspectRatioHeight(imgPhoto, Width);

            //If the image is of excessive height (>375px), cropping should be done equally from the top, and bottom of the picture.
            if (ResizedImg.Height > Height)
            {
                Bitmap croppedBitmap = new Bitmap(ResizedImg);
                int difference = ResizedImg.Height - Height;

                //int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));

                if (difference % 2 == 0) //difference Number is an Even Number
                {
                    int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));
                    //below code to crop from bottom
                    croppedBitmap = croppedBitmap.Clone(
                                    new Rectangle(0, 0, ResizedImg.Width, ResizedImg.Height - cropsize),
                                    System.Drawing.Imaging.PixelFormat.DontCare);

                    //below code to crop from top
                    croppedBitmap = croppedBitmap.Clone(
                                    new Rectangle(0, cropsize, ResizedImg.Width, croppedBitmap.Height - cropsize),
                                    System.Drawing.Imaging.PixelFormat.DontCare);
                }
                else //difference Number is an odd Number
                {
                    int basecropsize = Convert.ToInt32(Math.Ceiling(difference / (decimal)2));
                    int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));

                    //below code to crop from bottom
                    croppedBitmap = croppedBitmap.Clone(
                                    new Rectangle(0, 0, ResizedImg.Width, ResizedImg.Height - basecropsize),
                                    System.Drawing.Imaging.PixelFormat.DontCare);

                    //below code to crop from top
                    croppedBitmap = croppedBitmap.Clone(
                                    new Rectangle(0, cropsize, ResizedImg.Width, croppedBitmap.Height - cropsize),
                                    System.Drawing.Imaging.PixelFormat.DontCare);
                }

                return croppedBitmap;
            }
            else if (ResizedImg.Height < Height && ResizedImg.Width == 350)
            {
                //Resizing image to fixed size of Height 200px and constraint to the aspect ratio width of the image.
                System.Drawing.Image ResizeThumbImg = ResizeImageKeepAspectRatioWidth(imgPhoto, Height);

                //If the image is of excessive width (>350px), cropping should be done equally from the left, and right of the picture.
                if (ResizeThumbImg.Width > Width)
                {
                    Bitmap croppedBitmap = new Bitmap(ResizeThumbImg);
                    int difference = ResizeThumbImg.Width - Width;

                    //int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));

                    if (difference % 2 == 0) //difference Number is an Even Number
                    {
                        int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));
                        //below code to crop from left
                        croppedBitmap = croppedBitmap.Clone(
                                        new Rectangle(0, 0, ResizeThumbImg.Width - cropsize, ResizeThumbImg.Height),
                                        System.Drawing.Imaging.PixelFormat.DontCare);

                        //below code to crop from right
                        croppedBitmap = croppedBitmap.Clone(
                                        new Rectangle(cropsize, 0, croppedBitmap.Width - cropsize, ResizeThumbImg.Height),
                                        System.Drawing.Imaging.PixelFormat.DontCare);
                    }
                    else //difference Number is an odd Number
                    {
                        int basecropsize = Convert.ToInt32(Math.Ceiling(difference / (decimal)2));
                        int cropsize = Convert.ToInt32(Math.Floor(difference / (decimal)2));

                        //below code to crop from left
                        croppedBitmap = croppedBitmap.Clone(
                                        new Rectangle(0, 0, ResizeThumbImg.Width - basecropsize, ResizeThumbImg.Height),
                                        System.Drawing.Imaging.PixelFormat.DontCare);

                        //below code to crop from right
                        croppedBitmap = croppedBitmap.Clone(
                                        new Rectangle(cropsize, 0, croppedBitmap.Width - cropsize, ResizeThumbImg.Height),
                                        System.Drawing.Imaging.PixelFormat.DontCare);
                    }

                    return croppedBitmap;
                }//else If the image width is less than the mentioned width (<350px), then the image without stretching and maintaining its aspect ratio add black or white bars to the image
                else
                {
                    //Bitmap bmPhoto = FixedSize(imgPhoto, Width, Height);


                    int sourceWidth = imgPhoto.Width;
                    int sourceHeight = imgPhoto.Height;
                    int sourceX = 0;
                    int sourceY = 0;
                    int destX = 0;
                    int destY = 0;

                    float nPercent = 0;
                    float nPercentW = 0;
                    float nPercentH = 0;

                    nPercentW = ((float)Width / (float)sourceWidth);
                    nPercentH = ((float)Height / (float)sourceHeight);
                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentH;
                        destX = System.Convert.ToInt16((Width -
                        (sourceWidth * nPercent)) / 2);
                    }
                    else
                    {
                        nPercent = nPercentW;
                        destY = System.Convert.ToInt16((Height -
                        (sourceHeight * nPercent)) / 2);
                    }

                    int destWidth = (int)(sourceWidth * nPercent);
                    int destHeight = (int)(sourceHeight * nPercent);

                    Bitmap bmPhoto = new Bitmap(Width, Height,
                    PixelFormat.Format24bppRgb);
                    bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                    imgPhoto.VerticalResolution);

                    Graphics grPhoto = Graphics.FromImage(bmPhoto);
                    grPhoto.Clear(Color.White);
                    grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

                    grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

                    grPhoto.Dispose();
                    return bmPhoto;
                }
            }//else If the image height is less than the mentioned height (<375px), then the image without stretching and maintaining its aspect ratio add black or white bars to the image
            else
            {
                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;
                int sourceX = 0;
                int sourceY = 0;
                int destX = 0;
                int destY = 0;

                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                nPercentW = ((float)Width / (float)sourceWidth);
                nPercentH = ((float)Height / (float)sourceHeight);
                if (nPercentH < nPercentW)
                {
                    nPercent = nPercentH;
                    destX = System.Convert.ToInt16((Width -
                    (sourceWidth * nPercent)) / 2);
                }
                else
                {
                    nPercent = nPercentW;
                    destY = System.Convert.ToInt16((Height -
                    (sourceHeight * nPercent)) / 2);
                }

                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);

                Bitmap bmPhoto = new Bitmap(Width, Height,
                PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                imgPhoto.VerticalResolution);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.Clear(Color.White);
                grPhoto.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

                grPhoto.Dispose();
                return bmPhoto;
            }
        }

        public static System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
            PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
            imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode =
            InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }

        //for reference
        public static System.Web.Helpers.WebImage BestUsabilityCrop(System.Web.Helpers.WebImage image, decimal targetRatio)
        {
            decimal currentImageRatio = image.Width / (decimal)image.Height;
            int difference;

            //image is wider than targeted
            if (currentImageRatio > targetRatio)
            {
                int targetWidth = Convert.ToInt32(Math.Floor(targetRatio * image.Height));
                difference = image.Width - targetWidth;
                int left = Convert.ToInt32(Math.Floor(difference / (decimal)2));
                int right = Convert.ToInt32(Math.Ceiling(difference / (decimal)2));
                image.Crop(0, left, 0, right);
            }
            //image is higher than targeted
            else if (currentImageRatio < targetRatio)
            {
                int targetHeight = Convert.ToInt32(Math.Floor(image.Width / targetRatio));
                difference = image.Height - targetHeight;
                int top = Convert.ToInt32(Math.Floor(difference / (decimal)2));
                int bottom = Convert.ToInt32(Math.Ceiling(difference / (decimal)2));
                image.Crop(top, 0, bottom, 0);
            }
            return image;
        }

        public class chat
        {
            public int? ChatID { get; set; }
            public int? LeadId { get; set; }
            public int? ToId { get; set; }
            public int? FromId { get; set; }
            public DateTime? ChatDate { get; set; }
            public string ChatText { get; set; }
        }
    }
}