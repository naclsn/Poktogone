DELETE FROM [dbo].[sets];
INSERT INTO [dbo].[sets] ([id], [pokemon], [name], [move1], [move2], [move3], [move4], [ability], [item], [EV1], [EV2], [nature+], [nature-]) VALUES
(1, 1, 'Mixed sweeper', 1, 31, 44, 78, 1, 17, 'atk', 'spe', 'spe', 'spd'),
(2, 2, 'Physical sweeper', 38, 33, 13, 104, 3, 10, 'atk', 'spe', 'spe', 'spa'),
(3, 3, 'Sun sweeper', 0, 96, 119, 0, 4, 10, 'spa', 'spe', 'spa', 'atk'),--
(4, 4, 'Tortank', 46, 4, 60, 99, 5, 9, 'pdv', 'spa', 'spa', 'atk'),
(5, 6, 'Le Visage du Stall', 65, 99, 47, 56, 11, 9, 'pdv', 'def', 'def', 'atk'),
(6, 7, 'Physical sweeper', 120, 5, 70, 83, 34, 10, 'atk', 'spe', 'spe', 'spa'),
(7, 8, 'Defensive special sweeper', 119, 113, 32, 47, 10, 10, 'pdv', 'spa', 'spa', 'atk'),
(8, 8, 'Special support', 65, 47, 111, 8, 10, 10, 'pdv', 'spa', 'spa', 'atk'),
(9, 8, 'Stall unaware', 65, 99, 47, 113, 12, 9, 'pdv', 'def', 'def', 'atk'),
(10, 9, 'Special sweeper', 104, 95, 26, 50, 13, 10, 'spe', 'spa', 'spe', 'atk'),
(11, 10, 'Pressure PP stall', 27, 122, 10, 104, 14, 9, 'pdv', 'def', 'def', 'atk'),
(12, 10, 'Special sweeper', 27, 10, 102, 101, 1, 10, 'pdv', 'spa', 'spa', 'atk'),
(13, 11, 'Cleric', 0, 0, 17, 40, 4, 9, 'pdv', 'spd', 'spd', 'atk'),--
(14, 12, 'Specs sweeper', 9, 50, 0, 71, 3, 6, 'spa', 'spe', 'spe', 'atk'),--
(15, 13, 'Dragon dance sweeper', 120, 4, 59, 83, 17, 10, 'atk', 'spe', 'spe', 'spa'),
(16, 14, 'Pivot support', 118, 102, 103, 93, 18, 12, 'spe', 'pdv', 'spe', 'spa'),
(17, 15, 'Band sweeper', 80, 7, 114, 57, 20, 5, 'pdv', 'atk', 'atk', 'spa'),
(18, 15, 'Priority sweeper', 38, 5, 7, 80, 20, 10, 'pdv', 'atk', 'atk', 'spa'),
(19, 16, 'Band attacker', 77, 93, 80, 57, 22, 5, 'pdv', 'atk', 'atk', 'spa'),
(20, 17, 'Hazard setter', 65, 72, 104, 102, 23, 9, 'pdv', 'def', 'def', 'spa'),
(21, 17, 'Stall defog phaze', 42, 104, 103, 99, 23, 9, 'pdv', 'def', 'def', 'spa'),
(22, 18, 'Defensive sweeper', 119, 85, 60, 122, 14, 9, 'pdv', 'def', 'def', 'atk'),
(23, 18, 'PP staller', 122, 4, 42, 99, 14, 9, 'pdv', 'def', 'def', 'atk'),
(24, 19, 'Assault vest attacker', 67, 83, 85, 70, 25, 3, 'pdv', 'atk', 'atk', 'spa'),
(25, 19, 'Band sweeper', 67, 83, 85, 70, 25, 5, 'atk', 'spe', 'atk', 'spa'),
(26, 20, 'Defensive support', 65, 93, 86, 40, 11, 9, 'pdv ', 'def', 'def', 'atk'),
(27, 20, 'Special sweeper', 84, 17, 86, 115, 11, 9, 'spa', 'spe', 'spe', 'atk'),
(28, 21, 'All out attacker', 19, 17, 50, 0, 4, 10, 'spa', 'spe', 'spe', 'atk'),--
(29, 22, 'Physical sweeper', 38, 48, 70, 13, 27, 10, 'atk', 'spe', 'spe', 'spa'),
(30, 23, 'Defensive phazer', 4, 65, 99, 42, 28, 9, 'pdv', 'def', 'def', 'atk'),
(31, 23, 'Special sweeper', 4, 60, 71, 65, 5, 9, 'pdv', 'spa', 'spa', 'atk'),
(32, 24, 'Scarf sweeper', 87, 113, 90, 0, 30, 4, 'spa', 'spe', 'spa', 'atk'),--
(33, 25, 'Band attacker', 52, 54, 23, 66, 22, 5, 'atk', 'spe', 'atk', 'spa'),
(34, 25, 'Spore sweeper', 20, 38, 23, 54, 22, 10, 'atk', 'spe', 'spe', 'spa'),
(35, 26, 'Priority support', 80, 16, 40, 118, 33, 9, 'pdv', 'def', 'def', 'spa'),
(36, 27, 'Physical sweeper', 38, 12, 74, 114, 17, 10, 'pdv', 'atk', 'atk', 'spa'),
(37, 28, 'Assault Vest Attacker', 76, 70, 77, 75, 35, 3, 'pdv', 'atk', 'atk', 'spa'),
(38, 28, 'Physical sweeper', 89, 76, 29, 70, 35, 10, 'atk', 'spe', 'atk', 'spa'),
(39, 29, 'Scarftias', 112, 87, 0, 90, 36, 4, 'spa', 'spe', 'spa', 'atk'),--
(40, 29, 'Special sweeper', 119, 111, 86, 103, 36, 9, 'pdv', 'spa', 'spa', 'atk'),
(41, 30, 'Dragon dance sweeper', 120, 109, 70, 67, 34, 10, 'atk', 'spe', 'atk', 'spa'),
(42, 30, 'Physical sweeper', 38, 109, 70, 104, 34, 10, 'atk', 'spe', 'spe', 'spa'),
(43, 68, 'Band sweeper', 18, 76, 78, 115, 71, 5, 'spe', 'atk', 'spe', 'spa');
