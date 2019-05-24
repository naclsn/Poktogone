CREATE TABLE [dbo].[pokemons] (
    [id]    INT          NOT NULL,
    [name]  VARCHAR (50) NOT NULL,
    [type1] INT          NOT NULL,
    [type2] INT          NOT NULL,
    [hp]    INT          NOT NULL,
    [atk]   INT          NOT NULL,
    [def]   INT          NOT NULL,
    [spa]   INT          NOT NULL,
    [spd]   INT          NOT NULL,
    [spe]   INT          NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Type1] FOREIGN KEY ([type1]) REFERENCES [dbo].[types] ([id]),
    CONSTRAINT [FK_Type2] FOREIGN KEY ([type2]) REFERENCES [dbo].[types] ([id])
);

