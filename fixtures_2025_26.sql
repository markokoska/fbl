-- Bundesliga 2025/26 Fixtures from OpenLigaDB
-- Generated on 2026-03-25T18:13:54.308Z

-- Delete all existing matches
DELETE FROM "Matches";

-- Gameweek 1
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'Bayern Munich', 'RB Leipzig', 6, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'Bayer Leverkusen', 'TSG Hoffenheim', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'Eintracht Frankfurt', 'Werder Bremen', 4, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'SC Freiburg', 'FC Augsburg', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), '1. FC Union Berlin', 'VfB Stuttgart', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), '1. FC Heidenheim', 'VfL Wolfsburg', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'FC St. Pauli', 'Borussia Dortmund', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), '1. FSV Mainz 05', '1. FC Köln', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 1), 'Borussia Mönchengladbach', 'Hamburger SV', 0, 0, true);

-- Gameweek 2
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'Hamburger SV', 'FC St. Pauli', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'RB Leipzig', '1. FC Heidenheim', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'Werder Bremen', 'Bayer Leverkusen', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'VfB Stuttgart', 'Borussia Mönchengladbach', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'TSG Hoffenheim', 'Eintracht Frankfurt', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'FC Augsburg', 'Bayern Munich', 2, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'VfL Wolfsburg', '1. FSV Mainz 05', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), 'Borussia Dortmund', '1. FC Union Berlin', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 2), '1. FC Köln', 'SC Freiburg', 4, 1, true);

-- Gameweek 3
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'Bayer Leverkusen', 'Eintracht Frankfurt', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'SC Freiburg', 'VfB Stuttgart', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), '1. FSV Mainz 05', 'RB Leipzig', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'VfL Wolfsburg', '1. FC Köln', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), '1. FC Union Berlin', 'TSG Hoffenheim', 2, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), '1. FC Heidenheim', 'Borussia Dortmund', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'Bayern Munich', 'Hamburger SV', 5, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'FC St. Pauli', 'FC Augsburg', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 3), 'Borussia Mönchengladbach', 'Werder Bremen', 0, 4, true);

-- Gameweek 4
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'Eintracht Frankfurt', '1. FC Union Berlin', 3, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'VfB Stuttgart', 'FC St. Pauli', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'Werder Bremen', 'SC Freiburg', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'FC Augsburg', '1. FSV Mainz 05', 1, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'TSG Hoffenheim', 'Bayern Munich', 1, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'Hamburger SV', '1. FC Heidenheim', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'RB Leipzig', '1. FC Köln', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'Bayer Leverkusen', 'Borussia Mönchengladbach', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 4), 'Borussia Dortmund', 'VfL Wolfsburg', 1, 0, true);

-- Gameweek 5
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), 'Bayern Munich', 'Werder Bremen', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), '1. FSV Mainz 05', 'Borussia Dortmund', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), 'VfL Wolfsburg', 'RB Leipzig', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), 'FC St. Pauli', 'Bayer Leverkusen', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), '1. FC Heidenheim', 'FC Augsburg', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), 'Borussia Mönchengladbach', 'Eintracht Frankfurt', 4, 6, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), 'SC Freiburg', 'TSG Hoffenheim', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), '1. FC Köln', 'VfB Stuttgart', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 5), '1. FC Union Berlin', 'Hamburger SV', 0, 0, true);

-- Gameweek 6
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'TSG Hoffenheim', '1. FC Köln', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Bayer Leverkusen', '1. FC Union Berlin', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Borussia Dortmund', 'RB Leipzig', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Werder Bremen', 'FC St. Pauli', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'FC Augsburg', 'VfL Wolfsburg', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Eintracht Frankfurt', 'Bayern Munich', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'VfB Stuttgart', '1. FC Heidenheim', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Hamburger SV', '1. FSV Mainz 05', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 6), 'Borussia Mönchengladbach', 'SC Freiburg', 0, 0, true);

