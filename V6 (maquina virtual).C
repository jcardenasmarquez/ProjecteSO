#include <pthread.h>
#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <mysql/mysql.h>


//Estructura per fer accés excloent
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;

typedef struct{
	char nombre[20];
	int socket;
}Conectado;

typedef struct{
	Conectado conectados[100];
	int num;
}ListaConectados;

typedef struct{
	int partida;
	char usuario1[20];
	int puntuacion1;
	int nivel1;
	int vida1;
	int socket1;
	char usuario2[20];
	int puntuacion2;
	int nivel2;
	int vida2;
	int socket2;
	char usuario3[20];
	int puntuacion3;
	int nivel3;
	int vida3;
	int socket3;
	char usuario4[20];
	int puntuacion4;
	int nivel4;
	int vida4;
	int socket4;
}Partida;

Partida tablapartidas[100];

ListaConectados lista; //Declarada aqui para poder ser usada en el main y en todas las funciones

int i;
int sockets[100];
int aceptados = 0;


//genera nums aleatoris
void numerosAleatorios(int min, int max, int cant, char vectornum[512]) 
{ 
	int i;
	char vector[512];
	for (i = 0; i < cant; i++) { 
		int num = (rand() % (max - min + 1)) + min; 
		sprintf(vector,"%s%d,",vector, num);
		//printf(vectornum);
		
	}
	//char *p = strtok(vector, ",");
	sprintf(vectornum, "%s", vector);
}
//funcions consultes
void consulta_1(MYSQL *conn, char buff2[512], char jugador[20])
{
	// Estructura especial para almacenar resultados de consultas 
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char consulta[512];
	char resbuff2[512];
	
	//Consideramos las muertes totales los asesinatos + las muertes del personaje	
	strcpy(consulta, "SELECT DISTINCT jugador.id FROM jugador, partida, relacionJugPar WHERE partida.id IN (SELECT partida.id FROM jugador, partida, relacionJugPar WHERE jugador.id = '");
	strcat(consulta, jugador); 	
	strcat(consulta, "' AND jugador.id = relacionJugPar.idjug AND relacionJugPar.idpar = partida.id) AND partida.id = relacionJugPar.idpar and relacionJugPar.idjug = jugador.id ");
	
	int err = mysql_query(conn, consulta);
	if (err!=0) {
		sprintf (resbuff2,"Error al consultar datos de la base %u %s\n",
				 mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	resultado = mysql_store_result (conn);
	
	row = mysql_fetch_row (resultado);
	
	if (row == NULL)
		sprintf (resbuff2,"No se han obtenido datos en la consulta\n");
	else
	{
		while(row!=NULL)
		{
			sprintf(resbuff2, "%s%s,", resbuff2, row[0]);
			row = mysql_fetch_row(resultado);
		}
	}
	
	strcpy(buff2, resbuff2);
}		


void consulta_2(MYSQL *conn, char buff2[512], char jugador[20], char jugador2[20])
{
	// Estructura especial para almacenar resultados de consultas 
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char resbuff2[512];
	
	
	char consulta[500];
	
	
	sprintf(consulta, "SELECT partida.ganador FROM jugador, partida, relacionJugPar WHERE partida.id IN (SELECT partida.id FROM jugador, partida, relacionJugPar WHERE jugador.id = '%s' AND jugador.id = relacionJugPar.idjug AND relacionJugPar.idpar = partida.id) AND partida.id = relacionJugPar.idpar AND relacionJugPar.id = 's'", jugador, jugador2);
	
	int err = mysql_query(conn, consulta);
	if (err!=0) {
		sprintf (resbuff2,"Error al consultar datos de la base %u %s\n",
				 mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	resultado = mysql_store_result (conn);
	
	row = mysql_fetch_row (resultado);
	
	if (row == NULL)
		sprintf (resbuff2,"No se han obtenido datos en la consulta\n");
	else
		sprintf(resbuff2, row[0]);
	
	strcpy(buff2, resbuff2);
}	


void consulta_3(MYSQL *conn, char buff2[512], char jugador[20], char fecha[20], char fecha2[20])
{
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char resbuff2[512];

	int cont = 0;
	char consulta[500];
	
	
	//select relacionJugPar.personajes from partida, relacionJugPar where relacionJugPar.idpar = partida.id and partida.fecha = '12/02/2019';
	
	
	//primer necessito una taula on surti el nom de tots els personatges que es van jugar en la data demanada
	sprintf(consulta, "SELECT partida.id FROM jugador, partida, relacionJugPar WHERE jugador.id = '%s' AND jugador.id = relacionJugPar.idjug AND partida.id = relacionJugPar.idpar AND partida.fecha >= '%s' AND partida.fecha <= 's'", jugador, fecha, fecha2);
	
	int err = mysql_query(conn, consulta);
	if (err!=0) {
		sprintf (resbuff2,"Error al consultar datos de la base %u %s\n",
				 mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	resultado = mysql_store_result (conn);
	
	row = mysql_fetch_row (resultado);
	
	if (row == NULL)
		sprintf (resbuff2,"No se han obtenido datos en la consulta\n");
	else{
		while (row !=NULL) {
			sprintf(resbuff2, "%s%s,", resbuff2, row[0]);
			row = mysql_fetch_row (resultado);
		}
		
	}
	
	strcpy(buff2, resbuff2);
	
}

//Funcions per la bbdd

void ConnMYSQL (MYSQL *conn)
{
	//Creamos una conexion al servidor MYSQL 
	conn = mysql_init(NULL);
	if (conn==NULL) {
		printf ("Error al crear la conexiï¿ƒï¾³n: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	//inicializar la conexion
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "bbdd1", 0, NULL, 0);
	if (conn==NULL) {
		printf ("Error al inicializar la conexiï¿ƒï¾³n: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
}

void CercaJugador(MYSQL *conn,  char usuario[512], char contrasena [512], char buff2[512], int sock_conn, ListaConectados *milista) 
{
	// Consulta que contrasenya i usuari coincideixin amb el que hi ha a la BBDD 
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	int err;
	
/*	Primer hem de comprovar que aquest compte no està sent usat*/
	int k;
	int found = 0;
	for (k = 0; k < (milista->num) ; k++)
	{
		if ((strcmp((milista->conectados[k].nombre) , usuario)) == 0)
			found = 1;
		else
			k++;
	}
	
	if (found == 0) // si no està sent usat
	{
		char consulta[512];
		sprintf(consulta, "SELECT jugador.id FROM jugador WHERE jugador.id = '%s'", usuario);
		//Si l'usuari es troba ala BBDD (està registrat)
		err = mysql_query (conn, consulta); 
		if (err!=0) {
			printf ("Error al consultar datos de la base %u %s\n",
					mysql_errno(conn), mysql_error(conn));
			exit (1);
		}
		else
		{
			resultado = mysql_store_result (conn);
			row = mysql_fetch_row (resultado);
			
			if(row != NULL) // o sigui el jugador existeix
			{
				char query2[512];
				sprintf(query2, "SELECT jugador.contraseña FROM jugador WHERE jugador.id = '%s'", usuario);
				
				err = mysql_query (conn, query2); 
				if (err!=0) {
					printf ("Error al consultar datos de la base %u %s\n",
							mysql_errno(conn), mysql_error(conn));
					exit (1);
				}
				else
				{
					resultado = mysql_store_result (conn);
					row = mysql_fetch_row (resultado);
					
					if(strcmp(row[0],contrasena) == 0) //les contrasenyes son iguals...
					{
						if((milista->num) < 99)
							strcpy(buff2,"Login correcte");
						else
							strcpy(buff2,"Servidor ple");
					}
					else
					   strcpy(buff2,"Contrasenya incorrecta");
				}
			}
			else
				strcpy(buff2,"L'usuari no esta registrat");
		}
	}
	else
		strcpy(buff2,"L'usuari ja està connectat");
}

int AnadirCuenta (MYSQL *conn, char usuario[20], char contrasena[20])
{
	//DEVUELVE 0 SI OK, -1 SI NO SE HA PODIDO CREAR
	
	//Primer comprovar que l'usuari no està registrat
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char consulta[512];
	int err;
	
	sprintf(consulta, "SELECT jugador.id FROM jugador WHERE jugador.id = '%s'", usuario);
	//Si l'usuari es troba ala BBDD (està registrat)
	err = mysql_query (conn, consulta); 
	if (err!=0) {
		printf ("Error al consultar datos de la base %u %s\n",
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	else
	{
		resultado = mysql_store_result (conn);
		row = mysql_fetch_row (resultado);
		
		if(row == NULL) //No existeix
		{
			sprintf(consulta, "INSERT INTO jugador(id, contraseña, vida, nivel) VALUES ('%s','%s','%d','%d');", usuario, contrasena, 100, 0);
			err = mysql_query (conn, consulta);
			
			if(err!=0)
				return -1; //HA HABIDO ERROR AL ANADIR LA CUENTA
			else 
				return 0;
		}
		else
		   return -2;
	}
}

int EliminaCuenta (MYSQL *conn, char usuario[20], char contrasena[20])
{
	int err;
	
	char consulta[512];
	sprintf(consulta, "DELETE FROM jugador WHERE jugador.id = '%s' and jugador.contraseña = '%s'", usuario,contrasena);
	
	err=mysql_query (conn, consulta);
	if(err!=0)
		return -1; //HA HABIDO ERROR AL ELIMINAR LA CUENTA
	else 
		return 0;
}


void EliminaJugadorTabla(Partida tablapartidas[100], char nombre[20], int partida)
{
	
	if(strcmp(tablapartidas[partida].usuario1,nombre) == 0)
	{
		strcpy(tablapartidas[partida].usuario1, "-1");
		tablapartidas[partida].socket1 = -1;
	}
	else if(strcmp(tablapartidas[partida].usuario2,nombre) == 0)
	{
		strcpy(tablapartidas[partida].usuario2, "-1");
		tablapartidas[partida].socket2 = -1;
	}
	else if(strcmp(tablapartidas[partida].usuario3,nombre) == 0)
	{
		strcpy(tablapartidas[partida].usuario3, "-1");
		tablapartidas[partida].socket3 = -1;
	}
	else if(strcmp(tablapartidas[partida].usuario4,nombre) == 0)
	{
		strcpy(tablapartidas[partida].usuario4, "-1");
		tablapartidas[partida].socket4 = -1;
	}

}


//FUNCIONS PER LA LLISTA
int Pon(ListaConectados *lista, char nombre[20], int socket)
{
	if(lista->num == 100)
		return 0;
	else{
		lista->conectados[lista->num].socket = socket;
		strcpy(lista->conectados[lista->num].nombre, nombre);
		lista->num = lista->num +1;
		return 1;
	}
}


int DameSocket(ListaConectados *lista, char nombre[20])
{
	int encontrado = 0;
	int i =0;
	while ((i<lista->num) && (encontrado==0))
	{
		if (strcmp(lista->conectados[i].nombre, nombre)==0)
			encontrado =1;
		else
			i++;
	}
	if(encontrado==1)
		return lista->conectados[i].socket;
	else
		return -1;
}

void DameUsuario(ListaConectados *lista, int socket, char nombre[20])
{
	int encontrado = 0;
	int i =0;
	while ((i<lista->num) && (encontrado==0))
	{
		if (lista->conectados[i].socket == socket)
			encontrado =1;
		else
			i++;
	}
	if(encontrado==1)
		strcpy(nombre, lista->conectados[i].nombre);
}

int DamePos(ListaConectados *lista, char nombre[20])
{
	int encontrado = 0;
	int i = 0;
	while ((i<lista->num) && encontrado == 0)
	{
		if (strcmp(lista->conectados[i].nombre, nombre) == 0)
			encontrado=1;
		else 
			i =i+1;
	}
	if (encontrado ==1)
		return i;
	else 
		return -1;	
}

int EliminaConectado(ListaConectados *lista, char nombre[20])
{
	int pos = DamePos(lista, nombre);
	if (pos == -1)
		return -1;
	else
	{
		for (int i = pos; i < lista->num-1; i++)
		{
			lista->conectados[i]= lista->conectados[i+1];
		}
		lista->num --;
		return 0;
	}
}

void DameConectados(ListaConectados *lista, char conectados[100]) 
{
	//esta funcion nos dara todos los conectados de la siguiente manera: num,nombre, socket, nombre, socket.....
	//donde num es el total de personas conectadas 
	int i;
	sprintf(conectados,"%d", lista->num);
	
	for (i =0; i<lista->num; i++)
	{
		sprintf(conectados,"%s/%s/%d", conectados, lista->conectados[i].nombre, lista->conectados[i].socket);
	}
}

int AnadirPartida(Partida tablapartidas[100], char usuario1[20], int socket1, char usuario2[20], int socket2, char usuario3[20], int socket3, char usuario4[20], int socket4)
{
	int i=0;
	int encontrado = 0;
	while (i <100 && encontrado == 0) //bucle de busqueda de socket1 = -1
	{
		if(tablapartidas[i].socket1 == -1)
			encontrado = 1;
		else
			i++;
	}
	if (encontrado == 1)
	{
		strcpy(tablapartidas[i].usuario1, usuario1);
		strcpy(tablapartidas[i].usuario2, usuario2);
		strcpy(tablapartidas[i].usuario3, usuario3);
		strcpy(tablapartidas[i].usuario4, usuario4);
		tablapartidas[i].vida1 = 50;
		tablapartidas[i].vida2 = 50;
		tablapartidas[i].vida3 = 50;
		tablapartidas[i].vida4 = 50;
		tablapartidas[i].socket1 = socket1;
		tablapartidas[i].socket2 = socket2;
		tablapartidas[i].socket3 = socket3;
		tablapartidas[i].socket4 = socket4;
		return i;
	}
	else
		return -1;
}

void LongitudNombre(ListaConectados *lista, int longitud, char resultado[20])
{
	int i = 0;
	int encontrado = 0;
	while(i < lista->num && encontrado ==0)
	{
		if(strlen(lista->conectados[i].nombre) == longitud)
			encontrado =1;
		else
			i++;
	}
	if(encontrado ==1)
	{
		sprintf(resultado, "%s", lista->conectados[i].nombre);
	}
	else
		strcpy(resultado,"NO");
}


int Jugada_Ataque(MYSQL *conn, Partida tablapartidas[100], char atacado[20], char carta[20], int partida)
{
	
	int restavida;
	int nuevavida;
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char consulta[512];
	int err;
	
	sprintf(consulta, "SELECT cartas.valor FROM cartas WHERE cartas.idcarta = '%s'", carta);
	//Si l'usuari es troba ala BBDD (estï¿  registrat)
	err = mysql_query (conn, consulta); 
	if (err!=0) {
		printf ("Error al consultar datos de la base %u %s\n",
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	else
	{
		resultado = mysql_store_result (conn);
		row = mysql_fetch_row (resultado);
		int row1 = atoi(row[0]);
		restavida = row1;
	}
	
	if (strcmp(tablapartidas[partida].usuario1,atacado)==0)
	{
		nuevavida = tablapartidas[partida].vida1 - restavida;
		tablapartidas[partida].vida1 = nuevavida;
		return tablapartidas[partida].vida1;
	}
	if (strcmp(tablapartidas[partida].usuario2,atacado)==0)
	{
		nuevavida = tablapartidas[partida].vida2 - restavida;
		tablapartidas[partida].vida2 = nuevavida;
		return tablapartidas[partida].vida2;
	}
	if (strcmp(tablapartidas[partida].usuario3,atacado)==0)
	{
		nuevavida = tablapartidas[partida].vida3 - restavida;
		tablapartidas[partida].vida3 = nuevavida;
		return tablapartidas[partida].vida3;
	}
	if (strcmp(tablapartidas[partida].usuario4,atacado)==0)
	{
		nuevavida = tablapartidas[partida].vida4 - restavida;
		tablapartidas[partida].vida4 = nuevavida;
		return tablapartidas[partida].vida4;
	}
}

int Jugada_Defensa(MYSQL *conn, Partida tablapartidas[100], char defensado[20], char carta[20], int partida)
{
	int sumavida;
	int nuevavida;
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	char consulta[512];
	int err;
	
	sprintf(consulta, "SELECT cartas.valor FROM cartas WHERE cartas.idcarta = '%s'", carta);
	//Si l'usuari es troba ala BBDD (estï¿  registrat)
	err = mysql_query (conn, consulta); 
	if (err!=0) {
		printf ("Error al consultar datos de la base %u %s\n",
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	else
	{
		resultado = mysql_store_result (conn);
		row = mysql_fetch_row (resultado);			
		int row1 = atoi(row[0]); //afegir error si a row0 no hi ha res
		sumavida = row1;
	}
	
	if (strcmp(tablapartidas[partida].usuario1,defensado)==0)
	{
		
		nuevavida = tablapartidas[partida].vida1 + sumavida;
		tablapartidas[partida].vida1 = nuevavida;
		return tablapartidas[partida].vida1;
	}
	if (strcmp(tablapartidas[partida].usuario2,defensado)==0)
	{
		nuevavida = tablapartidas[partida].vida2 + sumavida;
		tablapartidas[partida].vida2 = nuevavida;
		return tablapartidas[partida].vida2;
	}
	if (strcmp(tablapartidas[partida].usuario3,defensado)==0)
	{
		nuevavida = tablapartidas[partida].vida3 + sumavida;
		tablapartidas[partida].vida3 = nuevavida;
		return tablapartidas[partida].vida3;
	}
	if (strcmp(tablapartidas[partida].usuario4,defensado)==0)
	{
		nuevavida = tablapartidas[partida].vida4 + sumavida;
		tablapartidas[partida].vida4 = nuevavida;
		return tablapartidas[partida].vida4;
	}
}


void *AtenderCliente(void *socket) //recibimos por referencia y no sabemos que es un int
{
	char personaje[20];
	char fecha[20];
	char jugador[20];
	int partida;
	char usuario[20];
	char contrasena[20];
	char consulta[512];

	
	
	//Connexio basedades (hacerlo en una fucion aparte)!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	MYSQL *conn;
	int err;
	
	//Creamos una conexion al servidor MYSQL 
	conn = mysql_init(NULL);
	
	if (conn==NULL) {
		printf ("Error al crear la conexiÃ¯Â¿Æ’Ã¯Â¾Â³n: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	//inicializar la conexion
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "bbdd1", 0, NULL, 0);
	if (conn==NULL) {
		printf ("Error al inicializar la conexiÃ¯Â¿Æ’Ã¯Â¾Â³n: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	
	
	
	int sock_conn = *(int*) socket; //estiramos del puntero para que nos traiga el numero
	
	char buff[512]; //peticion
	char buff2[512]; //respuesta
	int ret;
	
	//Bucle de atenciÃ³n al cliente
	int terminar = 0;
	while (terminar ==0)
	{
		ret=read(sock_conn,buff, sizeof(buff));
		printf ("Recibido\n");
		
		
		// Tenemos que a?adirle la marca de fin de string 
		// para que no escriba lo que hay despues en el buffer
		buff[ret]='\0';
		
		printf ("Se ha conectado: %s\n",buff);
		
		char *p = strtok(buff, "/");
		int codigo =  atoi (p);
		//int numForm;
		
		char msgtocl[512];
		
		//printf("%d", codigo);
		
		if ((codigo == 0) || (codigo == 6))
		{
			char notificacion[512];
			
			if (codigo==0) //Desconnexió
			{
				terminar = 1;
				p = strtok(NULL, "/");
				strcpy(usuario, p);
				pthread_mutex_lock(&mutex);
				int posicion = DamePos(&lista, usuario);
				int okEliminado = EliminaConectado(&lista, usuario);
				pthread_mutex_unlock(&mutex);
				if (okEliminado == 0)
					sprintf(msgtocl, "0/%s", usuario);
				else
					strcpy(msgtocl, "0/error");
				
				write(sock_conn, msgtocl, strlen(msgtocl));
			}
			
			else if (codigo ==6) //AÃ±adir a lista de conectados
			{
				p = strtok(NULL, "/");
				strcpy(usuario, p); //usuario = Conectado
				printf("El num de conectados es: %d \n",lista.num);  
				if(lista.num == 0) //en caso de que no haya ningun conectado ponemos los socket = -1
				{
					for(int j = 0; j <100; j++)
					{
						tablapartidas[j].socket1 = -1;
					}
				}
				pthread_mutex_lock(&mutex);
				int ponConectado = Pon(&lista, usuario, sock_conn);
				pthread_mutex_unlock(&mutex);
				printf("%s,%d", usuario, ponConectado);
			}
			
			char vectorCon[100];
			DameConectados(&lista, vectorCon);
			sprintf(notificacion, "6/%s", vectorCon);
			int j;
			for(j = 0; j < lista.num ; j++)
				write(lista.conectados[j].socket,notificacion,strlen(notificacion));
		}
		
		else if (codigo==3)	
		{
			char fecha1[20];
			char fecha2[20];
			char jugador[20];
			p = strtok(buff, "/");
			
			strcpy (fecha1, p);
			p = strtok( NULL, "/");
			
			strcpy (fecha2, p);
			p = strtok(NULL, "/");
			strcpy(jugador, p);
			
			consulta_3(conn, buff2, jugador, fecha1, fecha2);
			sprintf(msgtocl, "3/%s", buff2);
		}
		
		
		else if (codigo==2)
		{
			p = strtok( NULL, "/");
			char contrincant[20];
			strcpy (contrincant, p);
			char usuari[20];
			p = strtok(NULL, "/");
			strcpy(usuari, p);
			
			consulta_2(conn, buff2, contrincant, usuari);
			sprintf(msgtocl, "2/%s", buff2);
		}
		
		
		else if (codigo==1) //durada partida amb mes morts		
		{
			p = strtok(NULL, "/");
			char usuari[20];
			strcpy(usuari, p);
			consulta_1(conn, buff2, usuari);
			sprintf(msgtocl, "1/%s", buff2);
		}
		
		else if (codigo==4) //Comprovar validesa usuari i contrasenya- conectar
		{
			p = strtok( NULL, "/");
			
			strcpy (usuario, p);
			p = strtok( NULL, "/");
			strcpy (contrasena, p);
			
			
			CercaJugador(conn, usuario, contrasena, buff2, sock_conn, &lista);
			sprintf(msgtocl, "4/%s", buff2);			
		}
		
		else if (codigo ==5) //registrar usuari
		{
			//p = strtok( NULL, "/");
			//numForm = atoi(p);
			p = strtok( NULL, "/");
			strcpy (usuario, p);
			p = strtok( NULL, "/");
			strcpy (contrasena, p);
			pthread_mutex_lock(&mutex);
			int okr = AnadirCuenta(conn, usuario, contrasena);
			pthread_mutex_unlock(&mutex);
			sprintf(msgtocl, "5/%d",okr);
			printf(msgtocl);
			
		}
		
		else if (codigo==8)
		{
			int longitud;
			char resultado[20];
			p = strtok(NULL, "/");
			longitud = atoi(p);
			int i = 0;
			int encontrado = 0;
			pthread_mutex_lock(&mutex);
			LongitudNombre(&lista, longitud, resultado);
			pthread_mutex_unlock(&mutex);
			if(resultado != "NO")
				sprintf(msgtocl, "8/%s", resultado);
			else
				strcpy(msgtocl, "8/No hay nadie que tenga esa longitud de caracteres como nombre");
		}
		
		//FUNCION DEL CONTROL DE GRUPO 1 EN ORDENADOR (GERARD)
/*		else if (codigo==8)*/
/*		{*/
/*			p = strtok( NULL, "/");*/
/*			int num = atoi(p);*/
/*			char num3[10];*/
/*			sprintf(num3, "&d", num);*/
/*			printf (num3);*/
/*			char vectorCon[100];*/
/*			DameConectados(&lista, vectorCon);*/
/*			int num2 = lista.num;*/
/*			if (num == lista.num)*/
/*				strcpy(msgtocl, "8/Si");*/
/*			else*/
/*				strcpy(msgtocl,"8/No");*/
/*		}*/
		
		else if (codigo==9)
		{
			char usuario[20];
			char contrasena[20];
			p = strtok(NULL, "/");
			strcpy(usuario, p);
			p = strtok(NULL, "/");
			strcpy(contrasena, p);
			int CuentaEliminada;
			pthread_mutex_lock(&mutex);
			CuentaEliminada = EliminaCuenta(conn, usuario, contrasena);
			pthread_mutex_unlock(&mutex);
			
			if (CuentaEliminada == 0)
				strcpy(msgtocl, "9/Eliminado correctamente");
			else
				strcpy(msgtocl, "9/Error al eliminar cuenta");
		}
		
		else if (codigo==10)
		{
			char respuesta[512];
			char j1[20];
			char j2[20];
			char j3[20];
			char j4[20];
			int socketsInv[3];
			p = strtok(NULL, "/");
			strcpy(j2,p);
			socketsInv[0] = DameSocket(&lista,j2);
			p = strtok(NULL, "/");
			if (p != NULL)
			{
				strcpy(j3,p);
				socketsInv[1] = DameSocket(&lista,j3);
			}
			else
			{
				strcpy(j3,"-1");
				socketsInv[1] = -1;
			}
			if (j3 != "-1")
			{
				p = strtok(NULL, "/");
				if (p != NULL)
				{
					strcpy(j4,p);
					socketsInv[2] = DameSocket(&lista, j4);
				}
				else
				{
					strcpy(j4,"-1");
					socketsInv[2] = -1;
				}
			}
			DameUsuario(&lista,sock_conn,j1);
			printf("%s,%d,%s,%d,%s,%d,%s,%d \n", j1,sock_conn, j2,socketsInv[0], j3,socketsInv[1], j4,socketsInv[2]);
			pthread_mutex_lock(&mutex);
			int partida = AnadirPartida(tablapartidas,j1,sock_conn,j2,socketsInv[0],j3,socketsInv[1],j4,socketsInv[2]);
			pthread_mutex_unlock(&mutex);
			printf("la partida es la: %d \n", partida);
			int g =0;
			if (partida != -1)
			{
				sprintf(respuesta, "10/%s,%d",j1,partida);
				printf("el mensaje es: %s \n", respuesta);
				//enviamos los mensajes a los clientes invitados
				//hacer el bucle para enviar a todos los invitados
				while(g <= 2)
				{
					if (socketsInv[g] != -1)
					{
						write(socketsInv[g],respuesta,strlen(respuesta));
					}
					g++;
				}
			}
		}
		else if(codigo==11)
		{
			aceptados = aceptados + 1;
			p = strtok(NULL, "/");
			char jugadorAccepta[20];
			strcpy(jugadorAccepta,p);
			p= strtok(NULL, "/");
			int partida;
			partida = atoi(p);
			sprintf(msgtocl, "11/%d,%s", partida, jugadorAccepta);
			write(tablapartidas[partida].socket1, msgtocl, strlen(msgtocl)); //mirem el socket1 que es el que correspon al jugador que invita
			//Pero hem d'enviar a la resta també perquè els hi surti en pantalla
		}
		
		else if(codigo==12)
		{
			p = strtok(NULL, "/");
			char jugadorNOAccepta[20];
			strcpy(jugadorNOAccepta,p);
			p= strtok(NULL, "/");
			int partida;
			partida = atoi(p);
			pthread_mutex_lock(&mutex);
			EliminaJugadorTabla(tablapartidas,jugadorNOAccepta,partida);
			pthread_mutex_unlock(&mutex);
			
			sprintf(msgtocl, "12/%d,%s", partida, jugadorNOAccepta);
			write(tablapartidas[partida].socket1, msgtocl, strlen(msgtocl));
		}
		
		else if(codigo==13)
		{
			char msg[20];
			sprintf(msg, "13/%d", aceptados);
			write(tablapartidas[partida].socket1, msg, strlen(msg));
			write(tablapartidas[partida].socket2, msg, strlen(msg));
			write(tablapartidas[partida].socket3, msg, strlen(msg));
			write(tablapartidas[partida].socket4, msg, strlen(msg));
		}
		
		else if(codigo==14)
		{
			char notificacion2[512];
			char cartas_barajadas[512];
			numerosAleatorios(1,17,40,cartas_barajadas);
			printf("%s\n", cartas_barajadas);
			sprintf(notificacion2, "14/%s/%s/%s/%s/%s/", cartas_barajadas, tablapartidas[partida].usuario1,tablapartidas[partida].usuario2,tablapartidas[partida].usuario3,tablapartidas[partida].usuario4);
			write(tablapartidas[partida].socket1, notificacion2, strlen(notificacion2));
			write(tablapartidas[partida].socket2, notificacion2, strlen(notificacion2));
			write(tablapartidas[partida].socket3, notificacion2, strlen(notificacion2));
			write(tablapartidas[partida].socket4, notificacion2, strlen(notificacion2));
			
		}
		
		else if (codigo==15)
		{
			p = strtok(NULL, "/");
			char turno[20];
			strcpy(turno, p);
			char notificacion[512];
			sprintf(notificacion, "15/%s", turno);
			write(tablapartidas[partida].socket1, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket2, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket3, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket4, notificacion, strlen(notificacion));
			
		}
		
		else if (codigo==16) //jugada de defensa
		{
			p = strtok(NULL, "/");
			char defendido[20];
			strcpy(defendido, p);
			p = strtok(NULL, "/");
			char carta[20];
			strcpy(carta,p);
			int nuevavida;
			nuevavida = Jugada_Defensa(conn,tablapartidas,defendido,carta,partida);
			char notificacion[512];
			sprintf(notificacion, "16/%s,%d,%s,", defendido,nuevavida,carta);
			write(tablapartidas[partida].socket1, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket2, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket3, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket4, notificacion, strlen(notificacion));
			
			
		}
		
		else if (codigo==17)
		{
			p = strtok(NULL, "/");
			char atacante[20];
			strcpy(atacante, p);
			p = strtok(NULL, "/");
			char carta[20];
			strcpy(carta,p);
			p = strtok(NULL, "/");
			char atacado[20];
			strcpy(atacado,p);
			
			int nuevavida;
			nuevavida = Jugada_Ataque(conn, tablapartidas, atacado, carta, partida);
			char notificacion[512];
			sprintf(notificacion, "17/%s,%d,%s,", atacado, nuevavida, carta);
			write(tablapartidas[partida].socket1, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket2, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket3, notificacion, strlen(notificacion));
			write(tablapartidas[partida].socket4, notificacion, strlen(notificacion));
		}
		
		
		//envia el nombre de la persona que ha muerto
		else if (codigo ==18)
		{
			p = strtok(NULL, "/");
			char nombre[20];
			strcpy(nombre, p);
			char mensaje1[20];
			sprintf(mensaje1, "18/%s/",nombre);
			write(tablapartidas[partida].socket1, mensaje1, strlen(mensaje1));
			write(tablapartidas[partida].socket2, mensaje1, strlen(mensaje1));
			write(tablapartidas[partida].socket3, mensaje1, strlen(mensaje1));
			write(tablapartidas[partida].socket4, mensaje1, strlen(mensaje1));
		}
		
		else if(codigo==20)
		{
			p = strtok(NULL, "/");
			char xat[512];
			char emisor[20];
			strcpy(xat,p);
			char mensaje[512];
			//enviar mensaje a los usuuarios contra los que juegues (a los que has invitado y a ti mismo)
			//para que se puedan ir leyendo los mensajes y que fluya la conversacion
			
			DameUsuario(&lista,sock_conn,emisor);
			//sprintf(mensaje, "20/%s:%s \n", emisor, xat);
			printf("%s\n", mensaje);
			
			int i;
			int encontrado = 0;
			while(i<100 && encontrado==0)
			{
				if(tablapartidas[i].socket1 == sock_conn || tablapartidas[i].socket2 == sock_conn || tablapartidas[i].socket3 == sock_conn || tablapartidas[i].socket4 == sock_conn)
					encontrado =1;
				else
					i++;
			}
			if(encontrado ==1)
			{
				if(tablapartidas[i].socket1 != -1)
					sprintf(mensaje, "20/%s:%s/%s,%s,%s,%s, \n", emisor, xat,tablapartidas[i].usuario1,tablapartidas[i].usuario2,tablapartidas[i].usuario3,tablapartidas[i].usuario4);
					printf(mensaje, "20/%s:%s/%s,%s,%s,%s, \n", emisor, xat,tablapartidas[i].usuario1,tablapartidas[i].usuario2,tablapartidas[i].usuario3,tablapartidas[i].usuario4);
					write(tablapartidas[i].socket1, mensaje, strlen(mensaje));
				if(tablapartidas[i].socket2 != -1)
					sprintf(mensaje, "20/%s:%s/%s,%s,%s,%s, \n", emisor, xat,tablapartidas[i].usuario1,tablapartidas[i].usuario2,tablapartidas[i].usuario3,tablapartidas[i].usuario4);
					write(tablapartidas[i].socket2, mensaje, strlen(mensaje));
				if(tablapartidas[i].socket3 != -1)
					sprintf(mensaje, "20/%s:%s/%s,%s,%s,%s, \n", emisor, xat,tablapartidas[i].usuario1,tablapartidas[i].usuario2,tablapartidas[i].usuario3,tablapartidas[i].usuario4);
					write(tablapartidas[i].socket3, mensaje, strlen(mensaje));
				if(tablapartidas[i].socket4 != -1)
					sprintf(mensaje, "20/%s:%s/%s,%s,%s,%s, \n", emisor, xat,tablapartidas[i].usuario1,tablapartidas[i].usuario2,tablapartidas[i].usuario3,tablapartidas[i].usuario4);
					write(tablapartidas[i].socket4, mensaje, strlen(mensaje));
			}			
			
		}
		
		if ((codigo!=0) && (codigo!=6) && (codigo!=10) && (codigo!=11) && (codigo!=12) && (codigo!=20) && (codigo!=13) && (codigo!=14) && (codigo!=15) && (codigo!=16) && (codigo!=17) && (codigo!=18))
		{
			write (sock_conn,msgtocl, strlen(msgtocl));
		}		
	}
	// Se acabo el servicio para este cliente
	close(sock_conn); 
}


int main(int argc, char *argv[])
{
	int sock_conn, sock_listen;
	struct sockaddr_in serv_adr;
	
	
	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket");
	// Fem el bind al port
	
	memset(&serv_adr, 0, sizeof(serv_adr));// inicialitza a zero serv_addr
	serv_adr.sin_family = AF_INET;
	
	// associa el socket a cualquiera de las IP de la m?quina. 
	//htonl formatea el numero que recibe al formato necesario
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	// escucharemos en el port 50066/50067/50068, (para entorno de creacion usamos 9070)
	serv_adr.sin_port = htons(9070);
	if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf ("Error al bind");
	//La cola de peticiones pendientes no podra ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	
	
	pthread_t thread[100]; //estructura de un vector de 100 threads
	
	//bucle infinito
	for(;;){
		printf ("Escuchando\n");
		
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		//sock_conn es el socket que usaremos para este cliente
		
		sockets[i] =sock_conn;
		
		//crear thread y decirle lo que tiene que hacer
		pthread_create (&thread[i], NULL, AtenderCliente, &sockets[i]);//pasamos por referencia el socket; &thread (pasado por ref) guardara un num que es un identificador de thread
		
		i = i+1;
	}
	
}
