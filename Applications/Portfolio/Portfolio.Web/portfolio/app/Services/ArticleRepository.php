<?php

declare(strict_types=1);

require_once __DIR__ . '/Models/Article.php';

final class ArticleRepository
{
    private string $articlesPath;
    private ?array $articles = null;

    public function __construct(?string $articlesPath = null)
    {
        $this->articlesPath = $articlesPath ?? __DIR__ . '/../../content/articles';
    }

    public function getPublished(): array
    {
        $articles = array_values(array_filter(
            $this->loadAll(),
            static fn(Article $article): bool => $article->isPublished()
        ));

        usort(
            $articles,
            static fn(Article $left, Article $right): int => strcmp($right->publishedAt, $left->publishedAt)
        );

        return $articles;
    }

    public function findPublishedBySlug(string $slug): ?Article
    {
        $normalizedSlug = trim($slug);

        foreach ($this->getPublished() as $article) {
            if ($article->slug === $normalizedSlug) {
                return $article;
            }
        }

        return null;
    }

    public function findPublishedByRelatedAlbumPath(string $albumPath): array
    {
        $normalizedPath = trim(str_replace('\\', '/', $albumPath), '/');

        return array_values(array_filter(
            $this->getPublished(),
            static fn(Article $article): bool => $article->relatedAlbumPath === $normalizedPath
        ));
    }

    private function loadAll(): array
    {
        if ($this->articles !== null) {
            return $this->articles;
        }

        if (!is_dir($this->articlesPath)) {
            throw new RuntimeException(sprintf('Article directory not found: %s.', $this->articlesPath));
        }

        $articles = [];
        foreach (glob($this->articlesPath . '/*.php') ?: [] as $file) {
            $data = require $file;
            if (!is_array($data)) {
                throw new RuntimeException(sprintf('Article file must return an array: %s.', $file));
            }

            $article = Article::fromArray($data, $file);
            if (isset($articles[$article->slug])) {
                throw new RuntimeException(sprintf('Duplicate article slug "%s".', $article->slug));
            }

            $articles[$article->slug] = $article;
        }

        $this->articles = $articles;

        return $this->articles;
    }
}
