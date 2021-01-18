using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security;
using System.IO;
using System.Diagnostics;

namespace ClrSendMail_Console_
{
    class Program
    {
        static void Main(string[] args)
        {

            ServicePointManager.ServerCertificateValidationCallback = delegate (
           Object objt, X509Certificate certificate, X509Chain chain,
           SslPolicyErrors errors)
            {
                return (true);
            };


            DataForSend dataForSend = new DataForSend();
            File fm = new File();
            MailMessage mess;
            SmtpClient client = new SmtpClient
            {
                Host = "",
                Port = ,
                EnableSsl = true,
                Credentials = new NetworkCredential("login", "pass"),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };


            SqlConnection connection = new SqlConnection(@"Database=******; Data Source=*********; user id=********; Password=********;");
            connection.Open();

            dataForSend.Exec("183545",connection);

            for (int i = 0; i < dataForSend.CountDataMail(); i++)
            {
                string[] str = dataForSend.GetMail(i);

                mess = new MailMessage();
                mess.From = new MailAddress("*******");
                mess.To.Add(str[0]);
                mess.Subject = str[1];
                mess.Body = str[2];
                if(!fm.Exist())
                   fm = new File(str[3]);
                foreach(Attachment f in fm.GetAttachment())
                    mess.Attachments.Add(f);

                client.Send(mess);
            }

        }


    }


    class File
    {
        private string path;//путь к сетевой папке,которую нужно подключить
        private Attachment[] attachments;//Все файлы в формате attachment
        private string command;//комнада на подключение сетевой папки
        private string user = "*******";
        private string pass = "********";
        private List<string> allFilePath = new List<string>(); //Хранит все пути к файлам

        //подключаем сетевую папку
        public File(string path)
        {
            this.path = path;
            command = "net use H: " + "\"" + path + "\"" + " /user:" + user + " " + pass;
            ExecuteCommand(command, 5000);
        }

        public File() { }
        

        public Attachment[] GetAttachment()
        {
            GetRecursFiles(path);
            attachments = new Attachment[allFilePath.Count];
            for (int i = 0; i < allFilePath.Count; i++)
                attachments[i] = new Attachment(allFilePath.ElementAt(i));

            ExecuteCommand("net use H: /delete",5000);

            return attachments;

        }

        //рекурсивно проходит по всем каталогам и подкаталогам, заполняя allFilePath путями к файлам
        private void GetRecursFiles(string start_path)
        {
            foreach (string folder in Directory.GetDirectories(start_path))
                GetRecursFiles(folder);
            foreach (string filename in Directory.GetFiles(start_path))
            {
                allFilePath.Add(filename);
            }

        }

        //выполняет команду в cmd
        public static int ExecuteCommand(string command, int timeout)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = "C:\\",
            };

            var process = Process.Start(processInfo);
            process.WaitForExit(timeout);
            var exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }


        public bool Exist()
        {
            return (attachments == null) ? false: true ;
        }

    }



    class DataForSend
    {
        private class DataMail
        {
            public string mail;
            public string tema;
            public string soder;
            public string path;

            public DataMail(string mail,string tema,string soder,string file)
            {
                this.mail = mail;
                this.tema = tema;
                this.soder = soder;
                this.path = file;
            }

        }

        private List<DataMail> listDataMail = new List<DataMail>();




        public void Exec(string vcode, SqlConnection connection)
        {
            string sqlExpress = "select * from plm_sentMessForSZclr" + "("+vcode+")"; //команда для получения данных ждя отправки
            SqlCommand exportData = new SqlCommand(sqlExpress, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(exportData);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            foreach (DataRow row in dt.Rows)
            {
                var temp = row.ItemArray;
                DataMail dm = new DataMail(temp[0].ToString().Trim(), temp[1].ToString().Trim(), temp[2].ToString().Trim(), temp[3].ToString().Trim());
                listDataMail.Add(dm);

            }

        }

        public int CountDataMail()
        {
            return listDataMail.Count();
        }
        

        public string[] GetMail(int index)
        {
            DataMail temp = listDataMail.ElementAt(index);
            string[] str = new string[4];

            str[0] = temp.mail;
            str[1] = temp.tema;
            str[2] = temp.soder;
            str[3] = temp.path;

            return str;
        }
     
    }

}
