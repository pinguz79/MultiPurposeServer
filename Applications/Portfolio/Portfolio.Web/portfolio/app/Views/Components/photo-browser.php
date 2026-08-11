<?php

declare(strict_types=1);

$photoPage = $albumPage->photoPage ?? [];
$selectedPhotoId = $albumPage->selectedPhotoId;

$photos = isset($photoPage['items']) && is_array($photoPage['items']) ? $photoPage['items'] : [];

$currentPage = (int)($photoPage['page'] ?? 1);
$pageSize = (int)($photoPage['pageSize'] ?? 12);
$totalItems = (int)($photoPage['totalItems'] ?? 0);
$totalPages = (int)($photoPage['totalPages'] ?? 0);

$selectedPhotoIndex = 0;
$selectedPhoto = $photos[0] ?? null;

foreach ($photos as $index => $photo) {
    if (($photo['id'] ?? null) === $selectedPhotoId) {
        $selectedPhotoIndex = $index;
        $selectedPhoto = $photo;
        break;
    }
}

$selectedPhotoId = $selectedPhoto['id'] ?? null;
$selectedPhotoNumber = ($currentPage - 1) * $pageSize + $selectedPhotoIndex + 1;
$selectedPhotoName = $selectedPhoto['name'] ?? 'Fotografia';
$selectedPhotoAlt = $selectedPhoto['alt'] ?? $selectedPhotoName;
$selectedPhotoCode = trim((string)($selectedPhoto['selectionCode'] ?? ''));

$albumPath = trim(str_replace('\\', '/', $albumPage->currentAlbum['path'] ?? ''), '/');
$albumPathSegments = array_filter(explode('/', $albumPath), static fn(string $segment): bool => $segment !== '');
$encodedAlbumPath = implode('/', array_map('rawurlencode', $albumPathSegments));
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

$selectedPhotoUrl = $buildPageUrl($currentPage, $pageSize, $selectedPhotoId);
$photoShareText = 'Guarda questa fotografia di Marco Lepri Photography.';
?>

<section class="photo-browser">
    <div class="photo-browser-content">
        <div class="photo-grid-pane">
            <div class="photo-grid">
                <?php foreach ($photos as $photo): ?>
                    <?php
                    $photoId = $photo['id'] ?? '';
                    $photoName = $photo['name'] ?? 'Fotografia';
                    $photoAlt = $photo['alt'] ?? $photoName;
                    $photoCode = trim((string)($photo['selectionCode'] ?? ''));
                    $thumbnailUrl = $photo['thumbnailUrl'] ?? '';
                    $imageUrl = $photo['imageUrl'] ?? '';
                    $photoUrl = $buildPageUrl($currentPage, $pageSize, $photoId);
                    $isSelected = $photoId === $selectedPhotoId;
                    ?>

                    <a
                        class="photo-thumbnail<?= $isSelected ? ' selected' : '' ?>"
                        href="<?= htmlspecialchars($photoUrl) ?>"
                        data-photo-id="<?= htmlspecialchars($photoId) ?>"
                        data-photo-name="<?= htmlspecialchars($photoName) ?>"
                        data-photo-alt="<?= htmlspecialchars($photoAlt) ?>"
                        data-photo-code="<?= htmlspecialchars($photoCode) ?>"
                        data-image-url="<?= htmlspecialchars($imageUrl) ?>"
                        data-photo-url="<?= htmlspecialchars($photoUrl) ?>"
                        aria-current="<?= $isSelected ? 'true' : 'false' ?>"
                    >
                        <span class="photo-thumbnail-image">
                            <img
                                src="<?= htmlspecialchars($thumbnailUrl) ?>"
                                alt="<?= htmlspecialchars($photoAlt) ?>"
                                loading="lazy"
                            >
                        </span>

                        <span class="photo-thumbnail-title"><?= htmlspecialchars($photoName) ?></span>
                    </a>
                <?php endforeach; ?>
            </div>
        </div>

        <aside class="photo-preview-pane">
            <div class="photo-preview-frame">
                <img
                    id="photo-preview-image"
                    src="<?= htmlspecialchars($selectedPhoto['imageUrl'] ?? '') ?>"
                    alt="<?= htmlspecialchars($selectedPhotoAlt) ?>"
                >
            </div>

            <div id="photo-preview-title" class="photo-preview-title">
                <?= htmlspecialchars($selectedPhotoName) ?>
            </div>

            <div id="photo-preview-counter" class="photo-preview-counter">
                <?php if ($selectedPhotoCode !== ''): ?>
                    <span class="photo-preview-code">Codice #<?= htmlspecialchars($selectedPhotoCode) ?></span>
                    <span class="photo-preview-separator" aria-hidden="true">·</span>
                <?php endif; ?>
                <span class="photo-preview-position">Foto <?= $selectedPhotoNumber ?> di <?= $totalItems ?></span>
            </div>

            <div
                class="share"
                data-share-title="<?= htmlspecialchars($selectedPhotoName) ?>"
                data-share-text="<?= htmlspecialchars($photoShareText) ?>"
                data-share-url="<?= htmlspecialchars($selectedPhotoUrl) ?>"
            >
                <button class="share-button" type="button" data-share-action="toggle" aria-expanded="false">
                    Condividi foto
                </button>

                <div class="share-menu" data-share-menu hidden>
                    <button type="button" data-share-action="native" hidden>Condividi con un'app…</button>
                    <p class="share-native-hint" data-share-native-hint hidden>Per Instagram scegli questa opzione oppure copia il link.</p>
                    <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="facebook">Facebook</a>
                    <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="whatsapp">WhatsApp</a>
                    <a href="#" target="_blank" rel="noopener noreferrer" data-share-action="telegram">Telegram</a>
                    <a href="#" data-share-action="email">Email</a>
                    <button type="button" data-share-action="copy">Copia link</button>
                </div>

                <div class="share-feedback" data-share-feedback role="status" aria-live="polite"></div>
            </div>

            <div class="photo-preview-navigation">
                <button id="previous-photo" type="button" class="photo-navigation-button">← Precedente</button>
                <button id="next-photo" type="button" class="photo-navigation-button">Successiva →</button>
            </div>
        </aside>
    </div>

    <footer class="photo-pagination">
        <div class="photo-pagination-summary">
            Pagina <?= $currentPage ?> di <?= max(1, $totalPages) ?> · <?= $totalItems ?> fotografie
        </div>

        <div class="photo-pagination-controls">
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
                        <a href="<?= htmlspecialchars($buildPageUrl($pageNumber, $pageSize)) ?>">
                            <?= $pageNumber ?>
                        </a>
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
                        <option
                            value="<?= $availablePageSize ?>"
                            <?= $availablePageSize === $pageSize ? 'selected' : '' ?>
                        >
                            <?= $availablePageSize ?>
                        </option>
                    <?php endforeach; ?>
                </select>

                <input type="hidden" name="page" value="1">
            </form>
        </div>
    </footer>

    <div
        id="photo-browser-data"
        data-current-page="<?= $currentPage ?>"
        data-total-pages="<?= $totalPages ?>"
        data-page-size="<?= $pageSize ?>"
        data-album-url="<?= htmlspecialchars($albumUrl) ?>"
        data-selected-photo-number="<?= $selectedPhotoNumber ?>"
        data-total-photos="<?= $totalItems ?>"
        hidden
    ></div>
</section>

<script src="<?= BASE_PATH ?>/public/js/photo-browser.js"></script>
