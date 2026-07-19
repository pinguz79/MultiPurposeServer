<?php

declare(strict_types=1);

require_once __DIR__ . '/../Services/AlbumService.php';
require_once __DIR__ . '/../Services/RoutingCacheService.php';

class HomeController
{
    public function index(): void
    {
        $albums = (new AlbumService())->getRootAlbums();

        if ($albums === null) {
            http_response_code(502);
            echo 'Errore nel recupero delle gallerie.';
            return;
        }

        (new RoutingCacheService())->upsertAlbums($albums);

        $pageTitle = 'Marco Lepri Photography';

        $view = __DIR__ . '/../Views/Home/index.php';
        require __DIR__ . '/../Views/Layout/main.php';
    }
}
