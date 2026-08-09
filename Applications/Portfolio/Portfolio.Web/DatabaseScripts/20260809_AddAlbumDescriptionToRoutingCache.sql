-- Adds the album description required by Portfolio.Web pages and metadata.
-- The table is a rebuildable cache; existing rows can be cleared after this migration.

ALTER TABLE `pw_route_album_map`
    ADD COLUMN `description` TEXT COLLATE utf8mb4_unicode_ci DEFAULT NULL AFTER `name`;
