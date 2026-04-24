-- Generate gameweek history for all players
-- Distributes season totals randomly across GW 1-26

DO $$
DECLARE
    p RECORD;
    gw_id INT;
    gw_num INT;
    goals_left INT;
    assists_left INT;
    cs_left INT;
    yc_left INT;
    rc_left INT;
    pen_saves_left INT;
    pen_miss_left INT;
    og_left INT;
    bonus_left INT;
    gw_ids INT[];
    played_gws INT[];
    i INT;
    idx INT;
    minutes INT;
    got_goal BOOLEAN;
    got_assist BOOLEAN;
    got_cs BOOLEAN;
    bonus_val INT;
    pts INT;
    goal_pts INT;
    cs_pts INT;
    tmp INT;
BEGIN
    -- Clear existing match events
    DELETE FROM "MatchEvents";

    -- Get all finished gameweek IDs (1-26)
    SELECT array_agg("Id" ORDER BY "Number")
    INTO gw_ids
    FROM "Gameweeks"
    WHERE "Number" <= 26;

    FOR p IN SELECT * FROM "BundesligaPlayers" LOOP
        -- Skip players with 0 matches
        IF p."MatchesPlayed" <= 0 THEN CONTINUE; END IF;

        -- Pick random gameweeks this player played in
        played_gws := ARRAY[]::INT[];
        FOR i IN 1..26 LOOP
            played_gws := played_gws || i;
        END LOOP;

        -- Fisher-Yates shuffle
        FOR i IN REVERSE 26..2 LOOP
            idx := floor(random() * i + 1)::INT;
            tmp := played_gws[i];
            played_gws[i] := played_gws[idx];
            played_gws[idx] := tmp;
        END LOOP;

        -- Initialize remaining stats to distribute
        goals_left := p."Goals";
        assists_left := p."Assists";
        cs_left := p."CleanSheets";
        yc_left := p."YellowCards";
        rc_left := p."RedCards";
        pen_saves_left := p."PenaltySaves";
        pen_miss_left := p."PenaltiesMissed";
        og_left := p."OwnGoals";
        bonus_left := p."BonusPoints";

        -- Determine goal/cs points by position
        CASE p."Position"
            WHEN 1 THEN goal_pts := 6; cs_pts := 4;
            WHEN 2 THEN goal_pts := 6; cs_pts := 4;
            WHEN 3 THEN goal_pts := 5; cs_pts := 1;
            WHEN 4 THEN goal_pts := 4; cs_pts := 0;
            ELSE goal_pts := 4; cs_pts := 0;
        END CASE;

        FOR i IN 1..p."MatchesPlayed" LOOP
            gw_num := played_gws[i];
            gw_id := gw_ids[gw_num];

            -- Minutes played
            IF random() < 0.85 THEN
                minutes := 90;
            ELSIF random() < 0.5 THEN
                minutes := 60 + floor(random() * 30)::INT;
            ELSE
                minutes := 15 + floor(random() * 44)::INT;
            END IF;

            pts := CASE WHEN minutes >= 60 THEN 2 ELSE 1 END;

            INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
            VALUES (gw_id, p."Id", 0, minutes, pts);

            -- Goals
            got_goal := FALSE;
            IF goals_left > 0 AND random() < (goals_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 1, floor(random() * 90 + 1)::INT, goal_pts);
                goals_left := goals_left - 1;
                got_goal := TRUE;

                IF goals_left > 0 AND random() < 0.15 THEN
                    INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                    VALUES (gw_id, p."Id", 1, floor(random() * 90 + 1)::INT, goal_pts);
                    goals_left := goals_left - 1;
                END IF;
            END IF;

            -- Assists
            got_assist := FALSE;
            IF assists_left > 0 AND random() < (assists_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 2, floor(random() * 90 + 1)::INT, 3);
                assists_left := assists_left - 1;
                got_assist := TRUE;
            END IF;

            -- Clean sheets
            got_cs := FALSE;
            IF cs_left > 0 AND cs_pts > 0 AND random() < (cs_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 3, NULL, cs_pts);
                cs_left := cs_left - 1;
                got_cs := TRUE;
            END IF;

            -- Yellow cards
            IF yc_left > 0 AND random() < (yc_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 4, floor(random() * 90 + 1)::INT, -1);
                yc_left := yc_left - 1;
            END IF;

            -- Red cards
            IF rc_left > 0 AND random() < (rc_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 5, floor(random() * 90 + 1)::INT, -3);
                rc_left := rc_left - 1;
            END IF;

            -- Penalty saves
            IF pen_saves_left > 0 AND random() < (pen_saves_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 6, floor(random() * 90 + 1)::INT, 5);
                pen_saves_left := pen_saves_left - 1;
            END IF;

            -- Penalty misses
            IF pen_miss_left > 0 AND random() < (pen_miss_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 7, floor(random() * 90 + 1)::INT, -2);
                pen_miss_left := pen_miss_left - 1;
            END IF;

            -- Own goals
            IF og_left > 0 AND random() < (og_left::FLOAT / GREATEST(p."MatchesPlayed" - i + 1, 1)) THEN
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 8, floor(random() * 90 + 1)::INT, -2);
                og_left := og_left - 1;
            END IF;

            -- Bonus points on matches with goals/assists/cs
            IF bonus_left > 0 AND (got_goal OR got_assist OR got_cs OR random() < 0.1) THEN
                bonus_val := LEAST(bonus_left, CASE
                    WHEN got_goal AND got_assist THEN 3
                    WHEN got_goal OR got_cs THEN (floor(random() * 2)::INT + 2)
                    WHEN got_assist THEN (floor(random() * 2)::INT + 1)
                    ELSE 1
                END);
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_id, p."Id", 9, bonus_val, bonus_val);
                bonus_left := bonus_left - bonus_val;
            END IF;

        END LOOP;

        -- Dump leftover stats into last played GW
        IF goals_left > 0 THEN
            FOR i IN 1..goals_left LOOP
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_ids[played_gws[p."MatchesPlayed"]], p."Id", 1, floor(random() * 90 + 1)::INT, goal_pts);
            END LOOP;
        END IF;
        IF assists_left > 0 THEN
            FOR i IN 1..assists_left LOOP
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_ids[played_gws[p."MatchesPlayed"]], p."Id", 2, floor(random() * 90 + 1)::INT, 3);
            END LOOP;
        END IF;
        IF cs_left > 0 AND cs_pts > 0 THEN
            FOR i IN 1..cs_left LOOP
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_ids[played_gws[LEAST(p."MatchesPlayed", i)]], p."Id", 3, NULL, cs_pts);
            END LOOP;
        END IF;
        IF yc_left > 0 THEN
            FOR i IN 1..yc_left LOOP
                INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
                VALUES (gw_ids[played_gws[LEAST(p."MatchesPlayed", i)]], p."Id", 4, floor(random() * 90 + 1)::INT, -1);
            END LOOP;
        END IF;
        IF bonus_left > 0 THEN
            INSERT INTO "MatchEvents" ("GameweekId", "PlayerId", "EventType", "Minute", "Points")
            VALUES (gw_ids[played_gws[p."MatchesPlayed"]], p."Id", 9, bonus_left, bonus_left);
        END IF;

    END LOOP;

    RAISE NOTICE 'Done generating gameweek history for all players';
END $$;
