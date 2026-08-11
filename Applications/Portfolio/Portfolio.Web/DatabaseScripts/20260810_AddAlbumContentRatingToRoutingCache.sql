ALTER TABLE `pw_route_album_map`
    ADD COLUMN `content_rating` VARCHAR(32) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Standard' AFTER `kind`;
