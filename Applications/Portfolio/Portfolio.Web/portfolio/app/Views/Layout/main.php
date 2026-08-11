<?php

declare(strict_types=1);

require_once __DIR__ . '/../Models/PageMetadata.php';

$pageMetadata ??= new PageMetadata(
    title: 'Marco Lepri Photography',
    socialTitle: 'Marco Lepri Photography',
    description: 'Gallerie fotografiche di Marco Lepri Photography.',
    canonicalUrl: PUBLIC_BASE_URL . '/'
);

$stylesheets = [
    'base.css',
    'layout.css',
    'components.css',
    'home.css',
    'album.css',
    'about.css',
    'services.css',
    'stories.css',
];
?>

<!DOCTYPE html>
<html lang="it">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <title><?= htmlspecialchars($pageMetadata->title) ?></title>
    <meta name="description" content="<?= htmlspecialchars($pageMetadata->description) ?>">
    <link rel="canonical" href="<?= htmlspecialchars($pageMetadata->canonicalUrl) ?>">

    <meta property="og:type" content="<?= htmlspecialchars($pageMetadata->openGraphType) ?>">
    <meta property="og:site_name" content="Marco Lepri Photography">
    <meta property="og:title" content="<?= htmlspecialchars($pageMetadata->socialTitle) ?>">
    <meta property="og:description" content="<?= htmlspecialchars($pageMetadata->description) ?>">
    <meta property="og:url" content="<?= htmlspecialchars($pageMetadata->canonicalUrl) ?>">
    <?php if ($pageMetadata->imageUrl !== null): ?>
        <meta property="og:image" content="<?= htmlspecialchars($pageMetadata->imageUrl) ?>">
    <?php endif; ?>
    <?php if ($pageMetadata->publishedAt !== null): ?>
        <meta property="article:published_time" content="<?= htmlspecialchars($pageMetadata->publishedAt) ?>">
    <?php endif; ?>

    <meta name="twitter:card" content="<?= $pageMetadata->imageUrl !== null ? 'summary_large_image' : 'summary' ?>">
    <meta name="twitter:title" content="<?= htmlspecialchars($pageMetadata->socialTitle) ?>">
    <meta name="twitter:description" content="<?= htmlspecialchars($pageMetadata->description) ?>">
    <?php if ($pageMetadata->imageUrl !== null): ?>
        <meta name="twitter:image" content="<?= htmlspecialchars($pageMetadata->imageUrl) ?>">
    <?php endif; ?>

    <?php foreach ($stylesheets as $stylesheet): ?>
        <?php $stylesheetVersion = filemtime(__DIR__ . '/../../../public/css/' . $stylesheet); ?>
        <link rel="stylesheet" href="<?= BASE_PATH ?>/public/css/<?= rawurlencode($stylesheet) ?>?v=<?= $stylesheetVersion ?>">
    <?php endforeach; ?>

    <link rel="preload" as="script" href="https://cdn.iubenda.com/cs/iubenda_cs.js">
    <link rel="preload" as="script" href="https://cdn.iubenda.com/cs/tcf/stub-v2.js">
    <script src="https://cdn.iubenda.com/cs/tcf/stub-v2.js"></script>
    <script>
        (self._iub = self._iub || []).csConfiguration = {
            cookiePolicyId: 24901911,
            siteId: 3792730,
            localConsentDomain: 'marcolepriph.altervista.org',
            timeoutLoadConfiguration: 30000,
            lang: 'it',
            enableTcf: true,
            tcfVersion: 2,
            tcfPurposes: {
                2: 'consent_only',
                3: 'consent_only',
                4: 'consent_only',
                5: 'consent_only',
                6: 'consent_only',
                7: 'consent_only',
                8: 'consent_only',
                9: 'consent_only',
                10: 'consent_only'
            },
            invalidateConsentWithoutLog: true,
            googleAdditionalConsentMode: true,
            consentOnContinuedBrowsing: false,
            banner: {
                position: 'top',
                acceptButtonDisplay: true,
                customizeButtonDisplay: true,
                closeButtonDisplay: true,
                closeButtonRejects: true,
                fontSizeBody: '14px'
            }
        };
    </script>
    <script async src="https://cdn.iubenda.com/cs/iubenda_cs.js"></script>
</head>

<body>
    <header class="site-header">
        <div class="site-header-inner">
            <a class="brand" href="<?= BASE_PATH ?>/">
                <span class="brand-name">Marco Lepri</span>
                <span class="brand-subtitle">Photography</span>
            </a>
            <nav class="site-navigation" aria-label="Navigazione principale">
                <a href="<?= BASE_PATH ?>/">Portfolio</a>
                <a href="<?= BASE_PATH ?>/servizi-fotografici">Servizi</a>
                <a href="<?= BASE_PATH ?>/stories">Storie</a>
                <a href="<?= BASE_PATH ?>/chi-sono">Chi sono</a>
            </nav>
        </div>
    </header>

    <main>
        <?php require $view; ?>
    </main>

    <footer class="site-footer">
        <small>
            Powered by ModelBook.Cloud
            · <a href="<?= BASE_PATH ?>/servizi-fotografici">Servizi fotografici</a>
            · <a href="<?= BASE_PATH ?>/stories">Dietro le quinte</a>
            · <a href="<?= BASE_PATH ?>/chi-sono">Chi sono</a>
            · <a href="https://www.iubenda.com/privacy-policy/24901911" rel="noreferrer nofollow" target="_blank">Privacy Policy</a>
            · <a href="#" role="button" class="iubenda-advertising-preferences-link">Personalizza tracciamento pubblicitario</a>
        </small>
    </footer>
</body>

</html>
