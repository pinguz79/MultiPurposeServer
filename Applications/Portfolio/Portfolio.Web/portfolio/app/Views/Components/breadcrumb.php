<nav class="breadcrumb" aria-label="Percorso di navigazione">
    <a href="<?= htmlspecialchars(BASE_PATH . '/') ?>">← Tutte le gallerie</a>

    <?php foreach ($breadcrumbs as $index => $breadcrumb): ?>
        <?php
        $isCurrent = $index === array_key_last($breadcrumbs);
        $encodedBreadcrumbPath = implode('/', array_map('rawurlencode', explode('/', $breadcrumb['path'] ?? '')));
        $breadcrumbUrl = BASE_PATH . '/' . $encodedBreadcrumbPath;
        ?>

        <span class="breadcrumb-separator" aria-hidden="true">›</span>

        <?php if ($isCurrent): ?>
            <span class="breadcrumb-current" aria-current="page">
                <?= htmlspecialchars($breadcrumb['name'] ?? '') ?>
            </span>
        <?php else: ?>
            <a href="<?= htmlspecialchars($breadcrumbUrl) ?>">
                <?= htmlspecialchars($breadcrumb['name'] ?? '') ?>
            </a>
        <?php endif; ?>
    <?php endforeach; ?>
</nav>