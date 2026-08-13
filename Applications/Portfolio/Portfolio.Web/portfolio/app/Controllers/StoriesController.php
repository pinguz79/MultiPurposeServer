<?php

declare(strict_types=1);

require_once __DIR__ . '/../Services/ArticleRepository.php';
require_once __DIR__ . '/../Views/Models/PageMetadataFactory.php';

final class StoriesController {
    private ArticleRepository $articles;

    public function __construct(?ArticleRepository $articles = null) {
        $this->articles = $articles ?? new ArticleRepository();
    }

    public function index(): void {
        try {
            $articles = $this->articles->getPublished();
        } catch (RuntimeException $exception) {
            AppLogger::exception('Portfolio StoriesController', $exception);
            http_response_code(500);
            echo 'Errore nel recupero degli articoli.';
            return;
        }

        $pageMetadata = PageMetadataFactory::stories();
        $view = __DIR__ . '/../Views/Stories/index.php';
        require __DIR__ . '/../Views/Layout/main.php';
    }

    public function show(string $slug): void {
        try {
            $article = $this->articles->findPublishedBySlug($slug);
        } catch (RuntimeException $exception) {
            AppLogger::exception('Portfolio StoriesController', $exception, $slug);
            http_response_code(500);
            echo 'Errore nel recupero dell\'articolo.';
            return;
        }

        if ($article === null) {
            http_response_code(404);
            echo 'Articolo non trovato.';
            return;
        }

        $pageMetadata = PageMetadataFactory::article($article);
        $view = __DIR__ . '/../Views/Stories/show.php';
        require __DIR__ . '/../Views/Layout/main.php';
    }
}
