CREATE TABLE [dbo].[sets] (
    [id]      INT          NOT NULL,
    [pokemon] INT          NOT NULL,
    [name]    VARCHAR (50) NOT NULL,
    [move1]   INT          NOT NULL,
    [move2]   INT          NOT NULL,
    [move3]   INT          NOT NULL,
    [move4]   INT          NOT NULL,
    [ability] INT          NOT NULL,
    [item]    INT          NOT NULL,
    [EV1]     VARCHAR (3)  NOT NULL,
    [EV2]     VARCHAR (3)  NOT NULL,
    [nature+] VARCHAR (3)  NOT NULL,
    [nature-] VARCHAR (3)  NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Pokemon] FOREIGN KEY ([pokemon]) REFERENCES [dbo].[pokemons] ([id]),
    CONSTRAINT [FK_Move1] FOREIGN KEY ([move1]) REFERENCES [dbo].[moves] ([id]),
    CONSTRAINT [FK_Move2] FOREIGN KEY ([move2]) REFERENCES [dbo].[moves] ([id]),
    CONSTRAINT [FK_Move3] FOREIGN KEY ([move3]) REFERENCES [dbo].[moves] ([id]),
    CONSTRAINT [FK_Move4] FOREIGN KEY ([move4]) REFERENCES [dbo].[moves] ([id]),
    CONSTRAINT [FK_Ability] FOREIGN KEY ([ability]) REFERENCES [dbo].[abilities] ([id]),
    CONSTRAINT [FK_Item] FOREIGN KEY ([item]) REFERENCES [dbo].[items] ([id])
);

