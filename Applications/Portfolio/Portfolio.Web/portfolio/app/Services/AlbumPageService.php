<?php

declare(strict_types=1);

require_once __DIR__ . '/AlbumService.php';
require_once __DIR__ . '/RoutingCacheService.php';
require_once __DIR__ . '/Models/AlbumPage.php';

class AlbumPageService
{
    private const DEFAULT_PAGE_SIZE = 12;
    private const ALLOWED_PAGE_SIZES = [12, 24, 48];

    public function load(string $path, int $page = 1, int $pageSize = self::DEFAULT_PAGE_SIZE, ?string $selectedPhotoId = null): ?AlbumPage
    {
        $normalizedPath = $this->normalizePath($path);
        $routingCache = new RoutingCacheService();
        $albumService = new AlbumService();

        $albumId = $routingCache->getAlbumIdByPath($normalizedPath);

        if ($albumId === null) {
            $resolvedAlbum = $albumService->resolveAlbumPath($normalizedPath);

            if (!is_array($resolvedAlbum) || empty($resolvedAlbum['id']) || empty($resolvedAlbum['path'])) {
                return null;
            }

            $routingCache->upsertAlbum(
                $resolvedAlbum['path'],
                $resolvedAlbum['id'],
                $resolvedAlbum['name'] ?? null
            );

            $albumId = $resolvedAlbum['id'];
        }

        $albums = $albumService->getAlbumsByParentId($albumId);

        if (!is_array($albums)) {
            throw new RuntimeException('Unable to retrieve child albums.');
        }

        $routingCache->upsertAlbums($albums);

        $photoPage = null;

        if (empty($albums)) {
            $page = max(1, $page);
            $pageSize = in_array($pageSize, self::ALLOWED_PAGE_SIZES, true) ? $pageSize : self::DEFAULT_PAGE_SIZE;

            $photoPage = $albumService->getPhotosByAlbumId($albumId, $page, $pageSize);

            if (!is_array($photoPage)) {
                throw new RuntimeException('Unable to retrieve album photos.');
            }

            if ($selectedPhotoId === null) {
                $photos = isset($photoPage['items']) && is_array($photoPage['items']) ? $photoPage['items'] : [];
                $selectedPhotoId = $photos[0]['id'] ?? null;
            }
        }

        $currentAlbum = $routingCache->getAlbumByPath($normalizedPath) ?? [
            'id' => $albumId,
            'path' => $normalizedPath,
            'name' => basename($normalizedPath)
        ];

        return new AlbumPage(
            currentAlbum: $currentAlbum,
            breadcrumbs: $routingCache->getAlbumBreadcrumbs($normalizedPath),
            albums: $albums,
            photoPage: $photoPage,
            selectedPhotoId: $selectedPhotoId
        );
    }

    private function normalizePath(string $path): string
    {
        return trim(str_replace('\\', '/', $path), '/');
    }
}
