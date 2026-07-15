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

/*
 * Costruzione breadcrumb dal full path corrente.
 *
 * Esempio:
 * Calendari/2019/Sampdoria-2019
 *
 * diventa:
 * Gallerie > Calendari > 2019 > Sampdoria-2019
 */
$breadcrumbs = [];
$currentBreadcrumbPath = '';

if ($albumPath !== '') {
    $segments = array_values(array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== ''));

    foreach ($segments as $segment) {
        $currentBreadcrumbPath = $currentBreadcrumbPath === '' ? $segment : $currentBreadcrumbPath . '/' . $segment;
        $encodedBreadcrumbPath = implode('/', array_map('rawurlencode', explode('/', $currentBreadcrumbPath)));

        $breadcrumbs[] = [
            'name' => $segment,
            'url' => BASE_PATH . '/' . $encodedBreadcrumbPath
        ];
    }
}

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
    <nav class="breadcrumb" aria-label="Percorso di navigazione">
        <a href="<?= htmlspecialchars(BASE_PATH . '/') ?>">← Tutte le gallerie</a>

        <?php foreach ($breadcrumbs as $index => $breadcrumb): ?>
            <?php $isCurrent = $index === array_key_last($breadcrumbs); ?>

            <span class="breadcrumb-separator" aria-hidden="true">›</span>

            <?php if ($isCurrent): ?>
                <span class="breadcrumb-current" aria-current="page">
                    <?= htmlspecialchars($breadcrumb['name']) ?>
                </span>
            <?php else: ?>
                <a href="<?= htmlspecialchars($breadcrumb['url']) ?>">
                    <?= htmlspecialchars($breadcrumb['name']) ?>
                </a>
            <?php endif; ?>
        <?php endforeach; ?>
    </nav>

    <h2><?= htmlspecialchars($albumName) ?></h2>

    <?php if ($albumPath !== ''): ?>
        <div class="album-path">
            <?= htmlspecialchars($albumPath) ?>
        </div>
    <?php endif; ?>
</section>

<?php if (!empty($albums)): ?>
    <section class="album-children">
        <h3 class="section-title">Album</h3>

        <div class="grid">
            <?php foreach ($albums as $album): ?>
                <?php
                $cover = $album['coverImage'] ?? null;

                $coverUrl = $cover['thumbUrl'] ?? (BASE_PATH . '/public/img/album-placeholder.png');
                $coverAlt = $cover['alt'] ?? ($album['name'] ?? 'Fotografia');

                $albumFullPath = trim(str_replace('\\', '/', $album['fullPath'] ?? $album['path'] ?? ''), '/');
                $encodedChildAlbumPath = implode('/', array_map('rawurlencode', array_filter(explode('/', $albumFullPath), static fn(string $segment): bool => $segment !== '')));

                $childAlbumUrl = BASE_PATH . '/' . $encodedChildAlbumPath;

                $childrenCounter = $album['childrenCounter'] ?? $album['children'] ?? 0;
                $photosCounter = $album['photosCounter'] ?? $album['photos'] ?? 0;
                ?>

                <a class="card-link" href="<?= htmlspecialchars($childAlbumUrl) ?>">
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
            <?php endforeach; ?>
        </div>
    </section>

<?php elseif (empty($photos)): ?>
    <p class="empty-state">
        Questo album non contiene fotografie.
    </p>

