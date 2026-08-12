<?php

define('APP_NAME', 'Portfolio.Web');

define('BASE_PATH', '/portfolio');
define('PUBLIC_BASE_URL', 'https://marcolepriph.altervista.org/portfolio');

// Log
define('LOG_DIRECTORY', dirname(__DIR__) . '/internal/logs');
define('LOG_RETENTION_DAYS', 14);
define('LEGACY_ROUTE_LOG_THROTTLE_SECONDS', 3600);
define('LOG_FILE', LOG_DIRECTORY . '/portfolio-' . date('Ymd') . '.log');

if (!is_dir(LOG_DIRECTORY) && !mkdir(LOG_DIRECTORY, 0750, true) && !is_dir(LOG_DIRECTORY)) {
    throw new RuntimeException('Unable to create the application log directory.');
}

error_reporting(E_ALL);
ini_set('display_errors', '0');
ini_set('display_startup_errors', '0');
ini_set('log_errors', '1');
ini_set('error_log', LOG_FILE);

require_once dirname(__DIR__) . '/app/Logging/AppLogger.php';
AppLogger::initialize();

// API MPS
define('API_BASE_URL', 'https://www.modelbook.cloud/Portfolio');
define('API_TIMEOUT', 10);

// Database
define('DB_HOST', 'localhost');
define('DB_NAME', 'my_marcolepriph');
define('DB_USER', '');
define('DB_PASSWORD', '');
