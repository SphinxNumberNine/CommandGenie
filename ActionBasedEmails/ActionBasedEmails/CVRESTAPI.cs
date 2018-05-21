#region Using Directives
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.Xml;
using System.IO;
using System.Text;
#endregion Using Directives

namespace EdgeUploader
{
    /// <summary>
    /// Helper class to call CV rest APIs
    /// </summary>
    class CVAPI
    {
        #region Variables
        //private static string _serviceURI = "http{0}://{1}/webconsole/RestServlet/";
        private static string _serviceURI = "http{0}://{1}/webconsole/api/";
        //static string _serviceURI = "http{0}://{1}/SearchSvc/CVWebService.svc/";
        const string uploadURI = "drive/file/action/upload?uploadType=fullFile&forceRestart=true";
        const string chunkUploadURI = "drive/file/action/upload?uploadType=chunkedFile";
        const string getApplicationAccess = "drive";
        const string loginURI = "Login";
        const string logoutURI = "Logout";
        const string renewTokenURI = "RenewLoginToken";
        const string createFolderURI = "drive/folder";
        const string browseFolderURI = "service/Browse/SYNCFOLDER";
        const string getVersionURI = "GetVersionInfo";
        const string getGuidURI = "drive/action/metadata?path={0}";
        const string post = "POST";
        const string get = "GET";
        const int CV_STATUS_EDGE_DRIVE = 0x10000000; // Edge Drive Pseudo Client
        const string acceptaHeaderValue = "text/xml, application/xml, application/xhtml+xml, text/html;q=0.9, text/plain;q=0.8, text/css, image/png, image/jpeg," +
            "image/gif;q=0.8, application/x-shockwave-flash, video/mp4;q=0.9, flv-application/octet-stream;q=0.8, video/x-flv;q=0.7, audio/mp4, application/futuresplash, */*;q=0.5";
        const string browseRequestBody = @"<databrowse_WebBrowseRequest clientId=""{0}"" retrieveFacets=""false"" clientName=""Drive"" cloudId="""" instanceId=""{1}"" subclientId=""{2}"" 
                findQuery=""false"" applicationId=""{3}"" foldersOnly=""true"" filesOnly=""false"" viewVersions=""false"" userGuid=""{4}"" backupsetId=""{5}"" 
                showDeletedItems=""0"" browsePath=""{6}"" fromTime=""0"" toTime=""0"" appType=""0""  startIndex=""0"" searchKeywords="""" pageSize=""100"" contentIndexingEnabled=""1"" 
                overridePath=""{6}""><browseRequestPagingMetaInfo/><facetRequests></facetRequests><sortFields name='Flags' descending='0'/><sortFields name='FileName' descending='0'/>
                </databrowse_WebBrowseRequest>";
        private static string _authToken;
        private static string _deviceID;
        private static string _rootNodeGUID;
        private static string _hostName;
        private static string _isSecureConnection;
        private static string _userGuid;
        private static EdgeClient _clientDetails;
        private static string _userName;
        #endregion Variables

        public CVAPI()
        {
            //System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            //    delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            //            System.Security.Cryptography.X509Certificates.X509Chain chain,
            //            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            //            {
            //                return true; // **** Always accept
            //            };
        }

        #region Properties
        /// <summary>
        /// Get the AuthToken for the user
        /// </summary>
        public string AuthToken
        {
            get { return _authToken; }
            set { _authToken = value; }
        }

        /// <summary>
        /// Gets the deviceID for the login session
        /// </summary>
        public string DeviceID
        {
            get { return _deviceID; }
            set { _deviceID = value; }
        }

        public string ServiceURI
        {
            get { return string.Format(_serviceURI, _isSecureConnection, _hostName); }
        }

        public string Hostname
        {
            get { return _hostName; }
            set { _hostName = value; }
        }

        public string UserGUID
        {
            get { return _userGuid; }
            set { _userGuid = value; }
        }

        public string IsSecureConnection
        {
            get { return _isSecureConnection; }
            set { _isSecureConnection = value; }
        }

        public string UserName
        { get { return _userName; } }

        /// <summary>
        /// Gets the GUID of the Root node of the Drive
        /// </summary>
        public string RootNodeGUID
        {
            get 
            {
                bool fDummy = false;
                _rootNodeGUID = GetGuidOfDirectory("\\", ref fDummy); return _rootNodeGUID; 
            }
        }
        #endregion Properties