-- Gameweek 7
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), '1. FC Union Berlin', 'Borussia Mönchengladbach', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), '1. FSV Mainz 05', 'Bayer Leverkusen', 3, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), 'RB Leipzig', 'Hamburger SV', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), 'VfL Wolfsburg', 'VfB Stuttgart', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), '1. FC Heidenheim', 'Werder Bremen', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), '1. FC Köln', 'FC Augsburg', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), 'Bayern Munich', 'Borussia Dortmund', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), 'SC Freiburg', 'Eintracht Frankfurt', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 7), 'FC St. Pauli', 'TSG Hoffenheim', 0, 3, true);

-- Gameweek 8
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'TSG Hoffenheim', '1. FC Heidenheim', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Werder Bremen', '1. FC Union Berlin', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Eintracht Frankfurt', 'FC St. Pauli', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Borussia Mönchengladbach', 'Bayern Munich', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'FC Augsburg', 'RB Leipzig', 0, 6, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Hamburger SV', 'VfL Wolfsburg', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Borussia Dortmund', '1. FC Köln', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'Bayer Leverkusen', 'SC Freiburg', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 8), 'VfB Stuttgart', '1. FSV Mainz 05', 2, 1, true);

-- Gameweek 9
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), 'FC Augsburg', 'Borussia Dortmund', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), '1. FSV Mainz 05', 'Werder Bremen', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), 'RB Leipzig', 'VfB Stuttgart', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), '1. FC Union Berlin', 'SC Freiburg', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), 'FC St. Pauli', 'Borussia Mönchengladbach', 0, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), '1. FC Heidenheim', 'Eintracht Frankfurt', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), 'Bayern Munich', 'Bayer Leverkusen', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), '1. FC Köln', 'Hamburger SV', 4, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 9), 'VfL Wolfsburg', 'TSG Hoffenheim', 2, 3, true);

-- Gameweek 10
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'Eintracht Frankfurt', '1. FSV Mainz 05', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'Werder Bremen', 'VfL Wolfsburg', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'Bayer Leverkusen', '1. FC Heidenheim', 6, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), '1. FC Union Berlin', 'Bayern Munich', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'TSG Hoffenheim', 'RB Leipzig', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'Hamburger SV', 'Borussia Dortmund', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'Borussia Mönchengladbach', '1. FC Köln', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'SC Freiburg', 'FC St. Pauli', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 10), 'VfB Stuttgart', 'FC Augsburg', 3, 2, true);

-- Gameweek 11
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), '1. FC Köln', 'Eintracht Frankfurt', 3, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), '1. FSV Mainz 05', 'TSG Hoffenheim', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'Bayern Munich', 'SC Freiburg', 6, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'Borussia Dortmund', 'VfB Stuttgart', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'VfL Wolfsburg', 'Bayer Leverkusen', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'FC Augsburg', 'Hamburger SV', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), '1. FC Heidenheim', 'Borussia Mönchengladbach', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'RB Leipzig', 'Werder Bremen', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 11), 'FC St. Pauli', '1. FC Union Berlin', 0, 1, true);

-- Gameweek 12
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Bayer Leverkusen', 'Borussia Dortmund', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Borussia Mönchengladbach', 'RB Leipzig', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Bayern Munich', 'FC St. Pauli', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Werder Bremen', '1. FC Köln', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), '1. FC Union Berlin', '1. FC Heidenheim', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'TSG Hoffenheim', 'FC Augsburg', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Hamburger SV', 'VfB Stuttgart', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'Eintracht Frankfurt', 'VfL Wolfsburg', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 12), 'SC Freiburg', '1. FSV Mainz 05', 4, 0, true);

-- Gameweek 13
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), '1. FSV Mainz 05', 'Borussia Mönchengladbach', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'VfB Stuttgart', 'Bayern Munich', 0, 5, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'VfL Wolfsburg', '1. FC Union Berlin', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'FC Augsburg', 'Bayer Leverkusen', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), '1. FC Heidenheim', 'SC Freiburg', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), '1. FC Köln', 'FC St. Pauli', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'RB Leipzig', 'Eintracht Frankfurt', 6, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'Hamburger SV', 'Werder Bremen', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 13), 'Borussia Dortmund', 'TSG Hoffenheim', 2, 0, true);

