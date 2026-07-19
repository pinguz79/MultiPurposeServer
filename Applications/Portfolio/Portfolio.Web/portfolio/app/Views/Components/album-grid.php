<?php

declare(strict_types=1);

$albumGridTitle ??= null;
$albums ??= [];
?>

<section class="album-grid">
    <?php if (!empty($albumGridTitle)): ?>
        <h3 class="section-title"><?= htmlspecialchars($albumGridTitle) ?></h3>
    <?php endif; ?>

    <div class="grid">
        <?php foreach ($albums as $album): ?>
            <?php require __DIR__ . '/album-card.php'; ?>
        <?php endforeach; ?>
    </div>
</section>