        #region Public Methods
        /// <summary>
        /// POST login request to rest API
        /// </summary>
        /// <param name="UserName">Username to use for the login</param>
        /// <param name="PlainTextPassword">Plain text password</param>
        /// <returns></returns>
        public bool LoginRequest(string UserName, string PlainTextPassword, out string resultMsg)
        {
            bool result = false;
            _authToken = GetSessionToken(UserName, PlainTextPassword, out resultMsg);

            if (string.IsNullOrEmpty(_authToken))
                Logger.Error("Login Failed", "CVAPI::LoginRequest");
            else
            {
                Logger.Info("Login Successful", "CVAPI::LoginRequest");
                result = true;
            }
            return result;
        }

        /// <summary>
        /// POST logout request to rest API
        /// </summary>
        /// <returns></returns>
        public bool Logout()
        {
            bool success = false;
            string requestBody = string.Empty;
            HttpWebResponse ClientResp = SendRequest(ServiceURI + logoutURI, post, _authToken, requestBody);
            if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
            {
                success = true;
                _authToken = string.Empty;
            }
            else
            {
                Logger.Error("Logout failed", "CVAPI::Logout");
            }

            return success;
        }

        /// <summary>
        /// Renews the token
        /// </summary>
        /// <returns></returns>
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
                        Dictionary<String, object> data = new Dictionary<string,object>(), condition = new Dictionary<string,object>();
                        data["LastAccessTime"] = Helper.UnixTime(DateTime.Now.ToUniversalTime()); ;
                        data["AuthToken"] = _authToken;
                        condition["CommvaultUserName"] = Properties.Settings.Default.UserName;
                        condition["WindowsAccountName"] = Environment.UserName;

