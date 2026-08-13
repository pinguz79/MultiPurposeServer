<?php

declare(strict_types=1);

class AlbumPage {
    public function __construct(
        public readonly array $currentAlbum,
        public readonly array $breadcrumbs,
        public readonly array $albums,
        public readonly ?array $photoPage,
        public readonly ?string $selectedPhotoId
    ) {
    }
}
