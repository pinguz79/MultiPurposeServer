<?php

declare(strict_types=1);

$lastBreadcrumbIndex = array_key_last($breadcrumbs);
?>

<nav class="breadcrumb" aria-label="Percorso di navigazione">
    <a href="<?= htmlspecialchars(BASE_PATH . '/') ?>">← Tutte le gallerie</a>

    <?php foreach ($breadcrumbs as $index => $breadcrumb): ?>
        <?php
        $isCurrent = $index === $lastBreadcrumbIndex;

        $segments = array_filter(explode('/', $breadcrumb['path'] ?? ''));
        $breadcrumbUrl = BASE_PATH . '/' . implode('/', array_map('rawurlencode', $segments));
        ?>

        <span class="breadcrumb-separator" aria-hidden="true">›</span>

        <?php if ($isCurrent): ?>
            <span class="breadcrumb-current" aria-current="page"><?= htmlspecialchars($breadcrumb['name'] ?? '') ?></span>
        <?php else: ?>
            <a href="<?= htmlspecialchars($breadcrumbUrl) ?>"><?= htmlspecialchars($breadcrumb['name'] ?? '') ?></a>
        <?php endif; ?>
    <?php endforeach; ?>
</nav>
