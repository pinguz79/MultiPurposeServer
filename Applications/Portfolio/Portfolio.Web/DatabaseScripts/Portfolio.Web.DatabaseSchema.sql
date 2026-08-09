-- Portfolio.Web cache database schema
-- MySQL 8.0+
-- Creates the local, fully rebuildable cache tables used by Portfolio.Web.

SET NAMES utf8mb4;
SET time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS `pw_api_response_cache` (
    `cache_key` CHAR(64) COLLATE utf8mb4_unicode_ci NOT NULL,
    `request_url` TEXT COLLATE utf8mb4_unicode_ci NOT NULL,
    `response_json` MEDIUMTEXT COLLATE utf8mb4_unicode_ci NOT NULL,
    `http_code` INT NOT NULL DEFAULT 200,
    `created_at` DATETIME NOT NULL,
    `updated_at` DATETIME NOT NULL,
    `expires_at` DATETIME NOT NULL,
    PRIMARY KEY (`cache_key`),
    KEY `ix_pw_api_response_cache_expires_at` (`expires_at`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `pw_route_album_map` (
    `path` VARCHAR(700) COLLATE utf8mb4_unicode_ci NOT NULL,
    `album_id` CHAR(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `name` VARCHAR(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
    `description` TEXT COLLATE utf8mb4_unicode_ci DEFAULT NULL,
    `kind` VARCHAR(100) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`path`),
    UNIQUE KEY `ux_pw_route_album_map_album_id` (`album_id`),
    KEY `ix_pw_route_album_map_updated_at` (`updated_at`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `pw_route_photo_map` (
    `path` VARCHAR(700) COLLATE utf8mb4_unicode_ci NOT NULL,
    `photo_id` CHAR(36) COLLATE utf8mb4_unicode_ci NOT NULL,
    `album_id` CHAR(36) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
    `title` VARCHAR(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`path`),
    UNIQUE KEY `ux_pw_route_photo_map_photo_id` (`photo_id`),
    KEY `ix_pw_route_photo_map_album_id` (`album_id`),
    KEY `ix_pw_route_photo_map_updated_at` (`updated_at`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci;
