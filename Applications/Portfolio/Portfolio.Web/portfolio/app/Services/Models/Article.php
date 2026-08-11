<?php

declare(strict_types=1);

final class Article
{
    public function __construct(
        public readonly string $slug,
        public readonly string $status,
        public readonly string $publishedAt,
        public readonly string $title,
        public readonly string $subtitle,
        public readonly string $description,
        public readonly string $coverImageUrl,
        public readonly string $coverImageAlt,
        public readonly string $contentRating,
        public readonly array $sections,
        public readonly array $relatedAlbums,
        public readonly ?string $relatedAlbumPath = null,
        public readonly ?string $heroImageUrl = null
    ) {
    }

    public static function fromArray(array $data, string $source): self
    {
        $requiredStrings = [
            'slug',
            'status',
            'publishedAt',
            'title',
            'subtitle',
            'description',
            'coverImageUrl',
            'coverImageAlt',
            'contentRating'
        ];

        foreach ($requiredStrings as $field) {
            if (!isset($data[$field]) || !is_string($data[$field]) || trim($data[$field]) === '') {
                throw new RuntimeException(sprintf('Article field "%s" is missing or empty in %s.', $field, $source));
            }
        }

        $slug = trim($data['slug']);
        if (preg_match('/^[a-z0-9]+(?:-[a-z0-9]+)*$/', $slug) !== 1) {
            throw new RuntimeException(sprintf('Article slug "%s" is invalid in %s.', $slug, $source));
        }

        if (!in_array($data['status'], ['draft', 'published'], true)) {
            throw new RuntimeException(sprintf('Article status "%s" is invalid in %s.', $data['status'], $source));
        }

        if (!in_array($data['contentRating'], ['Standard', 'Restricted'], true)) {
            throw new RuntimeException(sprintf('Article content rating "%s" is invalid in %s.', $data['contentRating'], $source));
        }

        if (DateTimeImmutable::createFromFormat('!Y-m-d', $data['publishedAt']) === false) {
            throw new RuntimeException(sprintf('Article publication date "%s" is invalid in %s.', $data['publishedAt'], $source));
        }

        $sections = $data['sections'] ?? null;
        if (!is_array($sections) || $sections === []) {
            throw new RuntimeException(sprintf('Article sections are missing or empty in %s.', $source));
        }

        foreach ($sections as $index => $section) {
            if (!is_array($section)
                || !isset($section['heading'], $section['paragraphs'])
                || !is_string($section['heading'])
                || trim($section['heading']) === ''
                || !is_array($section['paragraphs'])
                || $section['paragraphs'] === []) {
                throw new RuntimeException(sprintf('Article section %d is invalid in %s.', $index, $source));
            }

            foreach ($section['paragraphs'] as $paragraph) {
                if (!is_string($paragraph) || trim($paragraph) === '') {
                    throw new RuntimeException(sprintf('Article section %d contains an invalid paragraph in %s.', $index, $source));
                }
            }
        }

        $relatedAlbums = $data['relatedAlbums'] ?? [];
        if (!is_array($relatedAlbums)) {
            throw new RuntimeException(sprintf('Article related albums are invalid in %s.', $source));
        }

        foreach ($relatedAlbums as $index => $album) {
            if (!is_array($album)
                || !isset($album['label'], $album['path'])
                || !is_string($album['label'])
                || trim($album['label']) === ''
                || !is_string($album['path'])
                || trim($album['path'], '/') === '') {
                throw new RuntimeException(sprintf('Article related album %d is invalid in %s.', $index, $source));
            }
        }

        $relatedAlbumPath = isset($data['relatedAlbumPath']) && is_string($data['relatedAlbumPath'])
            ? trim($data['relatedAlbumPath'], '/')
            : null;

        $heroImageUrl = isset($data['heroImageUrl']) && is_string($data['heroImageUrl'])
            ? trim($data['heroImageUrl'])
            : null;

        return new self(
            slug: $slug,
            status: $data['status'],
            publishedAt: $data['publishedAt'],
            title: trim($data['title']),
            subtitle: trim($data['subtitle']),
            description: trim($data['description']),
            coverImageUrl: trim($data['coverImageUrl']),
            coverImageAlt: trim($data['coverImageAlt']),
            contentRating: $data['contentRating'],
            sections: $sections,
            relatedAlbums: $relatedAlbums,
            relatedAlbumPath: $relatedAlbumPath !== '' ? $relatedAlbumPath : null,
            heroImageUrl: $heroImageUrl !== '' ? $heroImageUrl : null
        );
    }

    public function isPublished(?DateTimeImmutable $now = null): bool
    {
        $today = ($now ?? new DateTimeImmutable('now'))->setTime(0, 0);
        $publicationDate = DateTimeImmutable::createFromFormat('!Y-m-d', $this->publishedAt);

        return $this->status === 'published' && $publicationDate !== false && $publicationDate <= $today;
    }

    public function url(): string
    {
        return rtrim(PUBLIC_BASE_URL, '/') . '/stories/' . rawurlencode($this->slug);
    }

    public function formattedPublishedDate(): string
    {
        $date = DateTimeImmutable::createFromFormat('!Y-m-d', $this->publishedAt);
        if ($date === false) {
            return $this->publishedAt;
        }

        $months = [
            1 => 'gennaio',
            2 => 'febbraio',
            3 => 'marzo',
            4 => 'aprile',
            5 => 'maggio',
            6 => 'giugno',
            7 => 'luglio',
            8 => 'agosto',
            9 => 'settembre',
            10 => 'ottobre',
            11 => 'novembre',
            12 => 'dicembre'
        ];

        return sprintf('%d %s %d', (int)$date->format('j'), $months[(int)$date->format('n')], (int)$date->format('Y'));
    }
}
