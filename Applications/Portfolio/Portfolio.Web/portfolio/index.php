<?php

require_once __DIR__ . '/config/config.php';

$requestUri = trim(parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH), '/');

$requestPath = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH) ?? '/';
$basePath = trim(BASE_PATH, '/');
$requestPath = trim($requestPath, '/');

if ($requestPath === $basePath) {
    $request = '';
} elseif (str_starts_with($requestPath, $basePath . '/')) {
    $request = substr($requestPath, strlen($basePath) + 1);
} else {
    http_response_code(404);
    exit;
}

// I vecchi percorsi tecnici ZenPhoto devono essere rifiutati prima della risoluzione degli album.
$firstSegment = strtolower(explode('/', $request, 2)[0]);
$legacyZenPhotoSegments = ['zp-core', 'zp-data', 'zp-content', 'zp-extensions', 'zp-themes'];

if (in_array($firstSegment, $legacyZenPhotoSegments, true)) {
    $remoteAddress = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
    AppLogger::writeThrottled(
        'legacy-route|' . $request . '|' . $remoteAddress,
        sprintf(
            '[Portfolio LegacyRoute] HTTP 410; path: %s; user-agent: %s; remote-address: %s.',
            $request,
            $_SERVER['HTTP_USER_AGENT'] ?? 'unknown',
            $remoteAddress
        ),
        LEGACY_ROUTE_LOG_THROTTLE_SECONDS
    );

    http_response_code(410);
    echo 'Risorsa rimossa definitivamente.';
    exit;
}

// HOME: /portfolio/
if ($request === '') {
    require_once __DIR__ . '/app/Controllers/HomeController.php';

    (new HomeController())->index();
    exit;
}

// CHI SONO: /portfolio/chi-sono
if ($request === 'chi-sono') {
    require_once __DIR__ . '/app/Controllers/AboutController.php';

    (new AboutController())->index();
    exit;
}

// SERVIZI: /portfolio/servizi-fotografici
if ($request === 'servizi-fotografici') {
    require_once __DIR__ . '/app/Controllers/ServicesController.php';

    (new ServicesController())->index();
    exit;
}

// STORIE: /portfolio/stories e /portfolio/stories/{slug}
if ($request === 'stories' || str_starts_with($request, 'stories/')) {
    require_once __DIR__ . '/app/Controllers/StoriesController.php';

    $controller = new StoriesController();
    if ($request === 'stories') {
        $controller->index();
    } else {
        $slug = substr($request, strlen('stories/'));
        if ($slug === '' || str_contains($slug, '/')) {
            http_response_code(404);
            echo 'Articolo non trovato.';
        } else {
            $controller->show(rawurldecode($slug));
        }
    }
    exit;
}

// SITEMAP: /portfolio/sitemap.xml
if ($request === 'sitemap.xml') {
    require_once __DIR__ . '/app/Controllers/SitemapController.php';

    (new SitemapController())->index();
    exit;
}

// ALBUM: /portfolio/{path}
require_once __DIR__ . '/app/Controllers/AlbumController.php';
(new AlbumController())->showByPath($request);
