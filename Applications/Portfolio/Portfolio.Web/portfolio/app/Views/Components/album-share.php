<?php

declare(strict_types=1);

$currentAlbum = $albumPage->currentAlbum;

$albumName = $currentAlbum['name'] ?? 'Album fotografico';
$albumDescription = $currentAlbum['description'] ?? '';
$albumPath = trim(str_replace('\\', '/', $currentAlbum['path'] ?? ''), '/');

$albumPathSegments = array_filter(
    explode('/', $albumPath),
    static fn(string $segment): bool => $segment !== ''
);

$encodedAlbumPath = implode('/', array_map('rawurlencode', $albumPathSegments));
$albumUrl = BASE_PATH . '/' . $encodedAlbumPath;

$albumShareTitle = $albumName;
$albumShareText = $albumDescription !== ''
    ? $albumDescription
    : 'Guarda questo album fotografico di Marco Lepri Photography.';
?>

<div class="share"
     data-share-title="<?= htmlspecialchars($albumShareTitle) ?>"
     data-share-text="<?= htmlspecialchars($albumShareText) ?>"
     data-share-url="<?= htmlspecialchars($albumUrl) ?>">
    <button class="share-button" type="button" data-share-action="toggle" aria-expanded="false">Condividi album</button>

    <div class="share-menu" data-share-menu hidden>
        <button type="button" data-share-action="native" hidden>Condividi…</button>
        <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="facebook">Facebook</a>
        <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="whatsapp">WhatsApp</a>
        <button type="button" data-share-action="copy">Copia link</button>
    </div>

    <div class="share-feedback" data-share-feedback role="status" aria-live="polite"></div>
</div>

<script src="<?= BASE_PATH ?>/public/js/share.js"></script>
