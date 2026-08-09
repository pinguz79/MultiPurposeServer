<?php

declare(strict_types=1);

require_once __DIR__ . '/../Services/SitemapService.php';

class SitemapController
{
    public function index(): void
    {
        try {
            $urls = (new SitemapService())->getUrls();

            header('Content-Type: application/xml; charset=UTF-8');
            echo $this->render($urls);
        } catch (RuntimeException $exception) {
            AppLogger::write('[Portfolio SitemapController] ' . $exception->getMessage());
            http_response_code(503);
            header('Content-Type: text/plain; charset=UTF-8');
            echo 'Sitemap temporaneamente non disponibile.';
        }
    }

    private function render(array $urls): string
    {
        $items = array_map(
            static fn(string $url): string => sprintf(
                "  <url>\n    <loc>%s</loc>\n  </url>",
                htmlspecialchars($url, ENT_XML1 | ENT_QUOTES, 'UTF-8')
            ),
            $urls
        );

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            . "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
            . implode("\n", $items)
            . "\n</urlset>\n";
    }
}
