<?php
$albumName = $currentAlbum['name'] ?? 'Album';
$albumPath = $currentAlbum['path'] ?? '';

$albums = isset($albums) && is_array($albums) ? $albums : [];
$photoPage = isset($photoPage) && is_array($photoPage) ? $photoPage : null;

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

$breadcrumbs = isset($breadcrumbs) && is_array($breadcrumbs) ? $breadcrumbs : [];

$encodedAlbumPath = implode('/', array_map('rawurlencode', array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== '')));
$albumUrl = BASE_PATH . '/' . $encodedAlbumPath;

$buildPageUrl = static function(int $page, int $pageSize, ?string $photoId = null) use ($albumUrl): string {
    $query = [
        'page' => max(1, $page),
        'pageSize' => $pageSize
    ];

    if (!empty($photoId)) {
        $query['photoId'] = $photoId;
    }

    return $albumUrl . '?' . http_build_query($query);
};
?>

<section class="album-header">
    <?php require __DIR__ . '/../Components/breadcrumb.php'; ?>

    <h2><?= htmlspecialchars($albumName) ?></h2>
</section>

<?php if (!empty($albums)): ?>
    <?php
    $albumGridTitle = 'Album';
    require __DIR__ . '/../Components/album-grid.php';
    ?>

<?php elseif (empty($photos)): ?>
    <p class="empty-state">
        Questo album non contiene fotografie.
    </p>

<?php else: ?>
    <?php require __DIR__ . '/../Components/photo-browser.php'; ?>
<?php endif; ?>