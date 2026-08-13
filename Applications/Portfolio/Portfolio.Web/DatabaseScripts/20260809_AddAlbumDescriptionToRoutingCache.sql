-- Aggiunge la descrizione degli album richiesta dalle pagine e dai metadati di Portfolio.Web.
-- La tabella è una cache rigenerabile; dopo la migrazione le righe esistenti possono essere eliminate.

ALTER TABLE `pw_route_album_map`
    ADD COLUMN `description` TEXT COLLATE utf8mb4_unicode_ci DEFAULT NULL AFTER `name`;
