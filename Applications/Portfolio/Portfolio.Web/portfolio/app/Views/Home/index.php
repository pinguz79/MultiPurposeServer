<?php if (empty($albums) || !is_array($albums)): ?>
    <p>Nessuna galleria disponibile.</p>
<?php else: ?>

<section class="page-hero">
    <h2>Marco Lepri Photography</h2>
    <p>
        Gallerie fotografiche, calendari, shooting glamour, ritratti,
        sfilate e progetti editoriali.
    </p>
</section>

<div class="grid">
    <?php foreach ($albums as $album): ?>

        <?php
        $cover = $album['coverImage'] ?? null;

        $thumbUrl = $cover['thumbUrl'] ?? (BASE_PATH . '/public/img/album-placeholder.png');
        $alt = $cover['alt'] ?? ('Galleria fotografica ' . ($album['name'] ?? 'Album'));

        $albumPath = $album['path'] ?? '';
        $albumUrl = BASE_PATH . '/' . rawurlencode($albumPath);
        ?>

        <a class="card-link" href="<?= htmlspecialchars($albumUrl) ?>">
            <div class="card">
                <div class="cover">
                    <img
                        src="<?= htmlspecialchars($thumbUrl) ?>"
                        alt="<?= htmlspecialchars($alt) ?>">
                </div>

                <div class="title">
                    <?= htmlspecialchars($album['name'] ?? 'Album senza nome') ?>
                </div>

                <div class="meta">
                    <?= (int)($album['children'] ?? 0) ?> sub-album
                    •
                    <?= (int)($album['photos'] ?? 0) ?> foto
                </div>
            </div>
        </a>

    <?php endforeach; ?>
</div>

<?php endif; ?>