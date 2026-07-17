<?php

ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

require_once __DIR__ . '/config/config.php';

$requestUri = trim(parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH), '/');

// rimuove il prefisso "portfolio"
$request = preg_replace('#^portfolio/?#', '', $requestUri);
$request = trim($request, '/');

// HOME: /portfolio/
if ($request === '') {
    require_once __DIR__ . '/app/Controllers/HomeController.php';

    (new HomeController())->index();
    exit;
}

// ALBUM: /portfolio/{path}
require_once __DIR__ . '/app/Controllers/AlbumController.php';
(new AlbumController())->showByPath($request);
