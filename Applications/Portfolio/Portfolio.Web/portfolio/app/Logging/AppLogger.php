<?php

declare(strict_types=1);

class AppLogger {
    private const FATAL_ERROR_TYPES = E_ERROR | E_PARSE | E_CORE_ERROR | E_COMPILE_ERROR | E_USER_ERROR;

    public static function initialize(): void {
        ini_set('display_errors', '0');
        ini_set('display_startup_errors', '0');
        error_reporting(E_ALL);

        if (!is_file(LOG_FILE)) {
            self::write('[Portfolio AppLogger] Logging inizializzato.');
        }

        self::cleanupExpiredLogs();

        register_shutdown_function(static function (): void {
            $error = error_get_last();

            if ($error === null || ($error['type'] & self::FATAL_ERROR_TYPES) === 0) {
                return;
            }

            self::write(sprintf(
                '[Portfolio Fatal Error] %s in %s:%d',
                $error['message'] ?? 'Errore fatale sconosciuto.',
                $error['file'] ?? 'file sconosciuto',
                $error['line'] ?? 0
            ));
        });
    }

    public static function write(string $message): bool {
        $line = sprintf("[%s] %s%s", date(DATE_ATOM), $message, PHP_EOL);

        return error_log($line, 3, LOG_FILE);
    }

    public static function writeThrottled(string $key, string $message, int $intervalSeconds): bool {
        $marker = LOG_DIRECTORY . '/.throttle-' . hash('sha256', $key);
        $lastWrite = is_file($marker) ? filemtime($marker) : false;

        if ($lastWrite !== false && time() - $lastWrite < $intervalSeconds) {
            return false;
        }

        if (file_put_contents($marker, (string) time(), LOCK_EX) === false) {
            return self::write($message);
        }

        return self::write($message);
    }

    public static function exception(string $context, Throwable $exception, ?string $requestPath = null): bool {
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

    private static function cleanupExpiredLogs(): void {
        $cleanupMarker = LOG_DIRECTORY . '/.retention-cleanup';
        $lastCleanup = is_file($cleanupMarker) ? filemtime($cleanupMarker) : false;

        if ($lastCleanup !== false && time() - $lastCleanup < 86400) {
            return;
        }

        if (file_put_contents($cleanupMarker, (string) time(), LOCK_EX) === false) {
            self::write('[Portfolio AppLogger] Impossibile aggiornare il marker della pulizia periodica.');
        }

        $cutoff = time() - (LOG_RETENTION_DAYS * 86400);

        foreach (glob(LOG_DIRECTORY . '/portfolio*.log') ?: [] as $file) {
            if ($file !== LOG_FILE && is_file($file) && filemtime($file) < $cutoff) {
                if (!unlink($file)) {
                    self::write("[Portfolio AppLogger] Impossibile eliminare il log scaduto: {$file}.");
                }
            }
        }

        foreach (glob(LOG_DIRECTORY . '/.throttle-*') ?: [] as $marker) {
            if (is_file($marker) && filemtime($marker) < $cutoff) {
                if (!unlink($marker)) {
                    self::write("[Portfolio AppLogger] Impossibile eliminare il marker scaduto: {$marker}.");
                }
            }
        }
    }
}
