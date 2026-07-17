<?php

declare(strict_types=1);

$pageTitle ??= 'Portfolio';
?>

<!DOCTYPE html>
<html lang="it">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <title><?= htmlspecialchars($pageTitle) ?></title>

    <link rel="stylesheet" href="<?= BASE_PATH ?>/public/css/style.css">
</head>

<body>
    <header class="site-header">
        <div class="site-header-inner">
            <a class="brand" href="<?= BASE_PATH ?>/">
                <span class="brand-name">Marco Lepri</span>
                <span class="brand-subtitle">Photography</span>
            </a>
        </div>
    </header>

    <main>
        <?php require $view; ?>
    </main>

    <footer class="site-footer">
        <small>Portfolio.Web</small>
    </footer>
</body>

</html>
