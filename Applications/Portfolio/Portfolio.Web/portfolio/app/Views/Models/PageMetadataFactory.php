<?php

declare(strict_types=1);

require_once __DIR__ . '/PageMetadata.php';
require_once __DIR__ . '/../../Services/Models/AlbumPage.php';

class PageMetadataFactory
{
    private const SITE_NAME = 'Marco Lepri Photography';

    public static function home(): PageMetadata
    {
        return new PageMetadata(
            title: self::SITE_NAME,
            socialTitle: self::SITE_NAME,
            description: 'Gallerie fotografiche, calendari, shooting glamour, ritratti, sfilate e progetti editoriali.',
            canonicalUrl: PUBLIC_BASE_URL . '/'
        );
    }

    public static function album(AlbumPage $albumPage): PageMetadata
    {
        $albumName = trim((string)($albumPage->currentAlbum['name'] ?? 'Album fotografico'));
        $albumDescription = trim((string)($albumPage->currentAlbum['description'] ?? ''));
        $albumPath = trim(str_replace('\\', '/', (string)($albumPage->currentAlbum['path'] ?? '')), '/');
        $encodedPath = implode('/', array_map('rawurlencode', array_filter(explode('/', $albumPath))));

        return new PageMetadata(
            title: sprintf('%s | %s', $albumName, self::SITE_NAME),
            socialTitle: $albumName,
            description: $albumDescription !== ''
                ? $albumDescription
                : sprintf('Guarda l\'album %s di %s.', $albumName, self::SITE_NAME),
            canonicalUrl: PUBLIC_BASE_URL . '/' . $encodedPath,
            imageUrl: self::findAlbumImage($albumPage)
        );
    }

    private static function findAlbumImage(AlbumPage $albumPage): ?string
    {
        $photos = isset($albumPage->photoPage['items']) && is_array($albumPage->photoPage['items'])
            ? $albumPage->photoPage['items']
            : [];

        $imageUrl = $photos[0]['imageUrl'] ?? null;

        if (!is_string($imageUrl) || trim($imageUrl) === '') {
            $imageUrl = $albumPage->albums[0]['coverImage']['thumbUrl'] ?? null;
        }

        return is_string($imageUrl) && trim($imageUrl) !== ''
            ? self::absoluteUrl($imageUrl)
            : null;
    }

    private static function absoluteUrl(string $url): string
    {
        if (filter_var($url, FILTER_VALIDATE_URL) !== false) {
            return $url;
        }

        return rtrim(PUBLIC_BASE_URL, '/') . '/' . ltrim($url, '/');
    }
}