                        if (Sync.Update("User", data, condition, true) > 0)
                        {
                            Logger.Verbose("Renewed auth token and updated into the database.", function);
                            success = true;
                        }
                        else
                            Logger.Verbose("Renewed auth token but failed to update in the database.", function);
                    }
                }
            }
            else if (null != ClientResp)
            {
                Logger.Error(string.Format("RenewToken failed -{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
            }
            else
                Logger.Error("RenewToken failed.", function);
            return success;
        }

        /// <summary>
        /// Gets the GUID of the directory
        /// </summary>
        /// <param name="DirectoryPath">Valid directory path</param>
        /// <returns>GUID of the directory</returns>
        public string GetGuidOfDirectory(string DirectoryPath, ref bool isCallFailed)
        {
            const string function = "CVRESTAPI::GetGuidOfDirectory";
            // Need to prepend a "\" to denote the relative path
            //string uri = ServiceURI + string.Format(getGuidURI, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(@"\" + DirectoryPath)));
            string uri = ServiceURI + string.Format(getGuidURI, HttpUtility.UrlEncode(@"\" + DirectoryPath));
            string guid = string.Empty;
            
            HttpWebResponse ClientResp = SendRequest(uri, get, _authToken, null);
            if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ClientResp.GetResponseStream());
                XmlNode guidNode = xmlDoc.SelectSingleNode("/App_FileResourceResponse/fileResource/@GUID");
                if (null != guidNode)
                    guid = guidNode.Value;
            }
            else if (ClientResp != null)
            {
                if (ClientResp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string resultMsg;
                    RenewToken(out resultMsg);
                }
                else
                {
                    if ((HttpStatusCode.OK != ClientResp.StatusCode) && (HttpStatusCode.NotFound != ClientResp.StatusCode))
                    {
                        isCallFailed = true;
                    }

                    Logger.Error(string.Format("{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
                }
            }
            return guid;
        }

        /// <summary>
        /// Gets the list of sub-folders inside a folder
        /// </summary>
        /// <param name="FolderGuid">Edge drive folder guid</param>
        /// <returns>array of sub-folder details</returns>
        public FolderData[] BrowseFolders(string FolderGuid, out string errorMessage)
        {
            const string function = "CVAPI::BrowseFolders";
            FolderData[] result = null;
            errorMessage = string.Empty;
            try
            {
                // Get the client details
                if (_clientDetails.ClientID == null)
                    GetEdgeClientID(out _clientDetails);

                if (string.IsNullOrEmpty(_clientDetails.ClientID))
                {
                    errorMessage = "Server is not responding. Please try again later.";
                }
                else
                {
                    string requestBody = string.Format(browseRequestBody, _clientDetails.ClientID, _clientDetails.InstanceID, _clientDetails.SubClientID, _clientDetails.ApplicationID, _userGuid, "",
                                                       string.IsNullOrEmpty(FolderGuid) ? _clientDetails.RootWebFolderID : FolderGuid + Path.DirectorySeparatorChar.ToString());

                    HttpWebResponse ClientResp = SendRequest(ServiceURI.Replace("api/", "") + browseFolderURI, post, _authToken, requestBody);
                    if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(ClientResp.GetResponseStream());
                        XmlNodeList attrList = xmlDoc.SelectNodes("//databrowse_BrowseResponseWrapper/browseResponse/browseResult/dataResultSet/@displayName");
                        if (null != attrList)
                        {
                            XmlNode guidNode;
                            result = new FolderData[attrList.Count];
                            for (int i = 0; i < attrList.Count; i++)
                            {
                                result[i] = new FolderData();
                                result[i].FolderName = attrList[i].Value;
                                guidNode = xmlDoc.SelectSingleNode(string.Format("//dataResultSet[@displayName=\"{0}\"]/@path", attrList[i].Value));
                                if (null != guidNode)
                                {
                                    result[i].FolderGuid = guidNode.Value;
                                }
                                else
                                {
                                    Logger.Error("Folder guid attribute not found", function);
                                    result = null;
                                    break;
                                }
                            }
                        }
                    }
                    else if (ClientResp != null)
                    {
                        if (ClientResp.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            RenewToken(out errorMessage);
                        }
                        else
                        {
                            errorMessage = "Server is not responding. Please try again later.";
                            Logger.Error(string.Format("{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, function);
            }
            return result;
        }

        /// <summary>
        /// Created the folder inthe drive under the machineName as the root folder
        /// </summary>
        /// <param name="FolderName">Folder name to create</param>
        /// <param name="DirectoryPath">Directory path where the folder needs to be created</param>
        /// <returns>returns GUID of the newly created folder</returns>
        public string CreateFolder(string FolderName, string DirectoryPath)
        {
            string guid = string.Empty;
            string requestBody = string.Format("<App_FileResourceInfo name=\"{0}\" parentPath=\"{1}\"/>", FolderName, DirectoryPath);
            HttpWebResponse ClientResp = SendRequest(ServiceURI + createFolderURI, post, _authToken, requestBody);
            if (null != ClientResp)
            {
                if (ClientResp.StatusCode == HttpStatusCode.Created)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ClientResp.GetResponseStream());
                    XmlNode guidNode = xmlDoc.SelectSingleNode("/App_FileResourceResponse/fileResource/@GUID");
                    if (null != guidNode)
                        guid = guidNode.Value;
                }
                else if (ClientResp.StatusCode == HttpStatusCode.Conflict) // TODO
                {
                    Logger.Info(string.Format("Foldername '{0}' already present under '{1}' in the drive.", FolderName, DirectoryPath), "CVAPI::CreateFolder");
                }
            }
            else
            {
                Logger.Error(string.Format("Create Folder '{0}' call returned: {1}", Path.Combine(DirectoryPath, FolderName), ClientResp.StatusCode.ToString()), "CVAPI::CreateFolder");
            }
            return guid;
        }


        /// <summary>
        /// Check if the logged in user has right to access Edge Drive
        /// </summary>
        /// <param name=""></param>
        /// <returns>true/false as per the case</returns>
        public bool CanUserAccessEdgeDrive(out string edgeDriveVerification)
        {
            const string function = "CVRESTAPI::CanUserAccessEdgeDrive";
            bool isUserAllowedtoAccessEdge = false;
            edgeDriveVerification = string.Empty;

            string uri = ServiceURI + getApplicationAccess;

            HttpWebResponse ClientResp = SendRequest(uri, get, _authToken, null);
            if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ClientResp.GetResponseStream());

                Logger.Verbose(xmlDoc.InnerXml, function);

                if (false == xmlDoc.SelectSingleNode("/App_CreateSyncWebFolderResp/response/@errorCode").Equals("0"))
                {
                    XmlNode driveAccessCapability = xmlDoc.SelectSingleNode("/App_CreateSyncWebFolderResp/edgeDriveInfo/@capabilities");
                    if (null != driveAccessCapability)
                    {
                        long lDriveCapability = Convert.ToInt64(driveAccessCapability.InnerText);
                        if (0 == (lDriveCapability & (1 << (Constants.EV_UPLOAD - 1))))
                        {
                            isUserAllowedtoAccessEdge = false;
                            edgeDriveVerification = Constants.MessageConstants.EDGE_VERIFICATION_FAILED;
                            Logger.Error(string.Format("{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
                        }
                        else
                        {
                            isUserAllowedtoAccessEdge = true;
                        }
                    }
                }
                else
                {
                    isUserAllowedtoAccessEdge = false;
                    edgeDriveVerification = Constants.MessageConstants.EDGE_VERIFICATION_FAILED;
                    Logger.Error(string.Format("{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
                }
            }
            else
            {
                isUserAllowedtoAccessEdge = false;
                edgeDriveVerification = Constants.MessageConstants.EDGE_VERIFICATION_CALL_FAILED;
                Logger.Error(string.Format("{0}:{1}", Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode()), ClientResp.StatusDescription), function);
            }

            return isUserAllowedtoAccessEdge;
        }

        /// <summary>
        /// Creates the full directory and returns the last folder's GUID
        /// </summary>
        /// <param name="DirectoryPathToCreate"></param>
        /// <returns></returns>
        public string CreateFolderWithDirectory(string DirectoryPathToCreate)
        {
            // First Create the directory with recursive create folder call
            StringBuilder sb = new StringBuilder();
            bool fDummy = false;
            sb.Append(@"\");
            bool isFirst = true;
            string lastFolderGUID = string.Empty;
            DirectoryPathToCreate = DirectoryPathToCreate.Replace(":", "");
            foreach (var folder in DirectoryPathToCreate.Split(new char[] { '\\' }))
            {
                lastFolderGUID = CreateFolder(folder, sb.ToString());
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(@"\");
                sb.Append(folder);
            }

            return string.IsNullOrEmpty(lastFolderGUID) ? GetGuidOfDirectory(DirectoryPathToCreate, ref fDummy) : lastFolderGUID;
        }

        /// <summary>
        /// Uploads the file to the cloud
        /// </summary>
        /// <param name="fileChunkData"></param>
        /// <param name="fileGUID">out param. uploaded file guid.</param>
        /// <param name="chunkOffset">If next xhunk is left then returns the offset</param>
        /// <returns></returns>
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
                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(fileChunkData.ReadBytes(), 0, fileChunkData.Length);
                }

                //Logger.Verbose("Reading response for file -" + FileChunkData.FilePath, function);
                ClientResp = GetResponseWithoutException((HttpWebRequest)req, fileChunkData.FilePath);
                try 
                {
                    xmlDoc = new XmlDocument();
                    using(Stream stream = ClientResp.GetResponseStream()) // It seems in continuous uploads HttpWebResponse is not getting free and results in operation timeouts, so use USING statement.
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
                        else
                            Logger.Error("Upload failed! Server error-" + xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorString").Value, "CVAPI::UploadFile");
                    }
                }
                else if(ClientResp != null)
                {
                    int errorCode = 0;
                    string errorString = string.Empty;
                    XmlNode errorCodeNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorCode");
                    XmlNode errorStringNode = xmlDoc.SelectSingleNode("/DM2ContentIndexing_UploadFileResp/@errorString");
                    if(ClientResp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Logger.Info("Token expired. Renewing token.", function);
                        RenewToken(out errorString /*not using it anywhere*/);
                    }
                    else
                    {
                        if(ClientResp.StatusCode == HttpStatusCode.RequestEntityTooLarge /*Quota Exceeded*/)
                        {
                            isQuotaExceeded = true;
                        }

                        if(null != errorCodeNode)
                        {
                            errorCode = Convert.ToInt32(errorCodeNode.Value);
                            if(null != errorStringNode)
                                errorString = errorStringNode.Value;
                        }

                        Logger.Error(string.Format("Error [{0}] errorCode [{1}] status [{2}]", errorString, errorCode, Convert.ChangeType(ClientResp.StatusCode, ClientResp.StatusCode.GetTypeCode())), function);
                    }
                }
            }
            catch (WebException we)
            {
                Logger.Error(we.Message + "-" + fileChunkData.FilePath, function);
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

        /// <summary>
        /// Checks the minimum supported webserver version
        /// </summary>
        /// <returns>Returns true if it is supported</returns>
        public bool IsSupportedVersion()
        {
            bool success = false;
            const string function = "CVAPI::GetVersionInfo";
            try
            {
                // Get the min supported version in format 10.9.2
                string[] suportedVersion = AppSettingsReader.Instance.GetSettingValue("MinSupportedServerVersion").Split(new char[] { '.' });
                Logger.Info(string.Format("Supported version: {0}", AppSettingsReader.Instance.GetSettingValue("MinSupportedServerVersion")), function);

                HttpWebResponse ClientResp = SendRequest(ServiceURI + getVersionURI, get, _authToken, null);
                if (ClientResp.StatusCode == HttpStatusCode.OK)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(ClientResp.GetResponseStream());
                    Logger.Info("Server returned version: " + xmlDoc.InnerXml, function);
                    XmlNodeList nameAttrList = xmlDoc.SelectNodes("/App_GetVersionInfoResponse/WSVersionInfo/attributes/@name");
                    XmlNodeList valueAttrList = xmlDoc.SelectNodes("/App_GetVersionInfoResponse/WSVersionInfo/attributes/@value");
                    if (null != nameAttrList && null != valueAttrList && nameAttrList.Count == valueAttrList.Count)
                    {
                        // parse the response xml to get the version info
                        Dictionary<string, int> versionInfoDic = new Dictionary<string, int>();
                        for (int i = 0; i < nameAttrList.Count; i++)
                        {
                            versionInfoDic.Add(nameAttrList[i].Value.ToLower(), Convert.ToInt32(valueAttrList[i].Value));
                        }

                        // Compare w/ the supported version
                        if (versionInfoDic.ContainsKey("release") && versionInfoDic.ContainsKey("highestsp") && versionInfoDic.ContainsKey("spminorversion"))
                        {
                            if (versionInfoDic["release"] > Convert.ToInt32(suportedVersion[0])) // Release is greater than supported, no need to check further
                            {
                                return true;
                            }
                            else if (versionInfoDic["release"] == Convert.ToInt32(suportedVersion[0]))
                            {
                                if (versionInfoDic["highestsp"] > Convert.ToInt32(suportedVersion[1])) // Highest SP is greater than supported, no need to check further
                                {
                                    return true;
                                }
                                else if (versionInfoDic["highestsp"] == Convert.ToInt32(suportedVersion[1]))
                                {
                                    if (versionInfoDic["spminorversion"] >= Convert.ToInt32(suportedVersion[2])) // Minor version is supported
                                    {
                                        return true;
                                    }
                                }
                                else // release is supported but highest SP is lower than supported
                                {
                                    return false;
                                }
                            }
                            else // release is lower than supported
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Logger.Error("API did not return attributes node in required format:" + xmlDoc.InnerText, function);
                        }
                    }
                    else
                    {
                        Logger.Error("API did not return attributes node in required format:" + xmlDoc.InnerText, function);
                    }
                }
                else
                {
                    Logger.Error("GetVersionInfo api call failed.", function);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, function);
            }
            return success;
        }

        #endregion Public Methods

        #region Private Methods
        private void GetEdgeClientID(out EdgeClient ClientDetails)
        {
            const string function = "CVRESTAPI::GetEdgeClientID";
            ClientDetails = new EdgeClient();
            string requestBody = string.Format(@"<App_LapTopClientListReq scope=""MyClients""><filter getUserMailBoxes=""true"" getSchedules=""false"" getContent=""false"" 
                    getMailBoxClients=""true"" getSharePointClients=""false"" getAllProperties=""true"" getFsLikeClients=""true"" /><user _type_=""USER_ENTITY"" userName=""{0}"" 
                    userId=""{1}"" userGUID=""{2}"" /></App_LapTopClientListReq>", Properties.Settings.Default.UserName, "", _userGuid);
            HttpWebResponse ClientResp = SendRequest(ServiceURI + "LaptopClientList", post, _authToken, requestBody);
            if (null != ClientResp && ClientResp.StatusCode == HttpStatusCode.OK)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ClientResp.GetResponseStream());
                XmlNode subClientNode = xmlDoc.SelectSingleNode(string.Format("/App_LapTopClientLstResp/clientsFileSystem[@clientStatus={0}]/subClient", CV_STATUS_EDGE_DRIVE));
                if (null != subClientNode)
                {
                    foreach (XmlAttribute attr in subClientNode.Attributes)
                    {
                        switch (attr.Name.ToLower())
                        {
                            case "clientid":
                                ClientDetails.ClientID = attr.Value;
                                break;
                            case "subclientid":
                                ClientDetails.SubClientID = attr.Value;
                                break;
                            case "instanceid":
                                ClientDetails.InstanceID = attr.Value;
                                break;
                            case "applicationid":
                                ClientDetails.ApplicationID = attr.Value;
                                break;
                            default:
                                break;
                        }
                    }
                    subClientNode = xmlDoc.SelectSingleNode(string.Format("/App_LapTopClientLstResp/clientsFileSystem[@clientStatus={0}]/edgeDrive/@syncWebFolderId", CV_STATUS_EDGE_DRIVE));
                    if (null != subClientNode)
                    {
                        ClientDetails.RootWebFolderID = subClientNode.Value;
                    }
                    else
                        Logger.Warning("'edgeDrive' xml node not found in the web response.", function);
                }
                else
                    Logger.Warning("'subClient' xml node node found in the web response.", function);
            }
            else if (ClientResp != null && ClientResp.StatusCode == HttpStatusCode.Unauthorized)
            {
                string resultMsg;
                RenewToken(out resultMsg);
            }
        }

        private string GetSessionToken(string userName, string password, out string resultMsg)
        {
            byte[] pwd = System.Text.Encoding.UTF8.GetBytes(password);
            String encodedPassword = Convert.ToBase64String(pwd, 0, pwd.Length, Base64FormattingOptions.None);
            if (string.IsNullOrEmpty(_deviceID)) 
                _deviceID= Guid.NewGuid().ToString();
            //string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq mode=\"Webconsole\" username=\"{0}\" password=\"{1}\" />", userName, encodedPassword);
            string loginReq = string.Format("<DM2ContentIndexing_CheckCredentialReq mode=\"4\" flags=\"2\" deviceId=\"{0}\" username=\"{1}\" password=\"{2}\" clientType=\"4\"/>",
                _deviceID, userName, encodedPassword);

            HttpWebResponse resp = SendRequest(ServiceURI + loginURI, post, null, loginReq, out resultMsg);

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
                    Logger.Error(string.Format("Login request. Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription), "CVAPI::GetSessionToken");
                }
            }
            return _authToken;
        }

        private HttpWebResponse SendRequest(string serviceURL, string httpMethod, string token, string requestBody)
        {
            string resultMsg;
            return SendRequest(serviceURL, httpMethod, token, requestBody, out resultMsg);
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
                    req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_COOKIE2, token);
                //req.Headers.Add(Constants.ServiceHeaderConstants.HEADER_AUTH_TOKEN, token);
                if (!string.IsNullOrEmpty(requestBody))
                    WriteRequest(req, requestBody);

                //return req.GetResponse() as HttpWebResponse;
                return (GetResponseWithoutException(req, string.Empty) as HttpWebResponse);
            }
            catch (WebException we)
            {
                Logger.Error((Exception)we, "CVAPI::SendRequest");
                resultMsg = we.Message;
                return we.Response as HttpWebResponse;
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
                if (!e.Message.Contains("(404) Not Found."))
                    Logger.Error(string.Format("Exception occured while reading response for filepath[{0}] \n{1}{2}", filePath, e.Message, e.StackTrace), function);
                return e.Response as HttpWebResponse;
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

        private long ToUnixTime(DateTime UtcTime)
        {
            var timeSpan = (UtcTime - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        #endregion Private Methods

        /// <summary>
        /// Class to hold the file details required for upload
        /// </summary>
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

            /// <summary>
            /// Use this constructor for desktop content upload.
            /// </summary>
            /// <param name="filePath">Fully qualified filepath</param>
            /// <param name="relativePath">edge drive relative path to upload</param>
            public ChunkData(string filePath, string relativePath)
            {
                //_file = FileInfo;
                UploadRelativePath = relativePath;
                
                // Read all bytes using memorystream. It reads even when the file is locked.
                //_bytedata = default(byte[]);
                FilePath = filePath;
                FileInfo f = new FileInfo(FilePath);
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
                byte[] bytedata = new byte[Length];;
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

        private struct EdgeClient
        {
            public string ClientID;
            public string SubClientID;
            public string RootWebFolderID;
            public string InstanceID;
            public string ApplicationID;
        }

        public class FolderData
        {
            public string FolderGuid;
            public string FolderName;
        }
    }
}