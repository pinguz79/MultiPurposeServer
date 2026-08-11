<?php

declare(strict_types=1);

$structuredData = [
    '@context' => 'https://schema.org',
    '@type' => 'Article',
    'headline' => $article->title,
    'description' => $article->description,
    'image' => [$article->coverImageUrl],
    'datePublished' => $article->publishedAt,
    'mainEntityOfPage' => $article->url(),
    'author' => [
        '@type' => 'Person',
        'name' => 'Marco Lepri',
        'url' => PUBLIC_BASE_URL . '/chi-sono'
    ]
];
?>

<script type="application/ld+json"><?= json_encode(
    $structuredData,
    JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE | JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT
) ?></script>

<article class="story-page">
    <nav class="story-back" aria-label="Ritorno alle storie">
        <a href="<?= BASE_PATH ?>/stories">← Tutte le storie</a>
    </nav>

    <header class="story-header">
        <p class="stories-kicker">Dietro le quinte</p>
        <h1><?= htmlspecialchars($article->title) ?></h1>
        <p class="story-subtitle"><?= htmlspecialchars($article->subtitle) ?></p>
        <time datetime="<?= htmlspecialchars($article->publishedAt) ?>">Pubblicato il <?= htmlspecialchars($article->formattedPublishedDate()) ?></time>
    </header>

    <figure class="story-cover">
        <img src="<?= htmlspecialchars($article->coverImageUrl) ?>"
             alt="<?= htmlspecialchars($article->coverImageAlt) ?>">
        <figcaption><?= htmlspecialchars($article->coverImageAlt) ?></figcaption>
    </figure>

    <div class="story-content">
        <?php foreach ($article->sections as $section): ?>
            <section>
                <h2><?= htmlspecialchars($section['heading']) ?></h2>
                <?php foreach ($section['paragraphs'] as $paragraph): ?>
                    <p><?= htmlspecialchars($paragraph) ?></p>
                <?php endforeach; ?>
            </section>
        <?php endforeach; ?>
    </div>

    <?php if ($article->relatedAlbums !== []): ?>
        <aside class="story-related" aria-labelledby="story-related-title">
            <h2 id="story-related-title">Esplora il progetto</h2>
            <div class="story-related-links">
                <?php foreach ($article->relatedAlbums as $album): ?>
                    <a href="<?= htmlspecialchars(BASE_PATH . '/' . implode('/', array_map('rawurlencode', explode('/', trim($album['path'], '/'))))) ?>">
                        <?= htmlspecialchars($album['label']) ?>
                    </a>
                <?php endforeach; ?>
            </div>
        </aside>
    <?php endif; ?>

    <?php if ($article->contentRating === 'Standard'): ?>
        <?php
        $advertisementContext = 'navigation';
        require __DIR__ . '/../Components/advertisement.php';
        ?>
    <?php endif; ?>
</article>
