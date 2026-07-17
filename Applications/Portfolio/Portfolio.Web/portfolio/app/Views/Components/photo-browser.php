<?php
$selectedPhotoIndex = 0;

foreach ($photos as $index => $photo) {
    if (($photo['id'] ?? null) === $selectedPhotoId) {
        $selectedPhotoIndex = $index;
        break;
    }
}

$selectedPhotoNumber = ($currentPage - 1) * $pageSize + $selectedPhotoIndex + 1;
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

            <div id="photo-preview-counter" class="photo-preview-counter">
                Foto <?= $selectedPhotoNumber ?> di <?= $totalItems ?>
            </div>

            <div class="photo-share">
                <button id="share-photo" type="button" class="photo-share-button" aria-expanded="false" aria-controls="photo-share-menu">
                    Condividi foto
                </button>

                <div id="photo-share-menu" class="photo-share-menu" hidden>
                    <a id="share-photo-facebook" href="#" target="_blank" rel="noopener noreferrer">Facebook</a>
                    <a id="share-photo-whatsapp" href="#" target="_blank" rel="noopener noreferrer">WhatsApp</a>
                    <a id="share-photo-telegram" href="#" target="_blank" rel="noopener noreferrer">Telegram</a>
                    <a id="share-photo-email" href="#">Email</a>

                    <button id="copy-photo-link" type="button">
                        Copia link
                    </button>
                </div>

                <div id="photo-share-feedback" class="photo-share-feedback" role="status" aria-live="polite"></div>
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