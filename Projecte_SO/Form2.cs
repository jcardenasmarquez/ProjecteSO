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
using System.Threading;
using System.Media;

namespace Projecte_SO
{
    public partial class Form2 : Form
    {
        Socket server;
        
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((usuari.Text == "") || (contra.Text == "") || (repetircontra.Text == ""))
                MessageBox.Show("Error. Falta algún campo por poner");

            else
            {
                IPAddress direc = IPAddress.Parse("192.168.56.101");
                IPEndPoint ipep = new IPEndPoint(direc, 9070);

                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Connect(ipep);

                    //PONGO EN MARCHA EL THREAD AQUI PORQUE TIENE QUE RECIBIR UNA RESPUESTA DEL SERVIDOR ANTES DE CONECTARSE A EL
                    //el thread que atenderá los mensajes del servidor
                    //ThreadStart ts = delegate { AtenderServidor(); };
                    //atender = new Thread(ts);
                    //atender.Start();


                    string mensaje = "5/" + usuari.Text + "," + contra.Text + "," + repetircontra.Text;

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }

                catch (SocketException)
                {
                    //Si hay excepcion imprimimos error y salimos del programa con return 
                    MessageBox.Show("No he podido conectar con el servidor");
                    return;
                }

                catch (NullReferenceException)
                {
                    MessageBox.Show("Error. No he podido conectar con servidor");
                }
            }
        }
    }
}
