using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;



namespace clientFTP
{
    class FTP
    {
        private string user;
        private string pass;

        public void setUserAndPass(string _user, string _pass)
        {
            user = _user;
            pass = _pass;
        }

        private FtpWebRequest CreateFtpRequest(string ip, string path, string method)
        {
            var uri = new Uri(ip + path);
            var ftp = FtpWebRequest.Create(uri) as FtpWebRequest;
            ftp.Credentials = new NetworkCredential(user, pass);
            ftp.KeepAlive = false;
            ftp.UseBinary = true;
            ftp.Proxy = null;
            ftp.Method = method;
            return ftp;
        }

        public string[] GetDirectoryOfServer(string ip, string path)
        {
            var list = new List<string>();
            var req = CreateFtpRequest(ip, path, WebRequestMethods.Ftp.ListDirectoryDetails);
            using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
            {
                using (Stream responseStream = resp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        while(!reader.EndOfStream)
                        {
                            list.Add(reader.ReadLine());
                        }
                    }
                }
            }
            return list.ToArray();
        }

        /*
            -rwxr-xr-x 1 ftp ftp        2241216 Apr 28 11:04 FileZilla_Server-0_9_60_2.exe
            -rw-r--r-- 1 ftp ftp             22 May 14 19:16 lala.zip
            -rw-r--r-- 1 ftp ftp         436523 Jun 28  2016 large.gif
            -rw-r--r-- 1 ftp ftp           1648 Apr 19 23:35 studentInfo.xml
            -rw-r--r-- 1 ftp ftp           1480 Apr 19 23:35 stylesheet.xsl
            drwxr-xr-x 1 ftp ftp              0 May 14 15:14 Новая папка
            -rw-r--r-- 1 ftp ftp       24425847 Feb 27  2016 Э.Стиллмен, Дж.Грин - Изучаем C# (Head First O'Reilly) - 2014.pdf
            -rw-r--r-- 1 ftp ftp          21997 Apr 18 00:23 экономика 2.docx
         */

        public void GetDirectory(string ip, string path, ref ObservableCollection<FileInfo> cont)
        {
            cont.Clear();
            string[] info = GetDirectoryOfServer(ip, path);
            foreach(var item in info)
            {
                if(item.Substring(49).IndexOf(".") == -1 && item[0] == 'd')
                {
                    cont.Add(new FileInfo(  Convert.ToInt32(item.Substring(20, 15).Trim()), 
                                            "DIR", 
                                            item.Substring(49), 
                                            item.Substring(36, 12))
                                         );
                }
                else if(item.Substring(49).IndexOf(".") == -1)
                {
                    cont.Add(new FileInfo(  Convert.ToInt32(item.Substring(20, 15).Trim()), 
                                            "NOT", 
                                            item.Substring(49), 
                                            item.Substring(36, 12))
                                         );
                }
                else
                {
                    cont.Add(new FileInfo(  Convert.ToInt32(item.Substring(20, 15).Trim()), 
                                            item.Substring(49).Substring(item.Substring(49).LastIndexOf(".")), 
                                            item.Substring(49), 
                                            item.Substring(36, 12))
                                         );
                }
                
            }
        }

        public double getSizeFileInMB(string filePath)
        {
            System.IO.FileInfo t = new System.IO.FileInfo(filePath);
            return t.Length / 1024 / 1024;
        }

        public string UploadFile(string ip, string path, string fileName, string filePath)
        {
            FtpWebRequest req = CreateFtpRequest(ip, path + fileName, WebRequestMethods.Ftp.UploadFile);

            FileStream uploadFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] fileToBypes = new byte[uploadFile.Length];
            uploadFile.Read(fileToBypes, 0, fileToBypes.Length);
            uploadFile.Close();

            Stream writer = req.GetRequestStream();
            writer.Write(fileToBypes, 0, fileToBypes.Length);
            writer.Close();

            FtpWebResponse resp = (FtpWebResponse)req.GetResponse();
            string stat = resp.StatusDescription;
            resp.Close();
            
            return stat;
        }

        public string DeleteFile(string ip, string path, string fileName)
        {
            var req = CreateFtpRequest(ip, path + fileName, WebRequestMethods.Ftp.DeleteFile);
            FtpWebResponse ftpResponse = (FtpWebResponse)req.GetResponse();
            string stat = ftpResponse.StatusDescription;
            ftpResponse.Close();
            return stat;
        }

        public string DeleteDir(string ip, string path, string fileName)
        {
            var req = CreateFtpRequest(ip, path + fileName, WebRequestMethods.Ftp.RemoveDirectory);
            FtpWebResponse ftpResponse = (FtpWebResponse)req.GetResponse();
            string stat = ftpResponse.StatusDescription;
            ftpResponse.Close();
            return stat;
        }

        public bool IsAddressValid(string addrString)
        {
            IPAddress address;
            return IPAddress.TryParse(addrString, out address);
        }

        public string Download(string ip, string path, string fileName)
        {
            var req = CreateFtpRequest(ip, path + fileName, WebRequestMethods.Ftp.DownloadFile);
            FtpWebResponse resp = (FtpWebResponse)req.GetResponse();

            Stream respStream = resp.GetResponseStream();
            FileStream downloadFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

            byte[] buffer = new byte[1024];
            int size = 0;

            while((size = respStream.Read(buffer, 0, 1024)) > 0)
            {
                downloadFile.Write(buffer, 0, size);
            }

            string temp = resp.StatusDescription;

            downloadFile.Close();
            respStream.Close();
            resp.Close();
            return temp;
        }

        public void CreateDir(string ip, string path, string folderName)
        {
            var req = CreateFtpRequest(ip, path + folderName, WebRequestMethods.Ftp.MakeDirectory);
            FtpWebResponse resp = (FtpWebResponse)req.GetResponse();
            resp.Close();
        }
    }
}
