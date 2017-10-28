using System;
using System.Collections.Generic;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;

namespace Common
{
    class GoogleDriveHelper
    {

        /// <summary>
        /// Download a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/get
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_fileResource">File resource of the file to download</param>
        /// <param name="_saveTo">location of where to save the file including the file name to save it as.</param>
        /// <returns></returns>
        public static Boolean downloadFile(DriveService _service, File _fileResource, string _saveTo)
        {

            if (!String.IsNullOrEmpty(_fileResource.DownloadUrl))
            {
                try
                {
                    var x = _service.HttpClient.GetByteArrayAsync(_fileResource.DownloadUrl);
                    byte[] arrBytes = x.Result;
                    System.IO.File.WriteAllBytes(_saveTo, arrBytes);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return false;
            }
        }


        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        /// <summary>
        /// Uploads a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_uploadFile">path to the file to upload</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <returns>If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null</returns>
        public static File uploadFile(DriveService _service, string _uploadFile, string _parent)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Title = System.IO.Path.GetFileName(_uploadFile);
                body.Description = "File uploaded by Drive Sample";
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = _service.Files.Insert(body, stream, GetMimeType(_uploadFile));
                    //request.Convert = true;   // uncomment this line if you want files to be converted to Drive format
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }
        public static byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        } 

        /// <summary>
        /// Updates a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/update
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_uploadFile">path to the file to upload</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <param name="_fileId">the resource id for the file we would like to update</param>                      
        /// <returns>If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null</returns>
        public static File updateFile(DriveService _service, string _uploadFile, string _parent, string _fileId)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Title = System.IO.Path.GetFileName(_uploadFile);
                body.Description = "File updated by Drive Sample";
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.UpdateMediaUpload request = _service.Files.Update(body, _fileId, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

        }


        /// <summary>
        /// Create a new Directory.
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_title">The title of the file. Used to identify file or folder name.</param>
        /// <param name="_description">A short description of the file.</param>
        /// <param name="_parent">Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.</param>
        /// <returns></returns>
        public static File createDirectory(DriveService _service, string _title, string _description, string _parent)
        {

            File NewDirectory = null;

            // Create metaData for a new Directory
            File body = new File();
            body.Title = _title;
            body.Description = _description;
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };
            try
            {
                FilesResource.InsertRequest request = _service.Files.Insert(body);
                NewDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return NewDirectory;
        }


        /// <summary>
        /// List all of the files and directories for the current user.  
        /// 
        /// Documentation: https://developers.google.com/drive/v2/reference/files/list
        /// Documentation Search: https://developers.google.com/drive/web/search-parameters
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>        
        /// <param name="search">if Search is null will return all files</param>        
        /// <returns></returns>
        public static IList<File> GetFiles(DriveService service, string search)
        {

            IList<File> Files = new List<File>();

            try
            {
                //List all of the files and directories for the current user.  
                // Documentation: https://developers.google.com/drive/v2/reference/files/list
                FilesResource.ListRequest list = service.Files.List();
                list.MaxResults = 10000;
                if (search != null)
                {
                    list.Q = search;
                }
                FileList filesFeed = list.Execute();

                //// Loop through until we arrive at an empty page
                while (filesFeed.Items != null)
                {
                    // Adding each item  to the list.
                    foreach (File item in filesFeed.Items)
                    {
                        Files.Add(item);
                    }

                    // We will know we are on the last page when the next page token is
                    // null.
                    // If this is the case, break.
                    if (filesFeed.NextPageToken == null)
                    {
                        break;
                    }

                    // Prepare the next page of results
                    list.PageToken = filesFeed.NextPageToken;

                    // Execute and process the next page request
                    filesFeed = list.Execute();
                }
            }
            catch (Exception ex)
            {
                // In the event there is an error with the request.
                Console.WriteLine(ex.Message);
            }
            return Files;
        }

        /// <summary>
        /// Share content. Doc link: https://developers.google.com/drive/v2/reference/permissions/insert
        /// </summary>  
        public static void share(DriveService service, string fileId, string value, string type, string role)
        {
            var permission = new Permission { Value = value, Type = type, Role = role };
            service.Permissions.Insert(permission, fileId).Execute();
        }

        /// <summary>
        /// get App Folder Id, if folder not exist, create & share
        /// </summary>
        /// <param name="service">a Valid authenticated DriveService</param>
        /// <param name="appName">application folder name</param>
        /// <returns>application folder id</returns>
        public static string getAppFolderId(DriveService service, string appName)
        {
            string appFolderId = "";
            IList<File> appFolders = GoogleDriveHelper.GetFiles(service, "title = '" + appName + "'");
            if (appFolders.Count == 0)
            {
                // folder's not exist
                GoogleDriveHelper.createDirectory(service, appName, "folder description", "root");
                // reload 
                System.Threading.Thread.Sleep(5000);
                appFolders = GoogleDriveHelper.GetFiles(service, "title = '" + appName + "'");
            }
            foreach (File item in appFolders)
            {
                Console.WriteLine("File name ='" + item.Title + "'||'" + item.MimeType + "'" + "'||'" + item.Id + "'");
                appFolderId = item.Id;
            }
            GoogleDriveHelper.share(service, appFolderId, "", "anyone", "reader");
            return appFolderId;
        }

        public static void printAbout(DriveService service)
        {
            try
            {
                About about = service.About.Get().Execute();

                Console.WriteLine("Current user name: " + about.Name);
                Console.WriteLine("Root folder ID: " + about.RootFolderId);
                Console.WriteLine("Total quota (bytes): " + about.QuotaBytesTotal);
                Console.WriteLine("Used quota (bytes): " + about.QuotaBytesUsed);
                // TOTAL - USED = FREE SPACE
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("An error occurred: " + e);
            }
        }

        public static bool isUpload(DriveService service, long sizeFile)
        {
            try
            {

                About about = service.About.Get().Execute();
                if (about.QuotaBytesTotal - about.QuotaBytesUsed > 0)
                {
                    return true;
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("An error occurred: " + e);
            }
            return false;
        }
    }
}
