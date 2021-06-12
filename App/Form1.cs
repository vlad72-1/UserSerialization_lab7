using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;

namespace App
{

    public partial class Form1 : Form
    {

        static string pathToDownloads;
        static int serverPort = default;
        static int clientPort = default;
        static string localHost = "127.0.0.1";
        static IPAddress ipAddress = IPAddress.Parse (localHost);
        static Socket serverSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        Thread receiveThread = new Thread (() => {});
        Thread startThreadListener = new Thread (() => {});
        NetworkStream ns;

        delegate void forInvoke (Socket clientSocket);
        forInvoke forInvoke_;

        public Form1 ()
        {
            InitializeComponent ();

            comboBox1.SelectedIndex = 0;
            textBoxServPort.Text = 8004.ToString();
            textBoxClPort.Text = 8004.ToString();
            textBoxServPort.KeyPress += textBox_KeyPress;
            textBoxClPort.KeyPress += textBox_KeyPress;
        }

        private byte [] TxtFormat (string filename)
        {
            byte[] fileData = File.ReadAllBytes (filename);
            filename = Path.GetFileName (filename);
            byte[] fileNameByte = Encoding.ASCII.GetBytes (filename);
            byte[] fileNameLen = BitConverter.GetBytes (fileNameByte.Length);
            byte[] clientData = new byte [4 + fileNameByte.Length + fileData.Length];

            fileNameLen.CopyTo (clientData, 0);
            fileNameByte.CopyTo (clientData, 4);
            fileData.CopyTo (clientData, 4 + fileNameByte.Length);

            return clientData;
        }

        public void BinaryFormat (Socket clientSocket)
        {
            Person person = new Person ("Smorodinov", 23, "Novosibirsk", 630001);
            ns = new NetworkStream (clientSocket);
            BinaryFormatter bf = new BinaryFormatter ();
            bf.Serialize (ns, person);
        }

        public void JSONFormat (Socket clientSocket)
        {
            List <Student> students = new List <Student> ();

            students.Add (new Student ("Igorov", 21, "Krasnodar"));
            students.Add (new Student ("Ivanov", 26, "SanktPeterburg"));

            ns = new NetworkStream (clientSocket);
            DataContractJsonSerializer jsonFormat = new DataContractJsonSerializer (typeof (List<Student>));
            jsonFormat.WriteObject (ns, students);
        }

        public void XMLFormat (Socket clientSocket)
        {
            List <Person> persons = new List <Person> ();

            persons.Add (new Person ("Petrov", 24, "Tyumen", 625000));
            persons.Add (new Person ("Butakov", 19, "Moskow", 101000));

            ns = new NetworkStream (clientSocket);
            var xmlFormat = new XmlSerializer (typeof (List <Person>));         
            xmlFormat.Serialize (ns, persons);
        }

        public void Receive (Socket clientSocket)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                {
                    byte [] clientData = new byte [1024 * 5000];
                    int receivedBytesLen = clientSocket.Receive (clientData);
                    int fileNameLen = BitConverter.ToInt32 (clientData, 0);
                    string fileName = Encoding.ASCII.GetString (clientData, 4, fileNameLen);
                    fileName = Path.GetFileName (fileName);

                    BinaryWriter bWrite = new BinaryWriter (File.Open (pathToDownloads + fileName, FileMode.Create));
                    bWrite.Write (clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);
                    bWrite.Close ();

                    MessageBox.Show ($"Файл \"{fileName}\" сохранен.", "Результат работы программы");

                    break;
                }
                case 1:
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ns = new NetworkStream (clientSocket);
                    Person person = (Person) formatter.Deserialize (ns);
                    ns.Close ();

                    MessageBox.Show ("Данные \"" + person.Name + "\" \"" + person.Age + "\" \"" + person.City + "\" \"" +
                        person.Post + "\" получены.", "Результат работы программы");

