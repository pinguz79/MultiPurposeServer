<?php

require_once __DIR__ . '/../Services/RoutingCacheService.php';
require_once __DIR__ . '/../Services/AlbumService.php';

class AlbumController
{
    public function showByPath(string $path): void
    {
        $normalizedPath = trim(str_replace('\\', '/', $path), '/');

        $routingCache = new RoutingCacheService();
        $albumService = new AlbumService();

        $albumId = $routingCache->getAlbumIdByPath($normalizedPath);

        if ($albumId === null) {
            $resolved = $albumService->resolveAlbumPath($normalizedPath);

            if (!is_array($resolved) || empty($resolved['id']) || empty($resolved['path'])) {
                http_response_code(404);
                echo 'Album non trovato: ' . htmlspecialchars($normalizedPath);
                return;
            }

            $routingCache->upsertAlbum($resolved['path'], $resolved['id'], $resolved['name'] ?? null);
            $albumId = $resolved['id'];
        }

        $albums = $albumService->getAlbumsByParentId($albumId);

        if (!is_array($albums)) {
            http_response_code(502);
            echo 'Errore nel recupero degli album.';
            return;
        }

        $routingCache->upsertAlbums($albums);

        $photoPage = null;
        $selectedPhotoId = isset($_GET['photoId']) ? trim((string)$_GET['photoId']) : null;

        if (empty($albums)) {
            $page = isset($_GET['page']) ? max(1, (int)$_GET['page']) : 1;
            $pageSize = isset($_GET['pageSize']) ? (int)$_GET['pageSize'] : 12;
            $pageSize = in_array($pageSize, [12, 24, 48], true) ? $pageSize : 12;

            $photoPage = $albumService->getPhotosByAlbumId($albumId, $page, $pageSize);

            if (!is_array($photoPage)) {
                http_response_code(502);
                echo 'Errore nel recupero delle fotografie.';
                return;
            }

            $photos = isset($photoPage['items']) && is_array($photoPage['items']) ? $photoPage['items'] : [];

            if ($selectedPhotoId === null && !empty($photos)) {
                $selectedPhotoId = $photos[0]['id'] ?? null;
            }
        }

        $currentAlbum = $routingCache->getAlbumByPath($normalizedPath) ?? [
            'id' => $albumId,
            'path' => $normalizedPath,
            'name' => basename($normalizedPath)
        ];

        $breadcrumbs = $routingCache->getAlbumBreadcrumbs($normalizedPath);
        $view = __DIR__ . '/../Views/Album/index.php';

        require __DIR__ . '/../Views/Layout/main.php';
    }
}
