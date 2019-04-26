using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
namespace ParseFileList
{
    class DownloadURL
    {
        /// <summary>
        /// Download the specified text page.
        /// </summary>
        /// <param name="response">The HttpWebResponse to
        /// download from.</param>
        /// <param name="filename">The local file to save to.
        /// </param>
        public void DownloadBinaryFile(
        HttpWebResponse response, String filename)
        {
            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename,
            FileMode.Create);
            Stream stream = response.GetResponseStream();
            int count = 0;
            do
            {
                count = stream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                    os.Write(buffer, 0, count);
            } while (count > 0);
            response.Close();
            stream.Close();
            os.Close();
        }
        /// <summary>
        /// Download the specified text page.
        /// </summary>
        /// <param name="response">The HttpWebResponse to
        /// download from.</param>
        /// <param name="filename">The local file to save to.
        /// </param>
        public void DownloadTextFile(
        HttpWebResponse response, String filename)
        {
            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename,
                FileMode.Create);
            StreamReader reader = new StreamReader(
            response.GetResponseStream(),
            System.Text.Encoding.ASCII);
            StreamWriter writer = new StreamWriter(os,
            System.Text.Encoding.ASCII);
            String line;
            do
            {
                line = reader.ReadLine();
                if (line != null)
                    writer.WriteLine(line);
            } while (line != null);
            reader.Close();
            writer.Close();
            os.Close();
        }
        /// <summary>
        /// Download either a text or binary file from a URL.
        /// The URL's headers will be scanned to determine the
        /// type of tile.
        /// </summary>
        /// <param name="remoteURL">The URL to download from.
        /// </param>
        /// <param name="localFile">The local file to save to.
        /// </param>
        public void Download(Uri remoteURL, String localFile)
        {
            /******    got the following 4 lines from stackoverflow, they were necessary to eliminate error in original code    ******/
            /*** https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel# ***/
            // using System.Net;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

            WebRequest http = HttpWebRequest.Create(remoteURL);
            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            String type = response.Headers["Content-Type"].ToLower().Trim();
            if (type.StartsWith("text"))
                DownloadTextFile(response, localFile);
            else
                DownloadBinaryFile(response, localFile);
        }
        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        static void NotMain(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine(
                "Usage: Recipe4_3 [URL to Download] [Output File]");
            }
            else
            {
                DownloadURL d = new DownloadURL();
                d.Download(new Uri(args[0]), args[1]);
            }
        }
    }
}

