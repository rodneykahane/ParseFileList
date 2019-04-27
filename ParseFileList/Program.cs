/// right back at you

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

 namespace ParseFileList

{
    class Program
    {
        /// <summary>
        /// Used to convert strings to byte arrays.
        /// </summary>
        private System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        
        /// <summary>
        /// This method downloads the specified URL into a C#
        /// String. This is a very simple method, that you can
        /// reused anytime you need to quickly grab all data
        /// from a specific URL.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>The contents of the URL that was
        /// downloaded.</returns>
        public void DownloadBinaryFile(Uri url, String filename)
        {
            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename, FileMode.Create);
            WebRequest http = HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            Stream stream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = stream.Read(buffer, 0,
                buffer.Length);
                if (count > 0)
                    os.Write(buffer, 0, count);
            } while (count > 0);
            response.Close();
            stream.Close();
            os.Close();
        }

        /// <summary>
        /// Handle each list item, as it is found.
        /// </summary>
        /// <param name="item">The list item that was just found.</param>
        private void ProcessItem(String item)
        {
            Console.WriteLine(item);
        }

        /// <summary>
        /// Advance to the specified HTML tag.
        /// </summary>
        /// <param name="parse">The HTML parse object to use.
        /// </param>
        /// <param name="tag">The HTML tag.</param>
        /// <param name="count">How many tags like this to find.
        /// </param>
        /// <returns>True if found, false otherwise.</returns>
        private bool Advance(ParseHTML parse, String tag, int count)
        {
            int ch;
            while ((ch = parse.Read()) != -1)
            {
                if (ch == 0)
                {
                    if (String.Compare(parse.Tag.Name, tag, true) == 0)
                    {
                        count--;
                        if (count <= 0)
                            return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// This method is called once for each table row located,
        /// it contains a list of all columns in that row. The
        /// method provided simply prints the columns to the
        /// console.
        /// </summary>
        /// <param name="list">Columns that were found on
        /// this row.</param>
        private void ProcessTableRow(List<String> list)
        {
            StringBuilder result = new StringBuilder();
            foreach (String item in list)
            {
                if (result.Length > 0)
                    result.Append(",");
                result.Append('\"');
                result.Append(item);
                result.Append('\"');
            }
            Console.WriteLine(result.ToString());
        }


        /// <summary>
        /// Called to extract a list from the specified URL.
        /// </summary>
        /// <param name=”url”>The URL to extract the list
        /// from.</param>
        /// <param name=”listType”>What type of list, specify
        /// its beginning tag (i.e. <UL>)</param>
        /// <param name=”optionList”>Which list to search,
        /// zero for first.</param>

        public void ProcessTable(Uri url, int tableNum)
        {

            //ignore bad cert code
            IgnoreBadCertificates();

            //code to allow program with work with different authentication schemes
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            List<String> list = new List<String>();
            //WebRequest http = HttpWebRequest.Create(url);
            FileWebRequest http = (FileWebRequest)WebRequest.Create(url);
            //HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            FileWebResponse response = (FileWebResponse)http.GetResponse();
            Stream istream = response.GetResponseStream();
            ParseHTML parse = new ParseHTML(istream);
            StringBuilder buffer = new StringBuilder();
            bool capture = false;
            Advance(parse, "table", tableNum);
            int ch;
            while ((ch = parse.Read()) != -1)
            {
                if (ch == 0)
                {
                    HTMLTag tag = parse.Tag;
                    if (String.Compare(tag.Name, "tr", true) == 0)
                    {
                        list.Clear();
                        capture = false;
                        buffer.Length = 0;
                    }
                    else if (String.Compare(tag.Name, "/tr", true) == 0)
                    {
                        if (list.Count > 0)
                        {
                            ProcessTableRow(list);
                            list.Clear();
                        }
                    }
                    else if (String.Compare(tag.Name, "td", true) == 0)
                    {
                        if (buffer.Length > 0)
                            list.Add(buffer.ToString());
                        buffer.Length = 0;
                        capture = true;
                    }
                    else if (String.Compare(tag.Name, "/td", true) == 0)
                    {
                        list.Add(buffer.ToString());
                        buffer.Length = 0;
                        capture = false;
                    }
                    else if (String.Compare(tag.Name, "/table", true) == 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (capture)
                        buffer.Append((char)ch);
                }
            }
        }//end ProcessTable

        public void ProcessList(Uri url, String listType, int optionList)
        {

            //ignore bad cert code
            IgnoreBadCertificates();

            //code to allow program with work with different authentication schemes
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            String listTypeEnd = listType + "/";
            //WebRequest http = HttpWebRequest.Create(url);
            FileWebRequest http = (FileWebRequest)WebRequest.Create(url);
            //HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            FileWebResponse response = (FileWebResponse)http.GetResponse();
            Stream istream = response.GetResponseStream();
            ParseHTML parse = new ParseHTML(istream);
            StringBuilder buffer = new StringBuilder();
            bool capture = false;
            Advance(parse, listType, optionList);
            int ch;
            while ((ch = parse.Read()) != -1)
            {
                if (ch == 0)
                {
                    HTMLTag tag = parse.Tag;
                    if (String.Compare(tag.Name, "li", true) == 0)
                    {
                        if (buffer.Length > 0)
                            ProcessItem(buffer.ToString());
                        buffer.Length = 0;
                        capture = true;
                    }
                    else if (String.Compare(tag.Name, "/li", true) == 0)
                    {
                        // Console.WriteLine(buffer.ToString());  //creates a double listing of each list item, might be left over debugging code
                        ProcessItem(buffer.ToString());
                        buffer.Length = 0;
                        capture = false;
                    }
                    else if (String.Compare(tag.Name, listTypeEnd, true) == 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (capture)
                        buffer.Append((char)ch);
                }
            }
        }//end ProcessList


        static void Main(string[] args)
        {
            /*
            Uri u = new Uri("http://10.0.0.17/list.php");
            Program parse = new Program();
            parse.Process(u, "ul", 1);

            */
            /*
            // obtain a URL to use
            if (args.Length < 1)
            {
                Uri u = new Uri("https://10.0.0.17/list.php");
                Program parse = new Program();
                parse.Process(u, "ul", 1);
            }
            else
            {
                Uri u = new Uri(args[0]);
                Program parse = new Program();
                parse.Process(u, "ul", 1);
            }
            */

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ParseFileList [URL to Download] ['table' or 'list]");
            }
            else if (args[1] == "table") 
            {
                DownloadURL d = new DownloadURL();
                d.Download(new Uri("http://"+args[0]), "out.html");

               // Uri u = new Uri("file://C:/Users/rodney/Documents/Visual Studio 2015/Projects/bots/ParseFileList/ParseFileList/bin/Debug/out.html");
                Uri u = new Uri("file://C:/Users/rodney/Source/Repos/ParseFileList/ParseFileList/bin/Debug/out.html");
                Program parse = new Program();
                parse.ProcessTable(u, 1);
                Console.Write("Which file(s) would you like to download? ");
                String FileToDownload = Console.ReadLine();
                Program df = new Program();
                //Console.Write("URI being sent over is: " + "http://" + args[0] + "/" + FileToDownload);
                df.DownloadBinaryFile(new Uri("http://" + args[0] + "/" + FileToDownload), "./" + FileToDownload);
            }
            else if (args[1] == "list")
            {
                DownloadURL d = new DownloadURL();
                d.Download(new Uri("http://" + args[0]), "out.html");

                //Uri u = new Uri("file://C:/Users/rodney/Documents/Visual Studio 2015/Projects/bots/ParseFileList/ParseFileList/bin/Debug/out.html");
                Uri u = new Uri("file://C:/Users/rodney/Source/Repos/ParseFileList/ParseFileList/bin/Debug/out.html");
                Program parse = new Program();
                parse.ProcessList(u,"ul", 1);
                Console.Write("Which files would you like to download? ");
                String FileToDownload = Console.ReadLine();
                Program df = new Program();
               // Console.Write("URI being sent over is: " + "http://" + args[0] + "/" + FileToDownload);       
                df.DownloadBinaryFile(new Uri("http://"+args[0]+"/"+FileToDownload),"./"+FileToDownload);

            }

        }//end main

        //more bad cert code below

        /// <summary>
        /// Together with the AcceptAllCertifications method right
        /// below this causes to bypass errors caused by SLL-Errors.
        /// </summary>
        public static void IgnoreBadCertificates()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        /// <summary>
        /// In Short: the Method solves the Problem of broken Certificates.
        /// Sometime when requesting Data and the sending Webserverconnection
        /// is based on a SSL Connection, an Error is caused by Servers whoes
        /// Certificate(s) have Errors. Like when the Cert is out of date
        /// and much more... So at this point when calling the method,
        /// this behaviour is prevented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certification"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns>true</returns>
        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }//end class
}//end namespace