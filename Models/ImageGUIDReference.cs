using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace MySpace.Models
{
    ////////////////////////////////////////////////////////////////////////////////////
    // Gestion d'image sur disque référencée par un GUID
    //
    // Auteur : Nicolas Chourot
    ////////////////////////////////////////////////////////////////////////////////////
    public class ImageGUIDReference
    {
        public int MaxSize { get; set; }
        public bool HasThumbnail { get; set; }
        public int ThumbnailSize { get; set; }
        public String DefaultImage { get; set; }
        public String BasePath { get; set; }
        public ImageFormat imageFormat { get; set; }

        public ImageGUIDReference(String basePath, String defautImage, bool hasThumbnail = true)
        {
            this.BasePath = basePath;
            this.DefaultImage = defautImage;
            this.MaxSize = 4096;
            this.ThumbnailSize = 512;
            this.imageFormat = ImageFormat.Jpeg;
            this.HasThumbnail = hasThumbnail;
        }

        public String MakeUrl(String GUID, bool thumbnail = false)
        {
            String url;

            if (String.IsNullOrEmpty(GUID))
                url = "~" + BasePath + DefaultImage;
            else
                url = "~" + BasePath + (thumbnail ? @"Thumbnails/" : "") + GUID + "." + imageFormat.ToString();

            return url;
        }
        public String GetURL(String GUID, bool thumbnail = false)
        {
            String url = MakeUrl(GUID, thumbnail);

            if (thumbnail)
            {
                string imagePath = HttpContext.Current.Server.MapPath(url);
                if (!File.Exists(imagePath))
                {
                    url = MakeUrl(GUID);
                }
            }
            return url;
        }
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if ((image.Width > maxWidth) || (image.Height > maxHeight))
            {
                var ratioX = (double)maxWidth / image.Width;
                var ratioY = (double)maxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                var newImage = new Bitmap(newWidth, newHeight);

                using (var graphics = Graphics.FromImage(newImage))
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                return newImage;
            }
            return new Bitmap(image);
        }
        private void SaveImageFile(string GUID, string ImageData, bool thumbnail = false)
        {
            // Extract image data <MIME,DATA>
            string mime = ImageData.Split(',')[0];
            string data = ImageData.Split(',')[1];
            if (mime.IndexOf("webp") != -1)
            {
                // La classe Image ne supporte pas le format webp. Du coup pas possible de manipuler l'échelle pour créer un miniature.
                if (!thumbnail)
                {
                    var stream = new MemoryStream(Convert.FromBase64String(data));
                    FileStream file = new FileStream(HttpContext.Current.Server.MapPath(MakeUrl(GUID)), FileMode.Create, FileAccess.Write);
                    stream.WriteTo(file);
                    file.Close();
                    stream.Close();
                }
            }
            else
            {
                ImageFormat overrideFormat = (mime.IndexOf("png") != -1 ? ImageFormat.Png : imageFormat);
                var stream = new MemoryStream(Convert.FromBase64String(data));

                int maxSize = thumbnail ? ThumbnailSize : MaxSize;
                Image original = Image.FromStream(stream);

                // Limit size of image
                if ((original.Size.Width > maxSize) || (original.Size.Height > maxSize))
                    original = ScaleImage(original, maxSize, maxSize);
                original.Save(HttpContext.Current.Server.MapPath(MakeUrl(GUID, thumbnail)), overrideFormat);
            }

        }

        public String SaveImage(string ImageData, String PreviousGUID = "")
        {
            if (!string.IsNullOrEmpty(ImageData))
            {
                string imagePath = "";
                String GUID = "";
                if (!String.IsNullOrEmpty(PreviousGUID))
                {
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(MakeUrl(PreviousGUID)));
                    if (HasThumbnail)
                        System.IO.File.Delete(HttpContext.Current.Server.MapPath(MakeUrl(PreviousGUID, true /*thumbnail*/)));
                }
                do
                {
                    GUID = Guid.NewGuid().ToString();
                    imagePath = HttpContext.Current.Server.MapPath(GetURL(GUID));
                    // make sure new GUID does not already exists 
                } while (File.Exists(imagePath)); 

                SaveImageFile(GUID, ImageData);
                if (HasThumbnail)
                    SaveImageFile(GUID, ImageData, true /*thumbnail*/);

                return GUID;
            }
            return PreviousGUID;
        }
        public void Remove(String GUID)
        {
            if (!String.IsNullOrEmpty(GUID))
            {
                System.IO.File.Delete(HttpContext.Current.Server.MapPath(MakeUrl(GUID)));
                if (HasThumbnail)
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(MakeUrl(GUID, true /* thumbnail */)));
            }
        }
    }
}