<?php

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
            INSERT INTO pw_route_album_map (path, album_id, name, updated_at)
            VALUES (:path, :album_id, :name, NOW())
            ON DUPLICATE KEY UPDATE
                album_id = VALUES(album_id),
                name = VALUES(name),
                updated_at = NOW()
        ";

        $stmt = $db->prepare($sql);

        foreach ($albums as $album) {
            if (empty($album['path']) || empty($album['id'])) {
                continue;
            }

            $stmt->execute([
                ':path' => $album['fullPath'],
                ':album_id' => $album['id'],
                ':name' => $album['name'] ?? null,
            ]);
        }
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
                ':path' => $photo['path'],
                ':photo_id' => $photo['id'],
                ':album_id' => $photo['albumId'] ?? null,
                ':title' => $photo['title'] ?? null,
            ]);
        }
    }

    public function getAlbumIdByPath(string $path): ?string
    {
        $db = Db::connection();

        $stmt = $db->prepare("
            SELECT album_id
            FROM pw_route_album_map
            WHERE path = :path
            LIMIT 1
        ");

        $stmt->execute([':path' => $path]);

        $row = $stmt->fetch();

        return $row['album_id'] ?? null;
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

        $stmt->execute([':path' => $path]);

        $row = $stmt->fetch();

        return $row['photo_id'] ?? null;
    }
    
    public function upsertAlbum(string $path, string $albumId, ?string $name = null): void
	{
    	$this->upsertAlbums([
        	[
            	'path' => $path,
	            'id' => $albumId,
    	        'name' => $name
        	]
    	]);
	}
}