<?php

declare(strict_types=1);

$currentAlbum = $albumPage->currentAlbum;

$albumName = $currentAlbum['name'] ?? 'Album';
$albumDescription = $currentAlbum['description'] ?? '';
$albumKind = $currentAlbum['kind'] ?? null;


$photos = isset($albumPage->photoPage['items']) && is_array($albumPage->photoPage['items'])
    ? $albumPage->photoPage['items']
    : [];

$showsAlbumGrid = match ($albumKind) {
    'Gallery', 'Collection' => true,
    'PhotoAlbum' => false,
};
?>

<section class="album-header">
    <?php require __DIR__ . '/../Components/breadcrumb.php'; ?>

    <h2><?= htmlspecialchars($albumName) ?></h2>

    <?php if ($albumDescription !== ''): ?>
        <p class="album-description"><?= nl2br(htmlspecialchars($albumDescription)) ?></p>
    <?php endif; ?>

    <?php require __DIR__ . '/../Components/album-share.php'; ?>
</section>

<?php if ($showsAlbumGrid): ?>
    <?php if (empty($albumPage->albums)): ?>
        <p class="empty-state">Questa raccolta non contiene album.</p>
    <?php else: ?>
        <?php
        $albums = $albumPage->albums;
        $albumGridTitle = 'Album';

        require __DIR__ . '/../Components/album-grid.php';
        ?>
    <?php endif; ?>
<?php elseif (empty($photos)): ?>
    <p class="empty-state">Questo album non contiene fotografie.</p>
<?php else: ?>
    <?php require __DIR__ . '/../Components/photo-browser.php'; ?>
<?php endif; ?>
