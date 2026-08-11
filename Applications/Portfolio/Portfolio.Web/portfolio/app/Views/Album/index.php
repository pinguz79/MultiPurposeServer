<?php

declare(strict_types=1);

$currentAlbum = $albumPage->currentAlbum;

$albumName = $currentAlbum['name'] ?? 'Album';
$albumDescription = $currentAlbum['description'] ?? '';
$albumKind = $currentAlbum['kind'] ?? null;
$contentRating = $currentAlbum['content_rating'] ?? 'Standard';
$showsAdvertisement = $contentRating === 'Standard';


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

    <h1><?= htmlspecialchars($albumName) ?></h1>

    <?php if ($albumDescription !== ''): ?>
        <p class="album-description"><?= nl2br(htmlspecialchars($albumDescription)) ?></p>
    <?php endif; ?>

    <?php require __DIR__ . '/../Components/album-share.php'; ?>
</section>

<?php if (!empty($relatedArticles)): ?>
    <aside class="album-related-story" aria-label="Storie collegate">
        <p><strong>Dietro le quinte:</strong> scopri come è nato questo progetto.</p>
        <?php foreach ($relatedArticles as $relatedArticle): ?>
            <a href="<?= htmlspecialchars(BASE_PATH . '/stories/' . rawurlencode($relatedArticle->slug)) ?>">
                <?= htmlspecialchars($relatedArticle->title) ?>
            </a>
        <?php endforeach; ?>
    </aside>
<?php endif; ?>

<?php if ($showsAlbumGrid): ?>
    <?php if (empty($albumPage->albums)): ?>
        <p class="empty-state">Questa raccolta non contiene album.</p>
    <?php else: ?>
        <?php
        if ($showsAdvertisement) {
            $advertisementContext = 'navigation';
            require __DIR__ . '/../Components/advertisement.php';
        }

        $albums = $albumPage->albums;
        $albumGridTitle = 'Album';

        require __DIR__ . '/../Components/album-grid.php';
        ?>
    <?php endif; ?>
<?php elseif (empty($photos)): ?>
    <p class="empty-state">Questo album non contiene fotografie.</p>
<?php else: ?>
    <div class="photo-album-layout">
        <?php
        if ($showsAdvertisement) {
            $advertisementContext = 'photo-album';
            require __DIR__ . '/../Components/advertisement.php';
        }
        require __DIR__ . '/../Components/photo-browser.php';
        ?>
    </div>
<?php endif; ?>
