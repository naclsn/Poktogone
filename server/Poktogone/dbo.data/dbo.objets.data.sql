DELETE FROM [dbo].[objets];
INSERT INTO [dbo].[objets] ([id], [nom], [desc], [uniq]) VALUES
(1, 'Baie Iapapa', 'Soigne 50% des pdv lorsqu''à 25% de pdv ou moins', 1),
(2, 'Baie Sitrus', 'Soigne 25% des pdv lorsqu''à 50% des pdv ou moins', 1),
(3, 'Veste de Combat', 'Augmente la Défense Spéciale mais empêche l''utilisation d''attaques n''infligeant pas de dégâts ', 0),
(4, 'Mouchoir Choix', 'Augmente la vitesse mais bloque le pokemon sur la première attaque utilisée', 0),
(5, 'Bandeau Choix', 'Augmente l''attaque mais bloque le pokemon sur la première attaque utilisée', 0),
(6, 'Lunettes Choix', 'Augmente l''attaque spéciale mais bloque le pokemon sur la première attaque utilisée', 0),
(7, 'Evoluroche', 'Augmente les défenses de Leveinard', 0),
(8, 'Ceinture Force', 'Laisse à un point de vie après une attaque devant mettre le pokemon KO si il n''a pas pris de dégâts', 1),
(9, 'Restes', 'Soigne 6% des pdv par tour', 0),
(10, 'Orbe Vie', 'Les attaques de ce pokemon infligent 30% de dégâts suplémentaires mais lui font perdre 10% de pdv par attaque', 0),
(11, 'Casque Brut', 'Un adversaire attaquant et faisant contact s''inflige 12% de pdv', 0),
(12, 'Boue Noire', 'Soigne 6% des pdv / tour si de type poison sinon inflige 12% des pdv / tour', 0),
(13, 'Graine Elec', 'Augmente la défense du pokemon sous le terrain électrique', 1),
(14, 'Graine Psy', 'Augmente la défense du pokemon sous le terrain psy', 1),
(15, 'Orbe Toxic', 'Inflige Toxic au porteur', 0),
(16, 'Baie Yache', 'Diminue les dégâts d''une attaque glace super-efficace', 1),
(17, 'Orbe Lumière', 'Augmente les dégâts des attaques de Pikachu', 0);
