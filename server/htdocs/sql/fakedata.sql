DELETE FROM unlocks WHERE 1;
DELETE FROM battles WHERE 1;
DELETE FROM users WHERE 1;


INSERT INTO users (`id`, `name`, `pass`, `elo`) VALUES
    (NULL, 'Jean', '$2y$10$cH09FUHVEKIaJ53X0/WhI.F0zeZ9OVtNCsmJxHngc6/4qCD0YWhr2', 1000), -- 1234
    (NULL, 'Paul', '$2y$10$u40qgi0/BP/NRRSLMiz/qOZFg.EAZlUesEjX4iJYQSsJEMpyebxNK', 1000); -- piou

INSERT INTO battles (`id`, `date`, `winner`, `loser`, `P1`, `P2`, `status`) VALUES
    (NULL, NOW(), 2, 1, 2, 1, "end"),
    (NULL, NOW(), NULL, NULL, 1, 2, "start");

INSERT INTO unlocks (`user`, `poke`, `battle`) VALUES
    (1, 2, NULL),
    (1, 3, NULL),
    (1, 4, NULL),
    (2, 21, NULL),
    (2, 22, NULL),
    (2, 23, NULL),
    (1, 3, 1);
