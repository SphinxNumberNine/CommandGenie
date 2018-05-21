using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Management;
using RegistryUtils;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Office;
using System.Data.SqlClient;

namespace ActionBasedEmails
{
    public partial class Form1 : Form
    {
        List<String> authenticatedUsers;
        String exchangeUsername;
        String exchangePassword;
        String recipientEmailAddress;
        Boolean edgeUpload;
        String edgeUserName;
        String edgePassword;
        String edgeUploadDirectory;

         string strLogFile = @"C:\CmdGenie" + "\\" + "Log.txt";

        public Form1()
        {
            InitializeComponent();
            authenticatedUsers = new List<String>();
            //authenticatedUsers.Add("cramaswamy");
            //init();
            //runInBackground();
        }

        public void init()
        {
            String[] subKeys = Registry.CurrentUser.GetSubKeyNames();
            RegistryKey softwareRegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE");
            Boolean found = false;
            foreach (String key in Registry.CurrentUser.GetSubKeyNames())
            {
                if (key.Equals("CommandExecutor"))
                {
                    found = true;
                }
            }

            if (!(found))
            {
                //Microsoft.Win32.RegistryKey newKey = softwareRegKey.CreateSubKey("CommandExecutor");
                RegistryKey newKey = Registry.CurrentUser.CreateSubKey("CommandExecutor");
                newKey.SetValue("", 0);
                newKey.Close();
            }

            

            /**String cmdOutput = executeCommandLine("ipconfig");

            sendEmail("ACTION RESPONSE", cmdOutput);

            xtFile("C:\\Temp\\output.txt", cmdOutput);*/

            uploadToEdge("C:\\Temp\\output.txt");

            
        }

        public void executeCommandLine(String parameters)
        {
            writeToTextFile(strLogFile, "executeCommandLine", true);
            String originalParameters = parameters;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.StartupPath + @"\output.txt"))
            {
                file.WriteLine("Response for: " + parameters);
                file.Close();
            }
            String tempFilePath = "\"" + Application.StartupPath + @"\output.txt" + "\"";
            Console.WriteLine(tempFilePath);
            parameters += " >> " + tempFilePath;
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(parameters);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            tempFilePath = tempFilePath.Replace(@"\\", @"\");
            Console.WriteLine(tempFilePath);
            //String cmdOutput = cmd.StandardOutput.ReadToEnd();
            tempFilePath = tempFilePath.Substring(1, tempFilePath.Length - 2);

            checkReturnMethodAndSend(tempFilePath, originalParameters);
        }

