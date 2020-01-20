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
    
    public partial class Principal : Form
    {
        //Variables globals 
        int segundos2 = 5;
        int segundos3 = 5;
        int segundos4 = 5;
        int segundos = 20;
        int aceptados = 0;
        bool aceptado = false;
        bool eliminado = false;
        int uno = 0; // para que solo se pueda hacer una sola jugada por turno
        int num = 0;
        Socket server;
        Thread atender;
        Form2 register = new Form2();
        Stack<string> piladecartes = new Stack<string>();
        List<string> conectados = new List<string>();
        List<string> invitados = new List<string>();
        List<string> jugadores = new List<string>();
        List<PictureBox> cartas = new List<PictureBox>(); //per a moure les cartes
        bool registrat;
        string nombre; //usado para las jugadas de la partida
        string carta;
        string adversario; //usado para las jugadas de la partida
        string[] cartas_jug1 = new string[4];
        string[] cartas_jug2 = new string[4];
        string[] cartas_jug3 = new string[4];
        string[] cartas_jug4 = new string[4];
        bool host = false;
        bool miturno = false;
        string atacado;
        string nuevavida;
        string defendido;


        //No la fem servir
        private void emplenarLlistadecartes()
        {
            //Cada carta es un numero
            ///CARTES DEFENSA
            /// 1 -> +5 punts
            /// 2 -> +10 punts
            /// 3 -> +3 punts
            /// 4 -> +15 punts
            /// 5 -> +1 punt
            /// 6 -> +30 punts
            /// 7 -> +8 punts
            /// CARTES ATAC
            /// 8 -> -5 punts
            /// 9 -> -7 punts
            /// 10 -> -30 punts
            /// 11 -> -10 punts
            /// 12 -> -10 punts
            /// 13 -> -2 punts
            /// 14 -> -15 punts
            /// 15 -> -12 punts
            /// 16 -> -1 punt
            /// 17 -> -3 punts

            for (int i = 0; i < 100; i++)
            {
                PictureBox carta = new PictureBox();
                cartas.Add(carta);
            }
        }

        //Delegats per resoldre cross-threading
        delegate void DelegadoParaFondo(System.Drawing.Color color);
        delegate void DelegadoParaVisible();
        delegate void DelegadoParaInvisible();
        delegate void DelegadoParaPonerListaConectados();
        delegate void DelegadoParaEscribirTexto(string texto);
        delegate void DelegadoPonMensajeRTBox(RichTextBox rtb, string mensaje, Color color, string[] usuaris_xat);
        delegate void DelegadoParaRellenarInvitarBox(string[] missatgeinvitats);
        delegate void DelegadoParaTimer(int num);
        delegate void DelegadoParaNoPoderEsctibir();
        delegate void DelegadoParaPoderEscribir();
        delegate void DelegadoParaRepartir(Stack<string> baraja, List<string> jugadores);
        delegate void DelegadoParaCambiarVida(string vida);
        delegate void DelegadoParaRobar(Stack<string> baraja);
        delegate void DelegagadoParaVerJugada(string carta);
        delegate void DelegadoParaPonerGIF(string carta);
        delegate void DelegadoParaMatar(string nombre);
        delegate void DelegadoParaGanador();

        //FUNCIONS PER CROSS-THREADING
        //Funció que actualitza la llista de connectats, col·loca els connectats a un DataGridView
        private void PonListaConectados()
        {
            conectadosGrid.ColumnCount = 1;
            conectadosGrid.RowCount = conectados.Count;
            for (int k = 0; k < conectados.Count; k++)
            {
                conectadosGrid.Rows[k].Cells[0].Value = conectados[k];

            }   
        }

        //Funció que utilitzem per fer invisible el Tauler a la persona que ha mort i parar el timer, treure-lo de la partida
        private void HazInvisible_muerto()
        {
            label8.Visible = false;
            taulerdeJoc.Visible = false;
            probalabel.Visible = false;
            usuari1GroupBox.Visible = false;
            Usuari2GroupBox.Visible = false;
            Usuari3GroupBox.Visible = false;
            usuari4GroupBox.Visible = false;
            xatGroupBox.Visible = false;
            turnoLbl.Visible = false;
            timer2.Stop();
            timer3.Start();// Aquest timer mostra un missatge temporal informant a la persona que ha perdut
        }

        //Funció que fa invisible el panel al guanyador(quan només queda una persona viva)
        private void HazInvisible_ganador()
        {
            label8.Visible = false;
            taulerdeJoc.Visible = false;
            probalabel.Visible = false;
            usuari1GroupBox.Visible = false;
            Usuari2GroupBox.Visible = false;
            Usuari3GroupBox.Visible = false;
            usuari4GroupBox.Visible = false;
            xatGroupBox.Visible = false;
            turnoLbl.Visible = false;
            timer2.Stop();
            timer5.Start();//Mostra temporalment un missatge informant a la persona que ha guanyat
            
        }

        //Funció que posa un GIF temporal quan s'utilitzen les cartes més poderoses
        private void PonGIF(string carta)
        {
            
            pictureBoxGIF.SendToBack();
            if (carta == "6")
            {
                pictureBoxGIF.Visible = true;
                pictureBoxGIF.Image = Image.FromFile("escudoGIF.gif");
                pictureBoxGIF.SizeMode = PictureBoxSizeMode.StretchImage;
            }
                        
            if (carta == "10")
            {
                pictureBoxGIF.Visible = true;
                pictureBoxGIF.Image = Image.FromFile("ataqueGIF.gif");
                pictureBoxGIF.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        //Aquesta funció actualitza la vida de la persona atacada (la nova vida la envia el servidor)
        private void VidaAtaque(string nuevavida)
        {
            if (U1nom.Text == atacado)
                ValorvidaU1.Text = nuevavida;
            if (U2nom.Text == atacado)
                valorvidaU2.Text = nuevavida;
            if (U3nom.Text == atacado)
                valorvidaU3.Text = nuevavida;
            if (U4nom.Text == atacado)
                valorvidaU4.Text = nuevavida;
        }

        //Funció que actualitza la vida de la persona que escull defensar-se
        private void VidaDefensa(string nuevavida)
        {
            if (U1nom.Text == defendido)
                ValorvidaU1.Text = nuevavida;
            if (U2nom.Text == defendido)
                valorvidaU2.Text = nuevavida;
            if (U3nom.Text == defendido)
                valorvidaU3.Text = nuevavida;
            if (U4nom.Text == defendido)
                valorvidaU4.Text = nuevavida;
        }

        //Funció que invalida el Textbox1, ho fem perq un cop s'ha connectat l'usuari utilitzem el TextBox1 per estreure dades
        private void InvalidarTextbox1()
        {
            textBox1.ReadOnly = true;
        }

        //Torna a validar el textbox
        private void ValidarTextbox1()
        {
            textBox1.ReadOnly = false;
        }

        //Funció que comença un timer determinat segons el numero que rep com a parametre
        private void StartTimer(int num)
        {
            if (num == 1)
                timer1.Start();
            if(num == 2)
            {
                timer2.Start();
                uno = 1;
            }
            if (num == 3)
            {
                timer3.Start();
            }
            if (num == 4)
            {
                timer4.Start();
            }
        }
        
        //Funció per parar els timers
        private void StopTimer(int num)
        {
            if (num == 1)
                timer1.Stop();

            if (num == 2)
            {
                timer2.Stop();
                
            }
            if (num == 3)
            {
                timer3.Stop();
            }
            if (num == 4)
            {
                timer4.Stop();
            }
            
        }

        //Funció per escriure als labels quan algú et convida a jugar
        private void RellenarInvitarBox(string[] missatgeinvitats)
        {
            jugadorqueinvitaLbl.Text = missatgeinvitats[0]; //error cross threading
            partidaLbl.Text = missatgeinvitats[1];
        }

        //Canviar color del fons
        private void PonColor(System.Drawing.Color color)
        {
            this.BackColor = color;
        }

        

        private void HazVisibleBaraja()
        {
            pictureBoxGIF.SendToBack();
            pictureBoxJugadas.Visible = true;
            pictureBoxBaraja.Visible = true;
            pictureBoxBaraja.Image = Image.FromFile("parte trasera.jpg"); //hacer que la BackgroundImageLayout sea stretch
            pictureBoxBaraja.SizeMode = PictureBoxSizeMode.StretchImage;
            //pictureBoxBaraja.BackgroundImageLayout;
        }

        private void VerJugada(string carta)
        {
            pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
            pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        //Aquesta funció roba automàticament una carta del mazo quan has fet servir una de la teva mà
        private void Robar(Stack<string> baraja)
        {
            string cartaRobada = baraja.Pop();
            bool robada = false;
            if (baraja != null)
            {
                //Utilitzem el bool robada perque quan hagi robat , no entri en els altres ifs si per casualitat hi ha una carta que es igual
                if (pictureBox1.Tag.ToString() == carta && (robada == false))
                {
                    robada = true;
                    pictureBox1.Image = Image.FromFile(cartaRobada + ".jpg");
                    pictureBox1.Tag = cartaRobada;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }

                if (pictureBox2.Tag.ToString() == carta && (robada == false))
                {
                    robada = true;
                    pictureBox2.Image = Image.FromFile(cartaRobada + ".jpg");
                    pictureBox2.Tag = cartaRobada;
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                }

                if (pictureBox3.Tag.ToString() == carta && (robada == false))
                {
                    robada = true;
                    pictureBox3.Image = Image.FromFile(cartaRobada + ".jpg");
                    pictureBox3.Tag = cartaRobada;
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                }

                if (pictureBox4.Tag.ToString() == carta && (robada == false))
                {
                    robada = true;
                    pictureBox4.Image = Image.FromFile(cartaRobada + ".jpg");
                    pictureBox4.Tag = cartaRobada;
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
            else
            {
                MessageBox.Show("No quedan más cartas en la baraja");
                pictureBoxBaraja.Image = null;
                pictureBoxBaraja.BackColor = Color.Transparent;
            }
        }

        //Aquesta funció reparteix les cartes aleatoriament amb una pila creada a partir d'un vector que traiem del servidor.
        //Reparteix segons el nombre d'acceptats
        //Fem el .Pop per ananr eliminant les cartes que es reparteixen
        //Aquesta funció també posa el nom dels usaris al panel i la seva corresponent vida
        private void Repartir(Stack<string> baraja, List<string> usuaris)
        {

            baraja.Pop();
            if (aceptados == 1)
            {

                int j = 0;
                while (j < 4)
                {
                    cartas_jug1[j] = baraja.Pop();
                    cartas_jug2[j] = baraja.Pop();
                    j++;
                }

                if (usuaris[0] == textBox1.Text)
                {
                    U1nom.Text = usuaris[0];
                    U2nom.Text = usuaris[1];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug1[0] + ".jpg");
                    pictureBox1.Tag = cartas_jug1[0];
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug1[1] + ".jpg");
                    pictureBox2.Tag = cartas_jug1[1];
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug1[2] + ".jpg");
                    pictureBox3.Tag = cartas_jug1[2];
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug1[3] + ".jpg");
                    pictureBox4.Tag = cartas_jug1[3];
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else if (usuaris[1] == textBox1.Text)
                {
                    U1nom.Text = usuaris[1];
                    U2nom.Text = usuaris[0];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug2[0] + ".jpg");
                    pictureBox1.Tag = cartas_jug2[0];
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug2[1] + ".jpg");
                    pictureBox2.Tag = cartas_jug2[1];
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug2[2] + ".jpg");
                    pictureBox3.Tag = cartas_jug2[2];
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug2[3] + ".jpg");
                    pictureBox4.Tag = cartas_jug2[3];
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }

            if (aceptados == 2)
            {
                int j = 0;
                while (j < 4)
                {
                    cartas_jug1[j] = baraja.Pop();
                    cartas_jug2[j] = baraja.Pop();
                    cartas_jug3[j] = baraja.Pop();
                    j++;
                }

                if (usuaris[0] == textBox1.Text)
                {
                    U1nom.Text = usuaris[0];
                    U2nom.Text = usuaris[1];
                    U3nom.Text = usuaris[2];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug1[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug1[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug1[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug1[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug1[0];
                    pictureBox2.Tag = cartas_jug1[1];
                    pictureBox3.Tag = cartas_jug1[2];
                    pictureBox4.Tag = cartas_jug1[3];
                }
                else if (usuaris[1] == textBox1.Text)
                {
                    U1nom.Text = usuaris[1];
                    U2nom.Text = usuaris[0];
                    U3nom.Text = usuaris[2];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug2[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug2[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug2[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug2[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug2[0];
                    pictureBox2.Tag = cartas_jug2[1];
                    pictureBox3.Tag = cartas_jug2[2];
                    pictureBox4.Tag = cartas_jug2[3];
                }
                else if (usuaris[2] == textBox1.Text)
                {
                    U1nom.Text = usuaris[2];
                    U2nom.Text = usuaris[1];
                    U3nom.Text = usuaris[0];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug3[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug3[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug3[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug3[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug3[0];
                    pictureBox2.Tag = cartas_jug3[1];
                    pictureBox3.Tag = cartas_jug3[2];
                    pictureBox4.Tag = cartas_jug3[3];
                }
            }

            if (aceptados == 3)
            {
                int j = 0;
                while (j < 4)
                {
                    cartas_jug1[j] = baraja.Pop();
                    cartas_jug2[j] = baraja.Pop();
                    cartas_jug3[j] = baraja.Pop();
                    cartas_jug4[j] = baraja.Pop();
                    j++;
                }


                if (usuaris[0] == textBox1.Text)
                {

                    U1nom.Text = usuaris[0];
                    U2nom.Text = usuaris[1];
                    U3nom.Text = usuaris[2];
                    U4nom.Text = usuaris[3];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    valorvidaU4.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug1[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug1[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug1[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug1[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug1[0];
                    pictureBox2.Tag = cartas_jug1[1];
                    pictureBox3.Tag = cartas_jug1[2];
                    pictureBox4.Tag = cartas_jug1[3];
                }
                else if (usuaris[1] == textBox1.Text)
                {
                    U1nom.Text = usuaris[1];
                    U2nom.Text = usuaris[2];
                    U3nom.Text = usuaris[3];
                    U4nom.Text = usuaris[0];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    valorvidaU4.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug2[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug2[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug2[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug2[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug2[0];
                    pictureBox2.Tag = cartas_jug2[1];
                    pictureBox3.Tag = cartas_jug2[2];
                    pictureBox4.Tag = cartas_jug2[3];
                }
                else if (usuaris[2] == textBox1.Text)
                {
                    U1nom.Text = usuaris[2];
                    U2nom.Text = usuaris[3];
                    U3nom.Text = usuaris[1];
                    U4nom.Text = usuaris[0];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    valorvidaU4.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug3[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug3[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug3[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug3[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug3[0];
                    pictureBox2.Tag = cartas_jug3[1];
                    pictureBox3.Tag = cartas_jug3[2];
                    pictureBox4.Tag = cartas_jug3[3];
                }
                else if (usuaris[3] == textBox1.Text)
                {
                    U1nom.Text = usuaris[3];
                    U2nom.Text = usuaris[1];
                    U3nom.Text = usuaris[2];
                    U4nom.Text = usuaris[0];
                    ValorvidaU1.Text = "50";
                    valorvidaU2.Text = "50";
                    valorvidaU3.Text = "50";
                    valorvidaU4.Text = "50";
                    pictureBox1.Image = Image.FromFile(cartas_jug4[0] + ".jpg");
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = Image.FromFile(cartas_jug4[1] + ".jpg");
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox3.Image = Image.FromFile(cartas_jug4[2] + ".jpg");
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox4.Image = Image.FromFile(cartas_jug4[3] + ".jpg");
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Tag = cartas_jug4[0];
                    pictureBox2.Tag = cartas_jug4[1];
                    pictureBox3.Tag = cartas_jug4[2];
                    pictureBox4.Tag = cartas_jug4[3];
                }
            }
        }

        //Funció que posa una calavera al costat de la persona morta
        //Només ho veuen les persones que no han mort, la morta surt de la partida
        private void Muerte(string nombre)
        {
            if (U2nom.Text == nombre)
            {
                pictureBox17.Image = Image.FromFile("calavera.png");
                pictureBox17.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            if (U3nom.Text == nombre)
            {
                pictureBox19.Image = Image.FromFile("calavera.png");
                pictureBox19.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            if (U4nom.Text == nombre)
            {
                pictureBox18.Image = Image.FromFile("calavera.png");
                pictureBox18.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void PonMuertoGIF()
        {
            pictureBox20.Visible = true;
            pictureBox20.BringToFront();
            pictureBox20.Image = Image.FromFile("muerteGIF.gif");
            pictureBox20.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        //Funció que posa un gif temporal un cop guanya alguú
        private void PonGanador()
        {
            pictureBox20.Visible = true;
            pictureBox20.BringToFront();
            pictureBox20.Image = Image.FromFile("winnerGIF.gif");
            pictureBox20.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void HazVisibleDonarsedeBaixa()
        {
            donarsedebaixaGroupBox.Visible = true;
        }

        private void HazInvisibleDonarsedeBaixa()
        {
            donarsedebaixaGroupBox.Visible = false;
        }

        private void HazInvisibleBaraja()
        {
            pictureBoxBaraja.Visible = false;
        }

        private void HazVisible2jug()
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            usuari1GroupBox.Visible = true;
            Usuari2GroupBox.Visible = true;
        }

        private void HazVisible3jug()
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            pictureBox9.Visible = true;
            pictureBox10.Visible = true;
            pictureBox11.Visible = true;
            pictureBox12.Visible = true;
            usuari1GroupBox.Visible = true;
            Usuari2GroupBox.Visible = true;
            Usuari3GroupBox.Visible = true;
        }

        private void HazVisible4jug()
        {
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            pictureBox9.Visible = true;
            pictureBox10.Visible = true;
            pictureBox11.Visible = true;
            pictureBox12.Visible = true;
            pictureBox13.Visible = true;
            pictureBox14.Visible = true;
            pictureBox15.Visible = true;
            pictureBox16.Visible = true;
            usuari1GroupBox.Visible = true;
            Usuari2GroupBox.Visible = true;
            Usuari3GroupBox.Visible = true;
            usuari4GroupBox.Visible = true;
        }

        private void HazVisibleJugadas()
        {
            pictureBoxJugadas.Visible = true;
        }

        private void HazInvisibleJugadas()
        {
            pictureBoxJugadas.Visible = false;
        }

        private void HazVisibleGroupBox1()
        {
            groupBox1.Visible = true;
        }

        private void HazInvisibleGroupBox1()
        {
            groupBox1.Visible = false;
        }

        private void HazVisibleXatGroupBox()
        {
            xatGroupBox.Visible = true;
        }

        private void HazInvisibleXatGroupBox()
        {
            xatGroupBox.Visible = false;
        }

        private void HazVisibleRegisterGroupBox()
        {
            registerGroupBox.Visible = true;
        }

        private void HazInvisibleRegisterGroupBox()
        {
            registerGroupBox.Visible = false;
        }

        private void HazVisibleDBlink()
        {
            donarsedebaixaLinkLabel.Visible = true;
        }

        private void PonTextoTurno(string nombre)
        {
            this.turnoLbl.Text = "Es el turno de: " + nombre;
        }

        private void EsTuTurno(string mensaje)
        {
            this.turnoLbl.Text = mensaje;
        }

        private void Prueba(string mensaje)
        {
            this.probalabel.Text = mensaje;
        }

        //Funció que escriu el missatge al xatRichBox
        //Hem afegit una extensió del RiichBox al final del codi per tal de poder afegir la funció.Append amb colors
        //Segons si la persona ets tu o un altre et posa el missatge a la dreta o a la esquerra

        private void PonMensajeRTBox(RichTextBox rtb, string mensaje, Color color, string[] usuaris_xat)
        {
            string[] trozos = mensaje.Split(':');
            if (trozos[0] == textBox1.Text)
            {
                rtb.SelectionAlignment = HorizontalAlignment.Right;
                rtb.AppendText("Tu: ", color);//Amb la funció append decidim quina part del missatge volem canviar de color
                rtb.AppendText(trozos[1] + Environment.NewLine);
                //rtb.ForeColor = Color.Black;
            }
            else
            {
                //segons el nom de cada usuari posa un color
                rtb.SelectionAlignment = HorizontalAlignment.Left;
                if (trozos[0] == usuaris_xat[0])
                {
                    rtb.AppendText(trozos[0], Color.Red);
                }
                if (trozos[0] == usuaris_xat[1])
                {
                    rtb.AppendText(trozos[0], Color.Green);
                }
                if (trozos[0] == usuaris_xat[2])
                {
                    rtb.AppendText(trozos[0], Color.Coral);
                }
                if (trozos[0] == usuaris_xat[3])
                {
                    rtb.AppendText(trozos[0], Color.Purple);
                }
                
                rtb.AppendText(": " + trozos[1] + Environment.NewLine);
                //rtb.ForeColor = color;
            }
        }
        
        
        public void HazVisible()
        {
            label3.Visible = true;
            label5.Visible = true;
            jugtextBox.Visible = true;
            data1Box.Visible = true;
            data2Box.Visible = true;
            radioButton1.Visible = true;
            radioButton2.Visible = true;
            radioButton3.Visible = true;
            button2.Visible = true;
            label6.Visible = true;
            conectadosGrid.Visible = true;
            label10.Visible = true;
            invitarButton.Visible = true;
            invitadosBox.Visible = true;
        }

        public void HazVisiblePanel()
        {
            label8.Visible = true;
            taulerdeJoc.Visible = true;
            taulerdeJoc.BringToFront();
        }

        public void HazInvisiblePanel()
        {
            label8.Visible = false;
            taulerdeJoc.Visible = false;
            taulerdeJoc.SendToBack();
        }

        public void HazInvisible()
        {
            label3.Visible = false;
            label5.Visible = false;
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton3.Visible = false;
            jugtextBox.Visible = false;
            data1Box.Visible = false;
            data2Box.Visible = false;
            button2.Visible = false;
            label6.Visible = false;
            conectadosGrid.Visible = false;
            label8.Visible = false;
            taulerdeJoc.Visible = false;
            label10.Visible = false;
            invitarButton.Visible = false;
            invitadosBox.Visible = false;
            registerGroupBox.Visible = false;
            donarsedebaixaGroupBox.Visible = false;
            donarsedebaixaLinkLabel.Visible = false;
            usuari1GroupBox.Visible = false;
            Usuari2GroupBox.Visible = false;
            Usuari3GroupBox.Visible = false;
            usuari4GroupBox.Visible = false;
        }

        public void PonSegundos(string seg)
        {
            segonslbl.Text = seg;
        }

        public void SegundosTurno(string seg)
        {
            segundosTurno.Text = seg;
        }

        public Principal()
        {
            //Inicialitzem la connexió
            InitializeComponent();
            IPAddress direc = IPAddress.Parse("192.168.56.106"); //entorn de produccio: 147.83.117.22, maquina virtual: 192.168.56.106
            IPEndPoint ipep = new IPEndPoint(direc, 9070); //Juan: 50066, Gerard: 50067, Maria: 50068, maquina virtual: 9050
            //creem socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {

                server.Connect(ipep);//Intentem connectar el socket

                ThreadStart ts = delegate { AtendreServidor(); };
                atender = new Thread(ts);
                atender.Start();

            }
            catch (SocketException)
            {
                //Si hi ha exception imprimir l'error i sortim amb el return 
                MessageBox.Show("No he podido conectar con el servidor");
                return;
            }

            //CheckForIllegalCrossThreadCalls = false;
            label3.Visible = false;
            label5.Visible = false;
            radioButton1.Visible = false;
            radioButton2.Visible = false;
            radioButton3.Visible = false;
            jugtextBox.Visible = false;
            data1Box.Visible = false;
            data2Box.Visible = false;
            button2.Visible = false;
            label6.Visible = false;
            conectadosGrid.Visible = false;
            label8.Visible = false;
            taulerdeJoc.Visible = false;
            groupBox1.Visible = false;
            label10.Visible = false;
            invitarButton.Visible = false;
            invitadosBox.Visible = false;
            xatGroupBox.Visible = false;
            registerGroupBox.Visible = false;
            donarsedebaixaGroupBox.Visible = false;
            donarsedebaixaLinkLabel.Visible = false;
            usuari1GroupBox.Visible = false;
            Usuari2GroupBox.Visible = false;
            Usuari3GroupBox.Visible = false;
            usuari4GroupBox.Visible = false;
        }

        //Timer que es posa en marxa quan algu convida a altres usuaris
        //Els usuaris poden acceptar o rebutjar
        //Si un usuari no contesta es rebutja automaticament
        //Per cada usuari que accepta s'envia el codi 13 al servidor, que anirá sumant el nombre de acceptats

        public void timer1_Tick(object sender, EventArgs e)
        {
            this.segundos = this.segundos - 1;
            this.Invoke(new DelegadoParaEscribirTexto(PonSegundos), new object[] { Convert.ToString(segundos) });
            if ((this.segundos == 0) && (aceptado == true))
            {
                this.Invoke(new DelegadoParaTimer(StopTimer), new object[] { 1 });
                DelegadoParaVisible delegado7 = new DelegadoParaVisible(HazVisibleXatGroupBox);
                this.Invoke(delegado7);
                DelegadoParaVisible delegado6 = new DelegadoParaVisible(HazVisiblePanel);
                this.Invoke(delegado6);
                DelegadoParaVisible delegado19 = new DelegadoParaVisible(HazVisibleBaraja);
                this.Invoke(delegado19);
                DelegadoParaInvisible delegado20 = new DelegadoParaInvisible(HazInvisibleGroupBox1);
                this.Invoke(delegado20);
                this.segundos = 20;
                //Enviem el missatge 13 al servidor perque vagi contant el numero de persones que accepten
                string mensaje = "13/";
                // Enviamos al servidor 
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);

            }
            else if ((this.segundos == 0) && (eliminado == false)) 
            {
                this.Invoke(new DelegadoParaTimer(StopTimer), new object[] { 1 });
                DelegadoParaInvisible delegado6 = new DelegadoParaInvisible(HazInvisiblePanel);
                this.Invoke(delegado6);
                DelegadoParaInvisible delegado20 = new DelegadoParaInvisible(HazInvisibleGroupBox1);
                this.Invoke(delegado20);
                this.segundos = 20;
                //Rebutja la partida per lent
                string mensaje = "12/" + textBox1.Text + "/" + partidaLbl.Text;
                // Enviamos al servidor 
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }
            else if ((this.segundos == 0) && (aceptados == 0))
            {
                this.segundos = 20;
                this.Invoke(new DelegadoParaTimer(StopTimer), new object[] { 1 });
                invitados.Clear();
            }
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
                        case 0: //El servidor desconnecta el client i el client mostra si s'ha desconnectat correctament o ha hagutalgun error.
                            if (missatge == "error")
                            {
                                MessageBox.Show("Error en la desconnexió");
                            }
                            else
                            {
                                registrat = false;
                                MessageBox.Show("Usuari desconnectat");

                                //Ens desconnectem i posem en no visible el tauler
                                DelegadoParaFondo delegado = new DelegadoParaFondo(PonColor);
                                this.Invoke(delegado, new object[] { System.Drawing.Color.Gray });
                                DelegadoParaInvisible delegado3 = new DelegadoParaInvisible(HazInvisible);
                                this.Invoke(delegado3);
                                DelegadoParaInvisible delegado9 = new DelegadoParaInvisible(HazInvisibleGroupBox1);
                                this.Invoke(delegado9);
                                DelegadoParaInvisible delegado10 = new DelegadoParaInvisible(HazInvisibleXatGroupBox);
                                this.Invoke(delegado10);
                                //habilitar textbox1 para que se pueda volver a escribir y loggearte con otro usuario diferente
                                DelegadoParaPoderEscribir delegado11 = new DelegadoParaPoderEscribir(ValidarTextbox1);
                                this.Invoke(delegado11);

                                server.Shutdown(SocketShutdown.Both);
                                server.Close();
                                atender.Abort();
                            }
                            break;
                                
                        case 1: //CONSULTA 1
                            MessageBox.Show("Has jugat amb " + missatge);
                            break;

                        case 2: //CONSULTA 2
                            MessageBox.Show("Els guanyadors de les partides que has jugat amb " +  jugtextBox.Text + " han estat: " +  missatge);
                            break;

                        case 3: //CONSULTA 3   
                            MessageBox.Show("Partides jugades entre aquestes dates: " + missatge);
                            break;
                        
                        case 4://LOG-IN, si el servidor connecta be l'usuari, apareixerà el missatge de Login correcte i afegirà l'usuari a la llista de conectats
                               //Per afegir l'usuari a la llista de connectats enviem el numero 6 al servidor
                            if (missatge == "Login correcte")
                            {
                                registrat = true;
                                DelegadoParaFondo delegado = new DelegadoParaFondo(PonColor);
                                this.Invoke(delegado, new object[] { System.Drawing.Color.Beige });
                                DelegadoParaVisible delegado2 = new DelegadoParaVisible(HazVisible);
                                this.Invoke(delegado2);
                                DelegadoParaVisible delegado3 = new DelegadoParaVisible(HazVisibleDBlink);
                                this.Invoke(delegado3);
                                DelegadoParaNoPoderEsctibir delegado4 = new DelegadoParaNoPoderEsctibir(InvalidarTextbox1);
                                this.Invoke(delegado4);
                                MessageBox.Show(missatgerebut[1]);

                                //Añadir a lista de conectados
                                string mensaje = "6/" + textBox1.Text;
                                byte[] envia = System.Text.Encoding.ASCII.GetBytes(mensaje);
                                server.Send(envia);
                            }
                            else
                                MessageBox.Show(missatgerebut[1]);
                            break;

                        case 5:
                            //Rebem un missatge que ens informa de si s'ha registrat amb èxit o no
                            if (missatge == "0")
                            {
                                MessageBox.Show("Usuari creat correctament");
                            }
                            else
                            {
                                if (missatge == "-2")
                                    MessageBox.Show("Aquest usuari ja esta registrat a la BBDD");
                                else
                                    MessageBox.Show("Error creant l'usuari");
                            }
                            break;
                               

                        case 6: //Notificació que Afegeix els connectats a la llista i els mostra al DataGridView
                            int i = 2;
                            conectados.Clear();
                            while (i < missatgerebut.Length)
                            {
                                conectados.Add(missatgerebut[i]);
                                i = i + 2;
                            }

                            DelegadoParaPonerListaConectados delegado20 = new DelegadoParaPonerListaConectados(PonListaConectados);
                            this.Invoke(delegado20);
                            break;

                        

                        case 9://Donar-se de baixa
                            MessageBox.Show(missatge);
                            if (missatge == "Eliminado correctamente")
                            {
                                //Mensaje de desconexión
                                string mensaje = "0/" + textBox1.Text;
                                byte[] envia = System.Text.Encoding.ASCII.GetBytes(mensaje);
                                server.Send(envia);
                            }
                            break;
                        
                        case 10:

                            this.Invoke(new DelegadoParaTimer(StartTimer), new object[] { 1 });
                            DelegadoParaVisible delegado17= new DelegadoParaVisible(HazVisibleGroupBox1);
                            this.Invoke(delegado17);
                            string[] missatgeinvitats = missatge.Split(',');
                            groupBox1.Invoke(new DelegadoParaRellenarInvitarBox(RellenarInvitarBox), new object[] { missatgeinvitats });

                            break;

                        case 11: //Quan el client accepta la partida

                            aceptados++;
                            DelegadoParaVisible delegado7 = new DelegadoParaVisible(HazVisibleXatGroupBox);
                            this.Invoke(delegado7);
                            string[] resposta = missatge.Split(',');
                            MessageBox.Show(resposta[1] + " ha acceptat jugar la partida");
                            DelegadoParaVisible delegado6 = new DelegadoParaVisible(HazVisiblePanel);
                            this.Invoke(delegado6);
                            DelegadoParaVisible delegado19 = new DelegadoParaVisible(HazVisibleBaraja);
                            this.Invoke(delegado19);

                            if (aceptados == 0)
                            {
                                invitados.Clear();
                            }

                            if (aceptados == 1) //2 jugadores
                            {
                                DelegadoParaVisible delegado2 = new DelegadoParaVisible(HazVisible2jug);
                                this.Invoke(delegado2);
                            }

                            else if (aceptados == 2) //3 jugadores
                            {
                                DelegadoParaVisible delegado8 = new DelegadoParaVisible(HazVisible3jug);
                                this.Invoke(delegado8);
                            }

                            else if (aceptados == 3) //4 jugadores
                            {
                                DelegadoParaVisible delegado9 = new DelegadoParaVisible(HazVisible3jug);
                                this.Invoke(delegado9);
                            }

                            break;

                        case 12: //Quan el client rebutja la partida

                            string[] resposta2 = missatge.Split(',');
                            MessageBox.Show(resposta2[1] + " ha rebutjat la petició de jugar la partida");
                            DelegadoParaInvisible delegado12 = new DelegadoParaInvisible(HazInvisibleGroupBox1);
                            this.Invoke(delegado12);

                            break;

                        case 13: //Utilitzem el numero d'acceptats que rebem del servidor per enviar el numero 14 (nomes els usuaris que han acceptat) al servidor 
                            //Enviem el numero 14 per repartir les cartes
                            this.aceptados = Convert.ToInt32(missatge);

                            if (aceptados == 0)
                            {
                                invitados.Clear();
                            }

                            if (aceptados == 1) //2 jugadores
                            {
                                DelegadoParaVisible delegado2 = new DelegadoParaVisible(HazVisible2jug);
                                this.Invoke(delegado2);
                                if (host == true)
                                {
                                    string mensaje3 = "14/";
                                    byte[] envia3 = System.Text.Encoding.ASCII.GetBytes(mensaje3);
                                    server.Send(envia3);
                                }
                                
                            }

                            else if (aceptados == 2) //3 jugadores
                            {
                                DelegadoParaVisible delegado8 = new DelegadoParaVisible(HazVisible3jug);
                                this.Invoke(delegado8);
                                if (host == true)
                                {
                                    string mensaje3 = "14/";
                                    byte[] envia3 = System.Text.Encoding.ASCII.GetBytes(mensaje3);
                                    server.Send(envia3);
                                }

                            }

                            else if (aceptados == 3) //4 jugadores
                            {
                                DelegadoParaVisible delegado9 = new DelegadoParaVisible(HazVisible4jug);
                                this.Invoke(delegado9);
                                if (host == true)
                                {
                                    string mensaje3 = "14/";
                                    byte[] envia3 = System.Text.Encoding.ASCII.GetBytes(mensaje3);
                                    server.Send(envia3);
                                }
                            }
                            
                            

                            break;

                        case 14://Repartir cartes
                            //Rebem un vector de numeros aleatoris que utilitzem per afegir a una pila i crear un mazo de cartes
                            string[] cartes = missatge.Split(',');
                            
                            int k=0;
                            while (k < cartes.Length)
                            {
                                piladecartes.Push(cartes[k]);
                                k++;
                            }

                            //Afegim els jugadors que han acceptat nomes a la llista de jugadors
                            if (missatgerebut[2] != "-1")
                                jugadores.Add(missatgerebut[2].Split('\0')[0]);
                            if (missatgerebut[3] != "-1")
                                jugadores.Add(missatgerebut[3].Split('\0')[0]);
                            if (missatgerebut[4] != "-1")
                                jugadores.Add(missatgerebut[4].Split('\0')[0]);
                            if (missatgerebut[5] != "-1")
                                jugadores.Add(missatgerebut[5].Split('\0')[0]);
                            
                            //Label de prova per veure els jugadors que han acceptat
                            string msgprova = "";
                            if (aceptados == 1)
                            {
                                msgprova = jugadores[0] + "," + jugadores[1];
                            }
                            if (aceptados == 2)
                            {
                                msgprova = jugadores[0] + "," + jugadores[1] + "," + jugadores[2];
                            }
                            if (aceptados == 3)
                            {
                                msgprova = jugadores[0] + "," + jugadores[1] + "," + jugadores[2] + "," + jugadores[3];
                            }

                            this.Invoke(new DelegadoParaEscribirTexto(Prueba), new object[] { msgprova });
                            this.Invoke(new DelegadoParaRepartir(Repartir), new object[] {piladecartes, jugadores});
                            //Aqui imposem que comenci sempre el jugador de la posició 0
                            if (textBox1.Text == jugadores[0])
                            {
                                miturno = true;    
                            }
                            //Comença el timer i per tant el torn del primer jugador
                            if (miturno == true)
                            {
                                this.segundos = 20;
                                this.Invoke(new DelegadoParaTimer(StartTimer), new object[] { 2 });
                                string turno = "Es tu turno.";
                                this.Invoke(new DelegadoParaEscribirTexto(EsTuTurno), new object[] { turno });
                            }
                            else
                                this.Invoke(new DelegadoParaEscribirTexto(PonTextoTurno), new object[] { jugadores[0] });

                            break;

                        case 15://Passar de torn
                            //Agafem del servidor el seguent torn, que serà el seguent numero de la llista de jugadors i torna a començar el timer per a aquest jugador
                            num = Convert.ToInt32(missatge);
                            if (jugadores[num] == textBox1.Text)
                            {
                                miturno = true;
                                string turno = "Es tu turno.";
                                this.Invoke(new DelegadoParaEscribirTexto(EsTuTurno), new object[] { turno });

                                this.segundos = 20;
                                this.Invoke(new DelegadoParaTimer(StartTimer), new object[] { 2 });
                            }
                            else
                                this.Invoke(new DelegadoParaEscribirTexto(PonTextoTurno), new object[] { jugadores[num] });


                            break;

                        case 16: //Suma vida al jugador que ha realitzat jugada de defensa

                            segundos = 3;
                            string[] trozos = missatge.Split(',');
                            
                            defendido = trozos[0].Split('\0')[0];
                            nuevavida = trozos[1].Split('\0')[0];
                            carta = trozos[2].Split('\0')[0];
                            this.Invoke(new DelegadoParaCambiarVida(VidaDefensa), new object[] { nuevavida });
                            this.Invoke(new DelegagadoParaVerJugada(VerJugada), new object[] { carta });
                            this.Invoke(new DelegadoParaTimer(StartTimer), new object[] { 4 });
                            this.Invoke(new DelegadoParaRobar(Robar), new object[] { piladecartes });

                            break;

                        case 17: //Resta vida a l'adversari

                            segundos = 3;
                            string[] trozos1 = missatge.Split(',');
                            atacado = trozos1[0].Split('\0')[0];
                            nuevavida = trozos1[1].Split('\0')[0];
                            carta = trozos1[2].Split('\0')[0];
                            int vidanueva2 = Convert.ToInt32(nuevavida);
                            //Si la vida es més gran de 0, s'anirà actualitzant el label que mostra el valor
                            if (vidanueva2 > 0)
                                this.Invoke(new DelegadoParaCambiarVida(VidaAtaque), new object[] { nuevavida });
                            else//Si la vida és menor de 0, el jugador por i s'envia un 18 al servidor
                            {
                                string mensaje18 = "18/" + atacado;
                                //Enviamos al servidor
                                byte[] msg18 = System.Text.Encoding.ASCII.GetBytes(mensaje18);
                                server.Send(msg18);
                            }
                            this.Invoke(new DelegadoParaCambiarVida(VidaAtaque), new object[] { nuevavida });
                            this.Invoke(new DelegagadoParaVerJugada(VerJugada), new object[] { carta });
                            this.Invoke(new DelegadoParaPonerGIF(PonGIF), new object[] { carta });
                            this.Invoke(new DelegadoParaRobar(Robar), new object[] { piladecartes });
                            break;

                        case 18: //Quan un jugador mor
                            //El fem invisible i desactivem les opcions, torna al panel inicial
                            if (textBox1.Text == missatge)
                            {
                                DelegadoParaInvisible delegado23 = new DelegadoParaInvisible(HazInvisible_muerto);
                                this.Invoke(delegado23);
                            }

                            else
                            {   //Posem calavaeras als jugadors que hagin mort
                                this.Invoke(new DelegadoParaMatar(Muerte), new object[] { missatge });
                                jugadores.Remove(missatge);
                            }
                            if (jugadores.Count() == 1)
                            {   //Si només queda un jugador aquest es el guanyador
                                if(textBox1.Text == jugadores[0])
                                {
                                    DelegadoParaInvisible delegado27 = new DelegadoParaInvisible(HazInvisible_ganador);
                                    this.Invoke(delegado27);
                                }
                            }
                            break;

                        

                        case 20: //xat
                            string usuaris = missatgerebut[2];
                            string[] usuaris_xat = usuaris.Split(',');
                            
                            xatRichTextBox.Invoke(new DelegadoPonMensajeRTBox(PonMensajeRTBox), new object[] { xatRichTextBox, missatge, Color.Blue, usuaris_xat });

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
        //Botó de les consultes
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton1.Checked)
                {
                    string missatge = "1/" + textBox1.Text;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                }

                if (radioButton2.Checked)
                {
                    string missatge2 = "2/" + jugtextBox.Text + "/" + textBox1.Text;
                    byte[] msg2 = System.Text.Encoding.ASCII.GetBytes(missatge2);
                    server.Send(msg2);

                }

                if (radioButton3.Checked)
                {
                    string missatge3 = "3/" + data1Box.Text + "/" + data2Box.Text + "/" + textBox1.Text;
                    byte[] msg3 = System.Text.Encoding.ASCII.GetBytes(missatge3);
                    server.Send(msg3);

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

        //Botó per fer la connexió
        private void button1_Click(object sender, EventArgs e)
        {
            emplenarLlistadecartes();
            
            if ((textBox1.Text) == "" || (textBox2.Text) == "")
                MessageBox.Show("Emplena els camps de usuari i contraseña !!");
            else
            {
                IPAddress direc = IPAddress.Parse("192.168.56.106"); //entorn de produccio: 147.83.117.22, maquina virtual: 192.168.56.106
                IPEndPoint ipep = new IPEndPoint(direc, 9070); //Juan: 50066, Gerard: 50067, Maria: 50068, maquina virtual: 9050
                //creem socket
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {

                    server.Connect(ipep);//Intentem connectar el socket

                    
                    string missatge = "4/" + textBox1.Text + "/" + textBox2.Text;
                    // Enviem al servidor el 4 perque faci el LOGIN
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(missatge);
                    server.Send(msg);

                    ThreadStart ts = delegate { AtendreServidor(); };
                    atender = new Thread(ts);
                    atender.Start();

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
            DelegadoParaVisible delegado = new DelegadoParaVisible(HazVisibleRegisterGroupBox);
            this.Invoke(delegado);
        }


        //Botó de desconnexió
        private void button3_Click(object sender, EventArgs e)
        {
            //Mensaje de desconexión
            string mensaje = "0/" + textBox1.Text;

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);


        }

        //Botó per mostrar la contrasenya
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
      
        //Botó per invitar als jugadors
        private void invitarButton_Click(object sender, EventArgs e)
        {
            try
            {
                segundos = 20;
                host = true;
                string listainvitados = "";
                string mensajeinvitados = "";
                for (int i = 0; i < invitados.Count(); i++)
                {

                    if (invitados[i] != textBox1.Text)
                    {
                        listainvitados = listainvitados + invitados[i] + "\n";
                        mensajeinvitados = mensajeinvitados + invitados[i] + "/";
                    }

                }
                MessageBox.Show("Has invitado a: " + listainvitados);
                string mensaje = "10/" + mensajeinvitados;
                // Enviem el 10 al servidor perque afegeixi els jugadors a la partida, despprés si algú rebutja l'eliminará
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
                invitadosBox.Clear();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Error. Aquest jugador no existeix");
            }
        }

        //Funció que posa els conectados a la DatGridView
        private void conectadosGrid_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (conectadosGrid.CurrentCell.Value.ToString() != textBox1.Text)
            {
                invitadosBox.AppendText(conectadosGrid.CurrentCell.Value.ToString() + Environment.NewLine);
                invitados.Add(conectadosGrid.CurrentCell.Value.ToString());
            }
            else
                MessageBox.Show("Error. Invita a alguien que no seas tú mismo");
        }

        //Botó d'acceptar
        private void acceptarRadioButton_Click(object sender, EventArgs e)
        {
            aceptado = true;
            //enviar codigo, mi id y la partida
            string mensaje = "11/" + textBox1.Text + "/" + partidaLbl.Text;
            // Enviamos al servidor 
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

        }

        //Botó d'acceptar
        private void rebutjarRadioButton_Click(object sender, EventArgs e)
        {
            eliminado = true;
            DelegadoParaInvisible delegado5 = new DelegadoParaInvisible(HazInvisibleGroupBox1);
            this.Invoke(delegado5);

            string mensaje = "12/" + textBox1.Text + "/" + partidaLbl.Text;
            // Enviamos al servidor 
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);
        }

        private void enviarMissatgeXatButton_Click(object sender, EventArgs e)
        {
            if (xatTextBox.Text == "")
                MessageBox.Show("Escriu alguna cosa abans d'enviar");
            else
            {
                string mensaje = "20/" + xatTextBox.Text;
                // Enviamos al servidor 
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
            }
            xatTextBox.Clear();
        }

        //Botó de registrar
        private void registerButton_Click(object sender, EventArgs e)
        {
            if ((userRegisterTextBox.Text == "") || (contRegisterTextBox.Text == "") || (repcontRegisterTextBox.Text == ""))
                MessageBox.Show("Error. Falta algún campo por poner");

            else
            {
                if (repcontRegisterTextBox.Text == contRegisterTextBox.Text)
                {
                    string mensaje = "5/" + userRegisterTextBox.Text + "/" + contRegisterTextBox.Text + "/" + repcontRegisterTextBox.Text;
                    // Enviamos al servidor
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);

                    DelegadoParaInvisible delegado = new DelegadoParaInvisible(HazInvisibleRegisterGroupBox);
                    this.Invoke(delegado);
                }
                else
                    MessageBox.Show("Les contrasenyes no coincideixen");
            }
        }

        private void donarsedebaixaLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DelegadoParaVisible delegado = new DelegadoParaVisible(HazVisibleDonarsedeBaixa);
            this.Invoke(delegado);
        }

        private void DBButton_Click(object sender, EventArgs e)
        {
            if ((userDBTextBox.Text == "") || (contDBTextBox.Text == "") || (repContDBTextBox.Text == ""))
                MessageBox.Show("Error. Falta algún campo por poner");

            else
            {
                if (repContDBTextBox.Text == contRegisterTextBox.Text)
                {
                    string mensaje = "9/" + userDBTextBox.Text + "/" + contDBTextBox.Text + "/" + repContDBTextBox.Text;
                    // Enviamos al servidor
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);

                    DelegadoParaInvisible delegado = new DelegadoParaInvisible(HazInvisibleDonarsedeBaixa);
                    this.Invoke(delegado);
                }
                else
                    MessageBox.Show("Les contrasenyes no coincideixen");
            }
            
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.segundos = this.segundos - 1;
            this.Invoke(new DelegadoParaEscribirTexto(SegundosTurno), new object[] { Convert.ToString(segundos) });
            if ((this.segundos == 0))
            {
                this.Invoke(new DelegadoParaTimer(StopTimer), new object[] {2});
                miturno = false;
                num ++;
                if (num == (aceptados+1))
                    num = 0;

                string mensaje = "15/" + Convert.ToString(num);
                // Enviamos al servidor
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);     
            }
        }

        
        //JUGADAS PARTIDA

        //En els picture box agafem el tag de la carta, que enviarem al servidor
        //Només es pot fer durant el teu torn i un cop, per aixo nomes miturno== true and uno==1
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            if ((miturno == true) && (uno == 1))
            {
                pictureBoxGIF.Visible = false;
                nombre = textBox1.Text;
                carta = pictureBox1.Tag.ToString();
                uno = 0;
                if ((carta == "8") || (carta == "9") || (carta == "10") || (carta == "11") || (carta == "12") || (carta == "13") || (carta == "14") || (carta == "15") || (carta == "16") || (carta == "17"))
                {
                    pictureBox1.Image = null;
                    pictureBox1.BackColor = Color.Transparent;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    U2Atacar.Visible = true;
                    U3Atacar.Visible = true;
                    U4Atacar.Visible = true;
                }
                else if ((carta == "1") || (carta == "2") || (carta == "3") || (carta == "4") || (carta == "5") || (carta == "6") || (carta == "7"))
                {
                    pictureBox1.Image = null;
                    pictureBox1.BackColor = Color.Transparent;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
                    //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    //Movimiento de Carta Defensa (aumentarte vida)
                    string mensaje = "16/" + nombre + "/" + carta;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            if ((miturno == true) && (uno == 1))
            {
                pictureBoxGIF.Visible = false;
                nombre = textBox1.Text;
                carta = pictureBox2.Tag.ToString();
                uno = 0;
                if ((carta == "8") || (carta == "9") || (carta == "10") || (carta == "11") || (carta == "12") || (carta == "13") || (carta == "14") || (carta == "15") || (carta == "16") || (carta == "17"))
                {
                    pictureBox2.Image = null;
                    pictureBox2.BackColor = Color.Transparent;
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    U2Atacar.Visible = true;
                    U3Atacar.Visible = true;
                    U4Atacar.Visible = true;
                }
                else if ((carta == "1") || (carta == "2") || (carta == "3") || (carta == "4") || (carta == "5") || (carta == "6") || (carta == "7"))
                {
                    pictureBox2.Image = null;
                    pictureBox2.BackColor = Color.Transparent;
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
                    //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    //Movimiento de Carta Defensa (aumentarte vida)
                    string mensaje = "16/" + nombre + "/" + carta;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }
            }

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;          
            if ((miturno == true) && (uno == 1))
            {
                pictureBoxGIF.Visible = false;
                nombre = textBox1.Text;
                carta = pictureBox3.Tag.ToString();
                uno = 0;
                if ((carta == "8") || (carta == "9") || (carta == "10") || (carta == "11") || (carta == "12") || (carta == "13") || (carta == "14") || (carta == "15") || (carta == "16") || (carta == "17"))
                {
                    pictureBox3.Image = null;
                    pictureBox3.BackColor = Color.Transparent;
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    U2Atacar.Visible = true;
                    U3Atacar.Visible = true;
                    U4Atacar.Visible = true;
                }
                else if ((carta == "1") || (carta == "2") || (carta == "3") || (carta == "4") || (carta == "5") || (carta == "6") || (carta == "7"))
                {
                    pictureBox3.Image = null;
                    pictureBox3.BackColor = Color.Transparent;
                    pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                    //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
                    //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    //Movimiento de Carta Defensa (aumentarte vida)
                    string mensaje = "16/" + nombre + "/" + carta;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            if ((miturno == true) && (uno == 1))
            {
                pictureBoxGIF.Visible = false;
                nombre = textBox1.Text;
                carta = pictureBox4.Tag.ToString();
                uno = 0;
                if ((carta == "8") || (carta == "9") || (carta == "10") || (carta == "11") || (carta == "12") || (carta == "13") || (carta == "14") || (carta == "15") || (carta == "16") || (carta == "17"))
                {
                    pictureBox4.Image = null;
                    pictureBox4.BackColor = Color.Transparent;
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    U2Atacar.Visible = true;
                    U3Atacar.Visible = true;
                    U4Atacar.Visible = true;
                }
                else if ((carta == "1") || (carta == "2") || (carta == "3") || (carta == "4") || (carta == "5") || (carta == "6") || (carta == "7")) //Cartes defensa
                {

                    pictureBox4.Image = null;
                    pictureBox4.BackColor = Color.Transparent;
                    pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
                    //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;
                    
                    //Movimiento de Carta Defensa (aumentarte vida)
                    string mensaje = "16/" + nombre + "/" + carta;
                    // Enviamos al servidor 
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }
            }
        }

        private void CambiarCarta()
        {
            bool cambiada = false;
            if (pictureBox1.Tag.ToString() == carta && (cambiada == false))
            {
                cambiada = true;
                pictureBox1.Image = null;
                pictureBox1.BackColor = Color.Transparent;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            if (pictureBox2.Tag.ToString() == carta && (cambiada == false))
            {
                cambiada = true;
                pictureBox2.Image = null;
                pictureBox2.BackColor = Color.Transparent;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            if (pictureBox3.Tag.ToString() == carta && (cambiada == false))
            {
                cambiada = true;
                pictureBox3.Image = null;
                pictureBox3.BackColor = Color.Transparent;
                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            if (pictureBox4.Tag.ToString() == carta && (cambiada == false))
            {
                cambiada = true;
                pictureBox4.Image = null;
                pictureBox4.BackColor = Color.Transparent;
                pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }


        private void U2Atacar_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            adversario = U2nom.Text;

            //enviamos mensaje de Jugada Ataque
            string mensaje = "17/" + nombre + "/" + carta + "/" + adversario;
            // Enviamos al servidor 
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);
            CambiarCarta();
            
            //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
            //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;

            U2Atacar.Visible = false;
            U3Atacar.Visible = false;
            U4Atacar.Visible = false;
        }

        private void U3Atacar_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            adversario = U3nom.Text;

            //enviamos mensaje de Jugada Ataque
            string mensaje = "17/" + nombre + "/" + carta + "/" + adversario;
            // Enviamos al servidor 
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            CambiarCarta();
            
            //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
            //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;

            U2Atacar.Visible = false;
            U3Atacar.Visible = false;
            U4Atacar.Visible = false;
        }

        private void U4Atacar_Click(object sender, EventArgs e)
        {
            pictureBoxGIF.Visible = false;
            adversario = U4nom.Text;

            //enviamos mensaje de Jugada Ataque
            string mensaje = "17/" + nombre + "/" + carta + "/" + adversario;
            // Enviamos al servidor 
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            CambiarCarta();
            
            //pictureBoxJugadas.Image = Image.FromFile(carta + ".jpg");
            //pictureBoxJugadas.SizeMode = PictureBoxSizeMode.StretchImage;

            U2Atacar.Visible = false;
            U3Atacar.Visible = false;
            U4Atacar.Visible = false;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            this.segundos4 = this.segundos4 - 1;
            label18.Text = "HAS MUERTO";

            PonMuertoGIF();

            if (this.segundos4 == 0)
            {
                label18.Visible = false;
                timer3.Stop();
                pictureBox20.Image = null;
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            this.segundos2 = this.segundos2 - 1;
            PonGIF(carta);
            if (this.segundos2 == 0)
            {
                timer4.Stop();
                pictureBoxGIF.Image = null;
            }
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            this.segundos3 = this.segundos3 - 1;
            label18.Text = "HAS GANADO";

            PonGanador();

            if (this.segundos3 == 0)
            {
                label18.Visible = false;
                //resetear para poder hacer una nueva partida
                invitados.Clear();
                jugadores.Clear();
                U1nom.Text = "";
                U2nom.Text = "";
                U3nom.Text = "";
                U4nom.Text = "";
                ValorvidaU1.Text = "50";
                valorvidaU2.Text = "50";
                valorvidaU3.Text = "50";
                valorvidaU4.Text = "50";
                pictureBox17.Image = null;
                pictureBox18.Image = null;
                pictureBox19.Image = null;
                
                timer5.Stop();
                pictureBox20.Image = null;
            }
        }
    }

    //Extensio del richtextbox per afegir colors
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
