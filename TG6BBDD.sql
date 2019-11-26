DROP DATABASE IF EXISTS TG6BBDD;
CREATE DATABASE TG6BBDD;
USE TG6BBDD;

CREATE TABLE jugador (
	id VARCHAR(20) PRIMARY KEY NOT NULL,
	contraseña VARCHAR(20),
	partidasjugadas INTEGER,
	nivel INTEGER
)ENGINE = InnoDB;

CREATE TABLE partida (
	id INTEGER PRIMARY KEY NOT NULL,
	fecha VARCHAR(20) NOT NULL,
	ganador VARCHAR(20) NOT NULL,
	duracion FLOAT NOT NULL,
	numjugadores INTEGER NOT NULL
)ENGINE = InnoDB;

CREATE TABLE personajes (
	nombre VARCHAR(20) PRIMARY KEY NOT NULL,
	fuerza INTEGER,
	vida INTEGER,
	poderes VARCHAR(100)
)ENGINE = InnoDB;

CREATE TABLE relacionJugPar (
	idjug VARCHAR(20) NOT NULL,
	idpar INTEGER NOT NULL,
	personajes VARCHAR(20),
	puntuacion INTEGER,
	muertes INTEGER,
	asesinatos INTEGER,
	FOREIGN KEY (idjug) REFERENCES jugador(id),
	FOREIGN KEY (idpar) REFERENCES partida(id)
)ENGINE = InnoDB;

CREATE TABLE relacionJugPer (
	idjug VARCHAR(20) NOT NULL,
	nombrepersonaje VARCHAR(20),
	vecesjugado INTEGER,
	FOREIGN KEY (idjug) REFERENCES jugador(id),
	FOREIGN KEY (nombrepersonaje) REFERENCES personajes(nombre)
)ENGINE = InnoDB;

INSERT INTO jugador VALUES('Juan','putaespaña',14,70);
INSERT INTO jugador VALUES('Maria','bebesitaUAAAA',13,80);
INSERT INTO jugador VALUES('Gerard','cont1',8,23);
INSERT INTO jugador VALUES('Pol','contra4',7,16);
INSERT INTO jugador VALUES('Julia','cont66',2,3);

INSERT INTO partida (id, fecha, ganador, duracion, numjugadores) VALUES (1, '10-02-2019', '2', 1.5, 4);
INSERT INTO partida (id, fecha, ganador, duracion, numjugadores) VALUES (2, '12-02-2019', '1', 1, 3);
INSERT INTO partida (id, fecha, ganador, duracion, numjugadores) VALUES (3, '12-02-2019', '1', 2, 2);
INSERT INTO partida (id, fecha, ganador, duracion, numjugadores) VALUES (4, '12-02-2019', '3', 1.25, 5);
INSERT INTO partida (id, fecha, ganador, duracion, numjugadores) VALUES (5, '16-02-2019', '4', 0.5, 5);

INSERT INTO personajes (nombre) VALUES ('Lenin');
INSERT INTO personajes (nombre) VALUES ('Stalin');
INSERT INTO personajes (nombre) VALUES ('Jruschov');
INSERT INTO personajes (nombre) VALUES ('Malenkov');
INSERT INTO personajes (nombre) VALUES ('Gorbachov');

INSERT INTO relacionJugPar VALUES('Maria',2,'Lenin',30,5,3);
INSERT INTO relacionJugPar VALUES('Juan',5,'Jruschov',75,8,23);
INSERT INTO relacionJugPar VALUES('Gerard',1,'Lenin',49,12,4);
INSERT INTO relacionJugPar VALUES('Pol',3,'Stalin',78,8,10);
INSERT INTO relacionJugPar VALUES('Julia',4,'Stalin',59,2,49);


	
