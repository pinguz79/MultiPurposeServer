<?php

declare(strict_types=1);

require_once __DIR__ . '/../Http/ApiClient.php';

class AlbumService
{
    private const CACHE_TTL = 864000; // 10 giorni

    public function getRootAlbums(): ?array
    {
        return ApiClient::get('/FrontEnd/Home/Albums', self::CACHE_TTL);
    }

    public function resolveAlbumPath(string $path): ?array
    {
        return ApiClient::get('/FrontEnd/Routing/Album?path=' . rawurlencode($path), self::CACHE_TTL);
    }

    public function getAlbumsByParentId(string $albumId): ?array
    {
        return ApiClient::get('/FrontEnd/Home/Albums?id=' . rawurlencode($albumId), self::CACHE_TTL);
    }

    public function getPhotosByAlbumId(string $albumId, int $page = 1, int $pageSize = 12): ?array
    {
        $page = max(1, $page);
        $pageSize = in_array($pageSize, [12, 24, 48], true) ? $pageSize : 12;

        $url = sprintf('/FrontEnd/Home/Album/%s/Photos?page=%d&pageSize=%d', rawurlencode($albumId), $page, $pageSize);

        return ApiClient::get($url, self::CACHE_TTL);
    }
}
