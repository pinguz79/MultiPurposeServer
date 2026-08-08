<?php

declare(strict_types=1);

require_once __DIR__ . '/../Database/Db.php';

class RoutingCacheService
{
    public function upsertAlbums(array $albums): void
    {
        if (empty($albums)) {
            return;
        }

        $db = Db::connection();

        $sql = "
            INSERT INTO pw_route_album_map (path, album_id, name, kind, updated_at)
            VALUES (:path, :album_id, :name, :kind, NOW())
            ON DUPLICATE KEY UPDATE
                album_id = VALUES(album_id),
                name = VALUES(name),
                kind = VALUES(kind),
                updated_at = NOW()
        ";

        $stmt = $db->prepare($sql);

        foreach ($albums as $album) {
            $path = $this->requireAlbumField($album, 'fullPath');
            $albumId = $this->requireAlbumField($album, 'id');
            $kind = $this->requireAlbumField($album, 'kind');

            $stmt->execute([
                ':path' => $this->normalizePath($path),
                ':album_id' => $albumId,
                ':name' => $album['name'] ?? null,
                ':kind' => $kind
            ]);
        }
    }

    public function upsertAlbum(string $path, string $albumId, string $kind, ?string $name = null): void
    {
        $this->upsertAlbums([[
            'fullPath' => $path,
            'id' => $albumId,
            'name' => $name,
            'kind' => $kind
        ]]);
    }

    public function upsertPhotos(array $photos): void
    {
        if (empty($photos)) {
            return;
        }

        $db = Db::connection();

        $sql = "
            INSERT INTO pw_route_photo_map (path, photo_id, album_id, title, updated_at)
            VALUES (:path, :photo_id, :album_id, :title, NOW())
            ON DUPLICATE KEY UPDATE
                photo_id = VALUES(photo_id),
                album_id = VALUES(album_id),
                title = VALUES(title),
                updated_at = NOW()
        ";

        $stmt = $db->prepare($sql);

        foreach ($photos as $photo) {
            if (empty($photo['path']) || empty($photo['id'])) {
                continue;
            }

            $stmt->execute([
                ':path' => $this->normalizePath($photo['path']),
                ':photo_id' => $photo['id'],
                ':album_id' => $photo['albumId'] ?? null,
                ':title' => $photo['title'] ?? null
            ]);
        }
    }

    public function getAlbumByPath(string $path): ?array
    {
        $db = Db::connection();

        $stmt = $db->prepare("
            SELECT path, album_id, name, kind
            FROM pw_route_album_map
            WHERE path = :path
            LIMIT 1
        ");

        $stmt->execute([':path' => $this->normalizePath($path)]);
        $row = $stmt->fetch();

        return is_array($row) ? $row : null;
    }

    public function getAlbumIdByPath(string $path): ?string
    {
        $album = $this->getAlbumByPath($path);
        return $album['album_id'] ?? null;
    }

    public function getAlbumBreadcrumbs(string $path): array
    {
        $normalizedPath = $this->normalizePath($path);

        if ($normalizedPath === '') {
            return [];
        }

        $segments = explode('/', $normalizedPath);
        $paths = [];
        $currentPath = '';

        foreach ($segments as $segment) {
            $currentPath = $currentPath === '' ? $segment : $currentPath . '/' . $segment;
            $paths[] = $currentPath;
        }

        $parameters = [];
        $placeholders = [];

        foreach ($paths as $index => $breadcrumbPath) {
            $parameterName = ':path' . $index;
            $placeholders[] = $parameterName;
            $parameters[$parameterName] = $breadcrumbPath;
        }

        $db = Db::connection();

        $stmt = $db->prepare("
            SELECT path, album_id, name, kind
            FROM pw_route_album_map
            WHERE path IN (" . implode(', ', $placeholders) . ")
        ");

        $stmt->execute($parameters);

        $albumsByPath = [];

        while ($row = $stmt->fetch()) {
            $albumsByPath[$row['path']] = $row;
        }

        $breadcrumbs = [];

        foreach ($paths as $breadcrumbPath) {
            $album = $albumsByPath[$breadcrumbPath] ?? null;

            $breadcrumbs[] = [
                'id' => $album['album_id'] ?? null,
                'path' => $breadcrumbPath,
                'name' => $album['name'] ?? basename($breadcrumbPath),
                'kind' => $album['kind'] ?? null
            ];
        }

        return $breadcrumbs;
    }

    public function getPhotoIdByPath(string $path): ?string
    {
        $db = Db::connection();

        $stmt = $db->prepare("
            SELECT photo_id
            FROM pw_route_photo_map
            WHERE path = :path
            LIMIT 1
        ");

        $stmt->execute([':path' => $this->normalizePath($path)]);
        $row = $stmt->fetch();

        return $row['photo_id'] ?? null;
    }

    public function clearAlbums(): int
    {
        return Db::connection()->exec("DELETE FROM pw_route_album_map");
    }

    public function clearPhotos(): int
    {
        return Db::connection()->exec("DELETE FROM pw_route_photo_map");
    }

    private function normalizePath(string $path): string
    {
        return trim(str_replace('\\', '/', $path), '/');
    }

    private function requireAlbumField(array $album, string $field): string
    {
        $value = $album[$field] ?? null;

        if (!is_string($value) || trim($value) === '') {
            $albumId = isset($album['id']) && is_string($album['id']) ? $album['id'] : 'unknown';

            throw new UnexpectedValueException(sprintf(
                'Album "%s" cannot be cached because field "%s" is missing or empty.',
                $albumId,
                $field
            ));
        }

        return $value;
    }
}