                    break;
                }
                case 2:
                    {
                    DataContractJsonSerializer jsonFormat = new DataContractJsonSerializer (typeof (List <Student>));
                    ns = new NetworkStream (clientSocket);
                    var students = jsonFormat.ReadObject (ns) as List <Student>;
                    using (FileStream file = new FileStream (pathToDownloads + "file.json", FileMode.Create))
                        jsonFormat.WriteObject (file, students);
                    ns.Close ();

                    MessageBox.Show ($"Файл \"file.json\" сохранен.", "Результат работы программы");

                    break;
                }
                case 3:
                {
                    XmlSerializer xmlFormat = new XmlSerializer (typeof (List <Person>));
                    ns = new NetworkStream (clientSocket);
                    var persons = xmlFormat.Deserialize (ns) as List <Person>;
                    using (FileStream file = new FileStream (pathToDownloads + "file.xml", FileMode.Create))
                        xmlFormat.Serialize (file, persons);
                    ns.Close ();

                    MessageBox.Show ($"Файл \"file.xml\" сохранен.", "Результат работы программы");

                    break;
                }
            }

            clientSocket.Close ();
            receiveThread.Abort ();
        }

        private void Send (int clientPort, string filename = "")
        {
            IPAddress address = IPAddress.Parse (localHost);
            IPEndPoint clientEP = new IPEndPoint (address, clientPort);
            Socket clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            clientSocket.Connect (clientEP);

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                {
                    clientSocket.Send (TxtFormat (filename));
                    break;
                }
                case 1:
                {
                    BinaryFormat (clientSocket);
                    ns.Close ();
                    break;
                }
                case 2:
                {
                    JSONFormat (clientSocket);
                    ns.Close ();
                    break;
                }
                case 3:
                {
                    XMLFormat (clientSocket);
                    ns.Close ();
                    break;
                }
            }

            clientSocket.Close ();
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e)
        {
            serverSocket.Close ();
            startThreadListener.Abort ();
            receiveThread.Abort ();
        }

        private void button1_Click (object sender, EventArgs e)
        {
            if (textBoxClPort.Text != string.Empty)
            {
                clientPort = Int32.Parse (textBoxClPort.Text);
                switch (comboBox1.SelectedIndex)
                {
                    case 0: 
                    {
                        OpenFileDialog openFile = new OpenFileDialog();
                        openFile.Filter = "Text|*.txt|All|*.*";

                        if (openFile.ShowDialog () == DialogResult.Cancel)
                            return;
                        Send (clientPort, openFile.FileName);
                        break;
                    }
                    default: 
                    {
                        Send(clientPort);
                        break;
                    }
                }
            }
            else
                MessageBox.Show ("Введите номер порта.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button2_Click (object sender, EventArgs e)
        {
            if (textBoxServPort.Text != string.Empty)
            {
                button2.Enabled = false;
                serverPort = Int32.Parse (textBoxServPort.Text);
                IPEndPoint serverEP = new IPEndPoint (ipAddress, serverPort);
                forInvoke_ = Receive;

                startThreadListener = new Thread (() =>
                {
                    serverSocket.Bind (serverEP);
                    serverSocket.Listen (serverPort);

                    while (true)
                    {
                        Socket clientSocket = serverSocket.Accept();
                        receiveThread = new Thread (() =>
                        {
                            Invoke (forInvoke_, clientSocket);
                        });
                        receiveThread.Start ();
                    }
                });
                startThreadListener.Start();
            }
            else
                MessageBox.Show ("Введите номер порта.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button3_Click (object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog ();

            FBD.ShowNewFolderButton = false;

            if (FBD.ShowDialog () == DialogResult.OK)
                pathToDownloads = FBD.SelectedPath + @"\";

            button3.Enabled = false;
        }

        private void textBox_KeyPress (object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit (e.KeyChar) && !char.IsControl (e.KeyChar);
        }
    }
}
