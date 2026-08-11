<?php

declare(strict_types=1);
?>

<section class="stories-page">
    <header class="stories-header">
        <p class="stories-kicker">Dietro le quinte</p>
        <h1>Storie, progetti e fotografie</h1>
        <p>Idee, preparazione e retroscena dei progetti che hanno contribuito a costruire questo portfolio.</p>
    </header>

    <?php if ($articles === []): ?>
        <p class="empty-state">Non ci sono ancora storie pubblicate.</p>
    <?php else: ?>
        <div class="stories-grid">
            <?php foreach ($articles as $article): ?>
                <article class="story-card">
                    <a href="<?= htmlspecialchars(BASE_PATH . '/stories/' . rawurlencode($article->slug)) ?>">
                        <img src="<?= htmlspecialchars($article->coverImageUrl) ?>"
                             alt="<?= htmlspecialchars($article->coverImageAlt) ?>"
                             loading="lazy">
                        <div class="story-card-content">
                            <time datetime="<?= htmlspecialchars($article->publishedAt) ?>"><?= htmlspecialchars($article->formattedPublishedDate()) ?></time>
                            <h2><?= htmlspecialchars($article->title) ?></h2>
                            <p><?= htmlspecialchars($article->description) ?></p>
                            <span>Leggi la storia →</span>
                        </div>
                    </a>
                </article>
            <?php endforeach; ?>
        </div>
    <?php endif; ?>
</section>