-- Gameweek 14
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), '1. FC Union Berlin', 'RB Leipzig', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'Eintracht Frankfurt', 'FC Augsburg', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'Borussia Mönchengladbach', 'VfL Wolfsburg', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'FC St. Pauli', '1. FC Heidenheim', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'TSG Hoffenheim', 'Hamburger SV', 4, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'Bayer Leverkusen', '1. FC Köln', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'SC Freiburg', 'Borussia Dortmund', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'Bayern Munich', '1. FSV Mainz 05', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 14), 'Werder Bremen', 'VfB Stuttgart', 0, 4, true);

-- Gameweek 15
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'Borussia Dortmund', 'Borussia Mönchengladbach', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'VfB Stuttgart', 'TSG Hoffenheim', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'VfL Wolfsburg', 'SC Freiburg', 3, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'FC Augsburg', 'Werder Bremen', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), '1. FC Köln', '1. FC Union Berlin', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'Hamburger SV', 'Eintracht Frankfurt', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), 'RB Leipzig', 'Bayer Leverkusen', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), '1. FSV Mainz 05', 'FC St. Pauli', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 15), '1. FC Heidenheim', 'Bayern Munich', 0, 4, true);

-- Gameweek 16
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'SC Freiburg', 'Hamburger SV', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), '1. FC Heidenheim', '1. FC Köln', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'Borussia Mönchengladbach', 'FC Augsburg', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'Werder Bremen', 'TSG Hoffenheim', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'Eintracht Frankfurt', 'Borussia Dortmund', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), '1. FC Union Berlin', '1. FSV Mainz 05', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'Bayer Leverkusen', 'VfB Stuttgart', 1, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'Bayern Munich', 'VfL Wolfsburg', 8, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 16), 'FC St. Pauli', 'RB Leipzig', 1, 1, true);

-- Gameweek 17
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'VfB Stuttgart', 'Eintracht Frankfurt', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'Borussia Dortmund', 'Werder Bremen', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), '1. FSV Mainz 05', '1. FC Heidenheim', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'VfL Wolfsburg', 'FC St. Pauli', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'RB Leipzig', 'SC Freiburg', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'TSG Hoffenheim', 'Borussia Mönchengladbach', 5, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), '1. FC Köln', 'Bayern Munich', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'FC Augsburg', '1. FC Union Berlin', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 17), 'Hamburger SV', 'Bayer Leverkusen', 0, 1, true);

-- Gameweek 18
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'Werder Bremen', 'Eintracht Frankfurt', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'Borussia Dortmund', 'FC St. Pauli', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'VfL Wolfsburg', '1. FC Heidenheim', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'TSG Hoffenheim', 'Bayer Leverkusen', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), '1. FC Köln', '1. FSV Mainz 05', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'Hamburger SV', 'Borussia Mönchengladbach', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'RB Leipzig', 'Bayern Munich', 1, 5, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'VfB Stuttgart', '1. FC Union Berlin', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 18), 'FC Augsburg', 'SC Freiburg', 2, 2, true);

-- Gameweek 19
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'Bayern Munich', 'FC Augsburg', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'SC Freiburg', '1. FC Köln', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), '1. FC Heidenheim', 'RB Leipzig', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'FC St. Pauli', 'Hamburger SV', 0, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'Bayer Leverkusen', 'Werder Bremen', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'Eintracht Frankfurt', 'TSG Hoffenheim', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), '1. FSV Mainz 05', 'VfL Wolfsburg', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), '1. FC Union Berlin', 'Borussia Dortmund', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 19), 'Borussia Mönchengladbach', 'VfB Stuttgart', 0, 3, true);

-- Gameweek 20
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), '1. FC Köln', 'VfL Wolfsburg', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'Eintracht Frankfurt', 'Bayer Leverkusen', 1, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'RB Leipzig', '1. FSV Mainz 05', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'Werder Bremen', 'Borussia Mönchengladbach', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'FC Augsburg', 'FC St. Pauli', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'TSG Hoffenheim', '1. FC Union Berlin', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'Hamburger SV', 'Bayern Munich', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'VfB Stuttgart', 'SC Freiburg', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 20), 'Borussia Dortmund', '1. FC Heidenheim', 3, 2, true);

