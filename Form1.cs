using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ID_Auth
{
    
    public partial class Form1 : Form
    {
        public static DateTime Starttime;
        public Form1()
        {
            InitializeComponent();
        }

        private void Next_Click(object sender, EventArgs e)
        {
            try
            {
              
                if (txtmail.Text.Trim()=="")
                {
                    MessageBox.Show("Enter Mail ID");
                    return;
                }
                string mailid = txtmail.Text;
                lblmsg.Text = "Authenticating .....";
                // string DATA = @"{""email"":""pswinton@resiliantstage.com""}";
                //string DATA = @"{""email"":""@"+mailid+@"""}";
                string DATA = @"{""email"":""" + mailid + @"""}";

                System.Net.HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://authproxy.resiliantstage.com/api/request");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = DATA.Length;
                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(DATA);
                requestWriter.Close();

                try
                {
                    WebResponse webResponse = request.GetResponse();
                    Stream webStream = webResponse.GetResponseStream();
                    StreamReader responseReader = new StreamReader(webStream);
                    string response = responseReader.ReadToEnd();
                    var result1 = JObject.Parse(response);
                    string Transactionid = result1["data"]["TransactionId"].ToString();
                    Console.Out.WriteLine(response);
                    responseReader.Close();
                    MessageBox.Show("Authetication Completed..Transactionid is "+ Transactionid);
                    lblmsg.Text = "Authentication success .....";
                    //Code to another process
                    Starttime = DateTime.Now;
                    if (Call_Next_Process(mailid, Transactionid)==true)
                    {
                        lblmsg.Text = "Verify successfully...";
                        ProcessStartInfo info = new ProcessStartInfo(@"D:\windows-client\x64\Debug\SDP.exe");
                        Process.Start(info);
                        this.Close();
                    }
                    else
                    {
                        lblmsg.Text = "";
                        MessageBox.Show("Verification failed");
                    }








                }
                catch (Exception exc)
                {
                    lblmsg.Text = "";
                    MessageBox.Show("Authetication failed");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public Boolean Call_Next_Process(string mailid,string transactionid)
        {
            lblmsg.Text = "Verifying .....";
            string DATA = @"{""email"":""" + mailid + @"""}";

            string fullfil = "false";

         

            do
            {
                try
                {
                    Thread.Sleep(5000);
                    System.Net.HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://authproxy.resiliantstage.com/api/poll");
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = DATA.Length;
                    StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                    requestWriter.Write(DATA);
                    requestWriter.Close();
                    WebResponse webResponse = request.GetResponse();
                    Stream webStream = webResponse.GetResponseStream();
                    StreamReader responseReader = new StreamReader(webStream);
                    string response = responseReader.ReadToEnd();
                    var result1 = JObject.Parse(response);
                    fullfil = result1["data"]["fulfilled"].ToString();
                    Console.Out.WriteLine(response);
                    responseReader.Close();

                    if (result1["success"].ToString().ToUpper() =="FALSE")
                    {
                        lblmsg.Text = "";
                        MessageBox.Show("Unauthorized user");
                        return false;
                    }

                }
                catch (Exception exn)
                {
                    lblmsg.Text = "";
                    MessageBox.Show("Unauthorized user");
                    return false;
                }


            } while (fullfil.ToString().ToUpper()=="FALSE" && Cal_Process_time() <= 2);


            Boolean returntype = false;
            if(fullfil.ToString().ToUpper()=="TRUE")
            {
                returntype = true;
            }
            else
            {
                returntype = false;
            }
                
           

           

            return returntype;
        }
        public int Cal_Process_time()
        {
            DateTime date1 = Starttime;
            DateTime date2 = DateTime.Now;
            TimeSpan ts = date2 - date1;
            return ts.Minutes;
        }
    }
}
