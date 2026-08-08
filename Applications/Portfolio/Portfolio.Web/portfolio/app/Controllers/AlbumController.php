<?php

declare(strict_types=1);

require_once __DIR__ . '/../Services/AlbumPageService.php';

class AlbumController
{
    public function showByPath(string $path): void
    {
        $page = isset($_GET['page']) ? max(1, (int)$_GET['page']) : 1;
        $pageSize = isset($_GET['pageSize']) ? (int)$_GET['pageSize'] : 12;
        $photoId = isset($_GET['photoId']) ? trim((string)$_GET['photoId']) : '';
        $selectedPhotoId = $photoId !== '' ? $photoId : null;

        try {
            $albumPage = (new AlbumPageService())->load($path, $page, $pageSize, $selectedPhotoId);
        } catch (RuntimeException $exception) {
            AppLogger::exception('Portfolio AlbumController', $exception, $path);

            http_response_code(502);
            echo 'Errore nel recupero dei dati dell\'album.';
            return;
        }

        if ($albumPage === null) {
            http_response_code(404);
            echo 'Album non trovato: ' . htmlspecialchars($path);
            return;
        }

        $view = __DIR__ . '/../Views/Album/index.php';
        require __DIR__ . '/../Views/Layout/main.php';
    }
}