-- Gameweek 21
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), '1. FC Union Berlin', 'Eintracht Frankfurt', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), 'SC Freiburg', 'Werder Bremen', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), '1. FSV Mainz 05', 'FC Augsburg', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), 'VfL Wolfsburg', 'Borussia Dortmund', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), 'FC St. Pauli', 'VfB Stuttgart', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), '1. FC Heidenheim', 'Hamburger SV', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), 'Borussia Mönchengladbach', 'Bayer Leverkusen', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), '1. FC Köln', 'RB Leipzig', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 21), 'Bayern Munich', 'TSG Hoffenheim', 5, 1, true);

-- Gameweek 22
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'FC Augsburg', '1. FC Heidenheim', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'Borussia Dortmund', '1. FSV Mainz 05', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'Bayer Leverkusen', 'FC St. Pauli', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'Eintracht Frankfurt', 'Borussia Mönchengladbach', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'Werder Bremen', 'Bayern Munich', 0, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'TSG Hoffenheim', 'SC Freiburg', 3, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'Hamburger SV', '1. FC Union Berlin', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'VfB Stuttgart', '1. FC Köln', 3, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 22), 'RB Leipzig', 'VfL Wolfsburg', 2, 2, true);

-- Gameweek 23
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), '1. FC Köln', 'TSG Hoffenheim', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), '1. FSV Mainz 05', 'Hamburger SV', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), 'Bayern Munich', 'Eintracht Frankfurt', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), 'VfL Wolfsburg', 'FC Augsburg', 2, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), '1. FC Union Berlin', 'Bayer Leverkusen', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), 'RB Leipzig', 'Borussia Dortmund', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), 'SC Freiburg', 'Borussia Mönchengladbach', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), 'FC St. Pauli', 'Werder Bremen', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 23), '1. FC Heidenheim', 'VfB Stuttgart', 3, 3, true);

-- Gameweek 24
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'FC Augsburg', '1. FC Köln', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Bayer Leverkusen', '1. FSV Mainz 05', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Werder Bremen', '1. FC Heidenheim', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Borussia Mönchengladbach', '1. FC Union Berlin', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'TSG Hoffenheim', 'FC St. Pauli', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Borussia Dortmund', 'Bayern Munich', 2, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'VfB Stuttgart', 'VfL Wolfsburg', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Eintracht Frankfurt', 'SC Freiburg', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 24), 'Hamburger SV', 'RB Leipzig', 1, 2, true);

-- Gameweek 25
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), 'SC Freiburg', 'Bayer Leverkusen', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), '1. FSV Mainz 05', 'VfB Stuttgart', 2, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), 'VfL Wolfsburg', 'Hamburger SV', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), '1. FC Köln', 'Borussia Dortmund', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), '1. FC Union Berlin', 'Werder Bremen', 1, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), 'Bayern Munich', 'Borussia Mönchengladbach', 4, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), 'RB Leipzig', 'FC Augsburg', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), '1. FC Heidenheim', 'TSG Hoffenheim', 2, 4, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 25), 'FC St. Pauli', 'Eintracht Frankfurt', 0, 0, true);

-- Gameweek 26
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Borussia Mönchengladbach', 'FC St. Pauli', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Bayer Leverkusen', 'Bayern Munich', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Eintracht Frankfurt', '1. FC Heidenheim', 1, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Borussia Dortmund', 'FC Augsburg', 2, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'TSG Hoffenheim', 'VfL Wolfsburg', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Hamburger SV', '1. FC Köln', 1, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'Werder Bremen', '1. FSV Mainz 05', 0, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'SC Freiburg', '1. FC Union Berlin', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 26), 'VfB Stuttgart', 'RB Leipzig', 1, 0, true);

