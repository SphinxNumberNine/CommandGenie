using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using System.Web;
using System.Net;
using System.Xml;
using System.IO;

namespace ActionBasedEmails
{
    class EdgeUpload
    {

        private static string _authToken;
        private static string _deviceID;
        private static string _userGuid;
        private static string _userName;
       private static string _serviceURI = "http{0}://drive.commvault.com/webconsole/api/"; //add api
       // private static string _serviceURI = "http{0}://drive.commvault.com/"; //add api
        private static string _isSecureConnection;
        private static string _hostName;
        const string loginURI = "Login";
        const string post = "POST";
        const string acceptaHeaderValue = "text/xml, application/xml, application/xhtml+xml, text/html;q=0.9, text/plain;q=0.8, text/css, image/png, image/jpeg," +
              "image/gif;q=0.8, application/x-shockwave-flash, video/mp4;q=0.9, flv-application/octet-stream;q=0.8, video/x-flv;q=0.7, audio/mp4, application/futuresplash, */*;q=0.5";
        public const string HEADER_COOKIE2 = "Cookie2";
        const string chunkUploadURI = "drive/file/action/upload?uploadType=chunkedFile";
        const string uploadURI = "drive/file/action/upload?uploadType=fullFile&forceRestart=true";
        const string renewTokenURI = "RenewLoginToken";

        public EdgeUpload()
        {

        }

        public string ServiceURI
        {
              get { return string.Format(_serviceURI, _isSecureConnection, _hostName); }
        }


        public bool LoginRequest(string UserName, string PlainTextPassword, out string resultMsg)
          {
             bool result = false;
             _authToken = GetSessionToken(UserName, PlainTextPassword, out resultMsg);

             if (string.IsNullOrEmpty(_authToken))
                 return false;
             else
             {
                 result = true;
             }
              return result;
          }

        private string GetSessionToken(string userName, string password, out string resultMsg)
        {
              byte[] pwd = System.Text.Encoding.UTF8.GetBytes(password);
              String encodedPassword = Convert.ToBase64String(pwd, 0, pwd.Length, Base64FormattingOptions.None);
              if (string.IsNullOrEmpty(_deviceID))
                  _deviceID= Guid.NewGuid().ToString();
              //string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq mode=\"Webconsole\" username=\"{0}\" password=\"{1}\" />", userName, encodedPassword);
             // string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq mode=\"4\" flags=\"2\" deviceId=\"{0}\" username=\"{1}\" password=\"{2}\" clientType=\"4\"/>",
              //    _deviceID, userName, encodedPassword);
              string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq  username=\"{0}\" password=\"{1}\" />",
                    userName, encodedPassword);
  
              HttpWebResponse resp = SendRequest(ServiceURI + loginURI, post, null, loginReq, out resultMsg);

             // Stream temp = resp.GetResponseStream();
             // StreamReader reader = new StreamReader(temp);
             // String x = reader.ReadToEnd();
  
              //Check response code and check if the response has an attribute "token" set
              if (null != resp && resp.StatusCode == HttpStatusCode.OK && resp.ContentLength > 0)
              {
                  XmlDocument xmlDoc = new XmlDocument();
                  xmlDoc.Load(resp.GetResponseStream());
                  XmlNode authNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_CheckCredentialResp/@token");
                  if (null != authNode)
                  {
                      _authToken = authNode.Value;
                      authNode = null;
                      authNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_CheckCredentialResp/@userGUID");
                      if (null != authNode)
                      {
                          _userGuid = authNode.Value;
                      }
                      else
                      {
                          _userGuid = string.Empty;
                      }
                      authNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_CheckCredentialResp/@userName");
                      if (null != authNode)
                      {
                          _userName = authNode.Value;
                      }
                      else
                          _userName = string.Empty;
                  }
                  else
                  {
                      _authToken = string.Empty;
                  }
              }
              else
              {
                  _authToken = string.Empty;
                  if (null != resp)
                  {
                      resultMsg = string.IsNullOrEmpty(resp.StatusDescription) ? "Could not communicate to the server." : resp.StatusDescription;
                      return "";
                  }
              }
              return _authToken;
          }

