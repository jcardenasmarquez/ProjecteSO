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

namespace Projecte_SO
{
    public partial class Form1 : Form
    {
        Socket server;
        Thread atender;
        Form2 register = new Form2();
        List<string> conectados = new List<string>();
        List<PictureBox> cartas = new List<PictureBox>(); //per a moure les cartes
        bool registrat;

        public void emplenarLlistadecartes()
        {
            for (int i = 0; i < 100; i++)
            {
                PictureBox carta = new PictureBox();
                cartas.Add(carta);
            }
        }

        //Per resoldre cross-threading
        delegate void DelegadoParaFondo(System.Drawing.Color color);
        delegate void DelegadoParaVisible();
        delegate void DelegadoParaInvisible();

        //funcions cross-threading
        public void PonColor(System.Drawing.Color color)
        {
            this.BackColor = color;
        }

        public void HazVisible()
        {
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            radioButton1.Visible = true;
            radioButton2.Visible = true;
            radioButton3.Visible = true;
            comboBox1.Visible = true;
            comboBox2.Visible = true;
            comboBox3.Visible = true;
            comboBox4.Visible = true;
            button2.Visible = true;
            label6.Visible = true;
            conectadosGrid.Visible = true;
            label8.Visible = true;
            taulerdeJoc.Visible = true;
        }

        public void HazInvisible()
        {
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton3.Visible = false;
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            comboBox4.Visible = false;
            button2.Visible = false;
            label6.Visible = false;
            conectadosGrid.Visible = false;
            label8.Visible = false;
            taulerdeJoc.Visible = false;
        }

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton3.Visible = false;
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            comboBox3.Visible = false;
            comboBox4.Visible = false;
            button2.Visible = false;
            label6.Visible = false;
            conectadosGrid.Visible = false;
            label8.Visible = false;
            taulerdeJoc.Visible = false;
        }

        private void AtendreServidor()
        {
            while (true)
            {
                //Recibimos mensaje del servidor
                byte[] msg = new byte[512];
                server.Receive(msg);

                string[] missatgerebut = Encoding.ASCII.GetString(msg).Split('/');
                try
                {
                    int codi = Convert.ToInt32(missatgerebut[0].Split('\0')[0]);
                    string missatge = missatgerebut[1].Split('\0')[0];

                    switch (codi)
                    {
                        case 0: //Desconnecta el client
                            if (missatge == "error")
                            {
                                MessageBox.Show("Error en la desconnexió");
                            }
                            else
                            {
                                registrat = false;
                                MessageBox.Show("Usuari desconnectat");

                                //Nos desconectamos
                                DelegadoParaFondo delegado = new DelegadoParaFondo(PonColor);
                                this.Invoke(delegado, new object[] { System.Drawing.Color.Gray });
                                DelegadoParaInvisible delegado3 = new DelegadoParaInvisible(HazInvisible);
                                this.Invoke(delegado3);
                                server.Shutdown(SocketShutdown.Both);
                                server.Close();
                                atender.Abort();
                            }
                            break;
                                
                        case 1: //CONSULTA 1
                            MessageBox.Show("La durada de la partida es " + missatgerebut[1]);
                            break;

                        case 2: //CONSULTA 2
                            MessageBox.Show("Ha mort  " + missatgerebut[1] + " cops");
                            break;

                        case 3: //CONSULTA 3   
                            MessageBox.Show("El personatge s'ha escollit " + missatgerebut[1] + " cops");
                            break;
                        
                        case 4:
                            if (missatge == "Login correcte")
                            {
                                registrat = true;
                                DelegadoParaFondo delegado = new DelegadoParaFondo(PonColor);
                                this.Invoke(delegado, new object[] { System.Drawing.Color.Green });
                                DelegadoParaVisible delegado2 = new DelegadoParaVisible(HazVisible);
                                this.Invoke(delegado2);

                                //Añadir a lista de conectados
                                string mensaje = "6/" + textBox1.Text;
                                byte[] envia = System.Text.Encoding.ASCII.GetBytes(mensaje);
                                server.Send(envia);
                            }
                            else
                                MessageBox.Show(missatgerebut[1]);
                            break;

                        case 5:
                            register.DameMensaje(missatge);
                            register.Resultado();
                            break;
                               

                        case 6: //LLISTA CONNECTATS
                            int i = 2;
                            conectados.Clear();
                            while (i < missatgerebut.Length)
                            {
                                conectados.Add(missatgerebut[i]);
                                i = i + 2;
                            }

                            conectadosGrid.ColumnCount = 1;
                            conectadosGrid.RowCount = conectados.Count;
                            for (int k = 0; k < conectados.Count; k++)
                                conectadosGrid.Rows[k].Cells[0].Value = conectados[k];

                            break;
                                        
                    }
                }

                catch (SocketException)
                {
                    MessageBox.Show("El servidor ha caigut");
                }
            }
        }

//         ------------------------------------
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

                }

                if (radioButton2.Checked)
                {
                    string missatge = "2/" + comboBox1.Text + "/" + comboBox2.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                }

                if (radioButton3.Checked)
                {
                    string missatge = "3/" + comboBox3.Text + "/" + comboBox4.Text;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

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
            emplenarLlistadecartes();
            
            if ((textBox1.Text) == "" || (textBox2.Text) == "")
                MessageBox.Show("Emplena els camps de usuari i contraseña !!");
            else
            {
                IPAddress direc = IPAddress.Parse("192.168.56.102"); //entorn de produccio: 147.83.117.22, maquina virtual: 192.168.56.102
                IPEndPoint ipep = new IPEndPoint(direc, 9070); //Juan: 50066, Gerard: 50067, Maria: 50068
                //creem socket
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                   
                    server.Connect(ipep);//Intentem connectar el socket
                    

                    ThreadStart ts = delegate { AtendreServidor(); };
                    atender = new Thread(ts);
                    atender.Start();

                    string missatge = "4/" + textBox1.Text + "/" + textBox2.Text;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);        

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
            register.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Mensaje de desconexión
            string mensaje = "0/" + textBox1.Text;

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);


        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                this.textBox2.PasswordChar = '\0';
            else
                this.textBox2.PasswordChar = '*';

        }

        //INTERFAZ GRÁFICA

        int cont = 0;
        //Moure carta
        private void SeleccionarCarta(object sender, EventArgs e)
        {
            if (registrat == true)
            {
                PictureBox carta = (PictureBox)sender;
                carta.Location = new Point(360 + 10 * cont, 360); 
                carta.Image = new Bitmap("escudo.jpg");
                carta.SizeMode = PictureBoxSizeMode.StretchImage;
                cont++;
            }
        }

        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SeleccionarCarta(sender,e);
        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            SeleccionarCarta(sender,e);
        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            SeleccionarCarta(sender,e);
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            SeleccionarCarta(sender, e);
        }
    }
}

