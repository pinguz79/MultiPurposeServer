<?php
$albums = isset($albums) && is_array($albums) ? $albums : [];
?>

<section class="page-hero">
    <h2>Marco Lepri Photography</h2>
    <p>
        Gallerie fotografiche, calendari, shooting glamour, ritratti,
        sfilate e progetti editoriali.
    </p>
</section>

<?php if (empty($albums)): ?>
    <p class="empty-state">
        Nessuna galleria disponibile.
    </p>
<?php else: ?>
    <?php
    $albumGridTitle = null;
    require __DIR__ . '/../Components/album-grid.php';
    ?>
<?php endif; ?>