        private HttpWebResponse SendRequest(string serviceURL, string httpMethod, string token, string requestBody, out string resultMsg)
        {
              try
              {
                  resultMsg = string.Empty;
                  HttpWebRequest req = WebRequest.Create(serviceURL) as HttpWebRequest;
                  req.Method = httpMethod;
                  req.ContentType = @"application/xml; charset=utf-8";
                  req.Accept = acceptaHeaderValue;
                  //build headers with the received token
                  if (!string.IsNullOrEmpty(token))
                      req.Headers.Add(HEADER_COOKIE2, token);
                  //req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_AUTH_TOKEN, token);
                  if (!string.IsNullOrEmpty(requestBody))
                      WriteRequest(req, requestBody);
  
                  //return req.GetResponse() as HttpWebResponse;
                  return (GetResponseWithoutException(req, string.Empty) as HttpWebResponse);
              }
              catch (WebException we)
              {
                  resultMsg = we.Message;
                  return we.Response as HttpWebResponse;
              }
        }

        private void WriteRequest(WebRequest req, string input)
        {
            req.ContentLength = Encoding.UTF8.GetByteCount(input);
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(Encoding.UTF8.GetBytes(input), 0, Encoding.UTF8.GetByteCount(input));
            }
        }

        private HttpWebResponse GetResponseWithoutException(HttpWebRequest request, string filePath)
        {
              const string function = "CVRESTAPI::GetResponseWithoutException";
              if (request == null)
              {
                  throw new ArgumentNullException("request");
              }
  
              try
              {
                  return request.GetResponse() as HttpWebResponse;
              }
              catch (WebException e)
              {
                  if (!e.Message.Contains("(404) Not Found.")) ;
                  return e.Response as HttpWebResponse;
              }
        }

        public void uploadFile(String fileName, String path)
        {
            byte[] fname = System.Text.Encoding.UTF8.GetBytes(fileName);
            String encryptedFileName =  Convert.ToBase64String(fname, 0, fname.Length, Base64FormattingOptions.None);
            ChunkData fileChunkData = new ChunkData(fileName, "\\command", encryptedFileName);
            fileChunkData.isChunkUpload = false;
            fileChunkData.FileSize = (new FileInfo(fileName)).Length;
            fileChunkData.Length = (int) (new FileInfo(fileName)).Length;
            String guid;
            long chunkOffset;
            bool isQuotaExceeded;
            this.UploadFile(fileChunkData, false, out guid, out chunkOffset, out isQuotaExceeded);

        }
        
        public bool UploadFile(ChunkData fileChunkData, bool forceRestart, out string fileGUID, out long chunkOffset, out bool isQuotaExceeded)
        {
            const string function = "CVRESTAPI::UploadFile";
            bool result = false;
            fileGUID = string.Empty; isQuotaExceeded = false;
            chunkOffset = 0;
            string uploadService;
            if (fileChunkData.isChunkUpload)
            {
                if (true == forceRestart)
                {
                    uploadService = ServiceURI + chunkUploadURI + "&forceRestart=true";
                }
                else
                {
                    uploadService = ServiceURI + chunkUploadURI + (!string.IsNullOrEmpty(fileChunkData.RequestID) ? string.Format("&requestId={0}", fileChunkData.RequestID) : "");
                }
            }
            else
            {
                uploadService = ServiceURI + uploadURI;
            }

            WebRequest req = WebRequest.Create(uploadService);
            req.Timeout = 3 * 60 * 1000; //Default is 90 secs. Overriding it as Germany users were getting timeouts.
            req.Method = post;

            // Build headers with the received token
            req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_AUTH_TOKEN, _authToken);
            if (fileChunkData.isChunkUpload)
            {
                // Below headers need to be send only for the first chunk
                if (string.IsNullOrEmpty(fileChunkData.RequestID))
                {
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILENAME, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileChunkData.FileName), Base64FormattingOptions.None));
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_PARENTFOLDERPATH, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileChunkData.UploadRelativePath), Base64FormattingOptions.None));
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILESIZE, fileChunkData.FileSize.ToString());
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILEMODTIME, ToUnixTime(fileChunkData.FileModTime).ToString());
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILEEOF, "0");
                }
                // For last chunk set the flag to 1
                if (fileChunkData.IsLastChunk)
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILEEOF, "1");
            }
            else
            {
                req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILENAME, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileChunkData.FileName), Base64FormattingOptions.None));
                req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_PARENTFOLDERPATH, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileChunkData.UploadRelativePath), Base64FormattingOptions.None));
                req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILESIZE, fileChunkData.FileSize.ToString());
                req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_FILEMODTIME, ToUnixTime(fileChunkData.FileModTime).ToString());
            }

            HttpWebResponse ClientResp = null;
            XmlDocument xmlDoc = null;
            try
            {
                req.ContentLength = fileChunkData.Length;

                String x = req.Headers.ToString();

                
                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(fileChunkData.ReadBytes(), 0, fileChunkData.Length);
                }

                //Logger.Verbose("Reading response for file -" + FileChunkData.FilePath, function);
                ClientResp = GetResponseWithoutException((HttpWebRequest)req, fileChunkData.FilePath);

                
                try
                {
                    xmlDoc = new XmlDocument();
                    using (Stream stream = ClientResp.GetResponseStream()) // It seems in continuous uploads HttpWebResponse is not getting free and results in operation timeouts, so use USING statement.
                    {
                        xmlDoc.Load(stream);
                    }
                }
                catch { }
                if (null != ClientResp && (ClientResp.StatusCode == HttpStatusCode.OK || ClientResp.StatusCode == HttpStatusCode.Conflict))
                {
                    result = true;

                    // read the chunkoffset
                    XmlNode offsetNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@chunkOffset");
                    if (null != offsetNode)
                    {
                        chunkOffset = Convert.ToInt64(offsetNode.Value);
                        offsetNode = null;
                    }

                    if (string.IsNullOrEmpty(fileChunkData.RequestID) && fileChunkData.isChunkUpload)
                    {
                        XmlNode requestIDNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@requestId");
                        if (null != requestIDNode)
                        {
                            fileChunkData.RequestID = requestIDNode.Value;
                            requestIDNode = null;
                        }
                    }
                    if (fileChunkData.IsLastChunk || !fileChunkData.isChunkUpload)
                    {
                        XmlNode guidNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@fileGUID");
                        if (null != guidNode)
                        {
                            fileGUID = guidNode.Value;
                            guidNode = null;
                        }
                            //Logger.Error("Upload failed! Server error-" + xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorString").Value, "CVAPI::UploadFile");
                    }
                }
                else if (ClientResp != null)
                {
                    int errorCode = 0;
                    string errorString = string.Empty;
                    XmlNode errorCodeNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorCode");
                    XmlNode errorStringNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorString");
                    if (ClientResp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //Logger.Info("Token expired. Renewing token.", function);
                        RenewToken(out errorString /*not using it anywhere*/);
                    }
                    else
                    {
                        if (ClientResp.StatusCode == HttpStatusCode.RequestEntityTooLarge /*Quota Exceeded*/)
                        {
                            isQuotaExceeded = true;
                        }

                        if (null != errorCodeNode)
                        {
                            errorCode = Convert.ToInt32(errorCodeNode.Value);
                            if (null != errorStringNode)
                                errorString = errorStringNode.Value;
                        }

                        //Logger.Error(string.Format("Error [{0}] errorCode [{1}] status [{2}]", errorString, errorCode, Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode())), function);
                    }
                }
            }
            catch (WebException we)
            {
                //Logger.Error(we.Message + "-" + fileChunkData.FilePath, function);
            }
            finally
            {
                if (null != ClientResp)
                {
                    ClientResp.Close();
                    ClientResp = null;
                }
                if (null != xmlDoc)
                    xmlDoc = null;
                req = null;
            }
            return result;
        }

        public class ChunkData : IDisposable
        {
            private FileStream fs = null;
            public long StartIndex;
            public int Length;
            public string RequestID;
            public string FilePath = string.Empty;
            public string FileName;
            public long FileSize;
            public DateTime FileModTime;
            public string UploadRelativePath;
            public bool IsLastChunk = false;
            public bool isChunkUpload = false;
            public String encryptedFileName;

            /// <summary>
            /// Use this constructor for desktop content upload.
            /// </summary>
            /// <param name="filePath">Fully qualified filepath</param>
            /// <param name="relativePath">edge drive relative path to upload</param>
            public ChunkData(string filePath, string relativePath, string encryptedFileName)
            {
                //_file = FileInfo;
                UploadRelativePath = relativePath;
                this.encryptedFileName = encryptedFileName;

                // Read all bytes using memorystream. It reads even when the file is locked.
                //_bytedata = default(byte[]);
                FilePath = filePath;
                FileInfo f = new FileInfo(filePath);
                FileName = f.Name;
                FileSize = f.Length;
                FileModTime = f.LastWriteTimeUtc;
                //using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //{
                //    using (MemoryStream ms = new MemoryStream())
                //    {
                //        fs.CopyTo(ms);
                //        _bytedata = new byte[ms.Length];
                //        _bytedata = ms.ToArray();
                //        ms.Dispose();
                //    }
                //    fs.Dispose();
                //}
            }

            /// <summary>
            /// Use this constructor for the device content upload.
            /// </summary>
            /// <param name="byteData"></param>
            /// <param name="fileName"></param>
            /// <param name="fileSize"></param>
            /// <param name="filemodifiedTime"></param>
            /// <param name="relativePath"></param>
            public ChunkData(byte[] byteData, string fileName, long fileSize, DateTime filemodifiedTime, string relativePath)
            {
                //_bytedata = byteData;
                FileName = fileName;
                FileSize = fileSize;
                FileModTime = filemodifiedTime;
                UploadRelativePath = relativePath;
            }

            public byte[] ReadBytes()
            {
                byte[] bytedata = new byte[Length]; ;
                if (null == fs)
                {
                    fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                //using (MemoryStream ms = new MemoryStream())
                //{
                //fs.CopyTo(ms);
                fs.Read(bytedata, 0, Length);
                //ms.Dispose();
                //}
                return bytedata;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing && null != fs)
                {
                    fs.Dispose();
                }
            }
        }

        class Constants
        {
            public const string DeviceFolderName = "Mobile Devices(Cameras & Storage cards)";
            public static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Commvault\\EdgeUploader\\";
            public const string DriveFolderName = "Drive";
            public const string DriveRootPath = "\\";
            public const uint MaxFileSize = 5;
            public const int EV_UPLOAD = 5;

            public struct ServiceHeaderConstants
            {
                public const string HEADER_COOKIE = "Cookie";
                public const string HEADER_COOKIE2 = "Cookie2";
                public const string HEADER_ACCEPT = "Accept";
                public const string HEADER_SHARE_TOKEN = "SharedTokenID";
                public const string HEADER_CONTENT_TYPE = "Content-Type";
                public const string HEADER_CLIENT_LOCATION = "client-location";
                public const string HEADER_AUTH_TOKEN = "Authtoken";
                public const string HEADER_FORMAT_OUTPUT = "FormatOutput";
                public const string HEADER_LOCALE = "locale";
                public const string HEADER_FILENAME = "FileName";
                public const string HEADER_PARENTFOLDERPATH = "ParentFolderPath";
                public const string HEADER_FILESIZE = "FileSize";
                public const string HEADER_FILEMODTIME = "FileModifiedtime";
                public const string HEADER_FILEEOF = "FileEOF";
            }

            public struct DBQueryConstants
            {
                public const string SELECT_USER_EXIST = "SELECT userid FROM user WHERE WindowsAccountName='{0}' AND CommvaultUserName='{1}';";
                public const string SELECT_USER_EXIST_COND = "SELECT UserGuid, LastUsedServerName, UseSecureConnection, DeviceID, AuthToken, IsSignedIn, State FROM user WHERE WindowsAccountName='{0}' AND CommvaultUserName='{1}';";
                //            public const string SELECT_FILE_TOUPLOAD = @"SELECT f2.FileFRN, f2.ParentFRN, f2.FileName, f2.UploadRetryCount, f2.Size, f1.TotalSize 
                //                FROM (SELECT sum(size) as TotalSize FROM (SELECT Size from file WHERE (Flag=0 OR Flag=2) AND size>0 ORDER BY ModifiedTime DESC LIMIT {0})) as f1, 
                //                (SELECT FileFRN, ParentFRN, FileName, UploadRetryCount, Size FROM File WHERE (Flag=0 OR Flag=2) AND size>0 ORDER BY ModifiedTime DESC LIMIT {0}) as f2;";
                public const string SELECT_FILE_TOUPLOAD = @"SELECT f2.FileFRN, f2.ParentFRN, f2.FileName, f2.UploadRetryCount, f2.Size, f1.TotalSize 
                FROM (SELECT sum(size) as TotalSize FROM (SELECT fi.Size from file fi INNER JOIN Folder fo ON fi.ParentFRN=fo.FolderFRN WHERE (fi.Flag=0 OR fi.Flag=2) AND fo.Flag=1 AND fi.UploadRetryCount < {2}
                    AND strftime('%s','now', '-{3} minute') > fi.ModifiedTime AND fi.size>0 {1} ORDER BY fi.ModifiedTime DESC LIMIT {0})) as f1, 
                (SELECT fi.FileFRN, fi.ParentFRN, fi.FileName, fi.UploadRetryCount, fi.Size FROM File fi INNER JOIN Folder fo ON fi.ParentFRN=fo.FolderFRN WHERE (fi.Flag=0 OR fi.Flag=2) AND fo.Flag=1 AND fi.UploadRetryCount < {2}
                    AND strftime('%s','now', '-{3} minute') > fi.ModifiedTime AND fi.size>0 {1} ORDER BY fi.ModifiedTime DESC LIMIT {0}) as f2;";

                public const string SELECT_FILE_REMAINING = @"SELECT Count(*) as Total, sum(fi.Size) as TotalSize FROM FILE fi INNER JOIN Folder fo ON fo.FolderFRN = fi.ParentFRN
                WHERE fi.Flag!=3 AND fi.size>0 AND fo.Flag=1;";

                public const string SELECT_FILE_REMAINING1 = @"SELECT Count(*) as Total, sum(Size) as TotalSize FROM (select fi.FileName, fi.Size from File fi INNER JOIN Folder fo ON fo.FolderFRN = fi.ParentFRN 
                Where fi.Flag!=3 AND fo.Flag=1 AND fi.Size>0 AND strftime('%s','now', '-{1} minute') > fi.ModifiedTime {0} UNION ALL select FileName, Size FROM UsbdeviceContent WHERE Flag!=3 AND Size>0 
                    AND IsDownloaded=1 AND strftime('%s','now', '-{1} minute') > ModifiedTime );";

                public const string SELECT_FILE_EXIST = "SELECT Count(1) as Total,FileFRN, ParentFRN, ModifiedTime, FileName FROM File WHERE FileFRN='{0}';";
                public const string SELECT_FILE_EXIST1 = "SELECT Count(1) as Total,FileFRN FROM File WHERE ParentFRN='{0}' AND (FileName='{1}' OR FileName='{2}');";
                public const string SELECT_FOLDER_SHARED = "SELECT LocalPath, Flag FROM Folder WHERE ParentFRN='0';";
                public const string SELECT_FOLDER_SHARED1 = @"SELECT f.LocalPath, f.Flag, m.UploadPath FROM Folder f INNER JOIN FolderScanMeta m on f.FolderFRN=m.FolderFRN;";
                public const string SELECT_FOLDER_SCANTIME = "SELECT LastScanTime FROM FolderScanMeta NOLOCK WHERE FolderFRN='{0}';";
                public const string SELECT_FOLDER_EXIST = "SELECT FolderFRN, ParentFRN, Flag FROM Folder NOLOCK WHERE FolderFRN='{0}';";
                public const string SELECT_FOLDER_EXIST_BY_PATH = "SELECT FolderFRN, ParentFRN, Flag FROM Folder NOLOCK WHERE ParentFRN=0 AND LocalPath='{0}';";
                public const string SELECT_FOLDERSCANMETA_UPLOADPATH = "select fs.UploadPath FROM FolderscanMeta fs INNER JOIN Folder f ON fs.FolderFRN=f.FolderFRN WHERE f.LocalPath='{0}';";
                public const string SELECT_FILTER = "SELECT FileFilter,DirectoryFilter FROM Filter WHERE UserID='{0}';";
                public const string CTE_FOLDER_CHILDS = @"WITH RECURSIVE FRN(FolderFRN) AS (SELECT FolderFRN FROM Folder NOLOCK Where FolderFRN='{0}' UNION ALL 
                SELECT fo.FolderFRN FROM Folder fo INNER JOIN FRN fi ON fi.FolderFRN=fo.ParentFRN) Select FolderFRN FROM FRN NOLOCK;";
                public const string CTE_FOLDER_HIERARCHY = @"WITH RECURSIVE FilePath(LocalPath, ParentFRN) AS (SELECT LocalPath, ParentFRN FROM Folder NOLOCK Where FolderFRN='{0}' 
                UNION ALL SELECT fo.LocalPath, fo.ParentFRN FROM Folder fo INNER JOIN FilePath fi ON fo.FolderFRN=fi.ParentFRN) Select LocalPath FROM FilePath NOLOCK;";

                public const string SELECT_USBDEVICE_EXIST = "SELECT UID FROM UsbDevice WHERE UserID='{0}' AND DeviceID='{1}';";
                public const string SELECT_USBDEVICECONTENT_EXIST = @"SELECT dc.FileID,dc.ParentID,dc.FriendlyName,dc.ModifiedTime,d.UID,d.LastScanTime FROM UsbDeviceContent dc INNER JOIN UsbDevice d 
                ON dc.UID=d.UID WHERE FileID='{0}' AND FileName='{1}';";

                public const string SELECT_USBDEVICECONTENT_UPLOAD = @"SELECT f2.UID,f2.FileID,f2.Name as DeviceName, f2.FriendlyName, f2.UploadRetryCount, f2.Size, f1.TotalSize
                FROM    (SELECT sum(size) as TotalSize FROM (SELECT Size from UsbDeviceContent WHERE Flag!=3 AND size>0 AND IsDownloaded=1 AND UploadRetryCount < {1} AND strftime('%s','now', '-{2} minute') > ModifiedTime  ORDER BY ModifiedTime DESC LIMIT {0})) as f1, 
                        (SELECT udc.FileID,udc.FriendlyName, udc.UploadRetryCount, udc.Size, udc.UID, ud.Name FROM UsbDeviceContent udc INNER JOIN UsbDevice ud ON udc.UID=ud.UID WHERE 
                            udc.Flag!=3 AND udc.size>0 AND udc.IsDownloaded=1 AND udc.UploadRetryCount < {1} AND strftime('%s','now', '-{2} minute') > udc.ModifiedTime ORDER BY udc.ModifiedTime DESC LIMIT {0}) as f2;";

                public const string DELETE_FOLDER = "DELETE FROM Folder;";
                public const string DELETE_FOLDER_FRNCOND = "DELETE FROM Folder WHERE FolderFRN='{0}';";
                public const string DELETE_FOLDER_COND = "DELETE FROM Folder WHERE FolderFRN IN ({0});";
                public const string DELETE_FILE = "DELETE FROM File;";
                public const string DELETE_FILE_FILEFRNCOND = "DELETE FROM File WHERE FileFRN='{0}';";
                public const string DELETE_FILE_COND = "DELETE FROM File WHERE ParentFRN IN ({0});";
                public const string DELETE_FOLDERSCANMETA_COND = "DELETE FROM FolderScanMeta WHERE FolderFRN='{0}';";
                public const string DELETE_FOLDERSCANMETA = "DELETE FROM FolderScanMeta;";
                public const string DELETE_USBDEVICECONTENT_COND = "DELETE FROM UsbDeviceContent WHERE UID={0} AND FileName='{1}';";
                public const string DELETE_USBDEVICE = "DELETE FROM UsbDevice;";
                public const string DELETE_USBDEVICECONTENT = "DELETE FROM UsbDeviceContent;";

                public const string INSERT_FILTER = "INSERT OR IGNORE INTO Filter VALUES({0},'{1}','{2}');";
                public const string UPDATE_FILTER = "UPDATE Filter SET FileFilter='{0}', DirectoryFilter='{1}' WHERE UserID='{2}';";
            }

            public struct MessageConstants
            {
                public const string BALLOON_TIP_DEVICE_CONNECT = "Auto uploading pictures and videos from '{0}' device.\nPlease do not remove.";
                public const string BALLOON_TIP_TITLE = "Commvault edge drive uploader";
                public const string DIALOGBOX_LABEL_MESSAGE = "A new portable device \"{0}\" has been attached.\n\nWould you like to upload pictures & videos from this and all other devices in future.";
                public const string DIALOGBOX_LABEL_MESSAGE1 = "Portable devices found attched.\n\nWould you like to upload pictures & videos from these and all other devices in future.";
                public const string BALLOON_TIP_DEVICE_REMOVE = "You can remove '{0}' device now.";
                public const string ABOUT_MESSAGE = "                   Commvault Edge Drive Uploader Version {0}\r\n         Copyright 2014, Commvault, Solutions. All rights reserved.\r\n   This program is protected by U.S. and international copyright law.";
                public const string ABOUT_HEADING = "About Commvault Edge Drive Uploader";
                public const string EDGE_VERIFICATION_FAILED = "Upload capability not present for this user. Please contact administrator.";
                public const string EDGE_VERIFICATION_CALL_FAILED = "Application rights validation failed. For more details, please see logs.\r\n";
                public const string SERVER_NOT_RESPONDING = "Server is not responding. Please contact administrator.\r\n";
                public const string QUOTA_FULL_MESSAGE = "Your edge drive quota has been reached to its limitation. Please free up some space and then resume upload.";
                public const string QUOTA_FULL_STATUS_MESSAGE = "Edge drive is full.";
            }
        }

        public bool RenewToken(out string resultMsg)
        {
            const string function = "CVAPI::RenewToken";
            bool success = false;
            string logoutService = ServiceURI + renewTokenURI;
            string requestBody = string.Format("<DM2ContentIndexing_RenewSessionReq deviceId =\"{0}\" sessionId=\"{1}\" />", _deviceID, _authToken);
            HttpWebResponse ClientResp = SendRequest(logoutService, post, _authToken, requestBody, out resultMsg);
            if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ClientResp.GetResponseStream());
                XmlNode node = xmlDoc.SelectSingleNode("/DM2ContentIndexing_RenewSessionResp/error/@errLogMessage");
                if (null != node)
                {
                    if (node.InnerText.ToLower().Contains("renewal not needed"))
                    {
                        success = true;
                    }
                }
                else
                {
                    // Check if token has been reviewed
                    node = xmlDoc.SelectSingleNode("/DM2ContentIndexing_RenewSessionResp/@token");
                    if (null != node)
                    {
                        _authToken = node.Value;
                        Dictionary<String, object> data = new Dictionary<string, object>(), condition = new Dictionary<string, object>();
                        data["LastAccessTime"] = Helper.UnixTime(DateTime.Now.ToUniversalTime()); ;
                        data["AuthToken"] = _authToken;
                        condition["CommvaultUserName"] = "cramaswamy@commvault.com"; //Properties.Settings.Default.UserName;
                        condition["WindowsAccountName"] = Environment.UserName;

                        /**if (Sync.Update("User", data, condition, true) > 0)
                        {
                            //Logger.Verbose("Renewed auth token and updated into the database.", function);
                            success = true;
                        }*/
                    }
                }
            }
            else if (null != ClientResp)
            {
                //Logger.Error(string.Format("RenewToken failed -{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
            }
            return success;
        }

        class Helper
        {
            // Checks if the folder is a sub folder of the parent
            /// <summary>
            /// Determines if the folder is sub-folder
            /// </summary>
            /// <param name="parentPath">parent folder to be cgecked against.</param>
            /// <param name="childPath">folder to be checked as sub-folder.</param>
            /// <returns>TRUE if it is the sub-folder</returns>
            public static bool IsSubfolder(string parentPath, string childPath)
            {
                const string function = "Helper::IsSubfolder";
                try
                {
                    var parentUri = new Uri(parentPath);
                    var childUri = new DirectoryInfo(childPath).Parent;
                    while (childUri != null)
                    {
                        if (new Uri(childUri.FullName) == parentUri)
                        {
                            return true;
                        }

                        childUri = childUri.Parent;
                    }
                }
                catch (Exception e)
                {
                    //Logger.Warning(string.Format("{0}--ParentPath:{1},ChildPath:{2}", e.Message, parentPath, childPath), function);
                }
                return false;
            }

            /// <summary>
            /// Converts DateTime to Unix Time
            /// </summary>
            /// <param name="UtcTime">UTC Time to be converted</param>
            /// <returns>returns total seconds</returns>
            public static long UnixTime(DateTime UtcTime)
            {
                return (long)Math.Truncate((UtcTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            }
        }

         private long ToUnixTime(DateTime UtcTime)
         {
              var timeSpan = (UtcTime - new DateTime(1970, 1, 1, 0, 0, 0));
              return (long)timeSpan.TotalSeconds;
         }
    }
}
