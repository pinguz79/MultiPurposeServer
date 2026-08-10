<?php

declare(strict_types=1);
?>

<section class="page-hero">
    <h1>Marco Lepri Photography</h1>
    <p>Gallerie fotografiche, calendari, shooting glamour, ritratti, sfilate e progetti editoriali.</p>
</section>

<?php if (empty($albums)): ?>
    <p class="empty-state">Nessuna galleria disponibile.</p>
<?php else: ?>
    <?php
    $advertisementContext = 'navigation';
    require __DIR__ . '/../Components/advertisement.php';

    $albumGridTitle = null;
    require __DIR__ . '/../Components/album-grid.php';
    ?>
<?php endif; ?>
