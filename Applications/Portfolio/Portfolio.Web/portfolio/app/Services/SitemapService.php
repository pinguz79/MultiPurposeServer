<?php

declare(strict_types=1);

require_once __DIR__ . '/AlbumService.php';
require_once __DIR__ . '/ArticleRepository.php';

class SitemapService {
    private AlbumService $albumService;
    private ArticleRepository $articleRepository;
    private array $visitedAlbumIds = [];
    private array $urls = [];

    public function __construct() {
        $this->albumService = new AlbumService();
        $this->articleRepository = new ArticleRepository();
    }

    public function getUrls(): array {
        $this->visitedAlbumIds = [];
        $this->urls = [
            rtrim(PUBLIC_BASE_URL, '/') . '/',
            rtrim(PUBLIC_BASE_URL, '/') . '/servizi-fotografici',
            rtrim(PUBLIC_BASE_URL, '/') . '/chi-sono',
            rtrim(PUBLIC_BASE_URL, '/') . '/stories'
        ];

        foreach ($this->articleRepository->getPublished() as $article) {
            $this->urls[] = $article->url();
        }

        $rootAlbums = $this->albumService->getRootAlbums();
        if ($rootAlbums === null) {
            throw new RuntimeException('Unable to retrieve root albums for sitemap generation.');
        }

        $this->appendAlbums($rootAlbums);

        return array_values(array_unique($this->urls));
    }

    private function appendAlbums(array $albums): void {
        foreach ($albums as $album) {
            $albumId = trim((string)($album['id'] ?? ''));
            $fullPath = trim(str_replace('\\', '/', (string)($album['fullPath'] ?? '')), '/');

            if ($albumId === '' || $fullPath === '' || isset($this->visitedAlbumIds[$albumId])) {
                continue;
            }

            $this->visitedAlbumIds[$albumId] = true;
            $this->urls[] = rtrim(PUBLIC_BASE_URL, '/') . '/' . $this->encodePath($fullPath);

            $children = $this->albumService->getAlbumsByParentId($albumId);
            if ($children === null) {
                throw new RuntimeException(sprintf('Unable to retrieve child albums for sitemap node %s.', $albumId));
            }

            $this->appendAlbums($children);
        }
    }

    private function encodePath(string $path): string {
        return implode('/', array_map('rawurlencode', array_filter(explode('/', $path))));
    }
}
