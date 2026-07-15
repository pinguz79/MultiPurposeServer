<?php

require_once __DIR__ . '/../Services/AlbumService.php';
require_once __DIR__ . '/../Services/RoutingCacheService.php';

class HomeController
{
    public function index()
    {
        $albumService = new AlbumService();
        $albums = $albumService->getRootAlbums();

        if (is_array($albums)) {
            $routingCache = new RoutingCacheService();
            $routingCache->upsertAlbums($albums);
        }

        $view = __DIR__ . '/../Views/Home/index.php';

        require __DIR__ . '/../Views/Layout/main.php';
    }
}