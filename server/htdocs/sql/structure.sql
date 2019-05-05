DROP TABLE IF EXISTS unlocks;
DROP TABLE IF EXISTS battles;
DROP TABLE IF EXISTS users;


CREATE TABLE users (
    `id` INTEGER NOT NULL PRIMARY KEY auto_increment,
    `name` VARCHAR(32) NOT NULL,
    `pass` VARCHAR(64) NOT NULL,
    `elo` INTEGER NOT NULL
) engine=innodb CHARACTER SET utf8 collate utf8_unicode_ci;

CREATE TABLE battles (
    `id` INTEGER NOT NULL PRIMARY KEY auto_increment,
    `date` DATE NOT NULL,
    `winner` INTEGER,
    `loser` INTEGER,
    `P1` INTEGER NOT NULL,
    `P2` INTEGER NOT NULL,
    `status` VARCHAR(255), -- ("..." / $battle_state / NULL) for (waiting answer / on-going / ended)

    FOREIGN KEY (`winner`) REFERENCES users (`id`),
    FOREIGN KEY (`loser`) REFERENCES users (`id`),
    FOREIGN KEY (`P1`) REFERENCES users (`id`),
    FOREIGN KEY (`P2`) REFERENCES users (`id`)
) engine=innodb CHARACTER SET utf8 collate utf8_unicode_ci;

CREATE TABLE unlocks (
    `user` INTEGER,
    `poke` INTEGER, -- EXTERNAL KEY `poke` REFERENCES [dbo].[pokemons]([id])
    `battle` INTEGER,

    FOREIGN KEY (`user`) REFERENCES users(`id`),
    FOREIGN KEY (`battle`) REFERENCES battles(`id`)
) engine=innodb CHARACTER SET utf8 collate utf8_unicode_ci;
