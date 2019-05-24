CREATE TABLE [dbo].[movesxeffects] (
    [move]    INT NOT NULL,
    [effect]  INT NOT NULL,
    [percent] INT NOT NULL,
    [value]   INT NOT NULL,
    PRIMARY KEY CLUSTERED ([move] ASC, [effect] ASC),
    CONSTRAINT [FK_Effect] FOREIGN KEY ([effect]) REFERENCES [dbo].[effects] ([id]),
    CONSTRAINT [FK_Move] FOREIGN KEY ([move]) REFERENCES [dbo].[moves] ([id])
);

