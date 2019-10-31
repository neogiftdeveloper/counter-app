
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartAttendenceRegisterTCP_IP_Server.DataBase_Sql_Connection;
using SmartAttendenceRegisterTCP_IP_Server.DataBaseModelClasses;

namespace SmartAttendenceRegisterTCP_IP_Server
{
  public partial class TCP_IP_ServerForm : Form
  {


    #region gobal Vairiables
    private DataBaseConnection dataBase;
    private Student student;
    private Attendace attendance;


    private SimpleTcpServer TCP_Server;
    private SimpleTcpClient TCP_Client;

    private Socket serverSocket;
    private Socket clientSocket; // We will only accept one socket.
    private byte[] buffer;

    private string text = "";
    private string instruction = "";

    private string _IPAddress = "192.168.0.21";
    private string _PortNumber = "1011";

    private string Command = "";
    private string ValueString = "";
    private int fingerID = 0;
    #endregion




    public TCP_IP_ServerForm()
    {

      InitializeComponent();



      this.Text = this.Title_Label.Text;
      this.Date_Label.Text = DateTime.Now.ToLongDateString();
      this.StartServer();

      this.dataBase = new DataBaseConnection();
      this.student = new Student();
      this.attendance = new Attendace();

    }

    private void Title_Label_TextChanged(object sender, EventArgs e)
    {
      this.Text = this.Title_Label.Text;
    }

    private void CloseWindow_Button_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void MinimizeWindow_Button_Click(object sender, EventArgs e)
    {
      this.WindowState = FormWindowState.Minimized;
    }

    #region old School Method

    private static void ShowErrorDialog(string message)
    {
      MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }



    private void StartServer()
    {



      try
      {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(_IPAddress), Convert.ToInt32(_PortNumber)));

        serverSocket.Listen(10);
        serverSocket.BeginAccept(AcceptCallback, null);



      }
      catch (SocketException ex)
      {
        ShowErrorDialog(ex.Message);
      }
      catch (ObjectDisposedException ex)
      {
        ShowErrorDialog(ex.Message);
      }


    }

    private void AcceptCallback(IAsyncResult AR)
    {
      try
      {
        clientSocket = serverSocket.EndAccept(AR);
        buffer = new byte[clientSocket.ReceiveBufferSize];

        // Send a message to the newly connected client.
        //var sendData = Encoding.ASCII.GetBytes("Hello");
        //clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
        // Listen for client data.
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
        // Continue listening for clients.
        serverSocket.BeginAccept(AcceptCallback, null);
      }
      catch (SocketException ex)
      {
        ShowErrorDialog(ex.Message);
      }
      catch (ObjectDisposedException ex)
      {
        ShowErrorDialog(ex.Message);
      }
    }

    private void SendCallback(IAsyncResult AR)
    {
      try
      {
        clientSocket.EndSend(AR);
      }
      catch (SocketException ex)
      {
        ShowErrorDialog(ex.Message);
      }
      catch (ObjectDisposedException ex)
      {
        ShowErrorDialog(ex.Message);
      }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
      try
      {
        // Socket exception will raise here when client closes, as this sample does not
        // demonstrate graceful disconnects for the sake of simplicity.

        int received = clientSocket.EndReceive(AR);

        if (received == 0)
        {
          return;
        }

        byte[] recBuf = new byte[received];
        Array.Copy(buffer, recBuf, received);
        text = Encoding.ASCII.GetString(recBuf);

        if (text != "")
        {
          int index = text.IndexOf('#');

          if (index >= 0)
          {
            Command = text.Substring(0, index);
            ValueString = text.Substring(index + 1, ((text.Length - index) - 1));
          }

        }


        // Start receiving data again.
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
      }
      // Avoid Pokemon exception handling in cases like these.
      catch (SocketException ex)
      {
        ShowErrorDialog(ex.Message);
      }
      catch (ObjectDisposedException ex)
      {
        ShowErrorDialog(ex.Message);
      }
    }



    #endregion

    private void timer1_Tick(object sender, EventArgs e)
    {
      label2.Text = serverSocket.Available.ToString();

      if (clientSocket != null)
      {
        EndPoint neo = clientSocket.RemoteEndPoint;
        label3.Text = neo.ToString();

        int n = label3.Text.IndexOf(':');
        string ip = label3.Text.Substring(0, n);
        string port = label3.Text.Substring(n + 1, label3.Text.Length - (n + 1));

        label4.Text = ip;
        label5.Text = port;

        Socket newneo = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);



        var sendData = Encoding.ASCII.GetBytes(ip + port);
        ////newneo.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
      }



      if (Command == "GETSTUDENT")
      {

        this.student = dataBase.SelectFromStudentByFingerID(Convert.ToInt32(ValueString));

        if (this.student != null)
        {
          this.attendance = dataBase.SelectFromAttendanceByStudentID(this.student.StudentID);

          if (this.attendance == null)
          {
            dataBase.InsertToAttendance(student.StudentID);
            var sendData = Encoding.ASCII.GetBytes("DATABASEDATABACK#STUDENTDATA#" + student.Name + "," + student.Surname + ",");
            clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
            Command = "";
            ValueString = "";
          }
          else if (this.attendance.TimeOUT == null)
          {
            dataBase.UpdateAttendaceTimeOUT(student.StudentID);
            var sendData = Encoding.ASCII.GetBytes("DATABASEDATABACK#STUDENTDATAFINAL#" + student.Name + "," + student.Surname + ",");
            clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
            Command = "";
            ValueString = "";
          }
          else
          {
            var sendData = Encoding.ASCII.GetBytes("DATABASEDATABACK#STUDENTDATAERROR#" + student.Name + "," + student.Surname + ",YOU CAN'T REGISTER,");
            clientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, SendCallback, null);
            Command = "";
            ValueString = "";
          }
        }

      }





    }
  }
}