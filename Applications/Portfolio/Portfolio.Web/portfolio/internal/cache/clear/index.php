<?php
require_once __DIR__ . '/../../../config/config.php';
require_once __DIR__ . '/../../../app/Config/Secrets.php';
require_once __DIR__ . '/../../../app/Database/Db.php';
require_once __DIR__ . '/../../../app/Services/RoutingCacheService.php';
require_once __DIR__ . '/../../../app/Services/ApiCacheService.php';

header('Content-Type: application/json; charset=utf-8');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    header('Allow: POST');

    echo json_encode([
        'error' => 'Method not allowed.'
    ]);

    exit;
}

$sharedSecret = $_SERVER['HTTP_X_PORTFOLIO_SHARED_SECRET'] ?? '';

if ($sharedSecret === '' || !hash_equals(PORTFOLIO_SHARED_SECRET, $sharedSecret)) {
    http_response_code(401);

    echo json_encode([
        'error' => 'Unauthorized.'
    ]);

    exit;
}

$requestBody = file_get_contents('php://input');
$requestBody = preg_replace('/^\xEF\xBB\xBF/', '', $requestBody ?? '');

try {
    $request = json_decode($requestBody, true, 512, JSON_THROW_ON_ERROR);
}
catch (JsonException $exception) {
    error_log(sprintf(
        '[Portfolio Cache Clear] Invalid JSON: %s',
        $exception->getMessage()
    ));

    http_response_code(400);

    echo json_encode([
        'error' => 'Invalid JSON request.'
    ]);

    exit;
}

if (!is_array($request)) {
    http_response_code(400);

    echo json_encode([
        'error' => 'JSON request must be an object.'
    ]);

    exit;
}

$clearAlbumRoutingCache = ($request['clearAlbumRoutingCache'] ?? false) === true;
$clearPhotoRoutingCache = ($request['clearPhotoRoutingCache'] ?? false) === true;
$clearApiResponseCache = ($request['clearApiResponseCache'] ?? false) === true;

if (!$clearAlbumRoutingCache && !$clearPhotoRoutingCache && !$clearApiResponseCache) {
    http_response_code(400);

    echo json_encode([
        'error' => 'At least one cache must be selected.'
    ]);

    exit;
}

$db = Db::connection();
$routingCache = new RoutingCacheService();
$apiCache = new ApiCacheService();

try {
    $db->beginTransaction();

    $result = [
        'albumRoutingEntriesDeleted' => $clearAlbumRoutingCache ? $routingCache->clearAlbums() : 0,
        'photoRoutingEntriesDeleted' => $clearPhotoRoutingCache ? $routingCache->clearPhotos() : 0,
        'apiResponseEntriesDeleted' => $clearApiResponseCache ? $apiCache->clear() : 0
    ];

    $db->commit();

    echo json_encode($result);
}
catch (Throwable $exception) {
    if ($db->inTransaction()) {
        $db->rollBack();
    }

    error_log(sprintf(
        '[Portfolio Cache Clear] %s: %s in %s:%d',
        get_class($exception),
        $exception->getMessage(),
        $exception->getFile(),
        $exception->getLine()
    ));

    http_response_code(500);

    echo json_encode([
        'error' => 'Cache clear failed.'
    ]);
}
