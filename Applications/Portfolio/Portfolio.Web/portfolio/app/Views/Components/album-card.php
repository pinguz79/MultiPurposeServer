<?php

declare(strict_types=1);

$cover = $album['coverImage'] ?? null;

$coverUrl = $cover['thumbUrl'] ?? (BASE_PATH . '/public/img/album-placeholder.png');
$coverAlt = $cover['alt'] ?? ($album['name'] ?? 'Copertina album');

$albumPath = trim(str_replace('\\', '/', $album['fullPath'] ?? ''), '/');
$albumPathSegments = array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== '');
$encodedAlbumPath = implode('/', array_map('rawurlencode', $albumPathSegments));
$albumUrl = BASE_PATH . '/' . $encodedAlbumPath;

$childrenCounter = (int)($album['childrenCounter'] ?? $album['children'] ?? 0);
$photosCounter = (int)($album['photosCounter'] ?? $album['photos'] ?? 0);

$childrenLabel = $childrenCounter === 1 ? 'sub-album' : 'sub-album';
$photosLabel = $photosCounter === 1 ? 'foto' : 'foto';
?>

<a class="card-link" href="<?= htmlspecialchars($albumUrl) ?>">
    <div class="card">
        <div class="cover">
            <img src="<?= htmlspecialchars($coverUrl) ?>" alt="<?= htmlspecialchars($coverAlt) ?>" loading="lazy">
        </div>

        <div class="title"><?= htmlspecialchars($album['name'] ?? 'Album senza nome') ?></div>

        <div class="meta">
            <?= $childrenCounter ?> <?= $childrenLabel ?> • <?= $photosCounter ?> <?= $photosLabel ?>
        </div>
    </div>
</a>
