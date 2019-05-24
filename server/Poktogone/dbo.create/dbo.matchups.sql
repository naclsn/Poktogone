CREATE TABLE [dbo].[matchups] (
    [type_atk] INT        NOT NULL,
    [type_def] INT        NOT NULL,
    [coef]     FLOAT (53) NOT NULL,
    PRIMARY KEY CLUSTERED ([type_atk] ASC, [type_def] ASC),
    CONSTRAINT [FK_AtkType] FOREIGN KEY ([type_atk]) REFERENCES [dbo].[types] ([id]),
    CONSTRAINT [FK_DefType] FOREIGN KEY ([type_def]) REFERENCES [dbo].[types] ([id])
);

