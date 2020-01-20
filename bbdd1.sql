DROP DATABASE IF EXISTS bbdd1;
CREATE DATABASE bbdd1;
USE bbdd1;

CREATE TABLE jugador (
	id VARCHAR(20) PRIMARY KEY NOT NULL,
	contraseña VARCHAR(20),
	partidasjugadas INTEGER
)ENGINE = InnoDB;

CREATE TABLE partida (
	id INTEGER PRIMARY KEY NOT NULL,
	fecha DATE NOT NULL,
	ganador VARCHAR(20) NOT NULL,
	numjugadores INTEGER NOT NULL
)ENGINE = InnoDB;


CREATE TABLE relacionJugPar (
	idjug VARCHAR(20) NOT NULL,
	idpar INTEGER NOT NULL,
	puntuacion INTEGER,
	FOREIGN KEY (idjug) REFERENCES jugador(id),
	FOREIGN KEY (idpar) REFERENCES partida(id)
)ENGINE = InnoDB;

CREATE TABLE cartas (
	idcarta INTEGER NOT NULL,
	tipo VARCHAR(20),
	valor INTEGER
)ENGINE = InnoDB;

INSERT INTO jugador VALUES('Juan','juan',14);
INSERT INTO jugador VALUES('Maria','maria',13);
INSERT INTO jugador VALUES('Gerard','gerard',8);
INSERT INTO jugador VALUES('Pol','pol',7);
INSERT INTO jugador VALUES('Julia','julia',2);

INSERT INTO partida VALUES (1, '2019-2-10', 'julia', 4);
INSERT INTO partida VALUES (2, '2019-2-12', 'julia', 3);
INSERT INTO partida VALUES (3, '2019-2-12', 'gerard', 2);
INSERT INTO partida VALUES (4, '2019-2-12', 'maria', 3);
INSERT INTO partida VALUES (5, '2019-2-16', 'maria', 3);

INSERT INTO relacionJugPar VALUES('Maria',2, 30);
INSERT INTO relacionJugPar VALUES('Juan',2, 97);
INSERT INTO relacionJugPar VALUES('Gerard',2, 12);
INSERT INTO relacionJugPar VALUES('Pol',3, 45);
INSERT INTO relacionJugPar VALUES('Julia',4, 69);

INSERT INTO cartas VALUES(1, 'defensa',5);
INSERT INTO cartas VALUES(2, 'defensa',10);
INSERT INTO cartas VALUES(3, 'defensa',3);
INSERT INTO cartas VALUES(4, 'defensa',15);
INSERT INTO cartas VALUES(5, 'defensa',1);
INSERT INTO cartas VALUES(6, 'defensa',30);
INSERT INTO cartas VALUES(7, 'defensa',8);

INSERT INTO cartas VALUES(8, 'atac',5);
INSERT INTO cartas VALUES(9, 'atac',7);
INSERT INTO cartas VALUES(10, 'atac',30);
INSERT INTO cartas VALUES(11, 'atac',10);
INSERT INTO cartas VALUES(12, 'atac',10);
INSERT INTO cartas VALUES(13, 'atac',2);
INSERT INTO cartas VALUES(14, 'atac',15);
INSERT INTO cartas VALUES(15, 'atac',12);
INSERT INTO cartas VALUES(16, 'atac',1);
INSERT INTO cartas VALUES(17, 'atac',3);

	