        public void sendEmail(String subject, String cmdOutput)
        {
            writeToTextFile(strLogFile, "sendEmail", true);
            var client = new SmtpClient("smtp.commvault.com");
            String MailFrom = "CommandGenie@commvault.com";
            string UserEmail = recipientEmailAddress;

            try
            {
                MailMessage message1 = new MailMessage(
                          MailFrom,
                          UserEmail,
                          subject, cmdOutput
                         );
                client.Send(message1);
                //client.Send("akrishnan@commvault.com", "akrishnan@commvault.com", "test", "testbody");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Sent");
        }

        public void sendEmail(String subject, String cmdOutput, String attachmentFilePath)
        {
            writeToTextFile(strLogFile, "sendEmail with attachment", true);
            var client = new SmtpClient("smtp.commvault.com");
            String MailFrom = "CommandGenie@commvault.com";
            string UserEmail = recipientEmailAddress;

            try
            {
                MailMessage message1 = new MailMessage(
                          MailFrom,
                          UserEmail,
                          subject, cmdOutput
                         );
                message1.Attachments.Add(new Attachment(attachmentFilePath));
                client.Send(message1);
                //client.Send("akrishnan@commvault.com", "akrishnan@commvault.com", "test", "testbody");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Sent");
            File.Delete(attachmentFilePath);
        }

        public void writeToTextFile(String filepath, String body)
        {
            using(System.IO.StreamWriter file = new System.IO.StreamWriter(filepath))
            {
                file.WriteLine(body);
            }
        }

        public void writeToTextFile(String filepath, String body, bool append)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, append))
            {
                file.WriteLine(body);
            }
        }

        public void uploadToEdge(String filepath)
        {
            EdgeUpload edgeUpload = new EdgeUpload();
            String message;
            edgeUpload.LoginRequest(edgeUserName, edgePassword, out message);
            Console.WriteLine();
            edgeUpload.uploadFile(filepath, filepath);
            writeToTextFile(strLogFile, "uploadToEdge", true);
        }

        public void runInBackground()
        {

            RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, "CommandExecutor");
            monitor.RegChanged += new EventHandler(HandleEvent);
            monitor.Start();

            while (true) ;

            monitor.Stop();

        }

        public void FormActions(string strcommand)
        {
            File.Delete(Application.StartupPath + @"\FormCommand.txt");
            SqlConnection myConnection = new SqlConnection("user id=TestUMSUser;" +
                                      "password=TestUMSUser;server=CB11 ;" +
                                      "database=TestUpdateCenter ; " +
                                      "connection timeout=30");

            try
            {
                writeToTextFile(strLogFile, "Form actions opening updatecenter DB", true);
                myConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (strcommand.ToUpper().Contains("VIEW"))
            {
                try
                {
                    writeToTextFile(strLogFile, "Form actions viweing", true);
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("select  sPlainTestCase from MapFormToTestInfo where nBuildID = 1100080 and nFormID = " + strcommand.Substring(strcommand.IndexOf("VIEW") + 5), myConnection);

                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        Console.WriteLine(myReader["sPlainTestCase"].ToString());
                        writeToTextFile(Application.StartupPath + @"\FormCommand.txt", myReader["sPlainTestCase"].ToString(), true);
                    }
                    myReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("select sPropertyValue from FormProperties where sPropertyName like 'bug%' and nBuildID = 1100080 and nFormID = 33926", myConnection);

                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        Console.WriteLine(myReader[0].ToString());
                        writeToTextFile(Application.StartupPath + @"\FormCommand.txt", myReader[0].ToString(), true);
                    }
                    myReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else if (strcommand.ToUpper().Contains("APPROVE"))
            {
                writeToTextFile(strLogFile, "Form actions approve", true);
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("MoveFormToNextSubState 1108055, " + strcommand.Substring(strcommand.ToUpper().IndexOf("APPROVE") + 8) + ", 'cramaswamy'", myConnection);

                    myReader = myCommand.ExecuteReader();
                    //read form new state and send response back
                    myReader.Close();
                    writeToTextFile(Application.StartupPath + @"\FormCommand.txt", "Form Action Approved" + strcommand.Substring(strcommand.ToUpper().IndexOf("APPROVE") + 8), true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            checkReturnMethodAndSend(Application.StartupPath + @"\FormCommand.txt", "FORM ACTION: " + strcommand);
        }

        public void HandleEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Value Changed");
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "CommandExecutor";
            const string keyName = userRoot + "\\" + subkey;
            //Registry.SetValue(keyName, "", 0);
            String filePath = (String) Registry.GetValue(keyName, "filePath", "NULL");
            if (!File.Exists(filePath))
                return;

            
             StreamReader reader = new StreamReader(filePath);
             String from = reader.ReadLine();
             String body = reader.ReadLine();
             String target = reader.ReadLine();
             reader.Close();

             if (!AuthenticateUser(from))
             {
                 sendEmail("Unauthenticated user attempted to submit action!", from + "\r\n" + body);
                 return;
             }
             if (target != "")
             {
                 if (!AuthenticateTarget(target))
                 {
                     return;
                 }
             }
                
             ProcessCommand(body);
         
        }

        public bool AuthenticateUser(String user)
        {
            return true;
        }

        public bool AuthenticateTarget(String target)
        {
            if ((target.ToUpper().Contains(Environment.MachineName.ToUpper())) || (Environment.MachineName.ToUpper().Contains(target.ToUpper())))
            {
                return true;
            }

            return false;
        }

        public void ProcessCommand(String body)
        {
            writeToTextFile(strLogFile, body, true);
            if ((body.ToUpper().IndexOf("CMD") >= 0))
            {
                writeToTextFile(strLogFile, "CMD received", true);
                String parameters = body.Substring(body.ToUpper().IndexOf("CMD") + 4).Trim();
                executeCommandLine(parameters);
            }
            else if ((body.ToUpper().IndexOf("GET") >= 0))
            {
                writeToTextFile(strLogFile, "GET action received", true);
                String filepath = body.Substring(body.ToUpper().IndexOf("GET") + 4).Trim();
                //sendEmail("AUTOMATED RESPONSE: GET command", body, filepath);
                checkReturnMethodAndSend(filepath, "AUTOMATED RESPONSE: GET");
            }
            else if ((body.ToUpper().IndexOf("FORM") >= 0))
            {
                writeToTextFile(strLogFile, "FORM action received", true);
                String parameters = body.Substring(body.ToUpper().IndexOf("FORM") + 4).Trim();
                FormActions(parameters);
            }
        }

        private void uploadToEdgeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                edgeCredentialsLabel.Visible = true;
                edgeUsernameBox.Visible = true;
                edgeUsernameLabel.Visible = true;
                edgePasswordBox.Visible = true;
                edgePasswordLabel.Visible = true;
                edgeUploadPathBox.Visible = true;
                edgeUploadPathLabel.Visible = true;
            }
            else
            {
                edgeCredentialsLabel.Visible = false;
                edgeUsernameBox.Visible = false;
                edgeUsernameLabel.Visible = false;
                edgePasswordBox.Visible = false;
                edgePasswordLabel.Visible = false;
                edgeUploadPathBox.Visible = false;
                edgeUploadPathLabel.Visible = false;
            }
        }

        private void addPriviledgedUserBox_Click(object sender, EventArgs e)
        {
            String priviledgedUser = newPriviledgedUserBox.Text;
            newPriviledgedUserBox.Text = "";
            Boolean found = false;
            foreach (String users in priviledgedUsersBox.Lines)
            {
                if (priviledgedUser.ToUpper().Equals(users.ToUpper()))
                {
                    found = true;
                }
            }
            if (found)
            {
                MessageBox.Show("That user is already priviledged.");
            }
            else
            {
                priviledgedUsersBox.AppendText(priviledgedUser);
                priviledgedUsersBox.AppendText(Environment.NewLine);
            }
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            exchangeUsername = exchangeUsernameBox.Text;
            exchangePassword = exchangePasswordBox.Text;
            recipientEmailAddress = responseEmailAddressBox.Text;
            edgeUpload = uploadToEdgeCheckBox.Checked;
            edgeUserName = edgeUsernameBox.Text;
            edgePassword = edgePasswordBox.Text;
            edgeUploadDirectory = edgeUploadPathBox.Text;

            foreach (String line in priviledgedUsersBox.Lines)
            {
                if (!(line.Equals("")))
                {
                    authenticatedUsers.Add(line);
                }
            }

            this.Close();
            runInBackground();
        }

        private void checkCurrentExchangeUser()
        {

        }

        private void checkReturnMethodAndSend(String tempFilePath, String originalParameters)
        {
            writeToTextFile(strLogFile, "checkReturnMethodAndSend", true);
            if ((new FileInfo(tempFilePath).Length) < 10000)
            {
                writeToTextFile(strLogFile, "File less than 10k. sending email response", true);
                StreamReader reader = new StreamReader(tempFilePath);
                String cmdOutput = reader.ReadToEnd();
                sendEmail("AUTOMATED RESPONSE: " + originalParameters, cmdOutput);
                reader.Close();
            }
            else if (edgeUpload)
            {
                writeToTextFile(strLogFile, "uploaded to edge", true);
                uploadToEdge(tempFilePath);
                sendEmail("AUTOMATED RESPONSE: " + originalParameters, "File uploaded to EDGE: " + tempFilePath);
            }
            else
            {
                writeToTextFile(strLogFile, "File greater than 10k. sending attachment in email response", true);
                sendEmail("AUTOMATED RESPONSE: " + originalParameters, "The output was larger than 10kb and was delivered to you attached to this message.", tempFilePath);
            }
        }
    }
}
