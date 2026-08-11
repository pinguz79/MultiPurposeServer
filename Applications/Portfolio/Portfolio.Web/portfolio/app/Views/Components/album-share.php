<?php

declare(strict_types=1);

$currentAlbum = $albumPage->currentAlbum;

$albumShareTitle = $currentAlbum['name'] ?? 'Album fotografico';
$albumShareText = $pageMetadata->description;
$albumUrl = $pageMetadata->canonicalUrl;
?>

<div class="share"
     data-share-title="<?= htmlspecialchars($albumShareTitle) ?>"
     data-share-text="<?= htmlspecialchars($albumShareText) ?>"
     data-share-url="<?= htmlspecialchars($albumUrl) ?>">
    <button class="share-button" type="button" data-share-action="toggle" aria-expanded="false">Condividi album</button>

    <div class="share-menu" data-share-menu hidden>
        <button type="button" data-share-action="native" hidden>Condividi con un'app…</button>
        <p class="share-native-hint" data-share-native-hint hidden>Per Instagram scegli questa opzione oppure copia il link.</p>
        <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="facebook">Facebook</a>
        <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="whatsapp">WhatsApp</a>
        <button type="button" data-share-action="copy">Copia link</button>
    </div>

    <div class="share-feedback" data-share-feedback role="status" aria-live="polite"></div>
</div>

<script src="<?= BASE_PATH ?>/public/js/share.js?v=<?= filemtime(__DIR__ . '/../../../public/js/share.js') ?>"></script>
