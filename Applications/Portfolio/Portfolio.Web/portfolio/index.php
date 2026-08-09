<?php

require_once __DIR__ . '/config/config.php';

$requestUri = trim(parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH), '/');

// rimuove il prefisso "portfolio"
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

// SITEMAP: /portfolio/sitemap.xml
if ($request === 'sitemap.xml') {
    require_once __DIR__ . '/app/Controllers/SitemapController.php';

    (new SitemapController())->index();
    exit;
}

// ALBUM: /portfolio/{path}
require_once __DIR__ . '/app/Controllers/AlbumController.php';
(new AlbumController())->showByPath($request);
