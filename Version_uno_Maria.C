#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <mysql/mysql.h>

int main(int argc, char *argv[])
{
	int sock_conn, sock_listen, ret;
	struct sockaddr_in serv_adr;
	char buff[512];
	char buff2[512];
	char consulta[512];
	
	//Connexio basedades
	MYSQL *conn;
	int err;
	
	//Creamos una conexion al servidor MYSQL 
	conn = mysql_init(NULL);
	if (conn==NULL) {
		printf ("Error al crear la conexiￃﾳn: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	//inicializar la conexion
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "bbdd1", 0, NULL, 0);
	if (conn==NULL) {
		printf ("Error al inicializar la conexiￃﾳn: %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	
	// INICIALITZACIONS
	// Obrim el socket
	if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
		printf("Error creant socket");
	// Fem el bind al port
	
	
	memset(&serv_adr, 0, sizeof(serv_adr));// inicialitza a zero serv_addr
	serv_adr.sin_family = AF_INET;
	
	// asocia el socket a cualquiera de las IP de la m?quina. 
	//htonl formatea el numero que recibe al formato necesario
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	// escucharemos en el port 9050
	serv_adr.sin_port = htons(9070);
	if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf ("Error al bind");
	//La cola de peticiones pendientes no podra ser superior a 4
	if (listen(sock_listen, 2) < 0)
		printf("Error en el Listen");
	

	
	
	//funcions consultes
	void consulta_1(MYSQL *conn, char buff2[512])
	{
		// Estructura especial para almacenar resultados de consultas 
		MYSQL_RES *resultado;
		MYSQL_ROW row;
		char consulta[512];
		char resbuff2[512];
		//Consideramos las muertes totales los asesinatos + las muertes del personaje
		strcpy(consulta, "SELECT partida.duracion FROM partida, relacionJugPar WHERE relacionJugPar.idpar = partida.id AND (relacionJugPar.muertes+relacionJugPar.asesinatos) = ( SELECT MAX(relacionJugPar.muertes+relacionJugPar.asesinatos) FROM relacionJugPar)");
		
		
		err = mysql_query(conn, consulta);
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
	
	void consulta_2(MYSQL *conn,char buff2[512], char jugador[20], int partida)
	{
		// Estructura especial para almacenar resultados de consultas 
		MYSQL_RES *resultado;
		MYSQL_ROW row;
		char resbuff2[512];
		char partidax[10];
		sprintf (partidax,"%d",partida); //Pasar de un entero a un string
					
		char consulta[500];
		
				
		strcpy(consulta, "SELECT relacionJugPar.muertes FROM relacionJugPar WHERE relacionJugPar.idjug = '");
		strcat(consulta, jugador);
		strcat(consulta, "' AND relacionJugPar.idpar = '");
		strcat(consulta, partidax);
		strcat(consulta, "'");
		
		err = mysql_query(conn, consulta);
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
	
		
	void consulta_3(MYSQL *conn, char buff2[512], char personaje[20], char fecha[20])
	{
		MYSQL_RES *resultado;
		MYSQL_ROW row;
		char resbuff2[512];
		char fecha[11];		
		char personaje[20];
		
		int cont = 0;
		char consulta[500];
		
		
		//select relacionJugPar.personajes from partida, relacionJugPar where relacionJugPar.idpar = partida.id and partida.fecha = '12/02/2019';
		
		
		//primer necessito una taula on surti el nom de tots els personatges que es van jugar en la data demanada
		strcpy(consulta, "SELECT relacionJugPar.personajes FROM partida, relacionJugPar WHERE relacionJugPar.idpar = partida.id AND partida.fecha = '");
		strcat(consulta, fecha);
		strcat(consulta, "'");
		
		err = mysql_query(conn, consulta);
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
			while (row !=NULL) {
				if(strcmp(personaje, row[0])==0)
					cont = cont + 1;
				row = mysql_fetch_row (resultado);
			
			
		}
			sprintf(resbuff2, cont);
			strcpy(buff2, resbuff2);
			
	}
	
	int AnadirCuenta (MYSQL *conn,  char buff[512])
	{
		//Incluiremos mecanismos de acceso excluyente para garantizar que no haya problemas a la hora de modificar la base de datos
		//DEVUELVE 0 SI OK, -1 SI NO SE HA PODIDO CREAR
		
		//TROCEAMOS EL MENSAJE RECIBIDO PARA SACAR USUARIO Y CONTRASE�A
		char usuario [50];
		char contrasena [50];
		char consulta[512];
		
		char *p = strtok(buff, ",");
		strcpy(usuario, p);
		p = strtok(NULL, ",");
		strcpy(contrasena, p);

		
		int err;
		

		sprintf(consulta, "INSERT INTO Jugador VALUES ('%s','%s', 0, 0);", usuario, contrasena);
		
		err=mysql_query (conn, query);
		if(err!=0)
			return -1; //HA HABIDO ERROR AL ANADIR LA CUENTA
		else 
			return 0;
	}
	
	
		
	int i;
	// Atenderemos solo 20 peticiones
	for(i=0;i<20;i++){
		printf ("Escuchando\n");
		
		sock_conn = accept(sock_listen, NULL, NULL);
		printf ("He recibido conexi?n\n");
		//sock_conn es el socket que usaremos para este cliente
		
		// Ahora recibimos su nombre, que dejamos en buff
		ret=read(sock_conn,buff, sizeof(buff));
		printf ("Recibido\n");
		
		// Tenemos que a?adirle la marca de fin de string 
		// para que no escriba lo que hay despues en el buffer
		buff[ret]='\0';
		
		//Escribimos el nombre en la consola
		
		printf ("Se ha conectado: %s\n",buff);
		
		
		char *p = strtok( buff, "/");
		int codigo =  atoi (p);
		if (codigo==3)	
		{
			p = strtok( NULL, "/");
			char personaje[20];
			strcpy (personaje, p);
			p = strtok( NULL, "/");
			char fecha[20];
			strcpy (fecha, p);
			printf ("Codigo: %d, Personaje: %s, Fecha: %s\n", codigo, personaje, fecha);
		}
		if (codigo==2)
		{
			p = strtok( NULL, "/");
			char jugador[20];
			strcpy (jugador, p);
			char *p = strtok( NULL, "/");
			int partida;
			partida = atoi(p);
			printf ("Codigo: %d, Jugador: %s, Partida: %d\n", codigo, jugador, partida);			
		}
			
			
		if (codigo ==1) //duracio partida amb mes morts		
		{
			consulta_1(conn, buff2);
			strcpy(consulta, buff2);
			//sprintf (buff2,"%s",consulta1);
		}
		else if (codigo ==2)	
		{
			consulta_2(conn, buff2, jugador, partida);
			strcpy(consulta, buff2);

			// Y lo enviamos
			write (sock_conn,buff2, strlen(buff2));
		}
		else if (codigo == 3)
		{
			consulta_3(conn, buff2, personaje, fecha);
			strcpy(consulta, buff2);
			// Se acabo el servicio para este cliente
			close(sock_conn); 
		}
	}
}
