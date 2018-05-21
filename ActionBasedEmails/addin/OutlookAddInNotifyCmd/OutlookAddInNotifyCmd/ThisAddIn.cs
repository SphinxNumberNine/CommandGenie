using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OutlookAddInNotifyCmd
{
    public partial class ThisAddIn
    {
        Outlook.NameSpace outlookNameSpace;
        Outlook.MAPIFolder inbox;
        Outlook.Items items;
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            outlookNameSpace = this.Application.GetNamespace("MAPI");
            inbox = outlookNameSpace.GetDefaultFolder(
                    Microsoft.Office.Interop.Outlook.
                    OlDefaultFolders.olFolderInbox);

            items = inbox.Items;
            items.ItemAdd +=
                new Outlook.ItemsEvents_ItemAddEventHandler(items_ItemAdd);
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        void items_ItemAdd(object Item)
        {
            Outlook.MailItem mail = (Outlook.MailItem)Item;
            if (Item != null)
            {
                if (mail.Subject == "EXECUTE ACTION")
                {
                    string strPath = @"C:\CmdGenie" + "\\" + mail.EntryID + ".txt";
                    writeToTextFile(strPath, mail.SenderEmailAddress, mail.Body);
                    //update reg key
                    const string userRoot = "HKEY_CURRENT_USER";
                    const string subkey = "CommandExecutor";
                    const string keyName = userRoot + "\\" + subkey;

                    // An int value can be stored without specifying the
                    // registry data type, but long values will be stored
                    // as strings unless you specify the type. Note that
                    // the int is stored in the default name/value
                    // pair.
                    Registry.SetValue(keyName, "", 1);
                    Registry.SetValue(keyName, "filePath", strPath);
                    //MessageBox.Show("new Command Email is here. Process it");
                }
            }

        }

        public void writeToTextFile(String filepath, String sender, String body)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath))
            {
                file.WriteLine(sender);
                file.WriteLine(body);
            }
        }
        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