-- Gameweek 27
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'Bayern Munich', '1. FC Union Berlin', 4, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), '1. FC Heidenheim', 'Bayer Leverkusen', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), '1. FSV Mainz 05', 'Eintracht Frankfurt', 2, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'RB Leipzig', 'TSG Hoffenheim', 5, 0, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'VfL Wolfsburg', 'Werder Bremen', 0, 1, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), '1. FC Köln', 'Borussia Mönchengladbach', 3, 3, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'Borussia Dortmund', 'Hamburger SV', 3, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'FC St. Pauli', 'SC Freiburg', 1, 2, true);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 27), 'FC Augsburg', 'VfB Stuttgart', 2, 5, true);

-- Gameweek 28
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'Borussia Mönchengladbach', '1. FC Heidenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'Eintracht Frankfurt', '1. FC Köln', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'VfB Stuttgart', 'Borussia Dortmund', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'Bayer Leverkusen', 'VfL Wolfsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'SC Freiburg', 'Bayern Munich', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'Werder Bremen', 'RB Leipzig', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'TSG Hoffenheim', '1. FSV Mainz 05', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), 'Hamburger SV', 'FC Augsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 28), '1. FC Union Berlin', 'FC St. Pauli', NULL, NULL, false);

-- Gameweek 29
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'FC Augsburg', 'TSG Hoffenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'Borussia Dortmund', 'Bayer Leverkusen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'RB Leipzig', 'Borussia Mönchengladbach', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'VfL Wolfsburg', 'Eintracht Frankfurt', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), '1. FC Heidenheim', '1. FC Union Berlin', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'FC St. Pauli', 'Bayern Munich', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), '1. FC Köln', 'Werder Bremen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), 'VfB Stuttgart', 'Hamburger SV', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 29), '1. FSV Mainz 05', 'SC Freiburg', NULL, NULL, false);

-- Gameweek 30
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'FC St. Pauli', '1. FC Köln', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'Bayer Leverkusen', 'FC Augsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'Werder Bremen', 'Hamburger SV', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), '1. FC Union Berlin', 'VfL Wolfsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'TSG Hoffenheim', 'Borussia Dortmund', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'Eintracht Frankfurt', 'RB Leipzig', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'SC Freiburg', '1. FC Heidenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'Bayern Munich', 'VfB Stuttgart', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 30), 'Borussia Mönchengladbach', '1. FSV Mainz 05', NULL, NULL, false);

-- Gameweek 31
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'Borussia Dortmund', 'SC Freiburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), '1. FSV Mainz 05', 'Bayern Munich', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'RB Leipzig', '1. FC Union Berlin', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'VfB Stuttgart', 'Werder Bremen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'VfL Wolfsburg', 'Borussia Mönchengladbach', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'FC Augsburg', 'Eintracht Frankfurt', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), '1. FC Heidenheim', 'FC St. Pauli', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), '1. FC Köln', 'Bayer Leverkusen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 31), 'Hamburger SV', 'TSG Hoffenheim', NULL, NULL, false);

-- Gameweek 32
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'Bayern Munich', '1. FC Heidenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'Bayer Leverkusen', 'RB Leipzig', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'Eintracht Frankfurt', 'Hamburger SV', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'SC Freiburg', 'VfL Wolfsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'Werder Bremen', 'FC Augsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'Borussia Mönchengladbach', 'Borussia Dortmund', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), '1. FC Union Berlin', '1. FC Köln', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'FC St. Pauli', '1. FSV Mainz 05', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 32), 'TSG Hoffenheim', 'VfB Stuttgart', NULL, NULL, false);

-- Gameweek 33
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'Borussia Dortmund', 'Eintracht Frankfurt', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), '1. FSV Mainz 05', '1. FC Union Berlin', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'RB Leipzig', 'FC St. Pauli', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'VfB Stuttgart', 'Bayer Leverkusen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'VfL Wolfsburg', 'Bayern Munich', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'FC Augsburg', 'Borussia Mönchengladbach', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'TSG Hoffenheim', 'Werder Bremen', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), '1. FC Köln', '1. FC Heidenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 33), 'Hamburger SV', 'SC Freiburg', NULL, NULL, false);

