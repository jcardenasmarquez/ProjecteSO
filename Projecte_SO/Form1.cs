using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Projecte_SO
{
    public partial class Form1 : Form
    {
        Socket server;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton1.Checked)
                {
                    string missatge = "1/";
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                    //Rebem la resposta del servidor
                    byte[] msg2 = new byte[80];
                    server.Receive(msg2);
                    missatge = Encoding.ASCII.GetString(msg2).Split('\0')[0];
                    MessageBox.Show("La durada de la partida es " + missatge);
                }

                if (radioButton2.Checked)
                {
                    string missatge = "2/" + comboBox1.Text + "/" + comboBox2.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                    //Rebem la resposta del servidor
                    byte[] msg2 = new byte[80];
                    server.Receive(msg2);
                    missatge = Encoding.ASCII.GetString(msg2).Split('\0')[0];
                    MessageBox.Show("Ha mort  " + missatge + " cops");
                }

                if (radioButton2.Checked)
                {
                    string missatge = "3/" + comboBox3.Text + "/" + comboBox4.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                    //Rebem la resposta del servidor
                    byte[] msg2 = new byte[80];
                    server.Receive(msg2);
                    missatge = Encoding.ASCII.GetString(msg2).Split('\0')[0];
                    MessageBox.Show("El personatge s'ha escollit " + missatge + " cops");
                }
            }

            catch (SocketException)
            {
                MessageBox.Show("Error. No hi ha connexió");
                return;
            }

            catch (NullReferenceException)
            {
                MessageBox.Show("Error en les dades");
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text) == "" || (textBox2.Text) == "")
                MessageBox.Show("Emplena els camps de usuari i contraseña !!");
            else
            {
                IPAddress direc = IPAddress.Parse("192.168.56.101");
                IPEndPoint ipep = new IPEndPoint(direc, 9070);
                //creem socket
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);//Intentem connectar el socket
                    this.BackColor = Color.Green;
                }
                catch (SocketException)
                {
                    //Si hi ha exception imprimir l'error i sortim amb el return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form2 register = new Form2();
            register.ShowDialog();
        }
    }
}

