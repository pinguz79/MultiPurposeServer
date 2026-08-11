<?php

declare(strict_types=1);

require_once __DIR__ . '/PageMetadata.php';
require_once __DIR__ . '/../../Services/Models/AlbumPage.php';
require_once __DIR__ . '/../../Services/Models/Article.php';

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

    public static function about(): PageMetadata
    {
        return new PageMetadata(
            title: 'Chi sono | ' . self::SITE_NAME,
            socialTitle: 'Chi sono — Marco Lepri Photography',
            description: 'Intervista a Marco Lepri, fotografo freelance di Genova specializzato in fashion, glamour, ritratto, beauty ed editoriale.',
            canonicalUrl: PUBLIC_BASE_URL . '/chi-sono',
            imageUrl: PUBLIC_BASE_URL . '/public/img/marco-lepri-profile.png'
        );
    }

    public static function services(): PageMetadata
    {
        return new PageMetadata(
            title: 'Servizi fotografici | ' . self::SITE_NAME,
            socialTitle: 'Servizi fotografici — Marco Lepri Photography',
            description: 'Shooting fashion, glamour, ritratto, beauty ed editoriale a Genova: dalla progettazione condivisa alla selezione e consegna delle fotografie.',
            canonicalUrl: PUBLIC_BASE_URL . '/servizi-fotografici'
        );
    }

    public static function stories(): PageMetadata
    {
        return new PageMetadata(
            title: 'Dietro le quinte | ' . self::SITE_NAME,
            socialTitle: 'Dietro le quinte — Marco Lepri Photography',
            description: 'Storie, preparazione e retroscena dei progetti fotografici di Marco Lepri Photography.',
            canonicalUrl: PUBLIC_BASE_URL . '/stories'
        );
    }

    public static function article(Article $article): PageMetadata
    {
        return new PageMetadata(
            title: $article->title . ' | ' . self::SITE_NAME,
            socialTitle: $article->title,
            description: $article->description,
            canonicalUrl: $article->url(),
            imageUrl: $article->heroImageUrl ?? $article->coverImageUrl,
            openGraphType: 'article',
            publishedAt: $article->publishedAt
        );
    }

    public static function album(AlbumPage $albumPage): PageMetadata
    {
        $albumName = trim((string)($albumPage->currentAlbum['name'] ?? 'Album fotografico'));
        $albumDescription = trim((string)($albumPage->currentAlbum['description'] ?? ''));
        $albumPath = trim(str_replace('\\', '/', (string)($albumPage->currentAlbum['path'] ?? '')), '/');
        $encodedPath = implode('/', array_map('rawurlencode', array_filter(explode('/', $albumPath))));
        $parentAlbumName = self::findParentAlbumName($albumPage);
        $contextualAlbumName = $parentAlbumName !== null
            ? sprintf('%s — %s', $albumName, $parentAlbumName)
            : $albumName;

        return new PageMetadata(
            title: sprintf('%s | %s', $contextualAlbumName, self::SITE_NAME),
            socialTitle: $contextualAlbumName,
            description: $albumDescription !== ''
                ? $albumDescription
                : self::buildAlbumDescription($albumName, $parentAlbumName),
            canonicalUrl: PUBLIC_BASE_URL . '/' . $encodedPath,
            imageUrl: self::findAlbumImage($albumPage)
        );
    }

    private static function findParentAlbumName(AlbumPage $albumPage): ?string
    {
        if (count($albumPage->breadcrumbs) < 2) {
            return null;
        }

        $parentBreadcrumb = $albumPage->breadcrumbs[array_key_last($albumPage->breadcrumbs) - 1] ?? null;
        $parentName = trim((string)($parentBreadcrumb['name'] ?? ''));

        return $parentName !== '' ? $parentName : null;
    }

    private static function buildAlbumDescription(string $albumName, ?string $parentAlbumName): string
    {
        if ($parentAlbumName === null) {
            return sprintf('Guarda l\'album %s di %s.', $albumName, self::SITE_NAME);
        }

        return sprintf(
            'Guarda l\'album %s nella raccolta %s di %s.',
            $albumName,
            $parentAlbumName,
            self::SITE_NAME
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
