-- Execute antes de iniciar a API pela primeira vez com a nova migração.
-- A consulta não altera dados; ela apenas mostra nomes que impediriam a criação do índice único.
SELECT
    lower(btrim("Name")) AS normalized_name,
    COUNT(*) AS occurrences,
    array_agg("Id" ORDER BY "CreatedAt") AS game_ids,
    array_agg("Name" ORDER BY "CreatedAt") AS stored_names
FROM games."Games"
GROUP BY lower(btrim("Name"))
HAVING COUNT(*) > 1
ORDER BY normalized_name;
