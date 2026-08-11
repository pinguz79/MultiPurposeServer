<?php

declare(strict_types=1);

class PageMetadata
{
    public function __construct(
        public readonly string $title,
        public readonly string $socialTitle,
        public readonly string $description,
        public readonly string $canonicalUrl,
        public readonly ?string $imageUrl = null,
        public readonly string $openGraphType = 'website',
        public readonly ?string $publishedAt = null
    ) {
    }
}
