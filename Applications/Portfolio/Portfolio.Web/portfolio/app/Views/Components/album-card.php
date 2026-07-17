<?php
$cover = $album['coverImage'] ?? null;

$coverUrl = $cover['thumbUrl'] ?? (BASE_PATH . '/public/img/album-placeholder.png');
$coverAlt = $cover['alt'] ?? ($album['name'] ?? 'Fotografia');

$albumPath = trim(str_replace('\\', '/', $album['fullPath'] ?? $album['path'] ?? ''), '/');
$encodedAlbumPath = implode('/', array_map('rawurlencode', array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== '')));
$albumUrl = BASE_PATH . '/' . $encodedAlbumPath;

$childrenCounter = $album['childrenCounter'] ?? $album['children'] ?? 0;
$photosCounter = $album['photosCounter'] ?? $album['photos'] ?? 0;
?>

<a class="card-link" href="<?= htmlspecialchars($albumUrl) ?>">
    <div class="card">
        <div class="cover">
            <img src="<?= htmlspecialchars($coverUrl) ?>" alt="<?= htmlspecialchars($coverAlt) ?>" loading="lazy">
        </div>

        <div class="title">
            <?= htmlspecialchars($album['name'] ?? 'Album senza nome') ?>
        </div>

        <div class="meta">
            <?= (int)$childrenCounter ?> sub-album
            •
            <?= (int)$photosCounter ?> foto
        </div>
    </div>
</a>