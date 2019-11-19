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
        string mensaje;
        
        public Form2()
        {
            InitializeComponent();
        }

        public void DameMensaje(string mensaje)
        {
            this.mensaje = mensaje;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((usuari.Text == "") || (contra.Text == "") || (repetircontra.Text == ""))
                MessageBox.Show("Error. Falta algún campo por poner");

            else
            {
                if (contra.Text == repetircontra.Text)
                {
                    IPAddress direc = IPAddress.Parse("147.83.117.22"); //entorn de produccio: 147.83.117.22, maquina virtual: 192.168.56.102
                    IPEndPoint ipep = new IPEndPoint(direc, 50067);

                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        server.Connect(ipep);

                        string mensaje = "5/" + usuari.Text + "/" + contra.Text + "/" + repetircontra.Text;

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
                else
                    MessageBox.Show("Les contrasenyes no coincideixen");
            }
        }

        public void Resultado()
        {
            if (mensaje == "0")
            {
                MessageBox.Show("Usuari creat correctament");
                this.Close();
            }
            else
            {
                if (mensaje == "-2")
                    MessageBox.Show("Aquest usuari ja esta registrat a la BBDD");
                else
                    MessageBox.Show("Error creant l'usuari");
            }
        }
    }
}
