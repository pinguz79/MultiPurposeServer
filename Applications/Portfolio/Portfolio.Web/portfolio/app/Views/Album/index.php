<?php

declare(strict_types=1);

$albumName = $currentAlbum['name'] ?? 'Album';
$albumDescription = $currentAlbum['description'] ?? '';
$albumPath = $currentAlbum['path'] ?? '';

$albums = isset($albums) && is_array($albums) ? $albums : [];
$photoPage = isset($photoPage) && is_array($photoPage) ? $photoPage : null;
$breadcrumbs = isset($breadcrumbs) && is_array($breadcrumbs) ? $breadcrumbs : [];

$photos = isset($photoPage['items']) && is_array($photoPage['items']) ? $photoPage['items'] : [];
$currentPage = (int)($photoPage['page'] ?? 1);
$pageSize = (int)($photoPage['pageSize'] ?? 12);
$totalItems = (int)($photoPage['totalItems'] ?? 0);
$totalPages = (int)($photoPage['totalPages'] ?? 0);

$selectedPhoto = null;

if (!empty($photos)) {
    foreach ($photos as $photo) {
        if (($photo['id'] ?? null) === $selectedPhotoId) {
            $selectedPhoto = $photo;
            break;
        }
    }

    $selectedPhoto ??= $photos[0];
    $selectedPhotoId = $selectedPhoto['id'] ?? null;
}

$albumPathSegments = array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== '');
$encodedAlbumPath = implode('/', array_map('rawurlencode', $albumPathSegments));
$albumUrl = BASE_PATH . '/' . $encodedAlbumPath;

$buildPageUrl = static function(int $page, int $pageSize, ?string $photoId = null) use ($albumUrl): string
{
    $query = ['page' => max(1, $page), 'pageSize' => $pageSize];

    if (!empty($photoId)) {
        $query['photoId'] = $photoId;
    }

    return $albumUrl . '?' . http_build_query($query);
};

$albumShareTitle = $albumName;
$albumShareText = $albumDescription !== '' ? $albumDescription : 'Guarda questo album fotografico di Marco Lepri Photography.';
$albumShareUrl = $albumUrl;
?>

<section class="album-header">
    <?php require __DIR__ . '/../Components/breadcrumb.php'; ?>

    <h2><?= htmlspecialchars($albumName) ?></h2>

    <?php if ($albumDescription !== ''): ?>
        <p class="album-description"><?= nl2br(htmlspecialchars($albumDescription)) ?></p>
    <?php endif; ?>

    <?php require __DIR__ . '/../Components/album-share.php'; ?>
</section>

<?php if (!empty($albums)): ?>
    <?php
    $albumGridTitle = 'Album';
    require __DIR__ . '/../Components/album-grid.php';
    ?>
<?php elseif (empty($photos)): ?>
    <p class="empty-state">Questo album non contiene fotografie.</p>
<?php else: ?>
    <?php require __DIR__ . '/../Components/photo-browser.php'; ?>
<?php endif; ?>