<?php else: ?>
    <section class="photo-browser">
        <div class="photo-browser-content">
            <div class="photo-grid-pane">
                <div class="photo-grid">
                    <?php foreach ($photos as $photo): ?>
                        <?php
                        $photoId = $photo['id'] ?? '';
                        $photoName = $photo['name'] ?? 'Fotografia';
                        $photoAlt = $photo['alt'] ?? $photoName;
                        $thumbnailUrl = $photo['thumbnailUrl'] ?? '';
                        $imageUrl = $photo['imageUrl'] ?? '';
                        $isSelected = $photoId === $selectedPhotoId;
                        $photoUrl = $buildPageUrl($currentPage, $pageSize, $photoId);
                        ?>

                        <a
                            class="photo-thumbnail<?= $isSelected ? ' selected' : '' ?>"
                            href="<?= htmlspecialchars($photoUrl) ?>"
                            data-photo-id="<?= htmlspecialchars($photoId) ?>"
                            data-photo-name="<?= htmlspecialchars($photoName) ?>"
                            data-photo-alt="<?= htmlspecialchars($photoAlt) ?>"
                            data-image-url="<?= htmlspecialchars($imageUrl) ?>"
                            aria-current="<?= $isSelected ? 'true' : 'false' ?>"
                        >
                            <span class="photo-thumbnail-image">
                                <img src="<?= htmlspecialchars($thumbnailUrl) ?>" alt="<?= htmlspecialchars($photoAlt) ?>" loading="lazy">
                            </span>

                            <span class="photo-thumbnail-title">
                                <?= htmlspecialchars($photoName) ?>
                            </span>
                        </a>
                    <?php endforeach; ?>
                </div>
            </div>

            <aside class="photo-preview-pane">
                <div class="photo-preview-frame">
                    <img
                        id="photo-preview-image"
                        src="<?= htmlspecialchars($selectedPhoto['imageUrl'] ?? '') ?>"
                        alt="<?= htmlspecialchars($selectedPhoto['alt'] ?? $selectedPhoto['name'] ?? 'Fotografia') ?>"
                    >
                </div>

                <div id="photo-preview-title" class="photo-preview-title">
                    <?= htmlspecialchars($selectedPhoto['name'] ?? 'Fotografia') ?>
                </div>

                <div class="photo-preview-navigation">
                    <button id="previous-photo" type="button" class="photo-navigation-button">
                        ← Precedente
                    </button>

                    <button id="next-photo" type="button" class="photo-navigation-button">
                        Successiva →
                    </button>
                </div>
            </aside>
        </div>

        <footer class="photo-pagination">
            <div class="photo-pagination-summary">
                Pagina <?= $currentPage ?> di <?= max(1, $totalPages) ?>
                ·
                <?= $totalItems ?> fotografie
            </div>

            <nav class="photo-pagination-pages" aria-label="Paginazione fotografie">
                <?php if ($currentPage > 1): ?>
                    <a href="<?= htmlspecialchars($buildPageUrl($currentPage - 1, $pageSize)) ?>">‹</a>
                <?php else: ?>
                    <span class="disabled">‹</span>
                <?php endif; ?>

                <?php for ($pageNumber = 1; $pageNumber <= $totalPages; $pageNumber++): ?>
                    <?php if ($pageNumber === $currentPage): ?>
                        <span class="current" aria-current="page"><?= $pageNumber ?></span>
                    <?php else: ?>
                        <a href="<?= htmlspecialchars($buildPageUrl($pageNumber, $pageSize)) ?>"><?= $pageNumber ?></a>
                    <?php endif; ?>
                <?php endfor; ?>

                <?php if ($currentPage < $totalPages): ?>
                    <a href="<?= htmlspecialchars($buildPageUrl($currentPage + 1, $pageSize)) ?>">›</a>
                <?php else: ?>
                    <span class="disabled">›</span>
                <?php endif; ?>
            </nav>

            <form class="photo-page-size" method="get">
                <label for="pageSize">Foto per pagina</label>

                <select id="pageSize" name="pageSize" onchange="this.form.submit()">
                    <?php foreach ([12, 24, 48] as $availablePageSize): ?>
                        <option value="<?= $availablePageSize ?>"<?= $availablePageSize === $pageSize ? ' selected' : '' ?>>
                            <?= $availablePageSize ?>
                        </option>
                    <?php endforeach; ?>
                </select>

                <input type="hidden" name="page" value="1">
            </form>
        </footer>

        <div
            id="photo-browser-data"
            data-current-page="<?= $currentPage ?>"
            data-total-pages="<?= $totalPages ?>"
            data-page-size="<?= $pageSize ?>"
            data-album-url="<?= htmlspecialchars($albumUrl) ?>"
            hidden
        ></div>
    </section>

    <script src="<?= BASE_PATH ?>/public/js/photo-browser.js"></script>
<?php endif; ?>
