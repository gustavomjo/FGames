-- Execute uma vez antes de publicar a versão que normaliza e-mails.
-- A instrução falhará caso já existam contas que diferem apenas por
-- maiúsculas/minúsculas; nesse caso, resolva a duplicidade antes de repetir.
UPDATE users."Users"
SET "Email" = lower(btrim("Email"))
WHERE "Email" <> lower(btrim("Email"));
