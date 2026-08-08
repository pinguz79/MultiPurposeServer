<?php

declare(strict_types=1);

class AppLogger
{
    private const FATAL_ERROR_TYPES = E_ERROR | E_PARSE | E_CORE_ERROR | E_COMPILE_ERROR | E_USER_ERROR;

    public static function initialize(): void
    {
        ini_set('display_errors', '0');
        ini_set('display_startup_errors', '0');
        error_reporting(E_ALL);

        if (!is_file(LOG_FILE)) {
            self::write('[Portfolio AppLogger] Logging initialized.');
        }

        register_shutdown_function(static function (): void {
            $error = error_get_last();

            if ($error === null || ($error['type'] & self::FATAL_ERROR_TYPES) === 0) {
                return;
            }

            self::write(sprintf(
                '[Portfolio Fatal Error] %s in %s:%d',
                $error['message'] ?? 'Unknown fatal error.',
                $error['file'] ?? 'unknown file',
                $error['line'] ?? 0
            ));
        });
    }

    public static function write(string $message): bool
    {
        $line = sprintf("[%s] %s%s", date(DATE_ATOM), $message, PHP_EOL);

        return error_log($line, 3, LOG_FILE);
    }

    public static function exception(string $context, Throwable $exception, ?string $requestPath = null): bool
    {
        return self::write(sprintf(
            "[%s] %s: %s in %s:%d%s\nStack trace:\n%s",
            $context,
            get_class($exception),
            $exception->getMessage(),
            $exception->getFile(),
            $exception->getLine(),
            $requestPath === null ? '' : "\nRequest path: " . $requestPath,
            $exception->getTraceAsString()
        ));
    }
}