-- Gameweek 34
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'Bayern Munich', '1. FC Köln', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'Bayer Leverkusen', 'Hamburger SV', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'Eintracht Frankfurt', 'VfB Stuttgart', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'SC Freiburg', 'RB Leipzig', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'Werder Bremen', 'Borussia Dortmund', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'Borussia Mönchengladbach', 'TSG Hoffenheim', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), '1. FC Union Berlin', 'FC Augsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), 'FC St. Pauli', 'VfL Wolfsburg', NULL, NULL, false);
INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished") VALUES ((SELECT "Id" FROM "Gameweeks" WHERE "Number" = 34), '1. FC Heidenheim', '1. FSV Mainz 05', NULL, NULL, false);

-- Update Gameweek kickoff times and statuses
UPDATE "Gameweeks" SET "KickoffTime" = '2025-08-22T18:30:00.000Z', "Status" = 2 WHERE "Number" = 1;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-08-29T18:30:00.000Z', "Status" = 2 WHERE "Number" = 2;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-09-12T18:30:00.000Z', "Status" = 2 WHERE "Number" = 3;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-09-19T18:30:00.000Z', "Status" = 2 WHERE "Number" = 4;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-09-26T18:30:00.000Z', "Status" = 2 WHERE "Number" = 5;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-10-03T18:30:00.000Z', "Status" = 2 WHERE "Number" = 6;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-10-17T18:30:00.000Z', "Status" = 2 WHERE "Number" = 7;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-10-24T18:30:00.000Z', "Status" = 2 WHERE "Number" = 8;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-10-31T19:30:00.000Z', "Status" = 2 WHERE "Number" = 9;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-11-07T19:30:00.000Z', "Status" = 2 WHERE "Number" = 10;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-11-21T19:30:00.000Z', "Status" = 2 WHERE "Number" = 11;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-11-28T19:30:00.000Z', "Status" = 2 WHERE "Number" = 12;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-12-05T19:30:00.000Z', "Status" = 2 WHERE "Number" = 13;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-12-12T19:30:00.000Z', "Status" = 2 WHERE "Number" = 14;
UPDATE "Gameweeks" SET "KickoffTime" = '2025-12-19T19:30:00.000Z', "Status" = 2 WHERE "Number" = 15;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-01-09T19:30:00.000Z', "Status" = 2 WHERE "Number" = 16;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-01-13T17:30:00.000Z', "Status" = 2 WHERE "Number" = 17;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-01-16T19:30:00.000Z', "Status" = 2 WHERE "Number" = 18;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-01-23T19:30:00.000Z', "Status" = 2 WHERE "Number" = 19;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-01-30T19:30:00.000Z', "Status" = 2 WHERE "Number" = 20;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-02-06T19:30:00.000Z', "Status" = 2 WHERE "Number" = 21;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-02-13T19:30:00.000Z', "Status" = 2 WHERE "Number" = 22;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-02-20T19:30:00.000Z', "Status" = 2 WHERE "Number" = 23;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-02-27T19:30:00.000Z', "Status" = 2 WHERE "Number" = 24;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-03-06T19:30:00.000Z', "Status" = 2 WHERE "Number" = 25;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-03-13T19:30:00.000Z', "Status" = 2 WHERE "Number" = 26;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-03-20T19:30:00.000Z', "Status" = 2 WHERE "Number" = 27;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-04-04T13:30:00.000Z', "Status" = 0 WHERE "Number" = 28;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-04-10T18:30:00.000Z', "Status" = 0 WHERE "Number" = 29;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-04-17T18:30:00.000Z', "Status" = 0 WHERE "Number" = 30;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-04-25T13:30:00.000Z', "Status" = 0 WHERE "Number" = 31;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-05-02T13:30:00.000Z', "Status" = 0 WHERE "Number" = 32;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-05-09T13:30:00.000Z', "Status" = 0 WHERE "Number" = 33;
UPDATE "Gameweeks" SET "KickoffTime" = '2026-05-16T13:30:00.000Z', "Status" = 0 WHERE "Number" = 34;
