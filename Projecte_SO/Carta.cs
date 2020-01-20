using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projecte_SO
{
    public class Carta
    {
        //Atributs
        int ID; //numero associat a cada carta
        int vida; //vida que afegeix
        int atac; //si és carta d'atac, quant treu
        string jugador; //quin jugador té la carta
        string imatge; //nom de l'arxiu per posar la imatge  e.g."carta1.jpg"

        public void SetID(int id)
        {
            this.ID = id;
        }

        public int GetID()
        {
            return this.ID;
        }

        public void SetVida(int Vida)
        {
            this.vida = Vida;
        }

        public int GetVida()
        {
            return this.vida;
        }

        public void SetAtac(int atac)
        {
            this.atac = atac;
        }

        public int GetAtac()
        {
            return this.atac;
        }

        public void SetJugador(string jugador)
        {
            this.jugador = jugador;
        }

        public string GetJugador()
        {
            return this.jugador;
        }

        public void SetImatge(string Imatge)
        {
            this.imatge = Imatge;
        }

        public string GetImatge()
        {
            return this.imatge;
        }
    }
}
