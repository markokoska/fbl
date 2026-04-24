-- Generate a round-robin schedule for 18 Bundesliga teams across 34 gameweeks
-- GW 1-17: first half, GW 18-34: reverse fixtures (swap home/away)
-- GW 1-26 get realistic results, GW 27-34 are upcoming

DO $$
DECLARE
    teams TEXT[] := ARRAY[
        'Bayern Munich', 'Borussia Dortmund', 'RB Leipzig', 'Bayer Leverkusen',
        'VfB Stuttgart', 'Eintracht Frankfurt', 'SC Freiburg', 'VfL Wolfsburg',
        'TSG Hoffenheim', 'Werder Bremen', 'FC Augsburg', '1. FC Union Berlin',
        'Borussia Mönchengladbach', '1. FSV Mainz 05', 'VfL Bochum', 'FC St. Pauli',
        'Holstein Kiel', '1. FC Heidenheim'
    ];
    n INT := 18;
    round_teams INT[];
    gw_id INT;
    home_idx INT;
    away_idx INT;
    home_team TEXT;
    away_team TEXT;
    home_goals INT;
    away_goals INT;
    r INT;
    m INT;
    t INT;
    gw_ids INT[];
    -- Team strength for realistic results (index 1-18)
    strength FLOAT[] := ARRAY[
        0.90, 0.80, 0.75, 0.85,  -- Bayern, BVB, Leipzig, Leverkusen
        0.70, 0.68, 0.62, 0.58,  -- Stuttgart, Frankfurt, Freiburg, Wolfsburg
        0.52, 0.55, 0.48, 0.50,  -- Hoffenheim, Bremen, Augsburg, Union
        0.53, 0.50, 0.35, 0.42,  -- Gladbach, Mainz, Bochum, St Pauli
        0.38, 0.45               -- Kiel, Heidenheim
    ];
    home_str FLOAT;
    away_str FLOAT;
    home_exp FLOAT;
    away_exp FLOAT;
BEGIN
    -- Clear existing matches
    DELETE FROM "Matches";

    -- Get all gameweek IDs
    SELECT array_agg("Id" ORDER BY "Number")
    INTO gw_ids
    FROM "Gameweeks";

    -- Generate round-robin using circle method
    -- Fix team 1, rotate the rest
    FOR r IN 1..17 LOOP  -- 17 rounds for first half
        gw_id := gw_ids[r];

        -- Build the rotation for this round
        -- Position 0 is always team index 1 (fixed)
        -- Positions 1..16 rotate
        round_teams := ARRAY[]::INT[];
        round_teams := round_teams || 1; -- fixed team
        FOR t IN 1..17 LOOP
            round_teams := round_teams || (((t - 1 + r - 1) % 17) + 2);
        END LOOP;

        -- Pair up: (0, n-1), (1, n-2), (2, n-3), ...
        FOR m IN 0..8 LOOP  -- 9 matches per round
            IF m = 0 THEN
                -- Alternate home/away for the fixed team
                IF r % 2 = 1 THEN
                    home_idx := round_teams[1];   -- 1-based array
                    away_idx := round_teams[18];
                ELSE
                    home_idx := round_teams[18];
                    away_idx := round_teams[1];
                END IF;
            ELSE
                home_idx := round_teams[m + 1];
                away_idx := round_teams[18 - m];
            END IF;

            home_team := teams[home_idx];
            away_team := teams[away_idx];

            -- Generate result for finished GWs (1-26)
            IF r <= 26 THEN
                home_str := strength[home_idx];
                away_str := strength[away_idx];

                -- Expected goals based on strength + home advantage
                home_exp := (home_str * 2.0 + 0.3) * (0.7 + random() * 0.6);
                away_exp := (away_str * 1.6) * (0.7 + random() * 0.6);

                home_goals := LEAST(floor(home_exp)::INT, 6);
                away_goals := LEAST(floor(away_exp)::INT, 5);

                -- Add some randomness for upsets
                IF random() < 0.08 THEN  -- 8% chance of upset
                    home_goals := floor(random() * 2)::INT;
                    away_goals := floor(random() * 4 + 1)::INT;
                END IF;

                INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished")
                VALUES (gw_id, home_team, away_team, home_goals, away_goals, TRUE);
            ELSE
                INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished")
                VALUES (gw_id, home_team, away_team, NULL, NULL, FALSE);
            END IF;
        END LOOP;
    END LOOP;

    -- Second half: GW 18-34, reverse home/away
    FOR r IN 1..17 LOOP
        gw_id := gw_ids[r + 17];

        round_teams := ARRAY[]::INT[];
        round_teams := round_teams || 1;
        FOR t IN 1..17 LOOP
            round_teams := round_teams || (((t - 1 + r - 1) % 17) + 2);
        END LOOP;

        FOR m IN 0..8 LOOP
            IF m = 0 THEN
                IF r % 2 = 1 THEN
                    -- REVERSED from first half
                    away_idx := round_teams[1];
                    home_idx := round_teams[18];
                ELSE
                    away_idx := round_teams[18];
                    home_idx := round_teams[1];
                END IF;
            ELSE
                -- REVERSED
                away_idx := round_teams[m + 1];
                home_idx := round_teams[18 - m];
            END IF;

            home_team := teams[home_idx];
            away_team := teams[away_idx];

            -- GW 18-26 finished, 27-34 upcoming
            IF (r + 17) <= 26 THEN
                home_str := strength[home_idx];
                away_str := strength[away_idx];

                home_exp := (home_str * 2.0 + 0.3) * (0.7 + random() * 0.6);
                away_exp := (away_str * 1.6) * (0.7 + random() * 0.6);

                home_goals := LEAST(floor(home_exp)::INT, 6);
                away_goals := LEAST(floor(away_exp)::INT, 5);

                IF random() < 0.08 THEN
                    home_goals := floor(random() * 2)::INT;
                    away_goals := floor(random() * 4 + 1)::INT;
                END IF;

                INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished")
                VALUES (gw_id, home_team, away_team, home_goals, away_goals, TRUE);
            ELSE
                INSERT INTO "Matches" ("GameweekId", "HomeTeam", "AwayTeam", "HomeGoals", "AwayGoals", "IsFinished")
                VALUES (gw_id, home_team, away_team, NULL, NULL, FALSE);
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE 'Generated % matches', (SELECT COUNT(*) FROM "Matches");
END $$;
